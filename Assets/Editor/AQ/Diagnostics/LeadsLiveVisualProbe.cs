#if UNITY_EDITOR
using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.Diagnostics
{
    /// <summary>
    /// Read-only probe of the live LeadsBar visuals (cards, actors, requirement icons) in Edit or Play.
    /// Reports, per card:
    /// - Title / Id text (for identification)
    /// - ActorAnchor/Image: sprite name, alpha, enabled, and whether it is inside the Viewport mask
    /// - RequirementsRow/Req_1..3/Icon: sprite name and enabled
    /// Never modifies scene or assets.
    /// </summary>
    public static class LeadsLiveVisualProbe
    {
        [MenuItem("AQ/Diagnostics/Leads/Live Visual Probe (Cards & Actors)")]
        public static void Run()
        {
            var content = GameObject.Find("Content_Leads")?.transform;
            var viewport = GameObject.Find("LeadsBar")?.transform.Find("Viewport") as RectTransform;

            if (!content)
            {
                Debug.LogWarning("[AQ LiveProbe] Content_Leads not found.");
                return;
            }
            if (!viewport)
            {
                Debug.LogWarning("[AQ LiveProbe] LeadsBar/Viewport not found (mask presence cannot be evaluated).");
            }

            int cardCount = 0;
            foreach (Transform card in content)
            {
                // skip non-card children
                var hasTitle = card.Find("Text_Title");
                var hasReq   = card.Find("RequirementsRow");
                if (!hasTitle && !hasReq) continue;

                cardCount++;

                var title = card.Find("Text_Title")?.GetComponent<TMP_Text>()?.text ?? "(no title)";
                var idTxt = card.Find("Text_LeadId")?.GetComponent<TMP_Text>()?.text ?? "(no id)";

                // Actor image
                var actorTf  = card.Find("ActorAnchor/Image");
                var actorImg = actorTf ? actorTf.GetComponent<Image>() : null;
                string actorSprite = actorImg && actorImg.sprite ? actorImg.sprite.name : "(null)";
                bool actorEnabled  = actorImg && actorImg.enabled;
                float actorAlpha   = actorImg ? actorImg.color.a : -1f;
                string actorInside = viewport && actorTf is RectTransform
                    ? (IsInside(viewport, (RectTransform)actorTf) ? "inside" : "clipped")
                    : "(unknown)";

                // Requirement icons
                string[] reqInfo = new string[3];
                for (int i = 1; i <= 3; i++)
                {
                    var icon = card.Find($"RequirementsRow/Req_{i}/Icon")?.GetComponent<Image>();
                    var spriteName = icon && icon.sprite ? icon.sprite.name : "(null)";
                    var enabled = icon && icon.enabled;
                    reqInfo[i - 1] = $"Req_{i}={spriteName}/{(enabled ? "on" : "off")}";
                }

                Debug.Log(
                    $"[AQ LiveProbe] Card '{card.name}' title='{title}' id='{idTxt}' " +
                    $"ACTOR sprite={actorSprite} α={(actorAlpha<0?"(n/a)":actorAlpha.ToString("0.##"))} {(actorEnabled?"enabled":"disabled")} {actorInside} " +
                    $"| {string.Join(" ", reqInfo)}"
                );
            }

            Debug.Log($"[AQ LiveProbe] Cards scanned: {cardCount}. PlayMode={(Application.isPlaying ? "Yes":"No")}.");
        }

        // Returns true when the child's rect overlaps the viewport rect in the same space.
        private static bool IsInside(RectTransform viewport, RectTransform child)
        {
            if (!viewport || !child) return false;
            // Transform both rects to world and test overlap (cheap and good enough for UI masks)
            var vWorld = WorldRect(viewport);
            var cWorld = WorldRect(child);
            return vWorld.Overlaps(cWorld, true);
        }

        private static Rect WorldRect(RectTransform rt)
        {
            var corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            var min = corners[0];
            var max = corners[2];
            return new Rect(min, max - min);
        }
    }
}
#endif
