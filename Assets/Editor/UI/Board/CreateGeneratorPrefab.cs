// Assets/Editor/UI/Board/CreateGeneratorPrefab.cs
// Editor-only utility to help build a generator prefab from a selected slot or object.

#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace AQ.App.Editor.Board
{
    public static class CreateGeneratorPrefab
    {
        [MenuItem("AQ/Board/Generator/Create Generator Prefab From Selection")]
        private static void CreateFromSelection()
        {
            if (Selection.activeGameObject == null)
            {
                Debug.LogError("Select a GameObject that represents your generator tile.");
                return;
            }

            var go = Selection.activeGameObject;
            var path = EditorUtility.SaveFilePanelInProject(
                "Save Generator Prefab",
                go.name + "_Generator",
                "prefab",
                "Choose a location for the generator prefab (inside the project).");

            if (string.IsNullOrEmpty(path)) return;

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            if (prefab == null) { Debug.LogError("Failed to create prefab."); return; }

            Debug.Log($"Generator prefab created: {path}");
        }

        // Version-safe finder (kept here if you later want to auto-locate a default root)
        private static T FindOne<T>() where T : UnityEngine.Object
        {
#if UNITY_2023_1_OR_NEWER
            return UnityEngine.Object.FindFirstObjectByType<T>();
#else
            return UnityEngine.Object.FindObjectOfType<T>();
#endif
        }
    }
}
#endif
