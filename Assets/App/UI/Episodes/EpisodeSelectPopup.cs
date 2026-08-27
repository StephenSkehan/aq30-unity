using AQ.App.Episodes;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Namespace is deliberately AQ.App.UI, not AQ.App.UI.Episodes: a nested
// "Episodes" namespace would shadow AQ.App.Episodes for every `Episodes.`
// reference inside AQ.App.UI code.
namespace AQ.App.UI
{
    /// <summary>
    /// Code-built episode selector (multi-episode M6): the season's slots with
    /// Complete / In progress / Locked states, linear unlock, no replay of
    /// completed episodes (ruling R6). Registers itself as the EpisodeFlow
    /// selector opener; current entry points are the resolution screen and the
    /// dev Debug tab — a HUD entry point needs a scene pass (Stephen's).
    /// ⚠ Player-facing labels below are DRAFT copy, not yet Stephen-ruled.
    /// </summary>
    public static class EpisodeSelectPopup
    {
        private static GameObject _root;

        private static bool _sceneHookInstalled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            EpisodeFlow.SelectorOpener = Show;
            // Static event survives domain-reload-off replays — subscribe once.
            if (!_sceneHookInstalled)
            {
                _sceneHookInstalled = true;
                SceneManager.sceneUnloaded += _ => Close(); // never survive a scene change
            }
        }

        public static void Show()
        {
            if (_root != null) return;
            var catalog = EpisodeRuntime.Catalog;
            if (catalog == null || catalog.Episodes.Count == 0) return;

            _root = new GameObject("EpisodeSelectOverlay");
            var canvas = _root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Above the resolution overlay (6000) so "Episodes" can open from it.
            canvas.sortingOrder = 6100;
            _root.AddComponent<GraphicRaycaster>();
            _root.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            var bg = MakeRect("BG", _root.transform);
            bg.anchorMin = Vector2.zero;
            bg.anchorMax = Vector2.one;
            bg.sizeDelta = Vector2.zero;
            var bgImg = bg.gameObject.AddComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.88f);
            var bgBtn = bg.gameObject.AddComponent<Button>();
            bgBtn.transition = Selectable.Transition.None;
            bgBtn.onClick.AddListener(Close);

            var content = MakeRect("Content", _root.transform);
            content.anchorMin = new Vector2(0.5f, 0.5f);
            content.anchorMax = new Vector2(0.5f, 0.5f);
            content.pivot     = new Vector2(0.5f, 0.5f);
            content.sizeDelta = new Vector2(620f, 120f + catalog.Episodes.Count * 84f);
            var panelImg = content.gameObject.AddComponent<Image>();
            panelImg.color = new Color(0.09f, 0.11f, 0.15f, 0.98f);

            var vg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vg.spacing            = 12f;
            vg.childAlignment     = TextAnchor.UpperCenter;
            vg.childControlWidth  = true;
            vg.childControlHeight = false;
            vg.padding            = new RectOffset(24, 24, 24, 24);

            AddTMP(content, "Txt_Heading", "EPISODES", 34f, Color.white, FontStyles.Bold, 44f);

            string currentId = EpisodeRuntime.Current?.episodeId;
            foreach (var entry in catalog.Episodes)
            {
                if (entry == null) continue;
                BuildRow(content, catalog, entry, currentId);
            }

            AddRowButton(content, "Btn_Close", "CLOSE", new Color(0.3f, 0.3f, 0.4f, 1f), true, Close);
        }

        public static void Close()
        {
            if (_root != null) Object.Destroy(_root);
            _root = null;
        }

        private static void BuildRow(Transform parent, EpisodeCatalog catalog, EpisodeEntry entry, string currentId)
        {
            var progress  = EpisodeFlow.ProgressOf(entry.episodeId);
            bool unlocked = EpisodeFlow.IsUnlocked(catalog, entry.episodeId, EpisodeFlow.ProgressOf);
            bool current  = entry.episodeId == currentId;

            var row = MakeRect("Row_" + entry.episodeId, parent);
            row.sizeDelta = new Vector2(0f, 72f);
            var rowImg = row.gameObject.AddComponent<Image>();
            rowImg.color = new Color(1f, 1f, 1f, 0.06f);

            var title = MakeRect("Txt_Title", row);
            title.anchorMin = new Vector2(0f, 0f);
            title.anchorMax = new Vector2(0.62f, 1f);
            title.offsetMin = new Vector2(16f, 0f);
            title.offsetMax = Vector2.zero;
            var titleTmp = title.gameObject.AddComponent<TextMeshProUGUI>();
            // A locked or reserved slot never leaks its story title.
            titleTmp.text      = unlocked || progress.Started ? entry.title : "???";
            titleTmp.fontSize  = 22f;
            titleTmp.color     = unlocked || current ? Color.white : new Color(1f, 1f, 1f, 0.4f);
            titleTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // ⚠ DRAFT copy, pending Stephen's ruling.
            string status;
            Color statusColor;
            if (!entry.HasContent)              { status = "Coming soon";  statusColor = new Color(1f, 1f, 1f, 0.35f); }
            else if (progress.Complete)         { status = "Complete";     statusColor = new Color(0.5f, 0.85f, 0.55f, 1f); }
            else if (current)                   { status = "In progress";  statusColor = new Color(0.7f, 0.85f, 1f, 1f); }
            else if (progress.Started)          { status = "In progress";  statusColor = new Color(0.7f, 0.85f, 1f, 1f); }
            else if (unlocked)                  { status = "Ready";        statusColor = new Color(0.9f, 0.9f, 0.9f, 1f); }
            else                                { status = "Locked";       statusColor = new Color(1f, 1f, 1f, 0.35f); }

            var stat = MakeRect("Txt_Status", row);
            stat.anchorMin = new Vector2(0.62f, 0f);
            stat.anchorMax = new Vector2(0.8f, 1f);
            stat.offsetMin = stat.offsetMax = Vector2.zero;
            var statTmp = stat.gameObject.AddComponent<TextMeshProUGUI>();
            statTmp.text      = status;
            statTmp.fontSize  = 18f;
            statTmp.color     = statusColor;
            statTmp.alignment = TextAlignmentOptions.Midline;

            // Action: the current episode closes the popup; an unlocked, not-yet-
            // complete other episode switches (persist-then-reload); everything
            // else has no button. Completed episodes stay closed — R6.
            if (current && !progress.Complete)
                AddSideButton(row, "Continue", new Color(0.2f, 0.5f, 0.85f, 1f), Close);
            else if (unlocked && !progress.Complete && !current)
                AddSideButton(row, progress.Started ? "Resume" : "Start",
                    new Color(0.2f, 0.65f, 0.4f, 1f), () => Switch(entry.episodeId));
        }

        private static void Switch(string episodeId)
        {
            if (EpisodeFlow.TrySwitch(episodeId))
            {
                Close();
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else
            {
                Debug.LogError("[EpisodeSelect] switch failed to persist — staying in the current episode");
            }
        }

        // ---- UI helpers (CaseResolutionScreenMB pattern) ----

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

        private static void AddSideButton(RectTransform row, string label, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var rt = MakeRect("Btn_" + label, row);
            rt.anchorMin = new Vector2(0.81f, 0.15f);
            rt.anchorMax = new Vector2(0.98f, 0.85f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var img = rt.gameObject.AddComponent<Image>();
            var btn = rt.gameObject.AddComponent<Button>();
            AQTheme.StyleButton(img, color);
            btn.onClick.AddListener(onClick);

            var lbl = MakeRect("Lbl", rt);
            lbl.anchorMin = Vector2.zero;
            lbl.anchorMax = Vector2.one;
            lbl.sizeDelta = Vector2.zero;
            var tmp = lbl.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text      = label;
            tmp.fontSize  = 16f;
            tmp.color     = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
        }

        private static void AddRowButton(Transform parent, string name, string label, Color color, bool interactable, UnityEngine.Events.UnityAction onClick)
        {
            var rt = MakeRect(name, parent);
            rt.sizeDelta = new Vector2(0f, 52f);
            var img = rt.gameObject.AddComponent<Image>();
            var btn = rt.gameObject.AddComponent<Button>();
            AQTheme.StyleButton(img, color);
            btn.interactable = interactable;
            btn.onClick.AddListener(onClick);

            var lbl = MakeRect("Lbl", rt);
            lbl.anchorMin = Vector2.zero;
            lbl.anchorMax = Vector2.one;
            lbl.sizeDelta = Vector2.zero;
            var tmp = lbl.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text      = label;
            tmp.fontSize  = 18f;
            tmp.color     = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
        }
    }
}
