using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using AQ.App;
using AQ.App.Leads;
using AQ.App.Overflow;
using AQ.App.UI.Board;

namespace AQ.UI.Hints
{
    /// <summary>
    /// Requirement-driven source guidance (Stephen-ruled 2026-08-21, playtest
    /// round 3): every generator that produces an item some active lead still
    /// NEEDS pulses gently until those requirements are fulfilled, then stops.
    /// If a needed family's generator is not on the board but sits in the
    /// Stash, the STASH button pulses instead — the actionable step is placing
    /// it. This is standing board guidance, not a one-shot hint: it replaces
    /// "pulse whatever generator we find" (which pointed at the diner while
    /// audio items were required) with "pulse the SOURCE of what's missing".
    /// Pauses during dialogue and while the FTUE choreography or guided loop
    /// own the board.
    /// </summary>
    public sealed class RequiredSourcePulseMB : MonoBehaviour
    {
        private const float RecomputeEvery = 0.5f;
        private const float Amplitude = 0.09f; // +50% (Stephen-ruled 2026-08-22)

        private MergeBoardController _board;
        private LeadsRepository _repo;
        private float _nextRecomputeAt;
        private bool _dialogueOpen;
        private bool _stashPulse;
        private Transform _stashTarget;
        private readonly List<BoardTileView> _targets = new List<BoardTileView>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            Install();
            SceneManager.sceneLoaded += (_, __) => Install();
        }

        private static void Install()
        {
            if (Object.FindAnyObjectByType<RequiredSourcePulseMB>() != null) return;
            var go = new GameObject("__RequiredSourcePulse");
            DontDestroyOnLoad(go);
            go.AddComponent<RequiredSourcePulseMB>();
        }

        // Pulse arms only after 10s without a generator tap, and disarms the
        // moment one lands (Stephen-ruled 2026-08-22): guidance for the player
        // who has STOPPED producing, not a constant heartbeat.
        private const float ArmAfterSeconds = 10f;
        private float _lastGenTapAt;

        private void OnEnable()
        {
            DialogueRunner.DialogueOpened += OnDialogueOpened;
            DialogueRunner.DialogueClosed += OnDialogueClosed;
            MergeBoardController.GeneratorTapped += OnGeneratorTapped;
            _lastGenTapAt = Time.unscaledTime; // grace at boot, then arm
        }

        private void OnDisable()
        {
            DialogueRunner.DialogueOpened -= OnDialogueOpened;
            DialogueRunner.DialogueClosed -= OnDialogueClosed;
            MergeBoardController.GeneratorTapped -= OnGeneratorTapped;
            ClearVisuals();
        }

        private void OnGeneratorTapped()
        {
            _lastGenTapAt = Time.unscaledTime;
            ClearVisuals();
        }

        private void OnDialogueOpened(CaseGraph _) => _dialogueOpen = true;
        private void OnDialogueClosed() => _dialogueOpen = false;

        private void Update()
        {
            if (Suppressed() || Time.unscaledTime - _lastGenTapAt < ArmAfterSeconds)
            {
                ClearVisuals();
                _nextRecomputeAt = 0f;
                return;
            }

            if (Time.unscaledTime >= _nextRecomputeAt)
            {
                _nextRecomputeAt = Time.unscaledTime + RecomputeEvery;
                Recompute();
            }

            // Gentle, slightly slower than the hint pulse so the two read apart.
            float phase = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f / 1.1f) + 1f) * 0.5f;
            float scale = 1f + Amplitude * phase;

            foreach (var v in _targets)
                if (v != null && v.itemImage != null && v.itemImage.enabled)
                    v.itemImage.transform.localScale = Vector3.one * scale;

            if (_stashPulse)
            {
                if (_stashTarget == null)
                {
                    var go = GameObject.Find("BucketRoot");
                    _stashTarget = go != null ? go.transform : null;
                }
                if (_stashTarget != null)
                    _stashTarget.localScale = Vector3.one * scale;
            }
        }

        private bool Suppressed()
        {
            if (_dialogueOpen) return true;
            if (AQ.App.UI.StudioSplashMB.Showing) return true;
            if (GameObject.Find("FTUEFirstMergeChoreography") != null) return true;
            // Only while the loop is actively directing (banner up): suppressing
            // for its whole lifetime muted all source guidance mid-case, since
            // the loop object survives quietly until the L2 proceed
            // (Stephen playtest 2026-08-22: kit never pulsed).
            if (GuidedCaseLoopMB.OwnsBoard) return true;
            return false;
        }

        private void Recompute()
        {
            if (_board == null) _board = Object.FindAnyObjectByType<MergeBoardController>();
            if (_repo == null)  _repo  = Object.FindAnyObjectByType<LeadsRepository>();
            ClearVisuals();
            if (_board == null || !_board.GridReady || _repo == null) return;

            var requiredGenIds = RequiredGeneratorIds(_board, _repo);
            if (requiredGenIds.Count == 0) return;

            // Board generators of a required type pulse in place.
            var onBoard = new HashSet<string>();
            for (int r = 0; r < _board.Rows; r++)
                for (int c = 0; c < _board.Cols; c++)
                {
                    var v = _board.Get(r, c);
                    if (v == null || v.IsEmpty || v.Kind != TileKind.Generator) continue;
                    var id = _board.GetFamily(v);
                    if (!requiredGenIds.Contains(id)) continue;
                    onBoard.Add(id);
                    _targets.Add(v);
                }

            // A required source that only exists in the Stash: the actionable
            // step is PLACING it, so the Stash button carries the pulse — and
            // Gerald says why (Stephen-ruled 2026-08-22).
            foreach (var entry in OverflowBucketService.Items)
            {
                if (entry.kind != OverflowKind.Generator) continue;
                if (!requiredGenIds.Contains(entry.family) || onBoard.Contains(entry.family)) continue;
                _stashPulse = true;
                HintService.Request("stash_gen",
                    "Look in the Stash for new tools.",
                    () => { var go = GameObject.Find("BucketRoot"); return go != null ? go.transform : null; },
                    () => _stashPulse
                       && !AQ.App.UI.EvidenceBoard.EvidenceBoardScreen.IsOpen
                       && !AQ.App.UI.Board.LockerScreen.IsOpen);
                break;
            }
        }

        /// <summary>
        /// Generator type ids that produce any family some active lead still has
        /// an UNSATISFIED requirement for. Shared with the stall nudge so every
        /// pulsing system agrees on which sources matter.
        /// </summary>
        public static HashSet<string> RequiredGeneratorIds(MergeBoardController board, LeadsRepository repo)
        {
            var result = new HashSet<string>();
            if (board == null || repo == null) return result;

            var unsatFamilies = new HashSet<string>();
            var leads = repo.CurrentLeads;
            for (int i = 0; i < leads.Count; i++)
            {
                var lead = leads[i];
                if (lead == null || lead.RuntimeState == LeadState.Blocked) continue;
                var reqs = lead.requirements;
                if (reqs == null) continue;
                foreach (var req in reqs)
                {
                    if (req.IsSatisfied) continue;
                    var def = req.itemDefinition;
                    if (def != null && !string.IsNullOrEmpty(def.family))
                        unsatFamilies.Add(def.family);
                }
            }
            if (unsatFamilies.Count == 0) return result;

            var types = board.generatorTypes;
            if (types == null) return result;
            foreach (var so in types)
            {
                if (so == null || so.tiers == null) continue;
                foreach (var tier in so.tiers)
                {
                    if (tier?.dropTable == null) continue;
                    foreach (var e in tier.dropTable)
                        if (e.type == AQ.App.Generators.DropType.Item && unsatFamilies.Contains(e.itemFamily))
                        {
                            result.Add(so.generatorTypeId);
                            break;
                        }
                    if (result.Contains(so.generatorTypeId)) break;
                }
            }
            return result;
        }

        /// <summary>
        /// True when merging (family, tier) moves toward some UNSATISFIED lead
        /// requirement (same family, higher tier). Gates the ghost demo: never
        /// demonstrate a merge whose result progresses nothing (Stephen-ruled
        /// 2026-08-22).
        /// </summary>
        public static bool MergeProgressesTowardNeed(LeadsRepository repo, string family, int tier)
        {
            if (repo == null || string.IsNullOrEmpty(family)) return false;
            var leads = repo.CurrentLeads;
            for (int i = 0; i < leads.Count; i++)
            {
                var lead = leads[i];
                if (lead == null || lead.RuntimeState == LeadState.Blocked) continue;
                var reqs = lead.requirements;
                if (reqs == null) continue;
                foreach (var req in reqs)
                {
                    if (req.IsSatisfied) continue;
                    var def = req.itemDefinition;
                    if (def != null && def.family == family && def.tier > tier) return true;
                }
            }
            return false;
        }

        private void ClearVisuals()
        {
            foreach (var v in _targets)
                if (v != null && v.itemImage != null)
                    v.itemImage.transform.localScale = Vector3.one;
            _targets.Clear();

            if (_stashTarget != null) _stashTarget.localScale = Vector3.one;
            _stashPulse = false;
        }
    }
}
