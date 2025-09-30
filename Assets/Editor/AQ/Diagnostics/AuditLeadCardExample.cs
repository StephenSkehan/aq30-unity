#if UNITY_EDITOR
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.EditorTools.Diagnostics
{
    public static class AuditLeadCardExample
    {
        const string CardName = "LeadCardExample";

        [MenuItem("AQ/UI/Leads/Audit ▸ LeadCardExample (Edit)")]
        public static void AuditByName()
        {
            var target = Resources.FindObjectsOfTypeAll<RectTransform>()
                .FirstOrDefault(t =>
                    t && t.gameObject.scene.IsValid() &&
                    t.name == CardName);

            if (!target)
            {
                Debug.LogWarning($"[AQ Audit] Could not find '{CardName}' in the active scene (Edit mode). " +
                                 $"Select the card object and run 'Audit ▸ Selected' instead.");
                return;
            }
            DumpCard(target);
        }

        [MenuItem("AQ/UI/Leads/Audit ▸ Selected (Edit)")]
        public static void AuditSelected()
        {
            var tr = Selection.activeTransform as RectTransform;
            if (!tr)
            {
                Debug.LogWarning("[AQ Audit] Please select a GameObject (RectTransform) in the Scene hierarchy.");
                return;
            }
            DumpCard(tr);
        }

        // -------- internals --------
        static void DumpCard(RectTransform root)
        {
            var sb = new StringBuilder(4096);

            sb.AppendLine("=== [AQ LeadCard Audit] BEGIN ===");
            sb.AppendLine($"Path: {GetPath(root)}  (Scene='{root.gameObject.scene.name}')");

            DumpTransformRecursive(root, 0, sb);

            sb.AppendLine("=== [AQ LeadCard Audit] END ===");

            // One log entry -> easy to copy/paste back here
            Debug.Log(sb.ToString());
        }

        static void DumpTransformRecursive(Transform t, int depth, StringBuilder sb)
        {
            var go = t.gameObject;
            string indent = new string(' ', depth * 2);

            // Header line for this node
            sb.Append(indent).Append("• ").Append(go.name)
              .Append("  [activeSelf=").Append(go.activeSelf ? "on" : "off")
              .Append(" | inHierarchy=").Append(go.activeInHierarchy ? "on" : "off")
              .Append(" | layer=").Append(LayerMask.LayerToName(go.layer))
              .Append(" | tag=").Append(go.tag).AppendLine("]");

            // RectTransform basics
            var rt = t as RectTransform;
            if (rt)
            {
                var rect = rt.rect;
                sb.Append(indent).Append("  RectTransform")
                  .Append("  pos=").Append(F(rt.anchoredPosition))
                  .Append("  size=").Append($"({Round(rect.width)},{Round(rect.height)})")
                  .Append("  anchors=").Append($"{F(rt.anchorMin)}→{F(rt.anchorMax)}")
                  .Append("  pivot=").Append(F(rt.pivot))
                  .AppendLine();
            }

            // Image (visuals)
            var img = go.GetComponent<Image>();
            if (img)
            {
                sb.Append(indent).Append("  Image")
                  .Append("  enabled=").Append(img.enabled ? "on" : "off")
                  .Append("  sprite=").Append(img.sprite ? img.sprite.name : "(null)")
                  .Append("  type=").Append(img.type)
                  .Append("  preserveAspect=").Append(img.preserveAspect ? "on" : "off")
                  .Append("  color=").Append(Hex(img.color))
                  .Append("  maskable=").Append(img.maskable ? "on" : "off")
                  .Append("  raycast=").Append(img.raycastTarget ? "on" : "off")
                  .AppendLine();
            }

            // TMP text
            var tmp = go.GetComponent<TMP_Text>();
            if (tmp)
            {
                string labelPreview = tmp.text;
                if (!string.IsNullOrEmpty(labelPreview) && labelPreview.Length > 42)
                    labelPreview = labelPreview.Substring(0, 42) + "…";

                sb.Append(indent).Append("  TMP_Text")
                  .Append("  text=\"").Append(labelPreview).Append('"')
                  .Append("  size=").Append(Round(tmp.fontSize))
                  .Append("  bold=").Append((tmp.fontStyle & FontStyles.Bold) != 0 ? "on" : "off")
                  .Append("  color=").Append(Hex(tmp.color))
                  .Append("  alignment=").Append(tmp.alignment)
                  .AppendLine();
            }

            // CanvasGroup (visibility/interactivity)
            var cg = go.GetComponent<CanvasGroup>();
            if (cg)
            {
                sb.Append(indent).Append("  CanvasGroup")
                  .Append("  alpha=").Append(Round(cg.alpha))
                  .Append("  interactable=").Append(cg.interactable ? "on" : "off")
                  .Append("  blocksRaycasts=").Append(cg.blocksRaycasts ? "on" : "off")
                  .AppendLine();
            }

            // Layout components (if any)
            var hlg = go.GetComponent<HorizontalLayoutGroup>();
            if (hlg)
            {
                sb.Append(indent).Append("  HorizontalLayoutGroup")
                  .Append("  spacing=").Append(Round(hlg.spacing))
                  .Append("  childAlign=").Append(hlg.childAlignment)
                  .Append("  pad=(").Append(hlg.padding.left).Append(',')
                                      .Append(hlg.padding.right).Append(',')
                                      .Append(hlg.padding.top).Append(',')
                                      .Append(hlg.padding.bottom).Append(')')
                  .Append("  ctrlW=").Append(hlg.childControlWidth ? "on" : "off")
                  .Append("  ctrlH=").Append(hlg.childControlHeight ? "on" : "off")
                  .Append("  expandW=").Append(hlg.childForceExpandWidth ? "on" : "off")
                  .Append("  expandH=").Append(hlg.childForceExpandHeight ? "on" : "off")
                  .AppendLine();
            }

            var vlg = go.GetComponent<VerticalLayoutGroup>();
            if (vlg)
            {
                sb.Append(indent).Append("  VerticalLayoutGroup")
                  .Append("  spacing=").Append(Round(vlg.spacing))
                  .Append("  childAlign=").Append(vlg.childAlignment)
                  .Append("  pad=(").Append(vlg.padding.left).Append(',')
                                      .Append(vlg.padding.right).Append(',')
                                      .Append(vlg.padding.top).Append(',')
                                      .Append(vlg.padding.bottom).Append(')')
                  .AppendLine();
            }

            var glg = go.GetComponent<GridLayoutGroup>();
            if (glg)
            {
                sb.Append(indent).Append("  GridLayoutGroup")
                  .Append("  cellSize=").Append(F(glg.cellSize))
                  .Append("  spacing=").Append(F(glg.spacing))
                  .Append("  startAxis=").Append(glg.startAxis)
                  .Append("  childAlign=").Append(glg.childAlignment)
                  .AppendLine();
            }

            var csf = go.GetComponent<ContentSizeFitter>();
            if (csf)
            {
                sb.Append(indent).Append("  ContentSizeFitter")
                  .Append("  H=").Append(csf.horizontalFit)
                  .Append("  V=").Append(csf.verticalFit)
                  .AppendLine();
            }

            var rm2 = go.GetComponent<RectMask2D>();
            if (rm2)
            {
                sb.Append(indent).Append("  RectMask2D").AppendLine();
            }

            // Recurse
            for (int i = 0; i < t.childCount; i++)
                DumpTransformRecursive(t.GetChild(i), depth + 1, sb);
        }

        // helpers
        static string GetPath(Transform t)
        {
            var p = t.name;
            var cur = t.parent;
            while (cur != null) { p = cur.name + "/" + p; cur = cur.parent; }
            return p;
        }
        static string F(Vector2 v) => $"({Round(v.x)},{Round(v.y)})";
        static string Hex(Color c)
        {
            Color32 c32 = c;
            return $"#{c32.r:X2}{c32.g:X2}{c32.b:X2}{c32.a:X2}";
        }
        static string Round(float f) => f.ToString("0.##");
    }
}
#endif
