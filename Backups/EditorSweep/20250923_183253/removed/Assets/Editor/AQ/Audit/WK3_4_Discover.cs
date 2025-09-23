#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools.Audit
{
    public static class WK3_4_Discover
    {
        [MenuItem("AQ/Audit/WK3-4/Discover Rewards/Analytics APIs")]
        public static void Discover()
        {
            Debug.Log("=== WK3-4 Discovery: starting ===");

            // Probe likely types by name so we don’t depend on namespaces
            DumpType("IWallet");
            DumpType("WalletService");
            DumpType("Reward");
            DumpType("JsonSaveService");
            DumpType("SaveLoadDriver");
            DumpType("IAnalytics");
            DumpType("DebugLogAnalytics");
            DumpType("WalletAnalyticsBridge");
            DumpType("CaseFlowOrchestratorMB");
            DumpType("CaseFlowTypes");
            DumpType("ICaseFlowService");
            DumpType("InMemoryCaseFlowService");

            Debug.Log("=== WK3-4 Discovery: done ===");
        }

        static void DumpType(string typeName)
        {
            var t = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => SafeTypes(a))
                .FirstOrDefault(x => x.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase) ||
                                     x.FullName?.EndsWith("." + typeName, StringComparison.OrdinalIgnoreCase) == true);

            if (t == null) { Debug.Log($"[Discover] Type not found: {typeName}"); return; }

            Debug.Log($"[Discover] Type: {t.FullName}");
            foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                var pars = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                Debug.Log($" - {m.ReturnType.Name} {m.Name}({pars})");
            }
            foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                Debug.Log($" - prop {p.PropertyType.Name} {p.Name}");
            foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                Debug.Log($" - field {f.FieldType.Name} {f.Name}");
        }

        static Type[] SafeTypes(Assembly a)
        {
            try { return a.GetTypes(); } catch { return Array.Empty<Type>(); }
        }
    }
}
#endif
