#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AQ.EditorTools.Variants
{
    /// <summary>
    /// Editor menus to Apply Variants A/B/C and run overlay repairs.
    /// Supports binders that expose either:
    ///   - public static void Apply()            // preferred
    ///   - public static void Apply(bool edit)   // edit=true when not playing
    ///   - public void Apply()                   // instance (with or without default ctor)
    ///   - public void Apply(bool edit)
    /// </summary>
    public static class VariantMenus
    {
        private const string BinderA = "AQ.EditorTools.Variants.WK3_2_BindA_ToOverlay";
        private const string BinderB = "AQ.EditorTools.Variants.WK3_2_BindB_ToOverlay";
        private const string BinderC = "AQ.EditorTools.Variants.WK3_2_BindC_ToOverlay";

        // ---------------- Variant Apply ----------------

        [MenuItem("AQ/Variants/Apply Variant A %&F1")] // Ctrl/Cmd+Alt+F1
        public static void ApplyA() => ApplyVariant(BinderA, "A");

        [MenuItem("AQ/Variants/Apply Variant B %&F2")]
        public static void ApplyB() => ApplyVariant(BinderB, "B");

        [MenuItem("AQ/Variants/Apply Variant C %&F3")]
        public static void ApplyC() => ApplyVariant(BinderC, "C");

        private static void ApplyVariant(string binderTypeName, string label)
        {
            if (!TryInvokeBinderApply(binderTypeName, out var error, out var pathUsed))
            {
                Debug.LogError($"[Variants] Failed to apply Variant {label}: {error}");
                return;
            }

            if (!Application.isPlaying)
            {
                // Persist and mark dirty for all open scenes
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var s = SceneManager.GetSceneAt(i);
                    if (s.IsValid() && s.isLoaded) EditorSceneManager.MarkSceneDirty(s);
                }
                AssetDatabase.SaveAssets();
                Debug.Log($"[Variants] Applied Variant {label} via {pathUsed} (Edit Mode; scene(s) marked dirty).");
            }
            else
            {
                Debug.LogWarning($"[Variants] Applied Variant {label} via {pathUsed} in PLAY MODE. Changes are transient.");
            }
        }

        /// <summary>
        /// Attempts several invocation strategies, in order:
        ///  1) public static Apply() or Apply(bool editMode)
        ///  2) instance Apply() / Apply(bool) via:
        ///     - parameterless ctor, or
        ///     - ScriptableObject.CreateInstance if derived, or
        ///     - FormatterServices.GetUninitializedObject (editor-only last resort)
        /// </summary>
        private static bool TryInvokeBinderApply(string typeName, out string error, out string pathUsed)
        {
            pathUsed = "unknown";
            try
            {
                var t = ResolveType(typeName);
                if (t == null)
                {
                    error = $"Binder type not found: {typeName}";
                    return false;
                }

                bool edit = !Application.isPlaying;

                // 1) STATIC Apply
                var staticApplyNoArg = t.GetMethod("Apply", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                if (staticApplyNoArg != null)
                {
                    staticApplyNoArg.Invoke(null, null);
                    error = null; pathUsed = "static Apply()";
                    return true;
                }

                var staticApplyBool = t.GetMethod("Apply", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(bool) }, null);
                if (staticApplyBool != null)
                {
                    staticApplyBool.Invoke(null, new object[] { edit });
                    error = null; pathUsed = "static Apply(bool)";
                    return true;
                }

                // 2) INSTANCE Apply
                // Find instance method first (so we can decide what ctor strategy to use)
                MethodInfo instApply = t.GetMethod("Apply", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null)
                                     ?? t.GetMethod("Apply", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(bool) }, null);

                if (instApply == null)
                {
                    error = $"No Apply() method found on {typeName} (checked static/instance signatures).";
                    return false;
                }

                object instance = null;
                string ctorPath  = null;

                // Try parameterless public/protected/private ctor first
                var pwless = t.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                              .FirstOrDefault(c => c.GetParameters().Length == 0);
                if (pwless != null)
                {
                    instance = pwless.Invoke(null);
                    ctorPath = "parameterless ctor";
                }
                else if (typeof(ScriptableObject).IsAssignableFrom(t))
                {
                    instance = ScriptableObject.CreateInstance(t);
                    ctorPath = "ScriptableObject.CreateInstance";
                }
                else
                {
                    // Last resort for editor tool objects with no convenient ctor
                    instance = FormatterServices.GetUninitializedObject(t);
                    ctorPath = "FormatterServices.GetUninitializedObject";
                }

                if (instApply.GetParameters().Length == 0)
                {
                    instApply.Invoke(instance, null);
                    error = null; pathUsed = $"instance Apply() via {ctorPath}";
                    return true;
                }
                else
                {
                    instApply.Invoke(instance, new object[] { edit });
                    error = null; pathUsed = $"instance Apply(bool) via {ctorPath}";
                    return true;
                }
            }
            catch (TargetInvocationException tex)
            {
                error = tex.InnerException != null ? tex.InnerException.Message : tex.Message;
                return false;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static Type ResolveType(string fullName)
        {
            // Try fast path first
            var t = Type.GetType(fullName);
            if (t != null) return t;

            // Search all loaded assemblies (Editor + Domain Reload safe)
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    t = a.GetType(fullName);
                    if (t != null) return t;
                }
                catch { /* ignore reflection load errors */ }
            }
            return null;
        }

        // ---------------- Utilities ----------------

        [MenuItem("AQ/Variants/Fix All Overlay %#&O")] // Shift+Ctrl/Cmd+Alt+O
        public static void FixAllOverlay()
        {
            int fixedCount = 0;
            var roots = Resources.FindObjectsOfTypeAll<Transform>()
                .Where(t => t && t.name == "ResolutionRoot" && t.gameObject.scene.IsValid() && t.gameObject.scene.isLoaded);

            foreach (var tr in roots)
            {
                var go = tr.gameObject;

                // Ensure Canvas + Raycaster + Scaler + Group
                EnsureComponent<Canvas>(go, c => { c.renderMode = RenderMode.ScreenSpaceOverlay; });
                EnsureComponent<UnityEngine.UI.GraphicRaycaster>(go, _ => { });
                EnsureComponent<UnityEngine.UI.CanvasScaler>(go, cs =>
                {
                    cs.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    cs.referenceResolution = new Vector2(1080, 1920);
                });
                EnsureComponent<CanvasGroup>(go, cg => { cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true; });

                // Ensure required children exist
                var panel = EnsureChild(go.transform, "ResolutionPanel", out _);
                EnsureChild(panel, "TitleText", out _);
                EnsureChild(panel, "BodyText", out _);
                var btn = EnsureChild(panel, "ResolveButton", out _);
                EnsureChild(btn, "Text", out _);
                EnsureChild(panel, "Quest_0", out _);
                EnsureChild(panel, "Quest_1", out _);
                EnsureChild(panel, "Quest_2", out _);

                fixedCount++;
            }

            if (!Application.isPlaying)
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var s = SceneManager.GetSceneAt(i);
                    if (s.IsValid() && s.isLoaded) EditorSceneManager.MarkSceneDirty(s);
                }
            }

            Debug.Log($"[Variants/Fix] Normalized {fixedCount} ResolutionRoot instance(s).");
        }

        private static Transform EnsureChild(Transform parent, string name, out bool created)
        {
            var child = parent.Find(name);
            if (child == null)
            {
                var go = new GameObject(name, typeof(RectTransform));
                child = go.transform;
                child.SetParent(parent, false);
                created = true;
            }
            else created = false;
            return child;
        }

        private static T EnsureComponent<T>(GameObject go, Action<T> init) where T : Component
        {
            var c = go.GetComponent<T>();
            if (!c) c = go.AddComponent<T>();
            init?.Invoke(c);
            return c;
        }
    }
}
#endif
