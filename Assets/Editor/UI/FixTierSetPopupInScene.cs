#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.Editor.UI
{
    public static class FixTierSetPopupInScene
    {
        [MenuItem("AQ/UI/Fix Tier Popup in Scene")]
        public static void Fix()
        {
            var popupGo = GameObject.Find("TierSetPopup");
            if (!popupGo)
            {
                Debug.LogError("[TierPopupFix] No GameObject named 'TierSetPopup' found in scene.");
                return;
            }

            // purge missing scripts on root and children
            foreach (var t in popupGo.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);

            var popup = popupGo.GetComponent<AQ.UI.Leads.TierSetPopup>();
            if (!popup) popup = popupGo.AddComponent<AQ.UI.Leads.TierSetPopup>();

            // wire by common names
            popup.canvasGroup = popupGo.GetComponent<CanvasGroup>();
            popup.blocker     = Find<Image>(popupGo, "Blocker", "Backdrop", "Dim", "Overlay");
            popup.panelRoot   = Find<RectTransform>(popupGo, "Panel", "Root", "Window");
            popup.titleText   = Find<TMP_Text>(popupGo, "Title", "Text_Title");
            popup.gridRoot    = Find<RectTransform>(popupGo, "Grid", "Content", "Body");
            var closeBtn      = Find<Button>(popupGo, "Btn_Close", "Close");
            popup.closeButton = closeBtn;

            popup.HideImmediate();
            EditorUtility.SetDirty(popup);
            Debug.Log("[TierPopupFix] TierSetPopup repaired and hidden.");
        }

        private static T Find<T>(GameObject root, params string[] names) where T : Component
        {
            var all = root.GetComponentsInChildren<T>(true);
            return all.FirstOrDefault(c =>
            {
                var n = c.name.ToLowerInvariant();
                return names.Any(k => n.Contains(k.ToLowerInvariant()));
            });
        }
    }
}
#endif
