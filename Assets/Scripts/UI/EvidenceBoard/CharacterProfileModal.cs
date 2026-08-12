using System;
using System.Collections.Generic;
using AQ.App.Leads;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.EvidenceBoard
{
    public static class CharacterProfileModal
    {
        private static GameObject _root;

        /// <summary>True while the modal is up — the board's raw-input tap poll
        /// suspends itself so taps on the modal don't fall through to pins.</summary>
        public static bool IsOpen => _root != null;

        public static void Show(Sprite portrait, List<LeadData> relatedLeads, Action<LeadData> onReplay,
                                string displayName, string characterKey = null)
        {
            if (_root != null) return;
            bool hasDossier = Dossiers.DossierService.Has(characterKey);

            _root = new GameObject("__CharProfile", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            UnityEngine.Object.DontDestroyOnLoad(_root);

            var canvas            = _root.GetComponent<Canvas>();
            canvas.renderMode     = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder   = 9999;

            var scaler                  = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode          = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution  = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight   = 0.5f;

            // Dim
            var dim       = MakeRect("Dim", _root.transform);
            dim.anchorMin = Vector2.zero;
            dim.anchorMax = Vector2.one;
            dim.offsetMin = dim.offsetMax = Vector2.zero;
            dim.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);

            // Panel — sized to content. Name lives in the title bar
            // (Stephen-ruled 2026-08-12); portrait LEFT, Case File RIGHT.
            var leads     = relatedLeads ?? new List<LeadData>();
            const float headerH = 200f;
            float panelH  = 96f + 16f + headerH + 16f + 2f + 18f + leads.Count * 92f + 24f;
            float panelW  = 640f;

            var panel             = MakeRect("Panel", _root.transform);
            panel.anchorMin       = new Vector2(0.5f, 0.5f);
            panel.anchorMax       = new Vector2(0.5f, 0.5f);
            panel.pivot           = new Vector2(0.5f, 0.5f);
            panel.sizeDelta       = new Vector2(panelW, panelH);
            panel.anchoredPosition = Vector2.zero;
            panel.gameObject.AddComponent<Image>().color = new Color(0.12f, 0.10f, 0.08f, 1f);

            // Build content top → down using a cursor (from panel top, going negative)
            float cursor = panelH / 2f - 96f - 16f;

            // Portrait — left column (name lives in the title bar)
            var pRt              = PlaceRect("Portrait", panel, new Vector2(200f, 200f), new Vector2(-190f, cursor - 100f));
            var pImg             = pRt.gameObject.AddComponent<Image>();
            pImg.sprite          = portrait;
            pImg.preserveAspect  = true;
            pImg.raycastTarget   = false;
            if (portrait == null) pImg.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            // (No role/scene-count line — Stephen-ruled 2026-08-11: meta copy
            // breaks the fiction. Character role copy is a future bible pull.)

            // Case File — right column, centred on the header block
            if (hasDossier)
            {
                var fileBtn = PlaceButton("Case File", panel, new Color(0.55f, 0.40f, 0.16f, 1f),
                                          new Vector2(270f, 80f), new Vector2(155f, cursor - headerH / 2f));
                var keyCap  = characterKey;
                var portCap = portrait;
                fileBtn.onClick.AddListener(() => { Close(); Dossiers.DossierPopup.Show(keyCap, portCap); });
            }
            cursor -= headerH + 16f;

            // Divider
            var div       = PlaceRect("Divider", panel, new Vector2(panelW - 60f, 2f), new Vector2(0f, cursor - 1f));
            div.gameObject.AddComponent<Image>().color = new Color(0.35f, 0.30f, 0.25f, 1f);
            cursor -= 2f + 18f;

            // Replay buttons — always titled with the scene (Stephen-ruled 2026-08-11)
            foreach (var rl in leads)
            {
                string btnLabel  = $"Replay: {TruncateStr(rl.title, 28)}";
                var btn          = PlaceButton(btnLabel, panel, new Color(0.18f, 0.42f, 0.28f, 1f),
                                              new Vector2(panelW - 60f, 80f), new Vector2(0f, cursor - 40f));
                var capturedLead = rl;
                btn.onClick.AddListener(() => { Close(); onReplay?.Invoke(capturedLead); });
                cursor -= 80f + 12f;
            }

            // Title-bar treatment (Stephen-ruled 2026-08-12); the bar's X closes.
            AQTheme.TitleBar(panel, displayName ?? string.Empty, Close,
                "Everyone pinned to the evidence board. Replay any scene this character appears in; CASE FILE opens Ally's dossier on them.");
        }

        private static void Close()
        {
            if (_root == null) return;
            UnityEngine.Object.Destroy(_root);
            _root = null;
        }

        // Anchor centre, pivot centre
        private static RectTransform PlaceRect(string name, RectTransform parent, Vector2 size, Vector2 centre)
        {
            var go              = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt              = go.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
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
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.sizeDelta        = size;
            rt.anchoredPosition = centre;
            AQTheme.StyleButton(go.GetComponent<Image>(), color);

            var lbl       = MakeRect("Lbl", rt);
            lbl.anchorMin = Vector2.zero;
            lbl.anchorMax = Vector2.one;
            lbl.offsetMin = lbl.offsetMax = Vector2.zero;
            var tmp            = lbl.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text           = label;
            tmp.fontSize       = 30f;
            tmp.color          = Color.white;
            tmp.alignment      = TextAlignmentOptions.Center;
            tmp.raycastTarget  = false;

            return go.GetComponent<Button>();
        }

        private static RectTransform MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static string TruncateStr(string s, int max)
            => s != null && s.Length > max ? s.Substring(0, max - 1) + "…" : s ?? string.Empty;
    }
}
