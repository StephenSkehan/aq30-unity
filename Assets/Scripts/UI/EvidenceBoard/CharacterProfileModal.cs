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

        public static void Show(LeadData lead, List<LeadData> relatedLeads, Action<LeadData> onReplay)
        {
            if (_root != null) return;

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

            // Panel — sized to content
            var leads     = relatedLeads ?? new List<LeadData> { lead };
            float panelH  = 20f + 220f + 18f + 64f + 12f + 80f + 20f + 2f + 18f + leads.Count * 92f + 92f + 24f;
            float panelW  = 640f;

            var panel             = MakeRect("Panel", _root.transform);
            panel.anchorMin       = new Vector2(0.5f, 0.5f);
            panel.anchorMax       = new Vector2(0.5f, 0.5f);
            panel.pivot           = new Vector2(0.5f, 0.5f);
            panel.sizeDelta       = new Vector2(panelW, panelH);
            panel.anchoredPosition = Vector2.zero;
            panel.gameObject.AddComponent<Image>().color = new Color(0.12f, 0.10f, 0.08f, 1f);

            // Build content top → down using a cursor (from panel top, going negative)
            float cursor = panelH / 2f;

            // Portrait
            cursor -= 20f;
            var pRt              = PlaceRect("Portrait", panel, new Vector2(220f, 220f), new Vector2(0f, cursor - 110f));
            var pImg             = pRt.gameObject.AddComponent<Image>();
            pImg.sprite          = lead.actorPortrait;
            pImg.preserveAspect  = true;
            pImg.raycastTarget   = false;
            if (lead.actorPortrait == null) pImg.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            cursor -= 220f + 18f;

            // Name
            var nameRt     = PlaceRect("Name", panel, new Vector2(panelW - 40f, 64f), new Vector2(0f, cursor - 32f));
            var nameTmp    = nameRt.gameObject.AddComponent<TextMeshProUGUI>();
            nameTmp.text   = lead.title;
            nameTmp.fontSize    = 40f;
            nameTmp.fontStyle   = FontStyles.Bold;
            nameTmp.color       = Color.white;
            nameTmp.alignment   = TextAlignmentOptions.Center;
            nameTmp.raycastTarget = false;
            cursor -= 64f + 12f;

            // Role/subtitle
            var roleRt     = PlaceRect("Role", panel, new Vector2(panelW - 40f, 80f), new Vector2(0f, cursor - 40f));
            var roleTmp    = roleRt.gameObject.AddComponent<TextMeshProUGUI>();
            roleTmp.text   = lead.subtitle ?? string.Empty;
            roleTmp.fontSize    = 26f;
            roleTmp.color       = new Color(0.72f, 0.68f, 0.64f, 1f);
            roleTmp.alignment   = TextAlignmentOptions.Center;
            roleTmp.raycastTarget = false;
            cursor -= 80f + 20f;

            // Divider
            var div       = PlaceRect("Divider", panel, new Vector2(panelW - 60f, 2f), new Vector2(0f, cursor - 1f));
            div.gameObject.AddComponent<Image>().color = new Color(0.35f, 0.30f, 0.25f, 1f);
            cursor -= 2f + 18f;

            // Replay buttons
            foreach (var rl in leads)
            {
                string btnLabel  = leads.Count == 1 ? "Replay Evidence" : $"Replay: {TruncateStr(rl.title, 28)}";
                var btn          = PlaceButton(btnLabel, panel, new Color(0.18f, 0.42f, 0.28f, 1f),
                                              new Vector2(panelW - 60f, 80f), new Vector2(0f, cursor - 40f));
                var capturedLead = rl;
                btn.onClick.AddListener(() => { Close(); onReplay?.Invoke(capturedLead); });
                cursor -= 80f + 12f;
            }

            // Close
            cursor -= 4f;
            var close = PlaceButton("Close", panel, new Color(0.32f, 0.18f, 0.18f, 1f),
                                    new Vector2(260f, 80f), new Vector2(0f, cursor - 40f));
            close.onClick.AddListener(Close);
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
            go.GetComponent<Image>().color = color;

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
