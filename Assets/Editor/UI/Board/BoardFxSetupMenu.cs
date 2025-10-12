// Assets/Editor/UI/Board/BoardFxSetupMenu.cs
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AQ.Editor.UI.Board
{
    /// <summary>
    /// One-click FX setup without compile-time references to AQ.App.
    /// Works across asmdefs by resolving types at runtime.
    /// </summary>
    public static class BoardFxSetupMenu
    {
        // Fully-qualified type names from your runtime:
        const string T_MergeBoardController = "AQ.App.UI.Board.MergeBoardController";
        const string T_BoardFxPlayer        = "AQ.App.UI.Board.BoardFxPlayer";
        const string T_BoardFxObserver      = "AQ.App.UI.Board.BoardFxObserver";
        const string T_BoardFxConfigSO      = "AQ.App.UI.Board.BoardFxConfigSO";

        const string ConfigFolder    = "Assets/AQ/Config";
        const string ConfigAssetPath = ConfigFolder + "/BoardFxConfig.asset";

        [MenuItem("AQ/Board/Setup FX (Config + Components)")]
        public static void SetupAllControllers()
        {
            // Resolve runtime types (no asmdef refs needed)
            var controllerType = FindType(T_MergeBoardController);
            var fxPlayerType   = FindType(T_BoardFxPlayer);
            var observerType   = FindType(T_BoardFxObserver);
            var configType     = FindType(T_BoardFxConfigSO);

            if (controllerType == null || fxPlayerType == null || observerType == null || configType == null)
            {
                var missing = new List<string>();
                if (controllerType == null) missing.Add(T_MergeBoardController);
                if (fxPlayerType   == null) missing.Add(T_BoardFxPlayer);
                if (observerType   == null) missing.Add(T_BoardFxObserver);
                if (configType     == null) missing.Add(T_BoardFxConfigSO);
                Debug.LogError($"[AQ] FX setup aborted. Missing runtime type(s):\n - {string.Join("\n - ", missing)}\n" +
                               "Ensure your project compiles and those types exist in playmode assemblies.");
                return;
            }

            // Ensure config asset
            var config = EnsureConfigAsset(configType);
            if (config == null)
            {
                Debug.LogError("[AQ] Could not create/load BoardFxConfig asset.");
                return;
            }

            // Find controllers without compile-time generics
            var controllers = FindObjectsOfTypeScene(controllerType);
            if (controllers.Count == 0)
            {
                Debug.LogError("[AQ] No MergeBoardController found in the open scene(s). Open your Board scene and run again.");
                return;
            }

            int processed = 0;
            foreach (var ctrl in controllers)
            {
                var go = (ctrl as Component)?.gameObject;
                if (!go) continue;

                // Add/get BoardFxPlayer
                var fxPlayer = go.GetComponent(fxPlayerType);
                if (fxPlayer == null) fxPlayer = go.AddComponent(fxPlayerType);

                // Assign config field on BoardFxPlayer via SerializedObject
                var so = new SerializedObject(fxPlayer);
                var prop = so.FindProperty("config");
                if (prop != null && prop.objectReferenceValue == null)
                {
                    prop.objectReferenceValue = config;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(fxPlayer);
                }

                // Add/get BoardFxObserver
                var observer = go.GetComponent(observerType);
                if (observer == null)
                {
                    observer = go.AddComponent(observerType);
                    EditorUtility.SetDirty(go);
                }

                processed++;
            }

            // Mark scene dirty
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.IsValid()) EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log($"[AQ] Board FX setup complete. Controllers processed: {processed}. Config: {AssetDatabase.GetAssetPath(config)}");
        }

        [MenuItem("AQ/Board/Create FX Config (only)")]
        public static void CreateConfigOnly()
        {
            var configType = FindType(T_BoardFxConfigSO);
            if (configType == null)
            {
                Debug.LogError($"[AQ] Missing type: {T_BoardFxConfigSO}");
                return;
            }
            var cfg = EnsureConfigAsset(configType);
            if (cfg)
                Debug.Log($"[AQ] Created/Found BoardFxConfig at: {AssetDatabase.GetAssetPath(cfg)}");
        }

        // -------- helpers --------

        static Type FindType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(fullName, throwOnError: false, ignoreCase: false);
                if (t != null) return t;
            }
            return null;
        }

        static List<Component> FindObjectsOfTypeScene(Type componentType)
        {
            // Get all MonoBehaviours in scenes, filter by type
            var list = new List<Component>();
            var all = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var mb in all)
            {
                if (mb && componentType.IsInstanceOfType(mb))
                    list.Add(mb);
            }
            return list;
        }

        static ScriptableObject EnsureConfigAsset(Type configType)
        {
            // Try to load a previously created config
            var existing = AssetDatabase.LoadAssetAtPath<ScriptableObject>(ConfigAssetPath);
            if (existing != null) return existing;

            // Ensure folder tree
            EnsureFolder("Assets/AQ");
            EnsureFolder(ConfigFolder);

            // Create new ScriptableObject asset via reflection
            var cfg = ScriptableObject.CreateInstance(configType) as ScriptableObject;
            if (cfg == null) return null;

            // Sensible defaults via SerializedObject (no hard refs)
            var so = new SerializedObject(cfg);
            SetFloat(so, "spawnPopDuration", 0.12f);
            SetFloat(so, "mergePopDuration", 0.12f);
            SetFloat(so, "swapSlideDuration", 0.12f);
            SetFloat(so, "spawnStartScale", 0.85f);
            SetFloat(so, "popPeakScale", 1.15f);
            SetFloat(so, "invalidShakeDuration", 0.20f);
            SetFloat(so, "invalidShakeMagnitude", 10f);
            SetFloat(so, "sfxVolume", 0.75f);
            SetFloat(so, "sparkleLifetime", 0.7f);
            SetBool (so, "sparkleIsUI", true);
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(cfg, ConfigAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return cfg;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace("\\", "/");
            var leaf   = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        static void SetFloat(SerializedObject so, string name, float value)
        {
            var p = so.FindProperty(name);
            if (p != null) p.floatValue = value;
        }

        static void SetBool(SerializedObject so, string name, bool value)
        {
            var p = so.FindProperty(name);
            if (p != null) p.boolValue = value;
        }
    }

    // Small extensions to keep reflection usage tidy
    static class ComponentExt
    {
        public static Component GetComponent(this GameObject go, Type t)
            => go ? go.GetComponent(t) : null;

        public static Component AddComponent(this GameObject go, Type t)
            => go ? go.AddComponent(t) : null;
    }
}
#endif
