#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.UI.Leads
{
    public static class ActorsNudgeInsideMask
    {
        [MenuItem("AQ/UI/Leads/Live/Nudge Actors Inside Mask (-18)")]
        public static void Run()
        {
            // Unity 6000+: use FindObjectsByType (include inactive)
            var all = Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            int cards = 0, nudged = 0;
            foreach (var rt in all)
            {
                if (rt == null) continue;
                var go = rt.gameObject;
                if (go == null || !go.name.Contains("LeadCard")) continue; // heuristic for card roots
                cards++;

                // Find "Actors" child under the card
                RectTransform actorsT = null;
                foreach (Transform child in rt)
                {
                    if (child is RectTransform crt && child.name.Contains("Actors"))
                    {
                        actorsT = crt;
                        break;
                    }
                }
                if (!actorsT) continue;

                // Nudge Y inside mask and bring to front
                var ap = actorsT.anchoredPosition;
                ap.y = -18f;
                actorsT.anchoredPosition = ap;
                actorsT.SetAsLastSibling();
                nudged++;

                // Ensure images are visible
                var img = actorsT.GetComponent<Image>();
                if (img)
                {
                    var c = img.color; c.a = 1f; img.color = c;
                    img.enabled = true;
                }
            }

            Debug.Log($"[AQ Actors] Cards seen={cards} | Actors nudged to y=-18 and brought to front={nudged}.");
        }
    }
}
#endif
