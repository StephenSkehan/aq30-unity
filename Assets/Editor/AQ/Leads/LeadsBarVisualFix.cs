#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.Editor.Leads
{
    public static class LeadsBarVisualFix
    {
        private const string Root = "LeadsBar/ScrollLeads/Viewport/Content_Leads";
        private const string ViewportPath = "LeadsBar/ScrollLeads/Viewport";
        private const string DebugFrameName = "Viewport_DebugFrame";

        [MenuItem("AQ/Leads/Normalize Card Visuals")]
        public static void NormalizeCards()
        {
            var content = GameObject.Find(Root)?.transform as RectTransform;
            if (!content) { Debug.LogError("❌ Content_Leads not found. Run Leads → Verify + Repair first."); return; }

            Undo.IncrementCurrentGroup();
            int fixedCards = 0;

            for (int i = 0; i < content.childCount; i++)
            {
                var card = content.GetChild(i) as RectTransform;
                if (!card) continue;

                // Card size & layout
                var le = card.GetComponent<LayoutElement>() ?? Undo.AddComponent<LayoutElement>(card.gameObject);
                le.preferredWidth = 360; le.minWidth = 360;
                le.preferredHeight = 220; le.minHeight = 220;

                card.pivot = new Vector2(0, 0.5f);
                card.anchorMin = new Vector2(0, 0.5f);
                card.anchorMax = new Vector2(0, 0.5f);
                card.sizeDelta = new Vector2(360, 220);

                // Visible body: SIMPLE type + dark color so it actually renders
                var body = card.GetComponent<Image>() ?? Undo.AddComponent<Image>(card.gameObject);
                body.type = Image.Type.Simple;
                body.color = new Color(0.15f, 0.15f, 0.15f, 1f); // dark HUD strip feel
                body.raycastTarget = false;

                // Ensure inner vertical stack exists if we used the demo spawner
                var v = card.Find("V") as RectTransform;
                if (v)
                {
                    var vlg = v.GetComponent<VerticalLayoutGroup>();
                    if (vlg)
                    {
                        vlg.spacing = 6;
                        vlg.childControlWidth = false;
                        vlg.childControlHeight = false;
                        vlg.childForceExpandWidth = false;
                        vlg.childForceExpandHeight = false;
                    }
                    v.anchorMin = new Vector2(0, 0);
                    v.anchorMax = new Vector2(1, 1);
                    v.offsetMin = new Vector2(16, 16);
                    v.offsetMax = new Vector2(-16, -16);
                }

                // Text colors & wrapping
                foreach (var tmp in card.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    Undo.RecordObject(tmp, "Normalize TMP");
                    tmp.color = Color.white;
#if UNITY_2022_2_OR_NEWER
                    tmp.textWrappingMode = TextWrappingModes.NoWrap;
#else
                    tmp.enableWordWrapping = false;
#endif
                }

                // Requirement chips tint
                var reqRow = card.Find("V/Row_Requirements");
                if (reqRow)
                {
                    foreach (Transform child in reqRow)
                    {
                        var chipImg = child.GetComponent<Image>();
                        if (chipImg)
                        {
                            chipImg.type = Image.Type.Simple;
                            chipImg.color = new Color(0.10f, 0.55f, 0.55f, 1f); // teal chip
                            chipImg.raycastTarget = false;
                        }
                    }
                }

                fixedCards++;
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            Debug.Log($"✅ Normalized {fixedCards} lead card(s): visible body, chip tint, TMP no-wrap, fixed sizes.");
        }

        [MenuItem("AQ/Leads/Viewport Debug Frame (toggle)")]
        public static void ToggleViewportFrame()
        {
            var vp = GameObject.Find(ViewportPath)?.transform as RectTransform;
            if (!vp) { Debug.LogError("❌ Viewport not found. Run Leads → Verify + Repair first."); return; }

            var existing = vp.Find(DebugFrameName);
            if (existing)
            {
                Undo.DestroyObjectImmediate(existing.gameObject);
                Debug.Log("🧹 Removed viewport debug frame.");
                return;
            }

            Undo.IncrementCurrentGroup();
            var go = new GameObject(DebugFrameName, typeof(RectTransform), typeof(Image));
            Undo.RegisterCreatedObjectUndo(go, "Create Viewport_DebugFrame");
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(vp, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;  rt.offsetMax = Vector2.zero;

            var img = go.GetComponent<Image>();
            img.color = new Color(0f, 1f, 1f, 0.08f); // faint cyan fill to show the window
            img.raycastTarget = false;

            // Add a thin outline
            var outlineGO = new GameObject("Outline", typeof(RectTransform), typeof(Image));
            Undo.RegisterCreatedObjectUndo(outlineGO, "Create Viewport_DebugFrame Outline");
            var ort = outlineGO.GetComponent<RectTransform>();
            ort.SetParent(rt, false);
            ort.anchorMin = Vector2.zero; ort.anchorMax = Vector2.one;
            ort.offsetMin = new Vector2(0, 0); ort.offsetMax = new Vector2(0, 0);
            var oimg = outlineGO.GetComponent<Image>();
            oimg.color = new Color(0f, 1f, 1f, 0.35f); // brighter edge
            oimg.type = Image.Type.Sliced; // if you drop a 1px 9-slice later, this will outline neatly.

            Debug.Log("🔧 Added viewport debug frame (toggle again to remove).");
        }
    }
}
#endif
