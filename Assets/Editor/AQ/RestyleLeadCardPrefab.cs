using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using AQ.App.UI;
using AQ.App.UI.Leads;

namespace AQ.EditorTools
{
    /// <summary>
    /// One-shot LeadCard.prefab restructure for the noir theme. The current
    /// prefab has a 144px actor portrait and 150px requirement icons colliding
    /// with the texts; this shrinks/anchors everything into a clean layout:
    /// small round badge top-left, title/subtitle beside it, requirement row
    /// along the bottom.
    /// </summary>
    public static class RestyleLeadCardPrefab
    {
        const string PrefabPath = "Assets/UI/Prefabs/LeadCard.prefab";

        [MenuItem("AQ/Setup/Restyle Lead Card Prefab (Noir)")]
        public static void Restyle()
        {
            var rounded = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/App/UI/aq_rounded.png");
            var display = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/App/UI/Fonts/Staatliches SDF.asset");
            var body    = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/App/UI/Fonts/NunitoSans SDF.asset");
            if (rounded == null || display == null || body == null)
            {
                Debug.LogError("[LeadCard] rounded sprite or fonts missing — run the font/board setup items first.");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var presenter = root.GetComponent<LeadCardPresenter>();
                if (presenter == null) { Debug.LogError("[LeadCard] no LeadCardPresenter on prefab root."); return; }

                // Card body
                if (presenter.background != null)
                {
                    presenter.background.sprite = rounded;
                    presenter.background.type   = Image.Type.Sliced;
                    presenter.background.color  = AQTheme.Card;
                }

                // Actor bust: portraits are transparent now — render a larger bust
                // whose head rises slightly above the card's top edge (GH-customer
                // style). Requirement chips draw over its base, which reads as
                // intentional layering.
                if (presenter.actorAnchor != null)
                {
                    // Bust bottom aligns with the title's bottom edge (title spans
                    // y -10..-42), so the character stands ON the card header and
                    // the full subtitle width below is free.
                    var rt = presenter.actorAnchor.rectTransform;
                    rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
                    rt.pivot     = new Vector2(0f, 1f);
                    rt.anchoredPosition = new Vector2(10f, 94f);
                    rt.sizeDelta = new Vector2(96f, 136f);
                    presenter.actorAnchor.preserveAspect = true;
                }

                StyleText(presenter.titleText, display, 24f, AQTheme.Paper, TextAlignmentOptions.TopLeft);
                if (presenter.titleText != null)
                {
                    var rt = presenter.titleText.rectTransform;
                    rt.anchorMin = new Vector2(0f, 1f);
                    rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot     = new Vector2(0.5f, 1f);
                    rt.offsetMin = new Vector2(118f, 0f);
                    rt.offsetMax = new Vector2(-12f, 0f);
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -10f);
                    rt.sizeDelta = new Vector2(rt.sizeDelta.x, 32f);
                    presenter.titleText.overflowMode = TextOverflowModes.Ellipsis;
                    presenter.titleText.enableAutoSizing = false;
                }

                // Subtitle: full card width at a bigger size — the bust ends at the
                // title line, so the whole band below it belongs to the copy.
                StyleText(presenter.objectiveText, body, 18f, AQTheme.PaperDim, TextAlignmentOptions.TopLeft);
                if (presenter.objectiveText != null)
                {
                    var rt = presenter.objectiveText.rectTransform;
                    rt.anchorMin = new Vector2(0f, 1f);
                    rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot     = new Vector2(0.5f, 1f);
                    rt.offsetMin = new Vector2(14f, 0f);
                    rt.offsetMax = new Vector2(-12f, 0f);
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -48f);
                    rt.sizeDelta = new Vector2(rt.sizeDelta.x, 66f);
                    presenter.objectiveText.overflowMode = TextOverflowModes.Ellipsis;
                }

                // Requirements row along the bottom
                if (presenter.requirementsRow != null)
                {
                    var rt = presenter.requirementsRow;
                    rt.anchorMin = new Vector2(0f, 0f);
                    rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot     = new Vector2(0.5f, 0f);
                    rt.offsetMin = new Vector2(12f, 0f);
                    rt.offsetMax = new Vector2(-12f, 0f);
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, 10f);
                    rt.sizeDelta = new Vector2(rt.sizeDelta.x, 92f);
                }

                foreach (var slot in presenter.slots)
                {
                    if (slot == null) continue;
                    var srt = (RectTransform)slot.transform;
                    srt.sizeDelta = new Vector2(68f, 88f);

                    var icon = slot.icon != null ? slot.icon.rectTransform : null;
                    if (icon != null)
                    {
                        icon.anchorMin = icon.anchorMax = new Vector2(0.5f, 1f);
                        icon.pivot     = new Vector2(0.5f, 1f);
                        icon.anchoredPosition = new Vector2(0f, 0f);
                        icon.sizeDelta = new Vector2(60f, 60f);
                    }

                    var label = slot.transform.Find("Label");
                    if (label != null)
                    {
                        var lrt = (RectTransform)label;
                        lrt.anchorMin = lrt.anchorMax = new Vector2(0.5f, 0f);
                        lrt.pivot     = new Vector2(0.5f, 0f);
                        lrt.anchoredPosition = new Vector2(0f, 0f);
                        lrt.sizeDelta = new Vector2(84f, 24f);
                        StyleText(label.GetComponent<TMP_Text>(), body, 11f, AQTheme.PaperDim, TextAlignmentOptions.Top);
                    }

                    // Drop the prefab's plain-square tick; the runtime badge
                    // (RequirementSlotView.CreateTickOverlay) replaces it.
                    if (slot.tickOverlay != null)
                    {
                        Object.DestroyImmediate(slot.tickOverlay);
                        slot.tickOverlay = null;
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log("[LeadCard] prefab restyled.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        static void StyleText(TMP_Text t, TMP_FontAsset font, float size, Color color, TextAlignmentOptions align)
        {
            if (t == null) return;
            t.font      = font;
            t.fontSize  = size;
            t.color     = color;
            t.alignment = align;
            t.fontStyle  = FontStyles.Normal;
        }
    }
}
