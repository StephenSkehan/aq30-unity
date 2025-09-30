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
            var all = GameObject.FindObjectsOfType<RectTransform>(true);
            int cards = 0, nudged = 0;
            foreach (var rt in all)
            {
                // Works for LeadCard and LeadCard(Clone)
                if (!rt.name.StartsWith("LeadCard")) continue;
                cards++;

                var anchorT = rt.Find("ActorAnchor");
                if (!anchorT) continue;

                var imageT = anchorT.Find("Image") as RectTransform;
                if (!imageT)
                {
                    var go = new GameObject("Image", typeof(RectTransform), typeof(Image));
                    imageT = go.GetComponent<RectTransform>();
                    imageT.SetParent(anchorT, false);
                    var img = go.GetComponent<Image>();
                    img.raycastTarget = false;
                }

                // Ensure the portrait sits *inside* the card: top-anchored => down is negative Y.
                var pos = imageT.anchoredPosition;
                if (pos.y != -18f) { pos.y = -18f; imageT.anchoredPosition = pos; nudged++; }

                // Make sure it renders above the header/background.
                imageT.SetAsLastSibling();

                // Visible & opaque
                var image = imageT.GetComponent<Image>();
                if (image)
                {
                    var c = image.color; c.a = 1f; image.color = c;
                    image.enabled = true;
                }
            }

            Debug.Log($"[AQ Actors] Cards seen={cards} | Actors nudged to y=-18 and brought to front={nudged}.");
        }
    }
}
#endif
