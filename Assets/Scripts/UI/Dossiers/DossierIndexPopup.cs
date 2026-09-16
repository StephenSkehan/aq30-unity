using AQ.App.Leads;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Dossiers
{
    /// <summary>
    /// The case-file index — every dossier in the game (Stephen-ruled
    /// 2026-08-11: "the entire history of the game", so future episodes just
    /// grow DossierCatalog.Order). Rows show portrait, name, and progress;
    /// tapping one opens that character's file.
    /// </summary>
    public static class DossierIndexPopup
    {
        private static GameObject _root;

        public static bool IsOpen => _root != null;

        public static void Show()
        {
            if (_root != null) return;

            const float panelW = 820f;
            const float rowH   = 116f;
            int rows = DossierCatalog.Order.Length;
            float panelH = 96f + 20f + rows * (rowH + 10f) + 14f; // title bar + rows

            _root = new GameObject("__DossierIndex", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Object.DontDestroyOnLoad(_root);
            var canvas          = _root.GetComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000;
            var scaler                 = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight  = 0.5f;

            var dim       = MakeRect("Dim", _root.transform);
            dim.anchorMin = Vector2.zero;
            dim.anchorMax = Vector2.one;
            dim.offsetMin = dim.offsetMax = Vector2.zero;
            dim.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);

            var panel              = MakeRect("Panel", _root.transform);
            panel.anchorMin        = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot            = new Vector2(0.5f, 0.5f);
            panel.sizeDelta        = new Vector2(panelW, panelH);
            panel.anchoredPosition = Vector2.zero;
            // Rounded corners to match the title bar (Stephen-ruled 2026-08-21).
            AQTheme.Round(panel.gameObject.AddComponent<Image>(), new Color(0.12f, 0.10f, 0.08f, 1f));

            float cursor = panelH / 2f - 96f - 20f;

            foreach (var key in DossierCatalog.Order)
            {
                if (!DossierCatalog.All.TryGetValue(key, out var def)) continue;

                var row = new GameObject("Row_" + key, typeof(RectTransform), typeof(Image), typeof(Button));
                row.transform.SetParent(panel, false);
                var rt              = (RectTransform)row.transform;
                rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot            = new Vector2(0.5f, 0.5f);
                rt.sizeDelta        = new Vector2(panelW - 60f, rowH);
                rt.anchoredPosition = new Vector2(0f, cursor - rowH / 2f);
                row.GetComponent<Image>().color = new Color(0.17f, 0.145f, 0.10f, 1f);

                var sprite = DossierPortraits.Find(key);
                var pRt    = PlaceRect("Portrait", rt, new Vector2(92f, 92f), new Vector2(-panelW / 2f + 30f + 76f, 0f));
                var pImg   = pRt.gameObject.AddComponent<Image>();
                pImg.preserveAspect = true;
                pImg.raycastTarget  = false;
                if (sprite != null) pImg.sprite = sprite;
                else                pImg.color  = new Color(0.35f, 0.32f, 0.28f, 1f);

                var nRt = PlaceRect("Name", rt, new Vector2(360f, rowH), new Vector2(-30f, 0f));
                AddTmp(nRt, def.displayName, 30f, Color.white, FontStyles.Bold, TextAlignmentOptions.Left);

                int have    = DossierService.UnlockedCount(key);
                bool done   = DossierService.IsComplete(key);
                var progRt  = PlaceRect("Progress", rt, new Vector2(200f, rowH), new Vector2(panelW / 2f - 30f - 110f, 0f));
                AddTmp(progRt, done ? "COMPLETE" : have + " / " + def.facts.Count, 26f,
                       done ? new Color(0.85f, 0.68f, 0.32f, 1f) : new Color(0.66f, 0.62f, 0.56f, 1f),
                       FontStyles.Normal, TextAlignmentOptions.Right);

                var keyCap    = key;
                var spriteCap = sprite;
                row.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Close();
                    DossierPopup.Show(keyCap, spriteCap);
                });

                cursor -= rowH + 10f;
            }
            // Title-bar treatment (Stephen-ruled 2026-08-12); the bar's X closes.
            AQTheme.TitleBar(panel, "CASE FILES", Close,
                "Every dossier Ally keeps. Tap a name to open their file; progress shows how many entries you have unlocked.");
        }

        public static void Close()
        {
            if (_root == null) return;
            Object.Destroy(_root);
            _root = null;
        }

        // ---- helpers (same construction idiom as DossierPopup) ----

        private static TextMeshProUGUI AddTmp(RectTransform rt, string text, float size, Color color,
                                              FontStyles style, TextAlignmentOptions align)
        {
            var tmp           = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text          = text;
            tmp.fontSize      = size;
            tmp.color         = color;
            tmp.fontStyle     = style;
            tmp.alignment     = align;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static RectTransform PlaceRect(string name, RectTransform parent, Vector2 size, Vector2 centre)
        {
            var go              = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt              = go.GetComponent<RectTransform>();
            rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.sizeDelta        = size;
            rt.anchoredPosition = centre;
            return rt;
        }

        private static Button PlaceButton(string label, RectTransform parent, Color color, Vector2 size, Vector2 centre)
        {
            var go = new GameObject("Btn", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt              = go.GetComponent<RectTransform>();
            rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.sizeDelta        = size;
            rt.anchoredPosition = centre;
            AQTheme.StyleButton(go.GetComponent<Image>(), color);

            var lbl       = MakeRect("Lbl", rt);
            lbl.anchorMin = Vector2.zero;
            lbl.anchorMax = Vector2.one;
            lbl.offsetMin = lbl.offsetMax = Vector2.zero;
            AddTmp(lbl, label, 28f, Color.white, FontStyles.Normal, TextAlignmentOptions.Center);
            return go.GetComponent<Button>();
        }

        private static RectTransform MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }
    }

    /// <summary>
    /// Portrait lookup by character token — scans the loaded leads database
    /// (lead front portraits, then dialogue-node portraits) and prefers a
    /// neutral-emotion frame. Sprite naming: char_&lt;token&gt;_&lt;emotion&gt;_fNN.
    /// </summary>
    internal static class DossierPortraits
    {
        public static Sprite Find(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            var repo = Object.FindAnyObjectByType<LeadsRepository>();
            var leads = repo != null && repo.database != null ? repo.database.Leads : null;
            if (leads == null) return null;

            Sprite best = null;
            foreach (var lead in leads)
            {
                if (lead == null) continue;
                best = Prefer(best, Match(lead.actorPortrait, token));
                var nodes = lead.resolutionDialogue != null ? lead.resolutionDialogue.nodes : null;
                if (nodes != null)
                    foreach (var n in nodes)
                        if (n != null) best = Prefer(best, Match(n.portrait, token));
                if (Score(best) >= 2) return best;
            }

            // Package episodes (Four Keys onward) keep their scenes on the
            // package beat graphs, not on the cards: scan those too.
            var packages = AQ.App.Episodes.EpisodeRuntime.Current?.packages;
            if (packages != null)
                foreach (var p in packages.packages)
                {
                    var nodes = p != null && p.beatDialogue != null ? p.beatDialogue.nodes : null;
                    if (nodes == null) continue;
                    foreach (var n in nodes)
                        if (n != null) best = Prefer(best, Match(n.portrait, token));
                    if (Score(best) >= 2) return best;
                }

            // Standing cast who front UI without a scene in the running episode
            // (Gerald mentors the tutorial in every episode): Resources fallback.
            if (best == null)
                best = Resources.Load<Sprite>("App/Characters/char_" + token + "_neutral")
                    ?? Resources.Load<Sprite>("App/Characters/char_" + token + "_neutral_f01");
            return best;
        }

        public static string Token(Sprite sprite)
        {
            var n = sprite != null ? sprite.name.ToLowerInvariant() : string.Empty;
            if (!n.StartsWith("char_")) return string.Empty;
            int end = n.IndexOf('_', 5);
            return end > 5 ? n.Substring(5, end - 5) : n.Substring(5);
        }

        private static Sprite Match(Sprite s, string token) => Token(s) == token ? s : null;

        // Expression preference (Stephen-ruled 2026-08-12): neutral, then happy,
        // then whatever exists — worried/sad frames stop fronting the board.
        private static int Score(Sprite s)
        {
            if (s == null) return -1;
            var n = s.name.ToLowerInvariant();
            return n.Contains("neutral") ? 2 : n.Contains("happy") ? 1 : 0;
        }

        private static Sprite Prefer(Sprite current, Sprite candidate) =>
            Score(candidate) > Score(current) ? candidate : current;
    }
}
