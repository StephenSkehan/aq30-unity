using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.UI.Leads
{
    public static class ActorsForceOverlay
    {
        // Your preferred defaults
        const float POS_X = -120f;
        const float POS_Y = -18f;
        const float SIZE  = 132f;
        const int   SORT_ORDER = 50;

        [MenuItem("AQ/UI/Leads/Actors → Force Overlay (−120,−18 • 132x132)")]
        public static void Run()
        {
            int adjusted = 0, addedImages = 0, canvases = 0;

            foreach (var rt in Resources.FindObjectsOfTypeAll<RectTransform>())
            {
                if (!rt || rt.gameObject.name != "ActorAnchor") continue;
                if (!rt.gameObject.scene.IsValid()) continue;           // skip prefabs
                if (!rt.gameObject.activeInHierarchy) continue;

                // Ensure an Image exists
                var img = rt.GetComponent<Image>();
                if (!img) { img = rt.gameObject.AddComponent<Image>(); addedImages++; }
                img.raycastTarget = false;
                img.maskable = false;   // ignore RectMask2D

                // Top-center anchor; float slightly above card, offset left
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.sizeDelta = new Vector2(SIZE, SIZE);
                rt.anchoredPosition = new Vector2(POS_X, POS_Y);

                // Own overlay canvas so it renders above Status row
                var cv = rt.GetComponent<Canvas>();
                if (!cv) { cv = rt.gameObject.AddComponent<Canvas>(); canvases++; }
                cv.overrideSorting = true;
                cv.sortingLayerID = SortingLayer.NameToID("UI");
                cv.sortingOrder = SORT_ORDER;

                // Keep layout from pushing it around
                var le = rt.GetComponent<LayoutElement>();
                if (!le) le = rt.gameObject.AddComponent<LayoutElement>();
                le.ignoreLayout = true;

                rt.SetAsLastSibling(); // make sure it draws on top within the card
                adjusted++;
            }

            Debug.Log($"[AQ ActorsForceOverlay] Anchors adjusted={adjusted} imagesAdded={addedImages} overlayCanvases={canvases}. (Play-mode safe)");
        }
    }
}
