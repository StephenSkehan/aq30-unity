#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.UI.Leads
{
    /// <summary>
    /// Scene-only: for each card under Content_Leads,
    /// - Move ActorAnchor to last sibling (top-most in draw order)
    /// - Ensure ActorAnchor/Image exists, maskable, default UI material, alpha=1, enabled
    /// - For Req_1/2/3/Icon: move to last within slot, maskable, default UI material, enabled
    /// Does not touch prefabs or assets.
    /// </summary>
    public static class BringLiveLeadVisualsToFront
    {
        [MenuItem("AQ/UI/Leads/Live Tools/Bring Actor & Req Icons To Front")]
        public static void Run()
        {
            var content = GameObject.Find("Content_Leads")?.transform;
            if (!content) { Debug.LogWarning("[AQ Front] Content_Leads not found."); return; }

            int cards = 0, actors = 0, icons = 0;

            foreach (Transform card in content)
            {
                bool looksLikeCard = card.Find("Text_Title") || card.Find("RequirementsRow");
                if (!looksLikeCard) continue;
                cards++;

                // Actor
                var anchor = card.Find("ActorAnchor");
                if (anchor)
                {
                    anchor.SetAsLastSibling(); // on top vs other card children
                    var img = anchor.Find("Image")?.GetComponent<Image>();
                    if (!img)
                    {
                        var go = new GameObject("Image", typeof(RectTransform), typeof(Image));
                        var rt = go.GetComponent<RectTransform>();
                        rt.SetParent(anchor, false);
                        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                        rt.pivot = new Vector2(0.5f, 0.5f);
                        rt.sizeDelta = new Vector2(96, 96);
                        img = go.GetComponent<Image>();
                    }
                    img.maskable = true;
                    img.material = null; // default UI material
                    var c = img.color; c.a = 1f; img.color = c;
                    img.enabled = true;
                    img.raycastTarget = false;
                    actors++;
                }

                // Requirements icons
                var row = card.Find("RequirementsRow");
                if (!row) continue;
                for (int i = 1; i <= 3; i++)
                {
                    var slot = row.Find($"Req_{i}");
                    if (!slot) continue;
                    var icon = slot.Find("Icon")?.GetComponent<Image>();
                    if (!icon) continue;
                    slot.SetAsLastSibling();           // slot above other siblings in row
                    icon.transform.SetAsLastSibling(); // icon above tick etc.
                    icon.maskable = true;
                    icon.material = null;
                    icon.enabled = true;
                    icon.raycastTarget = false;
                    icons++;
                }
            }

            Debug.Log($"[AQ Front] Cards={cards} | Actors adjusted={actors} | Req icons adjusted={icons}.");
        }
    }
}
#endif
