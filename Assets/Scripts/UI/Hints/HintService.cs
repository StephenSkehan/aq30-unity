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

        private static readonly List<(string id, string text, Func<Transform> anchor)> _queue = new();
        private static readonly HashSet<string> _pending = new(); // queued or showing
        private static HintRunnerMB _runner;

        // Backlog drains in this order after the L1 gate lifts (Stephen-ruled
        // 2026-08-14): most-recent lesson first, then core loop, then chrome.
        private static readonly string[] Priority =
            { "casecash", "tick", "help", "energy", "stash", "locker",
              "boardzoom", "dossier", "evidence", "casekit", "swap", "longpress" };

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
            _queue.Add((id, text, anchor));
            EnsureRunner();
        }

        /// <summary>The player performed the taught action: retire the hint
        /// whether it is showing OR still queued — no lecturing about things
        /// already done (Stephen-ruled 2026-08-14).</summary>
        public static void ActionPerformed(string id)
        {
            if (_pending.Contains(id))
            {
                _queue.RemoveAll(h => h.id == id);
                MarkClosed(id);
            }
            if (_runner != null) _runner.CloseIfShowing(id);
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
            hint = default;
            if (_queue.Count == 0) return false;
            int best = 0, bestRank = int.MaxValue;
            for (int i = 0; i < _queue.Count; i++)
            {
                int rank = System.Array.IndexOf(Priority, _queue[i].id);
                if (rank < 0) rank = Priority.Length; // unknown ids after known, FIFO among themselves
                if (rank < bestRank) { bestRank = rank; best = i; }
            }
            hint = _queue[best];
            _queue.RemoveAt(best);
            return true;
        }
    }

    internal sealed class HintRunnerMB : MonoBehaviour
    {
        private GameObject _chip;
        private string _chipId;
        private RectTransform _closeRt;
        private Canvas _chipCanvas;
        private bool _dialogueOpen;

        private AQ.App.UI.TapRouter.Region _closeTapRegion;

        private void OnEnable()
        {
            DialogueRunner.DialogueOpened += OnDialogueOpened;
            DialogueRunner.DialogueClosed += OnDialogueClosed;

            // Close-X tap via TapRouter (2026-08-18): the chip canvas sorts at
            // 4000, so the router blocks the tap when a genuinely higher surface
            // covers the X, and claims it so nothing underneath fires too.
            _closeTapRegion = AQ.App.UI.TapRouter.Register("hint-close-x", 4000,
                contains: p => _closeRt != null &&
                               RectTransformUtility.RectangleContainsScreenPoint(_closeRt, p, null),
                onTap:    _ =>
                {
                    HintService.MarkClosed(_chipId);
                    Destroy(_chip);
                    _chip = null;
                },
                enabled:  () => this != null && _chip != null && !Suppressed());
        }

        private void OnDisable()
        {
            DialogueRunner.DialogueOpened -= OnDialogueOpened;
            DialogueRunner.DialogueClosed -= OnDialogueClosed;
            AQ.App.UI.TapRouter.Unregister(_closeTapRegion);
            _closeTapRegion = null;
        }

        private void OnDialogueOpened(CaseGraph _) => _dialogueOpen = true;
        private void OnDialogueClosed() => _dialogueOpen = false;

        internal void CloseIfShowing(string id)
        {
            if (_chip == null || _chipId != id) return;
            HintService.MarkClosed(_chipId);
            Destroy(_chip);
            _chip = null;
        }

        private void Update()
        {
            // Dossier opening has no event to hook; a per-frame static check is cheap.
            if (AQ.App.UI.Dossiers.DossierPopup.IsOpen) HintService.ActionPerformed("dossier");

            if (_chip != null)
            {
                // Persistent chip hides (not dies) while dialogue holds the stage.
                bool hidden = Suppressed();
                if (_chipCanvas != null && _chipCanvas.enabled == hidden)
                    _chipCanvas.enabled = !hidden;
                if (hidden) return;

                // X-close only (Stephen-ruled 2026-08-14) — tap handled by the
                // TapRouter region registered in OnEnable.
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
            if (!DialogueFlags.Has("aq.lead.e1_tip.seen")) return true;
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
            if (anchorFn != null && _chip != null)
                StartCoroutine(PulseWhenCanvasReady(anchorFn));
        }

        // The chip canvas has no valid rect or scaleFactor on its creation
        // frame, and the 1080x1920 reference is NOT the canvas's real unit
        // size under matchWidthOrHeight 0.5 — both made the hand-rolled
        // conversion miss its target (device finding 2026-08-14). Wait one
        // frame, then let Unity do the conversion.
        private System.Collections.IEnumerator PulseWhenCanvasReady(Func<Transform> anchorFn)
        {
            yield return null;
            if (_chip == null) yield break;
            var anchor = anchorFn.Invoke();
            if (anchor == null) yield break;

            var chipRoot = (RectTransform)_chip.transform;
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, anchor.position);
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(chipRoot, screen, null, out var local))
                yield break;

            // Ring sized to the target's on-screen footprint, with margin.
            var size = new Vector2(150f, 150f);
            if (anchor is RectTransform art)
            {
                var corners = new Vector3[4];
                art.GetWorldCorners(corners);
                float sf = _chipCanvas != null && _chipCanvas.scaleFactor > 0f ? _chipCanvas.scaleFactor : 1f;
                size = new Vector2(
                    Mathf.Max(90f, (corners[2].x - corners[0].x) / sf + 24f),
                    Mathf.Max(90f, (corners[2].y - corners[0].y) / sf + 24f));
            }

            var go = new GameObject("Pulse", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(chipRoot, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = local;
            var img = go.GetComponent<Image>();
            img.sprite = AQTheme.Rounded;
            img.type = Image.Type.Sliced;
            img.raycastTarget = false;
            yield return PulseRoutine(go, img);
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

            // Auto-clear signals: doing the thing retires its hint.
            BoardTileView.LongHeld += _ => HintService.ActionPerformed("longpress");
            AQ.App.Locker.EvidenceLockerService.LockerChanged += () => HintService.ActionPerformed("locker");
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
                "All this legwork costs energy. Recovery takes time.");
        }

        // Re-armed (Stephen-ruled 2026-08-14): L1's seeded-pair ticks are
        // ignored; the first fresh tick AFTER the gate teaches, with the pulse
        // pointing at a live example.
        private static void OnTickShown(BoardTileView tile)
        {
            if (!DialogueFlags.Has("aq.lead.e1_tip.seen")) return;
            HintService.Request("tick",
                "Green tick. That is evidence somebody is waiting on.",
                () => tile != null ? tile.transform : null);
        }

        private static int _lastBucketCount = -1;

        private static void OnBucketChanged()
        {
            int count = OverflowBucketService.Count;
            bool shrank = _lastBucketCount >= 0 && count < _lastBucketCount;
            _lastBucketCount = count;

            if (shrank) { HintService.ActionPerformed("stash"); return; } // player used it
            // Boot restore also raises this; only a mid-session arrival teaches.
            if (Time.realtimeSinceStartup - _bootTime < 5f) return;
            if (count == 0) return;
            HintService.Request("stash",
                "Special things end up in the Stash. Tap this when there is room on the board.",
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
                "A cluttered desk hides things. The locker keeps evidence safe until it is needed.",
                () => FindAny("__LockerBtn"));
        }

        private static int _lastSpecialTotal = -1;

        private static void OnSpecialsChanged()
        {
            int total = 0;
            foreach (SpecialId id in Enum.GetValues(typeof(SpecialId)))
                total += SpecialItemsService.CountOf(id);

            bool shrank = _lastSpecialTotal >= 0 && total < _lastSpecialTotal;
            _lastSpecialTotal = total;

            if (shrank) { HintService.ActionPerformed("casekit"); return; } // a special was placed
            if (total == 0) return;
            HintService.Request("casekit",
                "A new tool of the trade. Open the Case Kit, place it, and drag it where it is needed.",
                () => FindAny("__CaseKitBtn", "__KitBtn"));
        }

        private static void OnHelpBarBuilt(RectTransform helpBtn)
            => HintService.Request("help",
                "Lost? The ? knows the way. Every screen has one.",
                () => helpBtn);

        // ---- P2/P3 handlers ----

        // Long-press discovery once merging is habitual; counter persists
        // across sessions so a slow starter still gets taught. Requires 5+
        // items on the grid (Stephen-ruled 2026-08-14: no use showing the tip
        // when there is nothing to press) — under 5, a later merge re-tries.
        private static void OnTilesMerged(string fam, int tier)
        {
            if (HintService.Seen("longpress")) return;
            int n = PlayerPrefs.GetInt("aq.hint.merge_ct", 0) + 1;
            PlayerPrefs.SetInt("aq.hint.merge_ct", n);
            if (n < 10 || CountBoardItems() < 5) return;
            HintService.Request("longpress",
                "Look closer. Hold any item and it will tell you its story.");
        }

        private static int CountBoardItems()
        {
            var board = UnityEngine.Object.FindAnyObjectByType<MergeBoardController>();
            if (board == null) return 0;
            int items = 0;
            for (int r = 0; r < board.Rows; r++)
                for (int c = 0; c < board.Cols; c++)
                {
                    var t = board.Get(r, c);
                    if (t != null && t.Kind == TileKind.Item) items++;
                }
            return items;
        }

        private static void OnTilesSwapped()
            => HintService.Request("swap",
                "Those two do not mix. Different evidence trades places instead of merging.");

        // Dossier hint waits for the SECOND case beat (first close after L1 is
        // done) so it does not stack on the L1 payoff moment.
        private static void OnDialogueClosed()
        {
            if (!DialogueFlags.Has("aq.lead.e1_tip.seen")) return;
            HintService.Request("dossier",
                "Ally keeps notes on everyone and everything. Tap her to find out what she knows.",
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
                        "Your help earns CaseCash. Look out for places to spend it around town.");
                    break;
                }

            if (n >= 3)
                HintService.Request("evidence",
                    "Everything we learn is pinned to the evidence board, ready to remind you.",
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
