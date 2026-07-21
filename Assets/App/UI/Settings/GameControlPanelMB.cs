using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AQ.App.Audio;

namespace AQ.App.UI.Settings
{
    /// <summary>
    /// Programmatic multi-tab settings modal.
    /// Call GameControlPanelMB.Show() from any button.
    /// Add new tabs by calling RegisterTab() before the first Show().
    /// </summary>
    public class GameControlPanelMB : MonoBehaviour
    {
        // ── static access ────────────────────────────────────────────────────
        private static GameControlPanelMB _instance;

        public static void Show()
        {
            if (_instance == null) Bootstrap();
            _instance.gameObject.SetActive(true);
            _instance.Open();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("GameControlPanel");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<GameControlPanelMB>();
            go.SetActive(false);
        }

        // ── tab registry ─────────────────────────────────────────────────────
        private class TabDef
        {
            public string Label;
            public Action<RectTransform> BuildContent;
        }

        private readonly List<TabDef> _tabs = new List<TabDef>();
        private int _activeTab = 0;

        // External assemblies (e.g. Assembly-CSharp services) can contribute tabs.
        // Must be called before the panel's first Show(), i.e. from a
        // RuntimeInitializeOnLoadMethod(BeforeSceneLoad). Lower order sorts
        // earlier in the tab bar (registration order breaks ties) — needed
        // because RuntimeInitialize ordering across classes is unspecified.
        private static readonly List<(string label, Action<RectTransform> build, int order)> _externalTabs = new();
        public static void RegisterExternalTab(string label, Action<RectTransform> build, int order = 0)
            => _externalTabs.Add((label, build, order));

        // ── ui references ─────────────────────────────────────────────────────
        private Canvas       _canvas;
        private RectTransform _panelRt;
        private RectTransform _contentRoot;
        private readonly List<Button>        _tabButtons  = new List<Button>();
        private readonly List<RectTransform> _tabContents = new List<RectTransform>();
        private bool _built = false;

        // ── colours (AQTheme tokens) ─────────────────────────────────────────
        private static readonly Color kTabActive = AQTheme.Teal;
        private static readonly Color kTabIdle   = AQTheme.SteelDim;

        // ─────────────────────────────────────────────────────────────────────
        void Awake()
        {
            RegisterTab("Audio", BuildAudioTab);
            // The Debug tab (dev builds only) registers externally from
            // Assembly-CSharp (DebugSettingsTab) — its RESET needs BoardSaveSystem.
            foreach (var (label, build, _) in _externalTabs.OrderBy(t => t.order))
                RegisterTab(label, build);
        }

        void OnEnable() => Open();

        private void Open()
        {
            if (!_built) BuildUI();
            RefreshTabHighlight();
            SyncSlidersToService();
            AQTheme.PopIn(_panelRt);
        }

        // ── build full UI ─────────────────────────────────────────────────────
        private void BuildUI()
        {
            _built = true;

            // Canvas
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 500;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight  = 0.5f; // match the other popups; keeps the 800px panel on-screen on narrow devices

            gameObject.AddComponent<GraphicRaycaster>();

            // Full-screen backdrop
            var backdrop = MakeImage("Backdrop", transform, AQTheme.Scrim);
            Stretch(backdrop.rectTransform);
            var trigger = backdrop.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            // Backdrop click does NOT close — user must use X button to avoid accidental dismissal

            // Panel container  (800×900 centred)
            var panelGo = new GameObject("Panel");
            panelGo.transform.SetParent(transform, false);
            _panelRt = panelGo.AddComponent<RectTransform>();
            Centre(_panelRt, 800, 900);
            AQTheme.StylePanel(_panelRt);

            // Title bar
            var title = MakeText("Title", _panelRt, "SETTINGS", 46, AQTheme.Paper, display: true);
            var trt = title.rectTransform;
            trt.anchorMin = new Vector2(0.06f, 0.88f);
            trt.anchorMax = new Vector2(0.85f, 0.97f);
            trt.sizeDelta  = Vector2.zero;

            // Close button
            var closeBtn = MakeButton("CloseBtn", _panelRt, "✕", 38, AQTheme.Paper, AQTheme.AlertRed);
            var crt = closeBtn.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.86f, 0.895f);
            crt.anchorMax = new Vector2(0.965f, 0.955f);
            crt.sizeDelta  = Vector2.zero;
            closeBtn.onClick.AddListener(Close);

            // Tab bar
            var tabBar = new GameObject("TabBar");
            tabBar.transform.SetParent(_panelRt, false);
            var tbRt = tabBar.AddComponent<RectTransform>();
            tbRt.anchorMin = new Vector2(0, 0.78f);
            tbRt.anchorMax = new Vector2(1, 0.88f);
            tbRt.sizeDelta  = Vector2.zero;
            var hgroup = tabBar.AddComponent<HorizontalLayoutGroup>();
            hgroup.spacing = 4;
            hgroup.padding = new RectOffset(8, 8, 4, 4);
            hgroup.childForceExpandWidth  = true;
            hgroup.childForceExpandHeight = true;

            // Content root
            var contentGo = new GameObject("ContentRoot");
            contentGo.transform.SetParent(_panelRt, false);
            _contentRoot = contentGo.AddComponent<RectTransform>();
            _contentRoot.anchorMin = new Vector2(0, 0.02f);
            _contentRoot.anchorMax = new Vector2(1, 0.78f);
            _contentRoot.sizeDelta  = Vector2.zero;

            // Build each tab
            for (int i = 0; i < _tabs.Count; i++)
            {
                int idx = i;

                // Tab button
                var btn = MakeButton($"Tab_{_tabs[i].Label}", tbRt, _tabs[i].Label, 30, AQTheme.Paper, kTabIdle);
                btn.onClick.AddListener(() => SelectTab(idx));
                _tabButtons.Add(btn);

                // Tab content panel
                var content = new GameObject($"Content_{_tabs[i].Label}");
                content.transform.SetParent(_contentRoot, false);
                var ct = content.AddComponent<RectTransform>();
                Stretch(ct);
                _tabContents.Add(ct);

                _tabs[i].BuildContent(ct);
            }

            SelectTab(0);
        }

        private void RegisterTab(string label, Action<RectTransform> build) =>
            _tabs.Add(new TabDef { Label = label, BuildContent = build });

        private void SelectTab(int idx)
        {
            _activeTab = idx;
            for (int i = 0; i < _tabContents.Count; i++)
                _tabContents[i].gameObject.SetActive(i == idx);
            RefreshTabHighlight();
        }

        private void RefreshTabHighlight()
        {
            for (int i = 0; i < _tabButtons.Count; i++)
            {
                var img = _tabButtons[i].GetComponent<Image>();
                if (img) img.color = (i == _activeTab) ? kTabActive : kTabIdle;
            }
        }

        private void Close()
        {
            gameObject.SetActive(false);
        }

        // ── Audio tab ─────────────────────────────────────────────────────────
        private Slider _musicSlider;
        private Slider _dialogueSlider;
        private Slider _sfxSlider;

        private void BuildAudioTab(RectTransform parent)
        {
            float rowH = 0.22f;
            float gap  = 0.06f;
            float top  = 0.82f;

            _musicSlider    = AddSliderRow(parent, "Music Volume",    AudioSettingsService.MusicVolume,    top - 0 * (rowH + gap), rowH);
            _dialogueSlider = AddSliderRow(parent, "Dialogue Volume", AudioSettingsService.DialogueVolume, top - 1 * (rowH + gap), rowH);
            _sfxSlider      = AddSliderRow(parent, "SFX Volume",      AudioSettingsService.SFXVolume,      top - 2 * (rowH + gap), rowH);

            _musicSlider.onValueChanged.AddListener(v =>
            {
                AudioSettingsService.MusicVolume = v;
            });

            _dialogueSlider.onValueChanged.AddListener(v =>
            {
                AudioSettingsService.DialogueVolume = v;
                ApplyDialogueVolume(v);
            });

            _sfxSlider.onValueChanged.AddListener(v =>
            {
                AudioSettingsService.SFXVolume = v;
            });
        }

        private void SyncSlidersToService()
        {
            if (_musicSlider)    _musicSlider.SetValueWithoutNotify(AudioSettingsService.MusicVolume);
            if (_dialogueSlider) _dialogueSlider.SetValueWithoutNotify(AudioSettingsService.DialogueVolume);
            if (_sfxSlider)      _sfxSlider.SetValueWithoutNotify(AudioSettingsService.SFXVolume);
        }

        private static void ApplyDialogueVolume(float v)
        {
            var dr = FindAnyObjectByType<DialogueRunner>();
            if (dr != null && dr.voiceSource != null)
                dr.voiceSource.volume = v;
        }

        // ── UI helpers ────────────────────────────────────────────────────────
        private Slider AddSliderRow(RectTransform parent, string label, float initialValue, float anchorTop, float rowHeight)
        {
            var row = new GameObject(label.Replace(" ", "_"));
            row.transform.SetParent(parent, false);
            var rrt = row.AddComponent<RectTransform>();
            rrt.anchorMin = new Vector2(0.05f, anchorTop - rowHeight);
            rrt.anchorMax = new Vector2(0.95f, anchorTop);
            rrt.sizeDelta  = Vector2.zero;

            var lbl = MakeText("Label", rrt, label, 28, AQTheme.PaperDim);
            var lrt = lbl.rectTransform;
            lrt.anchorMin = new Vector2(0, 0.5f);
            lrt.anchorMax = new Vector2(0.45f, 1f);
            lrt.sizeDelta  = Vector2.zero;

            var slider = MakeSlider("Slider", rrt, initialValue);
            var srt = slider.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.47f, 0.1f);
            srt.anchorMax = new Vector2(1f, 0.9f);
            srt.sizeDelta  = Vector2.zero;

            return slider;
        }

        private static Image MakeImage(string name, Transform parent, Color color)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt  = go.AddComponent<RectTransform>();
            go.AddComponent<CanvasRenderer>();
            var img = go.AddComponent<Image>();
            img.color         = color;
            img.raycastTarget = true;
            return img;
        }

        private static TextMeshProUGUI MakeText(string name, Transform parent, string text, int size, Color color, bool display = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text      = text;
            t.fontSize  = size;
            t.color     = color;
            t.alignment = TextAlignmentOptions.MidlineLeft;
            t.raycastTarget = false;
            AQTheme.StyleText(t, display);
            return t;
        }

        private static Button MakeButton(string name, Transform parent, string label, int fontSize, Color textColor, Color bgColor)
        {
            var img = MakeImage(name, parent, bgColor);
            AQTheme.Round(img, bgColor);
            var btn = img.gameObject.AddComponent<Button>();
            var txt = MakeText("Label", img.transform, label, fontSize, textColor, display: true);
            txt.alignment = TextAlignmentOptions.Center;
            var trt = txt.rectTransform;
            Stretch(trt);
            return btn;
        }

        private static Slider MakeSlider(string name, Transform parent, float value)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();

            var bgImg = AQTheme.Round(MakeImage("Background", go.transform, AQTheme.Card), AQTheme.Card);
            Stretch(bgImg.rectTransform);
            bgImg.rectTransform.sizeDelta = Vector2.zero;

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            var faRt = fillArea.AddComponent<RectTransform>();
            faRt.anchorMin = new Vector2(0, 0.25f);
            faRt.anchorMax = new Vector2(1, 0.75f);
            faRt.sizeDelta  = new Vector2(-20, 0);
            faRt.anchoredPosition = new Vector2(-5, 0);

            var fill = AQTheme.Round(MakeImage("Fill", fillArea.transform, AQTheme.Teal), AQTheme.Teal);
            fill.rectTransform.sizeDelta = new Vector2(10, 0);

            var handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(go.transform, false);
            var haRt = handleArea.AddComponent<RectTransform>();
            Stretch(haRt);
            haRt.sizeDelta = new Vector2(-20, 0);

            var handle = AQTheme.Round(MakeImage("Handle", handleArea.transform, AQTheme.Paper), AQTheme.Paper);
            handle.rectTransform.sizeDelta = new Vector2(40, 0);

            var slider = go.AddComponent<Slider>();
            slider.fillRect   = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction  = Slider.Direction.LeftToRight;
            slider.minValue   = 0f;
            slider.maxValue   = 1f;
            slider.value      = value;

            return slider;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin      = Vector2.zero;
            rt.anchorMax      = Vector2.one;
            rt.sizeDelta      = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }

        private static void Centre(RectTransform rt, float w, float h)
        {
            rt.anchorMin      = new Vector2(0.5f, 0.5f);
            rt.anchorMax      = new Vector2(0.5f, 0.5f);
            rt.pivot          = new Vector2(0.5f, 0.5f);
            rt.sizeDelta      = new Vector2(w, h);
            rt.anchoredPosition = Vector2.zero;
        }
    }
}
