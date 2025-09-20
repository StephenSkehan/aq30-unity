#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AQ.EditorTools.Variants
{
    /// <summary>
    /// One-click default variant setter.
    /// Scans assets + loaded scenes for configs/MBs with fields/properties named:
    ///   DefaultVariant, ActiveVariant, SelectedVariant, Variant
    /// and sets them to "A"/"B"/"C" (string) or an enum/int value (0/1/2) as needed.
    ///
    /// Supports:
    /// - ScriptableObject assets anywhere in the project.
    /// - Scene MonoBehaviours (even on inactive objects).
    /// - Fields or properties (public/private) of type enum/string/int.
    ///
    /// Marks changed assets/scene(s) dirty and prints a compact audit log.
    /// </summary>
    public static class VariantDefaultSetter
    {
        private static readonly string[] CandidateMemberNames = {
            "DefaultVariant","ActiveVariant","SelectedVariant","Variant"
        };

        [MenuItem("AQ/Variants/Set Default Variant/A")]
        public static void SetA() => SetDefaultVariant("A");

        [MenuItem("AQ/Variants/Set Default Variant/B")]
        public static void SetB() => SetDefaultVariant("B");

        [MenuItem("AQ/Variants/Set Default Variant/C")]
        public static void SetC() => SetDefaultVariant("C");

        [MenuItem("AQ/Variants/Report Current Variant Targets")]
        public static void Report() => ReportTargets();

        // ---------------- Core ----------------

        private static void SetDefaultVariant(string letter)
        {
            var changes = new List<string>();
            int assetTouched = 0, sceneTouched = 0;

            // 1) Assets (ScriptableObjects in Project)
            foreach (var obj in FindAllScriptableObjectAssets())
            {
                if (TrySetOnObject(obj, letter, out string where))
                {
                    changes.Add($"[ASSET] {where}");
                    EditorUtility.SetDirty(obj);
                    assetTouched++;
                }
            }

            // 2) Scene components (MonoBehaviours)
            foreach (var comp in FindAllSceneComponents())
            {
                if (TrySetOnObject(comp, letter, out string where))
                {
                    changes.Add($"[SCENE] {where}");
                    var s = comp.gameObject.scene;
                    if (s.IsValid() && s.isLoaded) EditorSceneManager.MarkSceneDirty(s);
                    sceneTouched++;
                }
            }

            if (assetTouched > 0) AssetDatabase.SaveAssets();

            var msg =
                $"[VariantDefaultSetter] Set default variant → {letter}\n" +
                $"  Assets touched: {assetTouched}\n" +
                $"  Scene objects touched: {sceneTouched}\n" +
                (changes.Count > 0 ? "  Changes:\n    - " + string.Join("\n    - ", changes) : "  (No matching targets found)");

            Debug.Log(msg);
            if (changes.Count == 0)
            {
                EditorUtility.DisplayDialog("Set Default Variant",
                    $"Tried to set default variant to {letter}, but did not find any matching config fields.\n\n" +
                    "Looked for members named DefaultVariant / ActiveVariant / SelectedVariant / Variant in ScriptableObject assets and scene objects.",
                    "OK");
            }
        }

        private static void ReportTargets()
        {
            var hits = new List<string>();

            foreach (var obj in FindAllScriptableObjectAssets())
            {
                var desc = DescribeMatchingMembers(obj);
                if (!string.IsNullOrEmpty(desc)) hits.Add($"[ASSET] {ObjectPath(obj)}  →  {desc}");
            }
            foreach (var comp in FindAllSceneComponents())
            {
                var desc = DescribeMatchingMembers(comp);
                if (!string.IsNullOrEmpty(desc))
                    hits.Add($"[SCENE] {comp.GetType().FullName} on {GetHierarchyPath(comp.transform)}  →  {desc}");
            }

            if (hits.Count == 0)
            {
                Debug.Log("[VariantDefaultSetter] No candidate targets found in assets or loaded scenes.");
            }
            else
            {
                Debug.Log("[VariantDefaultSetter] Candidate targets:\n - " + string.Join("\n - ", hits));
            }
        }

        // ---------------- Reflection helpers ----------------

        private static bool TrySetOnObject(UnityEngine.Object obj, string letter, out string where)
        {
            where = null;
            var t = obj.GetType();
            var members = EnumerateCandidateMembers(t).ToList();
            if (members.Count == 0) return false;

            bool anyChanged = false;
            foreach (var m in members)
            {
                if (TrySetMemberValue(obj, m, letter, out string change))
                {
                    anyChanged = true;
                    if (where == null) where = $"{t.FullName} @ {LocationOf(obj)}";
                }
            }
            return anyChanged;
        }

        private static IEnumerable<MemberInfo> EnumerateCandidateMembers(Type t)
        {
            const BindingFlags BF = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (var name in CandidateMemberNames)
            {
                foreach (var f in t.GetFields(BF).Where(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    yield return f;
                foreach (var p in t.GetProperties(BF).Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    yield return p;
            }
        }

        private static bool TrySetMemberValue(UnityEngine.Object obj, MemberInfo m, string letter, out string change)
        {
            change = null;
            Type valueType = null;
            object currentValue = null;
            bool canWrite = false;

            switch (m)
            {
                case FieldInfo fi:
                    valueType = fi.FieldType;
                    currentValue = fi.GetValue(obj);
                    canWrite = true;
                    break;
                case PropertyInfo pi:
                    valueType = pi.PropertyType;
                    canWrite = pi.CanWrite;
                    if (pi.CanRead) currentValue = SafeGetProperty(pi, obj);
                    break;
                default:
                    return false;
            }
            if (!canWrite) return false;

            object newValue;
            if (valueType.IsEnum)
            {
                // Map "A"/"B"/"C" to enum by name; fallback to 0/1/2.
                var names = Enum.GetNames(valueType);
                var match = names.FirstOrDefault(n => string.Equals(n, letter, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    newValue = Enum.Parse(valueType, match);
                }
                else
                {
                    int idx = letter switch { "A" => 0, "B" => 1, "C" => 2, _ => 0 };
                    newValue = Enum.ToObject(valueType, idx);
                }
            }
            else if (valueType == typeof(string))
            {
                newValue = letter;
            }
            else if (valueType == typeof(int))
            {
                newValue = letter switch { "A" => 0, "B" => 1, "C" => 2, _ => 0 };
            }
            else
            {
                return false; // unsupported type
            }

            if (Equals(currentValue, newValue)) return false;

            switch (m)
            {
                case FieldInfo fi:
                    fi.SetValue(obj, newValue);
                    break;
                case PropertyInfo pi:
                    SafeSetProperty(pi, obj, newValue);
                    break;
            }

            change = $"{m.DeclaringType.FullName}.{m.Name} = {ValueToString(newValue)}";
            return true;
        }

        private static object SafeGetProperty(PropertyInfo pi, object target)
        {
            try { return pi.GetValue(target); }
            catch { return null; }
        }

        private static void SafeSetProperty(PropertyInfo pi, object target, object value)
        {
            try { pi.SetValue(target, value); }
            catch { /* ignored */ }
        }

        private static string ValueToString(object v)
        {
            if (v == null) return "null";
            return v is string s ? $"\"{s}\"" : v.ToString();
        }

        /// <summary>
        /// Builds a compact description of candidate members and their current values on an object.
        /// Used by Report() for a readable audit.
        /// </summary>
        private static string DescribeMatchingMembers(UnityEngine.Object obj)
        {
            var t = obj.GetType();
            var members = EnumerateCandidateMembers(t).ToArray();
            if (members.Length == 0) return null;

            var pairs = new List<string>();
            foreach (var m in members)
            {
                object val = null;
                Type vt = null;
                switch (m)
                {
                    case FieldInfo fi:
                        vt = fi.FieldType;
                        val = fi.GetValue(obj);
                        break;
                    case PropertyInfo pi:
                        vt = pi.PropertyType;
                        val = pi.CanRead ? SafeGetProperty(pi, obj) : null;
                        break;
                }
                var vstr = val == null ? "null" : (val is string s ? $"\"{s}\"" : val.ToString());
                pairs.Add($"{m.Name}:{vt?.Name}={vstr}");
            }
            return string.Join(", ", pairs);
        }

        // ---------------- Discovery helpers ----------------

        private static IEnumerable<ScriptableObject> FindAllScriptableObjectAssets()
        {
            var guids = AssetDatabase.FindAssets("t:ScriptableObject");
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (obj != null) yield return obj;
            }
        }

        private static IEnumerable<MonoBehaviour> FindAllSceneComponents()
        {
            // Includes inactive; excludes Project assets.
            return Resources.FindObjectsOfTypeAll<MonoBehaviour>()
                .Where(mb => mb != null && mb.gameObject.scene.IsValid() && mb.gameObject.scene.isLoaded);
        }

        // ---------------- Pretty paths ----------------

        private static string LocationOf(UnityEngine.Object obj)
        {
            if (obj is Component c && c.gameObject.scene.IsValid())
                return $"{c.gameObject.scene.name}/{GetHierarchyPath(c.transform)}";
            var path = AssetDatabase.GetAssetPath(obj);
            return string.IsNullOrEmpty(path) ? obj.name : path;
        }

        private static string ObjectPath(UnityEngine.Object obj)
        {
            var p = AssetDatabase.GetAssetPath(obj);
            return string.IsNullOrEmpty(p) ? obj.name : p;
        }

        private static string GetHierarchyPath(Transform t)
        {
            var parts = new List<string>();
            while (t != null) { parts.Add(t.name); t = t.parent; }
            parts.Reverse();
            return string.Join("/", parts);
        }
    }
}
#endif
