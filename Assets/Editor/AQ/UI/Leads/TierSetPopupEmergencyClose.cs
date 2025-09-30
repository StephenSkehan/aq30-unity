#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools.UI.Leads
{
    public static class TierSetPopupEmergencyClose
    {
        [MenuItem("AQ/UI/Leads/Popups/Force Close All (Scene)")]
        public static void Run()
        {
            var rts = GameObject.FindObjectsOfType<RectTransform>(true);
            int closed = 0;
            foreach (var rt in rts)
            {
                if (rt.name != "TierSetPopup") continue;
                var go = rt.gameObject;
                var cg = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
                go.SetActive(false);
                closed++;
            }
            Debug.Log($"[AQ Popups] Force closed {closed} TierSetPopup instance(s).");
        }
    }
}
#endif
