#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools.Leads
{
    using AQ.App.Leads;

    public static class LeadsVerifier
    {
        [MenuItem("AQ/Leads/Verify Lead Assets")]
        public static void Verify()
        {
            var guids = AssetDatabase.FindAssets("t:LeadCardSO");
            int ok = 0, fail = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var so = AssetDatabase.LoadAssetAtPath<LeadCardSO>(path);
                if (so == null) continue;

                bool hasId = !string.IsNullOrWhiteSpace(so.LeadCardId);
                bool reqOk = so.Requirements != null && so.Requirements.Length <= 3 && so.Requirements.All(r => !string.IsNullOrWhiteSpace(r.ItemId));
                bool titleOk = !string.IsNullOrWhiteSpace(so.Title);

                if (hasId && titleOk && reqOk)
                {
                    ok++;
                    Debug.Log($"[LeadsVerifier] PASS {so.LeadCardId} @ {path}");
                }
                else
                {
                    fail++;
                    Debug.LogError($"[LeadsVerifier] FAIL {so?.LeadCardId ?? "(null)"} @ {path} :: id:{hasId} title:{titleOk} reqs(<=3 & ids):{reqOk}");
                }
            }

            Debug.Log($"[LeadsVerifier] Summary: PASS={ok} FAIL={fail} Total={guids.Length}");
        }
    }
}
#endif
