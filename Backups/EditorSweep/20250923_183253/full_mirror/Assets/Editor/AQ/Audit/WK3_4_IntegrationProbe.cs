#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools.Audit
{
    public static class WK3_4_IntegrationProbe
    {
        [MenuItem("AQ/Audit/WK3-4/Integration Probe")]
        public static void Probe()
        {
            Debug.Log("=== WK3-4 Integration Probe ===");

            // -------- Discover types by full/short name (reflection) --------
            var walletSvcType   = FindType("AQ.SharedKernel.Economy.WalletService");
            var iWalletType     = FindType("AQ.SharedKernel.Economy.IWallet");
            var rewardType      = FindType("AQ.SharedKernel.Economy.Reward");
            var walletLocType   = FindType("AQ.App.Economy.WalletLocator");
            var walletBridgeType= FindType("AQ.App.Analytics.WalletAnalyticsBridge");

            var saveDriverType  = FindType("SaveLoadDriver");      // Assets/App/Save/SaveLoadDriver.cs
            var jsonSaveType    = FindType("JsonSaveService");     // Assets/App/Save/JsonSaveService.cs

            var iAnalyticsType  = FindType("AQ.App.Analytics.IAnalytics");
            var dbgAnalyticsType= FindType("AQ.App.Analytics.DebugLogAnalytics");

            LogType(walletSvcType,    "WalletService");
            LogType(iWalletType,      "IWallet");
            LogType(rewardType,       "Reward");
            LogType(walletLocType,    "WalletLocator");
            LogType(walletBridgeType, "WalletAnalyticsBridge");

            LogType(saveDriverType,   "SaveLoadDriver");
            LogType(jsonSaveType,     "JsonSaveService");

            LogType(iAnalyticsType,   "IAnalytics");
            LogType(dbgAnalyticsType, "DebugLogAnalytics");

            // -------- Scene instance checks (safe casts + null checks) --------
            // SaveLoadDriver instance
            string saveDriverName = "none";
            if (saveDriverType != null)
            {
                var obj = FindFirstObjectByType(saveDriverType);
                saveDriverName = obj != null ? obj.name : "none";
            }
            Debug.Log($"[Scene] SaveLoadDriver instance: {saveDriverName}");

            // WalletAnalyticsBridge instance
            string bridgeName = "none";
            if (walletBridgeType != null)
            {
                var obj = FindFirstObjectByType(walletBridgeType);
                bridgeName = obj != null ? obj.name : "none";
            }
            Debug.Log($"[Scene] WalletAnalyticsBridge instance: {bridgeName}");

            // Any IAnalytics implementation present
            string analyticsName = "none";
            if (iAnalyticsType != null)
            {
                var anyAnalytics = FindAllMonoBehaviours(true)
                    .FirstOrDefault(m => iAnalyticsType.IsAssignableFrom(m.GetType()));
                analyticsName = anyAnalytics != null ? anyAnalytics.name : "none";
            }
            Debug.Log($"[Scene] IAnalytics instance: {analyticsName}");

            // -------- Recommendation summary --------
            var canStrongWallet    = (walletLocType != null && rewardType != null) || walletSvcType != null || iWalletType != null;
            var canStrongSave      = saveDriverType != null || jsonSaveType != null;
            var canStrongAnalytics = iAnalyticsType != null;

            Debug.Log($"[Recommend] Wallet: {(canStrongWallet ? "Strong-typed grant via Reward.Soft/Energy + WalletLocator/WalletService" : "Reflection")}");
            Debug.Log($"[Recommend] Save:   {(canStrongSave   ? "Strong-typed SaveLoadDriver.Save()" : "Reflection")}");
            Debug.Log($"[Recommend] Anlytcs:{(canStrongAnalytics? "Strong-typed IAnalytics.LogEvent" : "Reflection")}");

            Debug.Log("=== WK3-4 Integration Probe (done) ===");
        }

        // ---------- Helpers ----------

        static Type FindType(string fullOrShort)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch { continue; }

                var hit = types.FirstOrDefault(t =>
                    t.FullName == fullOrShort ||
                    t.Name == fullOrShort ||
                    (t.FullName != null && t.FullName.EndsWith("." + fullOrShort, StringComparison.Ordinal)));
                if (hit != null) return hit;
            }
            return null;
        }

        static void LogType(Type t, string label)
        {
            if (t != null) Debug.Log($"[Type] {label}: {t.FullName}");
            else           Debug.Log($"[Type] {label}: (not found)");
        }

        static UnityEngine.Object FindFirstObjectByType(Type type)
        {
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
            // Non-generic overload returns UnityEngine.Object
            return UnityEngine.Object.FindFirstObjectByType(
                type,
                FindObjectsInactive.Include
            );
#else
            // Legacy fallback: slow path
            return UnityEngine.Object.FindObjectsOfType(type, true).FirstOrDefault() as UnityEngine.Object;
#endif
        }

        static MonoBehaviour[] FindAllMonoBehaviours(bool includeInactive)
        {
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
            return UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
#else
            return UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(includeInactive);
#endif
        }
    }
}
#endif
