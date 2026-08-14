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

        private static readonly Queue<(string id, string text, Func<Transform> anchor)> _queue = new();
        private static HintRunnerMB _runner;

        public static void Request(string id, string text, Func<Transform> anchor = null)
        {
            if (NarrativeFlags.Has(FlagPrefix + id)) return;
            // Flag at request time, not show time: a crash mid-queue loses the
            // hint rather than repeating it, matching the locker crash rule of
            // never double-presenting.
            NarrativeFlags.Set(FlagPrefix + id);
            _queue.Enqueue((id, text, anchor));
            EnsureRunner();
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
        private const float MinShowSeconds = 1.4f;
        private const float MaxShowSeconds = 7f;

        private GameObject _chip;
        private float _shownAt;
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
                float elapsed = Time.unscaledTime - _shownAt;
                bool tapped = elapsed > MinShowSeconds && Input.GetMouseButtonDown(0); // raw input per house GR lesson
                if (tapped || elapsed > MaxShowSeconds)
                {
                    Destroy(_chip);
                    _chip = null;
                }
                return;
            }

            if (Suppressed()) return;
            if (HintService.TryDequeue(out var hint))
                Show(hint.text, hint.anchor);
        }

        private bool Suppressed()
        {
            if (_dialogueOpen) return true;
            // FTUE first-merge choreography owns the board until its flag clears.
            return PlayerPrefs.GetInt("aq.ftue.first_merge.stage", 0) == 1;
        }

        private void Show(string text, Func<Transform> anchorFn)
        {
            _chip = new GameObject("__HintChip", typeof(Canvas), typeof(CanvasScaler));
            var canvas = _chip.GetComponent<Canvas>();
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
            prt.anchoredPosition = AnchoredPosition(anchorFn);
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
            trt.offsetMax = new Vector2(-24f, -14f);
            var tmp = txt.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 34f;
            tmp.color = new Color(0.94f, 0.92f, 0.86f, 1f);
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            AQTheme.StyleText(tmp);

            AQTheme.PopIn(prt);
            _shownAt = Time.unscaledTime;
        }

        // Above the anchor when one resolves and fits; upper third otherwise —
        // low placements collide with the corner buttons and the board bottom.
        private static Vector2 AnchoredPosition(Func<Transform> anchorFn)
        {
            var fallback = new Vector2(0f, 430f);
            var anchor = anchorFn?.Invoke();
            if (anchor == null) return fallback;

            var cam = (Camera)null; // overlay canvases position in screen space
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, anchor.position);
            float nx = (screen.x / Screen.width - 0.5f) * 1080f;
            float ny = (screen.y / Screen.height - 0.5f) * 1920f + 150f; // sit above the target
            nx = Mathf.Clamp(nx, -110f, 110f); // chip is near-full-width; keep it on screen
            ny = Mathf.Clamp(ny, -700f, 800f);
            return new Vector2(nx, ny);
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
        }

        // Second-ever generator tap: the first belongs to GeneratorTapHintMB's
        // arrow, and stacking both reads as clutter.
        private static void OnGeneratorTapped()
        {
            if (!NarrativeFlags.Has("aq.ftue.tap_generator.seen")) return;
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
