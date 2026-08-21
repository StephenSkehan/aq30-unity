using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using AQ.App;
using AQ.App.UI.Board;
using AQ.App.UI.Common;

namespace AQ.UI.Hints
{
    /// <summary>
    /// Stall detection with escalating nudges (SAS/feature-ftue-onboarding-v1.md,
    /// I4 — Stephen-ruled 2026-08-21; Candy Crush idle-hint convention). When the
    /// board sits idle AND an action is available, escalate gently:
    ///   ~8s  → soft pulse on a mergeable pair (or the generator if no pair)
    ///   ~16s → ghost-drag demonstration on the pair
    ///   ~30s → one short directive toast, then the cycle rests and repeats
    /// Any player action clears and resets. Retires for good once the player has
    /// merged 15 times (they know the loop); never runs during dialogue, the
    /// splash, the FTUE choreography or the guided loop; honors the hints switch.
    /// </summary>
    public sealed class StallNudgeMB : MonoBehaviour
    {
        private const float PulseAt = 8f;
        private const float DemoAt  = 16f;
        private const float ToastAt = 30f;
        private const int   RetireAfterMerges = 15;

        private float _idle;
        private bool  _dialogueOpen;
        private bool  _demoShowing;
        private readonly List<BoardTileView> _pulseTiles = new List<BoardTileView>();
        private MergeBoardController _board;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            Install();
            SceneManager.sceneLoaded += (_, __) => Install();
        }

        private static void Install()
        {
            if (PlayerPrefs.GetInt("aq.hint.merge_ct", 0) >= RetireAfterMerges) return;
            if (Object.FindAnyObjectByType<StallNudgeMB>() != null) return;
            var go = new GameObject("__StallNudge");
            DontDestroyOnLoad(go);
            go.AddComponent<StallNudgeMB>();
        }

        private void OnEnable()
        {
            MergeBoardController.GeneratorTapped         += ResetIdle;
            MergeBoardController.TilesMerged             += OnMerged;
            MergeBoardController.TilesSwapped            += ResetIdle;
            MergeBoardController.BoardCompositionChanged += ResetIdle;
            DialogueRunner.DialogueOpened += OnDialogueOpened;
            DialogueRunner.DialogueClosed += OnDialogueClosed;
        }

        private void OnDisable()
        {
            MergeBoardController.GeneratorTapped         -= ResetIdle;
            MergeBoardController.TilesMerged             -= OnMerged;
            MergeBoardController.TilesSwapped            -= ResetIdle;
            MergeBoardController.BoardCompositionChanged -= ResetIdle;
            DialogueRunner.DialogueOpened -= OnDialogueOpened;
            DialogueRunner.DialogueClosed -= OnDialogueClosed;
            ClearVisuals();
        }

        private void OnDialogueOpened(CaseGraph _) { _dialogueOpen = true; ClearVisuals(); ResetIdle(); }
        private void OnDialogueClosed() { _dialogueOpen = false; ResetIdle(); }

        private void OnMerged(string family, int tier)
        {
            ResetIdle();
            if (PlayerPrefs.GetInt("aq.hint.merge_ct", 0) >= RetireAfterMerges)
                Destroy(gameObject); // graduated — the player knows the loop
        }

        private void ResetIdle()
        {
            _idle = 0f;
            ClearVisuals();
        }

        private void Update()
        {
            if (Suppressed()) { _idle = 0f; return; }

            // Any touch/click counts as activity — even a miss means they're engaged.
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
            {
                ResetIdle();
                return;
            }

            _idle += Time.unscaledDeltaTime;

            if (_idle >= PulseAt && _pulseTiles.Count == 0)
                BeginPulse();

            if (_idle >= DemoAt && !_demoShowing && _pulseTiles.Count == 2)
            {
                _demoShowing = true;
                GhostDragDemoMB.Show(_pulseTiles[0], _pulseTiles[1]);
            }

            if (_idle >= ToastAt)
            {
                // DRAFT copy pending Stephen ruling (directive tier).
                ToastService.Show("stall",
                    _pulseTiles.Count == 2 ? "Drag matching items together."
                                           : "Tap the generator for more items.", 3f);
                _idle = 0f; // rest, then the cycle may repeat
                ClearVisuals();
            }

            // Animate the pulse.
            if (_pulseTiles.Count > 0)
            {
                float phase = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f / 0.9f) + 1f) * 0.5f;
                float scale = 1f + 0.07f * phase;
                foreach (var v in _pulseTiles)
                    if (v != null && v.itemImage != null && v.itemImage.enabled)
                        v.itemImage.transform.localScale = Vector3.one * scale;
            }
        }

        private bool Suppressed()
        {
            if (!HintService.Enabled) return true;
            if (_dialogueOpen) return true;
            if (AQ.App.UI.StudioSplashMB.Showing) return true;
            // The choreography and guided loop own the board while they exist.
            if (GameObject.Find("FTUEFirstMergeChoreography") != null) return true;
            if (GameObject.Find("GuidedCaseLoop") != null) return true;
            return false;
        }

        private void BeginPulse()
        {
            if (_board == null) _board = Object.FindAnyObjectByType<MergeBoardController>();
            if (_board == null || !_board.GridReady) return;

            // Prefer a mergeable pair; fall back to the generator if energy allows.
            var firstByKey = new Dictionary<(string fam, int tier), BoardTileView>();
            for (int r = 0; r < _board.Rows; r++)
                for (int c = 0; c < _board.Cols; c++)
                {
                    var v = _board.Get(r, c);
                    if (v == null || v.IsEmpty || v.Kind != TileKind.Item) continue;
                    if (!_board.IsMergeCandidate(v)) continue;
                    var key = (_board.GetFamily(v), v.Tier);
                    if (firstByKey.TryGetValue(key, out var first))
                    {
                        _pulseTiles.Add(first);
                        _pulseTiles.Add(v);
                        return;
                    }
                    firstByKey[key] = v;
                }

            var wallet = AQ.App.Economy.WalletLocator.Instance;
            bool hasEnergy = wallet == null || wallet.Get(AQ.SharedKernel.Economy.Currency.Energy) > 0;
            if (!hasEnergy) return; // nothing actionable — stay quiet

            for (int r = 0; r < _board.Rows; r++)
                for (int c = 0; c < _board.Cols; c++)
                {
                    var v = _board.Get(r, c);
                    if (v != null && !v.IsEmpty && v.Kind == TileKind.Generator)
                    {
                        _pulseTiles.Add(v);
                        return;
                    }
                }
        }

        private void ClearVisuals()
        {
            foreach (var v in _pulseTiles)
                if (v != null && v.itemImage != null)
                    v.itemImage.transform.localScale = Vector3.one;
            _pulseTiles.Clear();
            if (_demoShowing)
            {
                GhostDragDemoMB.Hide();
                _demoShowing = false;
            }
        }
    }
}
