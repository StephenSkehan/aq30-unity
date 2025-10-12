#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.UI.Leads
{
    public static class TierSetPopupEmergencyClose
    {
        [MenuItem("AQ/UI/Leads/TierSetPopup/Emergency Close All")]
        public static void CloseAll()
        {
            // Unity 6000+: use FindObjectsByType (include inactive, unsorted for speed)
            var all = Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            int seen = 0, closed = 0;
            foreach (var rt in all)
            {
                var go = rt.gameObject;
                if (!go) continue;

                // Heuristic: by component name or object name contains "TierSetPopup"
                bool looksLikePopup = go.name.Contains("TierSetPopup");
                if (!looksLikePopup)
                {
                    // Try to detect a component literally named "TierSetPopup" without compile-time dependency
                    var comps = go.GetComponents<MonoBehaviour>();
                    foreach (var c in comps)
                    {
                        if (c && c.GetType().Name == "TierSetPopup") { looksLikePopup = true; break; }
                    }
                }

                if (!looksLikePopup) continue;
                seen++;

                // Prefer calling a "Close" method if present
                bool closedViaMethod = false;
                var scripts = go.GetComponents<MonoBehaviour>();
                foreach (var s in scripts)
                {
                    if (!s) continue;
                    var m = s.GetType().GetMethod("Close", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    if (m != null)
                    {
                        m.Invoke(s, null);
                        closedViaMethod = true;
                        break;
                    }
                }

                if (!closedViaMethod)
                {
                    // Fallback: deactivate
                    go.SetActive(false);
                }

                // Ensure any Image is fully opaque (so re-open looks correct later)
                var img = go.GetComponent<Image>();
                if (img)
                {
                    var c = img.color; c.a = 1f; img.color = c;
                }

                closed++;
            }

            Debug.Log($"[AQ TierSetPopup] Popups seen={seen}, closed={closed}.");
        }
    }
}
#endif
