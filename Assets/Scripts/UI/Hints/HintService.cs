using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AQ.App;
using AQ.App.Overflow;
using AQ.App.UI;
using AQ.App.UI.Board;
using AQ.App.UI.Specials;

namespace AQ.UI.Hints
{
    /// <summary>
    /// One-time contextual hints (cohort feedback 2026-08-14: "lack of
    /// tutorial"). Each hint fires exactly once per save (NarrativeFlags
    /// aq.hint.*), queues while dialogue or the FTUE choreography holds the
    /// stage, and dismisses on tap or timeout. Copy is Ally-dry, no chrome,
    /// no em dashes.
    /// </summary>
    public static class HintService
    {
        private const string FlagPrefix = "aq.hint.";
        public const string EnabledPref = "aq.hints.enabled"; // Config tab switch

        private static readonly Queue<(string id, string text, Func<Transform> anchor)> _queue = new();
        private static readonly HashSet<string> _pending = new(); // queued or showing
        private static HintRunnerMB _runner;

        public static bool Enabled
        {
            get => PlayerPrefs.GetInt(EnabledPref, 1) == 1;
            set { PlayerPrefs.SetInt(EnabledPref, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static void Request(string id, string text, Func<Transform> anchor = null)
        {
            // Seen is flagged on manual close (Stephen-ruled 2026-08-14), not at
            // request: hints persist until X-closed, and a crash or a disabled
            // switch re-offers on the next natural trigger rather than silently
            // burning the one chance. _pending dedups repeat triggers meanwhile.
            if (!Enabled) return;
            if (NarrativeFlags.Has(FlagPrefix + id)) return;
            if (!_pending.Add(id)) return;
            _queue.Enqueue((id, text, anchor));
            EnsureRunner();
        }

        internal static void MarkClosed(string id)
        {
            NarrativeFlags.Set(FlagPrefix + id);
            _pending.Remove(id);
        }

        public static bool Seen(string id) => NarrativeFlags.Has(FlagPrefix + id);
        public static void ResetAll(IEnumerable<string> ids)
        {
            foreach (var id in ids) NarrativeFlags.Clear(FlagPrefix + id);
        }

        private static void EnsureRunner()
        {
            if (_runner != null) return;
            var go = new GameObject("__HintRunner");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _runner = go.AddComponent<HintRunnerMB>();
        }

        internal static bool TryDequeue(out (string id, string text, Func<Transform> anchor) hint)
        {
            if (_queue.Count > 0) { hint = _queue.Dequeue(); return true; }
            hint = default;
            return false;
        }
    }

    internal sealed class HintRunnerMB : MonoBehaviour
    {
        private GameObject _chip;
        private string _chipId;
        private RectTransform _closeRt;
        private Canvas _chipCanvas;
        private bool _dialogueOpen;

        private void OnEnable()
        {
            DialogueRunner.DialogueOpened += OnDialogueOpened;
            DialogueRunner.DialogueClosed += OnDialogueClosed;
        }

        private void OnDisable()
        {
            DialogueRunner.DialogueOpened -= OnDialogueOpened;
            DialogueRunner.DialogueClosed -= OnDialogueClosed;
        }

        private void OnDialogueOpened(CaseGraph _) => _dialogueOpen = true;
        private void OnDialogueClosed() => _dialogueOpen = false;

        private void Update()
        {
            if (_chip != null)
            {
                // Persistent chip hides (not dies) while dialogue holds the stage.
                bool hidden = Suppressed();
                if (_chipCanvas != null && _chipCanvas.enabled == hidden)
                    _chipCanvas.enabled = !hidden;
                if (hidden) return;

                // X-close only (Stephen-ruled 2026-08-14). Raw input per house GR lesson.
                if (Input.GetMouseButtonDown(0) && _closeRt != null &&
                    RectTransformUtility.RectangleContainsScreenPoint(_closeRt, Input.mousePosition, null))
                {
                    HintService.MarkClosed(_chipId);
                    Destroy(_chip);
                    _chip = null;
                }
                return;
            }

            if (Suppressed()) return;
            if (HintService.TryDequeue(out var hint))
                Show(hint.id, hint.text, hint.anchor);
        }

        private bool Suppressed()
        {
            if (_dialogueOpen) return true;
            // Teaching begins after FTUE + L1 are fully done (Stephen-ruled
            // 2026-08-14): the choreography and first payoff stay clean, and the
            // queue holds anything triggered earlier.
            if (!NarrativeFlags.Has("aq.lead.e1_tip.seen")) return true;
            return PlayerPrefs.GetInt("aq.ftue.first_merge.stage", 0) == 1;
        }

        private void Show(string id, string text, Func<Transform> anchorFn)
        {
            _chipId = id;
            _chip = new GameObject("__HintChip", typeof(Canvas), typeof(CanvasScaler));
            var canvas = _chip.GetComponent<Canvas>();
            _chipCanvas = canvas;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 4000; // above popups (<=1000), below DragGhost (5000)
            var scaler = _chip.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(_chip.transform, false);
            var prt = (RectTransform)panel.transform;
            prt.sizeDelta = new Vector2(820f, 128f);
            prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0.5f);
            // One fixed slot below the HUD (Stephen-ruled 2026-08-14): persistent
            // chips must never cover board cells. The target gets a pulse instead.
            prt.anchoredPosition = new Vector2(0f, 570f);
            PulseTarget(anchorFn);
            var img = panel.GetComponent<Image>();
            img.sprite = AQTheme.Rounded;
            img.type = Image.Type.Sliced;
            img.color = new Color(0.10f, 0.12f, 0.16f, 0.96f);
            img.raycastTarget = false;

            var accent = new GameObject("Accent", typeof(RectTransform), typeof(Image));
            accent.transform.SetParent(prt, false);
            var art = (RectTransform)accent.transform;
            art.anchorMin = new Vector2(0f, 0f);
            art.anchorMax = new Vector2(0f, 1f);
            art.offsetMin = new Vector2(0f, 10f);
            art.offsetMax = new Vector2(8f, -10f);
            var aimg = accent.GetComponent<Image>();
            aimg.color = new Color(0.96f, 0.72f, 0.25f, 1f); // case-file amber
            aimg.raycastTarget = false;

            var txt = new GameObject("Text", typeof(RectTransform));
            txt.transform.SetParent(prt, false);
            var trt = (RectTransform)txt.transform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(34f, 14f);
            trt.offsetMax = new Vector2(-92f, -14f); // clear the X button
            var tmp = txt.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 34f;
            tmp.color = new Color(0.94f, 0.92f, 0.86f, 1f);
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            AQTheme.StyleText(tmp);

            // Manual X close (raw-input hit test in Update, not a Button).
            var close = new GameObject("Close", typeof(RectTransform), typeof(Image));
            close.transform.SetParent(prt, false);
            _closeRt           = (RectTransform)close.transform;
            _closeRt.anchorMin = _closeRt.anchorMax = new Vector2(1f, 0.5f);
            _closeRt.pivot     = new Vector2(1f, 0.5f);
            _closeRt.sizeDelta = new Vector2(64f, 64f);
            _closeRt.anchoredPosition = new Vector2(-14f, 0f);
            AQTheme.StyleButton(close.GetComponent<Image>(), AQTheme.Steel);
            AQTheme.AddDrawnX(_closeRt, AQTheme.Paper, 22f, 4f);

            AQTheme.PopIn(prt);
        }

        // Brief teal pulse over the hint's subject while the chip sits in its
        // fixed slot — spatial pointing without a persistent chip on the board.
        private void PulseTarget(Func<Transform> anchorFn)
        {
            var anchor = anchorFn?.Invoke();
            if (anchor == null || _chip == null) return;

            Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, anchor.position);
            var go = new GameObject("Pulse", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_chip.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(150f, 150f);
            rt.anchoredPosition = new Vector2(
                (screen.x / Screen.width - 0.5f) * 1080f,
                (screen.y / Screen.height - 0.5f) * 1920f);
            var img = go.GetComponent<Image>();
            img.sprite = AQTheme.Rounded;
            img.type = Image.Type.Sliced;
            img.raycastTarget = false;
            StartCoroutine(PulseRoutine(go, img));
        }

        private System.Collections.IEnumerator PulseRoutine(GameObject go, Image img)
        {
            const float duration = 2.2f;
            float t = 0f;
            while (t < duration && go != null)
            {
                t += Time.unscaledDeltaTime;
                float wave = (Mathf.Sin(t * Mathf.PI * 3f) + 1f) * 0.5f; // ~3 beats
                img.color = new Color(AQTheme.Teal.r, AQTheme.Teal.g, AQTheme.Teal.b, wave * 0.55f);
                go.transform.localScale = Vector3.one * (1f + wave * 0.12f);
                yield return null;
            }
            if (go != null) Destroy(go);
        }
    }

    /// <summary>Wires the P1 hint set to live game signals. Self-installs.</summary>
    internal static class HintTriggers
    {
        private static bool _installed;
        private static float _bootTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (_installed) return;
            _installed = true;
            _bootTime = Time.realtimeSinceStartup;

            MergeBoardController.GeneratorTapped += OnGeneratorTapped;
            BoardTileView.TickShown += OnTickShown;
            OverflowBucketService.BucketChanged += OnBucketChanged;
            MergeBoardController.BoardCompositionChanged += OnBoardChanged;
            SpecialItemsService.Changed += OnSpecialsChanged;
            AQTheme.HelpBarBuilt += OnHelpBarBuilt;

            // P2/P3 set
            MergeBoardController.TilesMerged += OnTilesMerged;
            MergeBoardController.TilesSwapped += OnTilesSwapped;
            DialogueRunner.DialogueClosed += OnDialogueClosed;
            SubscribeWallet();
        }

        // The wallet may not exist until the save restores; hook the restore
        // event as the fallback rather than polling.
        private static void SubscribeWallet()
        {
            var wallet = AQ.App.Economy.WalletLocator.Instance;
            if (wallet != null) { wallet.Granted += OnRewardsGranted; return; }
            BoardSaveSystem.WalletRestoreCompleted += OnWalletReady;
        }

        private static void OnWalletReady()
        {
            BoardSaveSystem.WalletRestoreCompleted -= OnWalletReady;
            var wallet = AQ.App.Economy.WalletLocator.Instance;
            if (wallet != null) wallet.Granted += OnRewardsGranted;
        }

        // First tap that puts energy visibly below 90 (Stephen-ruled 2026-08-14):
        // teach when the number is moving, which is when a player wonders.
        private static void OnGeneratorTapped()
        {
            var wallet = AQ.App.Economy.WalletLocator.Instance;
            if (wallet == null) return;
            if (wallet.Get(AQ.SharedKernel.Economy.Currency.Energy) >= 90) return;
            HintService.Request("energy",
                "Working the board costs energy. It refills on its own, slowly.");
        }

        private static void OnTickShown(BoardTileView tile)
            => HintService.Request("tick",
                "A green tick means a lead needs that item.",
                () => tile != null ? tile.transform : null);

        private static void OnBucketChanged()
        {
            // Boot restore also raises this; only a mid-session arrival teaches.
            if (Time.realtimeSinceStartup - _bootTime < 5f) return;
            if (OverflowBucketService.Count == 0) return;
            HintService.Request("stash",
                "Extra finds wait in the Stash. Tap it when the board has room.",
                () => FindAny("__OverflowBtn", "__StashBtn"));
        }

        private static void OnBoardChanged()
        {
            var board = UnityEngine.Object.FindAnyObjectByType<MergeBoardController>();
            if (board == null) return;
            int total = 0, filled = 0;
            for (int r = 0; r < board.Rows; r++)
                for (int c = 0; c < board.Cols; c++)
                {
                    total++;
                    if (board.Get(r, c) != null) filled++;
                }
            if (total == 0 || filled < total * 0.8f) return;
            HintService.Request("locker",
                "Desk filling up? The locker stores evidence off the board.",
                () => FindAny("__LockerBtn"));
        }

        private static void OnSpecialsChanged()
        {
            bool any = false;
            foreach (SpecialId id in Enum.GetValues(typeof(SpecialId)))
                if (SpecialItemsService.CountOf(id) > 0) { any = true; break; }
            if (!any) return;
            HintService.Request("casekit",
                "New tool in the Case Kit. Place it on the board, then drag it onto its target.",
                () => FindAny("__CaseKitBtn", "__KitBtn"));
        }

        private static void OnHelpBarBuilt(RectTransform helpBtn)
            => HintService.Request("help",
                "Any screen with a ? will explain itself.",
                () => helpBtn);

        // ---- P2/P3 handlers ----

        // Long-press discovery once merging is habitual; counter persists
        // across sessions so a slow starter still gets taught.
        private static void OnTilesMerged(string fam, int tier)
        {
            if (HintService.Seen("longpress")) return;
            int n = PlayerPrefs.GetInt("aq.hint.merge_ct", 0) + 1;
            PlayerPrefs.SetInt("aq.hint.merge_ct", n);
            if (n < 10) return;
            HintService.Request("longpress",
                "Hold any item to examine it and see its whole family.");
        }

        private static void OnTilesSwapped()
            => HintService.Request("swap",
                "Different evidence families trade places instead of merging.");

        // Dossier hint waits for the SECOND case beat (first close after L1 is
        // done) so it does not stack on the L1 payoff moment.
        private static void OnDialogueClosed()
        {
            if (!NarrativeFlags.Has("aq.lead.e1_tip.seen")) return;
            HintService.Request("dossier",
                "Tap Ally to open her case files.",
                () => FindAny("Img_Player"));
        }

        // One lesson per payoff: CaseCash at the first lead close, the
        // evidence board at the third (L1 close would otherwise stack three
        // chips on the game's best moment).
        private static void OnRewardsGranted(AQ.SharedKernel.Economy.RewardsGranted e)
        {
            if (e == null || e.Reason != "lead.outcome") return;

            int n = PlayerPrefs.GetInt("aq.hint.lead_ct", 0) + 1;
            PlayerPrefs.SetInt("aq.hint.lead_ct", n);

            foreach (var r in e.Rewards)
                if (r.Currency == AQ.SharedKernel.Economy.Currency.Soft)
                {
                    HintService.Request("casecash",
                        "CaseCash earned. It pays for locker slots, case file pages, and shop stock later on.");
                    break;
                }

            if (n >= 3)
                HintService.Request("evidence",
                    "Everything you learn is pinned to the evidence board.",
                    () => FindAny("__EvidBoardBtn"));
        }

        private static Transform FindAny(params string[] names)
        {
            foreach (var n in names)
            {
                var go = GameObject.Find(n);
                if (go != null) return go.transform;
            }
            return null;
        }
    }
}
