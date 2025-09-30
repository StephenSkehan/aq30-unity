#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace AQ.EditorTools.UI.Leads
{
    public static class ActorsForceTopViaCanvas
    {
        [MenuItem("AQ/UI/Leads/Live/Actors: Force Top (Nested Canvas)")]
        public static void Run()
        {
            int cards=0, canvases=0;
            var rts = Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var card in rts)
            {
                if (!card.name.StartsWith("LeadCard")) continue; // works for LeadCard & LeadCard(Clone)
                cards++;

                var anchor = card.Find("ActorAnchor") as RectTransform;
                if (!anchor) continue;

                var rootCanvas = card.GetComponentInParent<Canvas>();
                if (!rootCanvas) continue;

                // create / fetch nested canvas
                Canvas actorCanvas = null;
                var existing = anchor.GetComponentsInChildren<Canvas>(true);
                foreach (var c in existing) if (c.transform.parent == anchor) { actorCanvas = c; break; }
                if (actorCanvas == null)
                {
                    var go = new GameObject("ActorCanvas", typeof(RectTransform), typeof(Canvas));
                    var rt = go.GetComponent<RectTransform>();
                    rt.SetParent(anchor, false);
                    actorCanvas = go.GetComponent<Canvas>();
                }

                actorCanvas.overrideSorting = true;
                actorCanvas.sortingLayerID = rootCanvas.sortingLayerID;
                actorCanvas.sortingOrder   = rootCanvas.sortingOrder + 10; // above the card, below popups

                // do NOT block clicks
                var gr = actorCanvas.GetComponent<GraphicRaycaster>();
                if (gr) Object.DestroyImmediate(gr);

                // ensure the Image is child of the nested canvas and visible
                var img = anchor.GetComponentInChildren<Image>(true);
                if (img == null || img.transform.parent == anchor)
                {
                    if (img == null)
                    {
                        var igo = new GameObject("Image", typeof(RectTransform), typeof(Image));
                        img = igo.GetComponent<Image>();
                    }
                    img.rectTransform.SetParent(actorCanvas.transform, false);
                }
                img.raycastTarget = false;
                var ccol = img.color; ccol.a = 1f; img.color = ccol;
                canvases++;
            }

            Debug.Log($"[AQ Actors] Cards seen={cards} | Actor canvases ready={canvases}. (Forced on top)");
        }
    }
}
#endif
