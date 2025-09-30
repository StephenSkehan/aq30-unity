#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.Editor.Leads
{
    public static class LeadsBarCardDebugPaint
    {
        // After “Conform to Audit”, Viewport is under LeadsBar.
        private const string PostConformContent = "LeadsBar/Viewport/Content_Leads";
        // Fallback for pre-conform layouts.
        private const string PreConformContent  = "LeadsBar/ScrollLeads/Viewport/Content_Leads";

        [MenuItem("AQ/Leads/Debug Paint Cards (visible)")]
        public static void Paint()
        {
            Transform content = GameObject.Find(PostConformContent)?.transform;
            if (!content)
                content = GameObject.Find(PreConformContent)?.transform;

            if (!content)
            {
                Debug.LogError("❌ Content_Leads not found. Run 'AQ → Leads → Conform to Audit (ScrollRect on LeadsBar)' first.");
                return;
            }

            Undo.IncrementCurrentGroup();
            int n = 0;

            foreach (Transform child in content)
            {
                var card = child as RectTransform;
                if (!card) continue;

                // Body: visible medium gray so it pops from the dark HUD strip
                var img = card.GetComponent<Image>() ?? Undo.AddComponent<Image>(card.gameObject);
                img.type = Image.Type.Simple;
                img.color = new Color(0.30f, 0.30f, 0.30f, 1f);
                img.raycastTarget = false;

                // Thin white border using UI Outline (no sprite required)
                var outline = card.GetComponent<Outline>() ?? Undo.AddComponent<Outline>(card.gameObject);
                outline.effectColor = new Color(1f, 1f, 1f, 0.18f);
                outline.effectDistance = new Vector2(1f, -1f);
                outline.useGraphicAlpha = true;

                // Ensure a visible title exists (top-left)
                var title = card.Find("V/Text_Title")?.GetComponent<TextMeshProUGUI>();
                if (!title)
                {
                    var tgo = new GameObject("Text_Title", typeof(RectTransform), typeof(TextMeshProUGUI));
                    var trt = tgo.GetComponent<RectTransform>();
                    trt.SetParent(card, false);
                    trt.anchorMin = new Vector2(0f, 1f);
                    trt.anchorMax = new Vector2(0f, 1f);
                    trt.pivot     = new Vector2(0f, 1f);
                    trt.anchoredPosition = new Vector2(16f, -16f);
                    title = tgo.GetComponent<TextMeshProUGUI>();
                }
                title.text = "Demo Lead";
                title.fontSize = 30;
                title.color = Color.white;
#if UNITY_2022_2_OR_NEWER
                title.textWrappingMode = TextWrappingModes.NoWrap;
#else
                title.enableWordWrapping = false;
#endif
                n++;
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            Debug.Log($"🎨 Painted {n} card(s): visible body + outline + title.");
        }
    }
}
#endif
