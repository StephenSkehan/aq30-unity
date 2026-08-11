using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Specials
{
    /// <summary>
    /// "Show what will happen" confirmation before a special item is spent
    /// (Stephen-ruled 2026-08-11). Renders before → after icons: item sprites,
    /// a ??? silhouette for Search Warrant reveals (no spoilers), or a drawn X
    /// for removals. Dynamic canvas above the targeting overlay.
    /// </summary>
    public static class SpecialConfirmPopup
    {
        private static GameObject _root;

        public static bool IsOpen => _root != null;

        public static void Show(string title, string desc, IList<Sprite> before, IList<Sprite> after,
                                bool afterUnknown, Action onConfirm, Action onCancel)
        {
            if (_root != null) return;

            const float panelW = 700f;
            const float panelH = 452f;

            _root = new GameObject("__SpecialConfirm", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            UnityEngine.Object.DontDestroyOnLoad(_root);
            var canvas          = _root.GetComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 12000; // above the 9000 targeting overlay
            var scaler                 = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight  = 0.5f;

            var dim       = MakeRect("Dim", _root.transform);
            dim.anchorMin = Vector2.zero;
            dim.anchorMax = Vector2.one;
            dim.offsetMin = dim.offsetMax = Vector2.zero;
            dim.gameObject.AddComponent<Image>().color = AQTheme.Scrim;

            var panel              = MakeRect("Panel", _root.transform);
            panel.anchorMin        = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot            = new Vector2(0.5f, 0.5f);
            panel.sizeDelta        = new Vector2(panelW, panelH);
            panel.anchoredPosition = Vector2.zero;
            AQTheme.Round(panel.gameObject.AddComponent<Image>(), AQTheme.Panel);

            float cursor = panelH / 2f - 24f;

            var titleRt = PlaceRect("Title", panel, new Vector2(panelW - 60f, 40f), new Vector2(0f, cursor - 20f));
            AddTmp(titleRt, title, 32f, AQTheme.Paper, FontStyles.Bold, TextAlignmentOptions.Center);
            cursor -= 40f + 12f;

            var descRt = PlaceRect("Desc", panel, new Vector2(panelW - 80f, 68f), new Vector2(0f, cursor - 34f));
            AddTmp(descRt, desc, 25f, AQTheme.PaperDim, FontStyles.Normal, TextAlignmentOptions.Center);
            cursor -= 68f + 16f;

            // Icon strip: before > after
            const float icon = 108f, gap = 10f, arrowW = 56f;
            int nBefore = before != null ? before.Count : 0;
            int nAfter  = afterUnknown ? 1 : (after != null && after.Count > 0 ? after.Count : 1);
            float stripW = nBefore * icon + (nBefore - 1) * gap + arrowW + nAfter * icon + (nAfter - 1) * gap + 24f;
            float x = -stripW / 2f;
            float stripY = cursor - 62f;

            for (int i = 0; i < nBefore; i++)
            {
                PlaceIcon(panel, before[i], new Vector2(x + icon / 2f, stripY));
                x += icon + gap;
            }
            x += -gap + 12f;
            var arrowRt = PlaceRect("Arrow", panel, new Vector2(arrowW, icon), new Vector2(x + arrowW / 2f, stripY));
            AddTmp(arrowRt, ">", 46f, AQTheme.PaperDim, FontStyles.Bold, TextAlignmentOptions.Center);
            x += arrowW + 12f;

            if (afterUnknown)
            {
                PlaceMysteryBox(panel, new Vector2(x + icon / 2f, stripY));
            }
            else if (after == null || after.Count == 0)
            {
                PlaceRemovedBox(panel, new Vector2(x + icon / 2f, stripY));
            }
            else
            {
                for (int i = 0; i < after.Count; i++)
                {
                    PlaceIcon(panel, after[i], new Vector2(x + icon / 2f, stripY));
                    x += icon + gap;
                }
            }
            cursor -= 124f + 24f;

            var cancel = PlaceButton("Cancel", panel, AQTheme.Steel,
                                     new Vector2(280f, 80f), new Vector2(-165f, cursor - 40f));
            cancel.onClick.AddListener(() => { Close(); onCancel?.Invoke(); });
            var ok = PlaceButton("Confirm", panel, AQTheme.Success,
                                 new Vector2(280f, 80f), new Vector2(165f, cursor - 40f));
            ok.onClick.AddListener(() => { Close(); onConfirm?.Invoke(); });
        }

        public static void Close()
        {
            if (_root == null) return;
            UnityEngine.Object.Destroy(_root);
            _root = null;
        }

        // ---- pieces ----

        private static void PlaceIcon(RectTransform parent, Sprite sprite, Vector2 centre)
        {
            var cell = PlaceRect("Icon", parent, new Vector2(108f, 108f), centre);
            var bg   = cell.gameObject.AddComponent<Image>();
            AQTheme.Round(bg, AQTheme.Card);
            bg.raycastTarget = false;

            var inner = PlaceRect("Sprite", cell, new Vector2(92f, 92f), Vector2.zero);
            var img   = inner.gameObject.AddComponent<Image>();
            img.raycastTarget  = false;
            img.preserveAspect = true;
            if (sprite != null) img.sprite = sprite;
            else                img.color  = new Color(0.4f, 0.4f, 0.4f, 1f);
        }

        private static void PlaceMysteryBox(RectTransform parent, Vector2 centre)
        {
            var cell = PlaceRect("Mystery", parent, new Vector2(108f, 108f), centre);
            var bg   = cell.gameObject.AddComponent<Image>();
            AQTheme.Round(bg, new Color(0.05f, 0.08f, 0.15f, 0.95f)); // family-view silhouette tone
            bg.raycastTarget = false;
            var lbl = PlaceRect("Lbl", cell, new Vector2(108f, 108f), Vector2.zero);
            AddTmp(lbl, "???", 34f, AQTheme.PaperDim, FontStyles.Bold, TextAlignmentOptions.Center);
        }

        private static void PlaceRemovedBox(RectTransform parent, Vector2 centre)
        {
            var cell = PlaceRect("Removed", parent, new Vector2(108f, 108f), centre);
            var bg   = cell.gameObject.AddComponent<Image>();
            AQTheme.Round(bg, AQTheme.BoardFrame);
            bg.raycastTarget = false;
            AQTheme.AddDrawnX(cell, AQTheme.AlertRed, 52f, 8f);
        }

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
}
