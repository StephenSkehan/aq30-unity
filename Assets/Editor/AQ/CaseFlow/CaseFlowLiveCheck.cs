// Assets/Editor/AQ/CaseFlow/CaseFlowLiveCheck.cs
// Play-mode runtime inspector for CaseFlow without compile-time coupling.
// Finds fields/properties by heuristics (case-insensitive, public or private).
// No scene changes; only logs what Unity is actually doing.

using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools
{
    public static class CaseFlowLiveCheck
    {
        [MenuItem("AQ/CaseFlow/Live Check (Play Mode)")]
        public static void LiveCheck()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning("[LIVE] Enter Play mode first, then run AQ/CaseFlow/Live Check (Play Mode).");
                return;
            }

            // --- 1) Orchestrator ---
            var orch = FindByTypeName("CaseFlowOrchestratorMB");
            if (!orch)
            {
                Debug.LogError("[LIVE] Could not find CaseFlowOrchestratorMB in the scene.");
                return;
            }

            int stepIndex = GetIntSmart(orch, "CurrentStepIndex", "StepIndex", "Step", "Index");
            int stepCount = GetIntSmart(orch, "StepCount", "TotalSteps", "Count");
            LogInfo($"Orchestrator: stepIndex={(stepIndex >= 0 ? stepIndex.ToString() : "?")}/{(stepCount > 0 ? stepCount.ToString() : "?")}");

            // If we still couldn't read, dump candidate members once to help future-proofing.
            if (stepIndex < 0 || stepCount <= 0)
                DumpLikelyMembers(orch, "orchestrator");

            // --- 2) Gates we care about by object name (your scene names) ---
            CheckGate("Minigame_Scrub", stepIndex);
            CheckGate("ResolutionRoot", stepIndex);

            Debug.Log("[LIVE] Done.");
        }

        // ---------------- helpers ----------------

        static void CheckGate(string goName, int currentStep)
        {
            var go = GameObject.Find(goName);
            if (!go)
            {
                LogWarn($"Gate: '{goName}' not found.");
                return;
            }

            var gate = go.GetComponents<Component>().FirstOrDefault(c => c && c.GetType().Name == "CaseFlowGateMB");
            if (!gate)
            {
                LogWarn($"Gate: '{goName}' has no CaseFlowGateMB.");
                return;
            }

            // Try to read config by common names first, then by heuristics
            string mode = GetEnumNameSmart(gate, "Mode", "GateMode") ?? "?";
            int reqIndex = GetIntSmart(gate, "RequiredIndex", "Index", "TargetIndex");
            bool? activeOnMatch = GetBoolSmart(gate, "ActiveOnMatch", "ActivateOnMatch", "ActiveIfMatch");

            // If anything unknown, dump a short member list to help
            if (mode == "?" || reqIndex < 0 || !activeOnMatch.HasValue)
                DumpLikelyMembers(gate, goName);

            bool active = go.activeInHierarchy;

            // Print compact reality
            var aom = activeOnMatch.HasValue ? activeOnMatch.Value.ToString() : "unknown";
            LogOk($"[{goName}] gateMode={mode} reqIndex={(reqIndex >= 0 ? reqIndex.ToString() : "?")} activeOnMatch={aom} | activeInHierarchy={active}");

            // If we could infer expectation, compare it
            if (mode.IndexOf("Index", StringComparison.OrdinalIgnoreCase) >= 0 && activeOnMatch.HasValue && reqIndex >= 0 && currentStep >= 0)
            {
                bool expect = (currentStep == reqIndex) == activeOnMatch.Value;
                if (expect != active)
                    LogWarn($"[{goName}] EXPECT active={expect} but actual active={active} (stepIndex={currentStep}).");
            }
        }

        // ---- reflection utils (public + private) ----
        static BindingFlags Flags => BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        static UnityEngine.Object FindByTypeName(string typeName)
        {
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
            foreach (var c in UnityEngine.Object.FindObjectsByType<Component>(FindObjectsSortMode.None))
                if (c && c.GetType().Name == typeName) return c;
#else
            foreach (var c in UnityEngine.Object.FindObjectsOfType<Component>())
                if (c && c.GetType().Name == typeName) return c;
#endif
            return null;
        }

        static int GetIntSmart(object obj, params string[] preferredNames)
        {
            // Try preferred names
            foreach (var n in preferredNames)
            {
                if (TryGetInt(obj, n, out var val)) return val;
            }
            // Heuristic: any int field/prop whose name contains "step" or "index" and "current"
            var t = obj.GetType();
            foreach (var p in t.GetProperties(Flags))
                if (p.PropertyType == typeof(int) && NameMatches(p.Name, "step", "index") && NameMatches(p.Name, "curr"))
                    return (int)p.GetValue(obj, null);
            foreach (var f in t.GetFields(Flags))
                if (f.FieldType == typeof(int) && NameMatches(f.Name, "step", "index") && NameMatches(f.Name, "curr"))
                    return (int)f.GetValue(obj);

            // Fallback: first int that mentions step/index
            foreach (var p in t.GetProperties(Flags))
                if (p.PropertyType == typeof(int) && NameMatches(p.Name, "step", "index"))
                    return (int)p.GetValue(obj, null);
            foreach (var f in t.GetFields(Flags))
                if (f.FieldType == typeof(int) && NameMatches(f.Name, "step", "index"))
                    return (int)f.GetValue(obj);

            return -1;
        }

        static bool TryGetInt(object obj, string name, out int value)
        {
            value = -1;
            var t = obj.GetType();
            var p = t.GetProperty(name, Flags);
            if (p != null && p.PropertyType == typeof(int)) { value = (int)p.GetValue(obj, null); return true; }
            var f = t.GetField(name, Flags);
            if (f != null && f.FieldType == typeof(int)) { value = (int)f.GetValue(obj); return true; }
            return false;
        }

        static bool? GetBoolSmart(object obj, params string[] names)
        {
            foreach (var n in names)
            {
                var t = obj.GetType();
                var p = t.GetProperty(n, Flags);
                if (p != null && p.PropertyType == typeof(bool)) return (bool)p.GetValue(obj, null);
                var f = t.GetField(n, Flags);
                if (f != null && f.FieldType == typeof(bool)) return (bool)f.GetValue(obj);
            }
            // heuristic: first bool containing "active" and "match"
            var tt = obj.GetType();
            foreach (var p in tt.GetProperties(Flags))
                if (p.PropertyType == typeof(bool) && NameMatches(p.Name, "active") && NameMatches(p.Name, "match"))
                    return (bool)p.GetValue(obj, null);
            foreach (var f in tt.GetFields(Flags))
                if (f.FieldType == typeof(bool) && NameMatches(f.Name, "active") && NameMatches(f.Name, "match"))
                    return (bool)f.GetValue(obj);
            return null;
        }

        static string GetEnumNameSmart(object obj, params string[] names)
        {
            foreach (var n in names)
            {
                var t = obj.GetType();
                var p = t.GetProperty(n, Flags);
                if (p != null && p.PropertyType.IsEnum) return p.GetValue(obj, null)?.ToString();
                var f = t.GetField(n, Flags);
                if (f != null && f.FieldType.IsEnum) return f.GetValue(obj)?.ToString();
            }
            // heuristic: any enum containing "mode"
            var tt = obj.GetType();
            foreach (var p in tt.GetProperties(Flags))
                if (p.PropertyType.IsEnum && NameMatches(p.Name, "mode")) return p.GetValue(obj, null)?.ToString();
            foreach (var f in tt.GetFields(Flags))
                if (f.FieldType.IsEnum && NameMatches(f.Name, "mode")) return f.GetValue(obj)?.ToString();
            return null;
        }

        static bool NameMatches(string name, params string[] tokens)
        {
            var lower = name.ToLowerInvariant();
            return tokens.All(t => lower.Contains(t.ToLowerInvariant()));
        }

        static void DumpLikelyMembers(object obj, string label)
        {
            var t = obj.GetType();
            Debug.Log($"[LIVE] {label}: inspecting type {t.FullName}");
            var props = t.GetProperties(Flags)
                         .Where(p => p.PropertyType == typeof(int) || p.PropertyType == typeof(bool) || p.PropertyType.IsEnum)
                         .Select(p => $"{p.PropertyType.Name} prop {p.Name}");
            var fields = t.GetFields(Flags)
                         .Where(f => f.FieldType == typeof(int) || f.FieldType == typeof(bool) || f.FieldType.IsEnum)
                         .Select(f => $"{f.FieldType.Name} field {f.Name}");
            foreach (var line in props.Concat(fields).Take(30))
                Debug.Log($"[LIVE]   {line}");
        }

        static void LogInfo(string msg) => Debug.Log($"[LIVE] {msg}");
        static void LogWarn(string msg) => Debug.LogWarning($"[LIVE] {msg}");
        static void LogOk(string msg) => Debug.Log($"[OK] {msg}");
    }
}
