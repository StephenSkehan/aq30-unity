// Assembly: AQ.App
// File: Assets/App/UI/Packages/PackageBeatPresenterMB.cs
// Purpose: The beat presentation surface for lead packages (chapter 1 slice;
//          the "interstitial" option from feature-lead-packages-v1 ruling 1,
//          built first because it is the cheap option).
//
// Flow: PackageRuntimeMB raises BeatReady -> this presenter shows exactly one
// beat at a time. Dialogue beats route through the shipped DialogueRunner
// (BootWithGraph + DialogueEnded); art/caption/fact beats show a code-built
// full-screen interstitial (canvas conventions mirrored from
// EpisodeSelectPopup; taps consumed natively by the overlay's raycaster, so
// TapRouter's stacking rule is honoured without a bespoke poll).
// Dismissal calls runtime.NotifyBeatDismissed, which pays (idempotent) and
// sets beat_seen (rule 5). If more beats are pending, the next shows.

using System.Collections.Generic;
using AQ.App.Leads.Packages;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Packages
{
    [DefaultExecutionOrder(-4)] // after PackageRuntimeMB (-5)
    public sealed class PackageBeatPresenterMB : MonoBehaviour
    {
        [SerializeField] public PackageRuntimeMB runtime;
        [SerializeField] public AQ.App.DialogueRunner dialogueRunner;

        private readonly Queue<PackageData> _queue = new Queue<PackageData>();
        private PackageData _showing;
        private GameObject _overlayRoot;

        private void Awake()
        {
            if (runtime == null) runtime = FindFirstObjectByType<PackageRuntimeMB>();
        }

        private void OnEnable()  => PackageRuntimeMB.BeatReady += OnBeatReady;

        private void OnDisable()
        {
            PackageRuntimeMB.BeatReady -= OnBeatReady;
            CloseOverlay();
        }

        private void Start()
        {
            // Restore path: beats queued by the runtime's Start scan fire before
            // our OnEnable in fresh scenes, so drain anything already pending.
            if (runtime == null) return;
            foreach (var p in runtime.PendingBeats)
                if (!_queue.Contains(p)) _queue.Enqueue(p);
            TryShowNext();
        }

        private void OnBeatReady(PackageData p)
        {
            if (p == null) return;
            _queue.Enqueue(p);
            TryShowNext();
        }

        private void TryShowNext()
        {
            if (_showing != null || _queue.Count == 0) return;
            _showing = _queue.Dequeue();

            bool hasDialogue = _showing.beatDialogue != null && dialogueRunner != null;
            if (hasDialogue)
            {
                dialogueRunner.DialogueEnded += OnDialogueEnded;
                dialogueRunner.gameObject.SetActive(true);
                dialogueRunner.BootWithGraph(_showing.beatDialogue);
            }
            else
            {
                ShowInterstitial(_showing);
            }
        }

        private void OnDialogueEnded()
        {
            if (dialogueRunner != null) dialogueRunner.DialogueEnded -= OnDialogueEnded;
            Dismiss();
        }

        private void Dismiss()
        {
            var done = _showing;
            _showing = null;
            CloseOverlay();
            if (done != null && runtime != null) runtime.NotifyBeatDismissed(done);
            TryShowNext();
        }

        // ---- interstitial (art + caption / character fact / Ally line) ----

        private void ShowInterstitial(PackageData p)
        {
            CloseOverlay();

            _overlayRoot = new GameObject("PackageBeatOverlay");
            var canvas = _overlayRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Below the resolution overlay (6000) and the episode popup (6100);
            // above the board HUD.
            canvas.sortingOrder = 5800;
            _overlayRoot.AddComponent<GraphicRaycaster>();
            _overlayRoot.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            var bg = MakeRect("BG", _overlayRoot.transform);
            bg.anchorMin = Vector2.zero;
            bg.anchorMax = Vector2.one;
            bg.sizeDelta = Vector2.zero;
            var bgImg = bg.gameObject.AddComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.92f);

            var content = MakeRect("Content", _overlayRoot.transform);
            content.anchorMin = new Vector2(0.5f, 0.5f);
            content.anchorMax = new Vector2(0.5f, 0.5f);
            content.pivot     = new Vector2(0.5f, 0.5f);
            bool hasArt = p.beatArt != null;
            content.sizeDelta = new Vector2(640f, hasArt ? 760f : 380f);

            var vg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vg.spacing            = 16f;
            vg.childAlignment     = TextAnchor.MiddleCenter;
            vg.childControlWidth  = true;
            vg.childControlHeight = false;
            vg.padding            = new RectOffset(28, 28, 28, 28);

            if (hasArt)
            {
                var artRt = MakeRect("Img_BeatArt", content);
                artRt.sizeDelta = new Vector2(0f, 420f);
                var img = artRt.gameObject.AddComponent<Image>();
                img.sprite = p.beatArt;
                img.preserveAspect = true;
            }

            if (!string.IsNullOrEmpty(p.beatCaption))
                AddTMP(content, "Txt_Caption", p.beatCaption, 30f, new Color(0.93f, 0.9f, 0.84f, 1f), FontStyles.Italic, 160f);
            else
                AddTMP(content, "Txt_Caption", p.title, 30f, Color.white, FontStyles.Bold, 80f);

            AddContinueButton(content);
        }

        private void AddContinueButton(Transform parent)
        {
            var rt = MakeRect("Btn_Continue", parent);
            rt.sizeDelta = new Vector2(0f, 88f);
            var img = rt.gameObject.AddComponent<Image>();
            // AQTheme.StyleButton must run once, BEFORE the text child (its
            // layers stack over later siblings; standing trap).
            AQTheme.StyleButton(img, new Color(0.16f, 0.32f, 0.5f, 1f));
            var btn = rt.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(Dismiss);
            AddTMP(rt, "Txt_Label", "CONTINUE", 30f, Color.white, FontStyles.Bold, 88f);
            var label = rt.Find("Txt_Label") as RectTransform;
            if (label != null)
            {
                label.anchorMin = Vector2.zero;
                label.anchorMax = Vector2.one;
                label.sizeDelta = Vector2.zero;
            }
        }

        private void CloseOverlay()
        {
            if (_overlayRoot != null) Destroy(_overlayRoot);
            _overlayRoot = null;
        }

        private static RectTransform MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<RectTransform>();
        }

        private static void AddTMP(Transform parent, string name, string text, float size,
            Color color, FontStyles style, float height)
        {
            var rt = MakeRect(name, parent);
            rt.sizeDelta = new Vector2(0f, height);
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = size;
            tmp.color     = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
        }
    }
}
