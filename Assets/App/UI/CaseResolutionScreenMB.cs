using System;
using AQ.App.Events;
using AQ.App.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AQ.App.UI
{
    /// Listens for CaseResolvedEvent and builds a full-screen resolution overlay.
    public sealed class CaseResolutionScreenMB : MonoBehaviour
    {
        IDisposable _sub;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoInstall()
        {
            if (FindObjectOfType<CaseResolutionScreenMB>() != null) return;
            var go = new GameObject("[CaseResolutionScreen]");
            DontDestroyOnLoad(go);
            go.AddComponent<CaseResolutionScreenMB>();
        }

        void Awake()
        {
            _sub = GlobalBus.Bus.Subscribe<CaseResolvedEvent>(OnCaseResolved);
        }

        void OnDestroy()
        {
            _sub?.Dispose();
            _sub = null;
        }

        void OnCaseResolved(CaseResolvedEvent evt)
        {
            _sub?.Dispose();
            _sub = null;
            BuildOverlay();
        }

        void BuildOverlay()
        {
            // Root canvas — draws above everything
            var root = new GameObject("CaseResolutionOverlay");
            DontDestroyOnLoad(root);

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            root.AddComponent<GraphicRaycaster>();
            root.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            // Dark background panel
            var bg = MakeRect("BG", root.transform);
            bg.anchorMin = Vector2.zero;
            bg.anchorMax = Vector2.one;
            bg.sizeDelta = Vector2.zero;
            var bgImg = bg.gameObject.AddComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.88f);

            // Content container — centred column
            var content = MakeRect("Content", root.transform);
            content.anchorMin        = new Vector2(0.5f, 0.5f);
            content.anchorMax        = new Vector2(0.5f, 0.5f);
            content.pivot            = new Vector2(0.5f, 0.5f);
            content.sizeDelta        = new Vector2(640f, 560f);
            content.anchoredPosition = Vector2.zero;

            var vg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vg.spacing            = 20f;
            vg.childAlignment     = TextAnchor.MiddleCenter;
            vg.childControlWidth  = true;
            vg.childControlHeight = false;
            vg.padding            = new RectOffset(32, 32, 32, 32);

            // "EPISODE CLOSED" heading
            AddTMP(content, "Txt_Heading",
                "EPISODE CLOSED",
                48f, Color.white, FontStyles.Bold,
                TextAlignmentOptions.Center, 72f);

            // Case title — TODO: source from episode data when a second episode
            // ships; hardcoded copy went stale once already (Ghost Student text
            // survived into The Listener's finale until 2026-07-18).
            AddTMP(content, "Txt_CaseTitle",
                "The Listener",
                28f, new Color(0.7f, 0.85f, 1f, 1f), FontStyles.Italic,
                TextAlignmentOptions.Center, 40f);

            // Divider
            var div = MakeRect("Divider", content);
            div.sizeDelta = new Vector2(0f, 2f);
            var divImg = div.gameObject.AddComponent<Image>();
            divImg.color = new Color(1f, 1f, 1f, 0.25f);

            // Summary body
            AddTMP(content, "Txt_Summary",
                "You found Dot — safe, and listening still.\nThe man who came at noon has lost his cover. Harbourline’s trail leads deeper — and so does your father’s unfinished case.",
                20f, new Color(0.88f, 0.88f, 0.88f, 1f), FontStyles.Normal,
                TextAlignmentOptions.Center, 80f);

            // Button row
            var btnRow = MakeRect("BtnRow", content);
            btnRow.sizeDelta = new Vector2(0f, 64f);
            var hlg = btnRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing           = 24f;
            hlg.childAlignment    = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childForceExpandWidth = false;

            AddButton(btnRow, "Btn_Replay",    "Play Again",    new Color(0.3f, 0.3f, 0.4f, 1f), OnReplay);
            AddButton(btnRow, "Btn_Continue",  "Keep Playing",  new Color(0.2f, 0.5f, 0.85f, 1f), () => Destroy(root));
        }

        // ----- Helpers -----

        static RectTransform MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<RectTransform>();
        }

        static void AddTMP(Transform parent, string name, string text, float size,
            Color color, FontStyles style, TextAlignmentOptions align, float height)
        {
            var rt = MakeRect(name, parent);
            rt.sizeDelta = new Vector2(0f, height);
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = size;
            tmp.color     = color;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.enableWordWrapping = true;
        }

        static void AddButton(Transform parent, string name, string label, Color bgColor, Action onClick)
        {
            var rt = MakeRect(name, parent);
            rt.sizeDelta = new Vector2(220f, 56f);

            var img = rt.gameObject.AddComponent<Image>();
            img.color = bgColor;

            var btn = rt.gameObject.AddComponent<Button>();
            var cs  = btn.colors;
            cs.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
            btn.colors = cs;
            btn.onClick.AddListener(() => onClick());

            var labelRt = MakeRect("Lbl", rt);
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.sizeDelta = Vector2.zero;
            var tmp = labelRt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text      = label;
            tmp.fontSize  = 18f;
            tmp.color     = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
        }

        static void OnReplay()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
