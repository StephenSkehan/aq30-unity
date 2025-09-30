#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.Diagnostics
{
    /// <summary>
    /// Scene-only, read-only:
    /// Reports activation state of RequirementsRow and each Req slot,
    /// plus whether ActorAnchor/Image exists & enabled.
    /// This explicitly calls out inactive ancestors (not just component.enabled).
    /// </summary>
    public static class LiveCardActivationAudit
    {
        [MenuItem("AQ/Diagnostics/Leads/Live Activation Audit (Rows/Slots/Actor)")]
        public static void Run()
        {
            var content = GameObject.Find("Content_Leads")?.transform;
            if (!content) { Debug.LogWarning("[AQ ActivAudit] Content_Leads not found."); return; }

            int cards = 0;
            foreach (Transform card in content)
            {
                bool looksLikeCard = card.Find("Text_Title") || card.Find("RequirementsRow");
                if (!looksLikeCard) continue;
                cards++;

                var sb = new StringBuilder();
                sb.Append("[AQ ActivAudit] ").Append(card.name).Append(" | ");

                var row = card.Find("RequirementsRow");
                sb.Append("Row=").Append(State(row?.gameObject)).Append("  ");

                for (int i = 1; i <= 3; i++)
                {
                    var slot = row ? row.Find($"Req_{i}") : null;
                    sb.Append($"Req_{i}=").Append(State(slot?.gameObject)).Append("  ");
                }

                var actorImg = card.Find("ActorAnchor/Image")?.GetComponent<Image>();
                sb.Append("ActorImg=");
                if (!actorImg) sb.Append("(none)");
                else sb.Append(actorImg.enabled && actorImg.gameObject.activeInHierarchy ? "on" : "off");

                Debug.Log(sb.ToString());
            }
            Debug.Log("[AQ ActivAudit] Cards scanned: " + cards + ".");
        }

        private static string State(GameObject go)
        {
            if (!go) return "(none)";
            return go.activeInHierarchy ? "active" : go.activeSelf ? "inactive(parent-off)" : "inactive(self)";
        }
    }
}
#endif
