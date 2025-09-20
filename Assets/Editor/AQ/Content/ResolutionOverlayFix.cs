using UnityEngine;
using UnityEditor;

namespace AQ.EditorTools.Content
{
    public static class ResolutionOverlayFix
    {
        [MenuItem("AQ/Content/Fix Resolution Overlay")]
        public static void Fix()
        {
            var root = GameObject.Find("ResolutionRoot");
            var panel = GameObject.Find("ResolutionPanel");

            if (root == null || panel == null)
            {
                Debug.LogWarning("[ResolutionOverlayFix] Could not find ResolutionRoot/Panel in scene.");
                return;
            }

            // Ensure panel has a CanvasGroup for alpha control
            var group = panel.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = panel.AddComponent<CanvasGroup>();
                Debug.Log("[ResolutionOverlayFix] Added CanvasGroup to ResolutionPanel.");
            }

            // Default tidy: ensure active and alpha=1
            root.SetActive(true);
            panel.SetActive(true);
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;

            Debug.Log("[ResolutionOverlayFix] Resolution overlay reset to visible/interactive.");
        }
    }
}
