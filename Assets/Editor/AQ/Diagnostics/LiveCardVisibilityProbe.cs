#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.Diagnostics
{
    /// <summary>
    /// Scene-only read: for each card under Content_Leads, reports:
    /// - Actor Image present/enabled? culled?
    /// - Each Req Icon present/enabled? culled?
    /// - Approx screen rect of Actor Image
    /// </summary>
    public static class LiveCardVisibilityProbe
    {
        [MenuItem("AQ/Diagnostics/Leads/Probe Live Card Visibility")]
        public static void Run()
        {
            var content = GameObject.Find("Content_Leads")?.transform;
            var viewport = GameObject.Find("Viewport")?.GetComponent<RectTransform>();
            if (!content) { Debug.LogWarning("[AQ VisProbe] Content_Leads not found."); return; }
            if (!viewport) { Debug.LogWarning("[AQ VisProbe] Viewport not found."); return; }

            int scanned = 0;
            foreach (Transform card in content)
            {
                bool looksLikeCard = card.Find("Text_Title") || card.Find("RequirementsRow");
                if (!looksLikeCard) continue;
                scanned++;

                var sb = new StringBuilder();
                sb.Append("[AQ VisProbe] Card '").Append(card.name).Append("': ");

                // Actor
                var actorImg = card.Find("ActorAnchor/Image")?.GetComponent<Image>();
                AppendGraphicInfo(sb, "Actor", actorImg, viewport);

                // Req icons
                for (int i = 1; i <= 3; i++)
                {
                    var icon = card.Find($"RequirementsRow/Req_{i}/Icon")?.GetComponent<Image>();
                    AppendGraphicInfo(sb, $"Req_{i}", icon, viewport);
                }

                Debug.Log(sb.ToString());
            }
            Debug.Log("[AQ VisProbe] Live cards scanned: " + scanned + ".");
        }

        private static void AppendGraphicInfo(System.Text.StringBuilder sb, string label, Graphic g, RectTransform viewport)
        {
            if (!g)
            {
                sb.Append(label).Append("=(none)  ");
                return;
            }
            var culled = g.canvasRenderer != null && g.canvasRenderer.cull;
            bool enabled = g.enabled && g.gameObject.activeInHierarchy;

            // Rough screen rect of the element (in viewport space)
            var rt = g.transform as RectTransform;
            Rect rect = Rect.zero;
            if (rt)
            {
                var world = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, rt);
                rect = new Rect(world.min, world.size);
            }

            sb.Append(label).Append("=[")
              .Append(enabled ? "on" : "off")
              .Append(culled ? " culled" : " visible")
              .Append(" rect=").Append(rect.ToString("F1"))
              .Append(" sprite=").Append(g is Image img && img.sprite ? img.sprite.name : "(null)")
              .Append("]  ");
        }
    }
}
#endif
