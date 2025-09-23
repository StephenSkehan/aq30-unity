using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AQ.Tools {
  public static class SwapPrefabBindings {
    // Menu convenience (uses the GUIDs baked at generation time)
    [MenuItem("AQ/Tools/Swap HUD prefab (shadow -> live)", priority = 2000)]
    public static void RunMenu() => Run("13549558efc002f45b1084982aade024", "c30138bc133f6784da157679f91d38f2");

    // Batchmode entry: -executeMethod AQ.Tools.SwapPrefabBindings.Run <movedGuid> <liveGuid>
    public static void Run(string movedGuid, string liveGuid) {
      var movedPath = AssetDatabase.GUIDToAssetPath(movedGuid);
      var livePath  = AssetDatabase.GUIDToAssetPath(liveGuid);
      if (string.IsNullOrEmpty(movedPath) || string.IsNullOrEmpty(livePath)) {
        Debug.LogError($"[SwapPrefabBindings] Invalid GUID(s). moved='{movedPath}' live='{livePath}'");
        return;
      }

      var livePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(livePath);
      if (livePrefab == null) {
        Debug.LogError($"[SwapPrefabBindings] Could not load live prefab at '{livePath}'");
        return;
      }

      // Only touch scenes that referenced the shadow HUD per audit
      var scenes = new [] {
        "Assets/Scenes/WK2_BoardDemo.unity",
        "Assets/Scenes/WK2_ThemeDemo.unity"
      };

      int scenesChanged = 0, instancesReplaced = 0;

      // Prefer hierarchy-based matching to keep overrides & references where structure matches
      // (Unity docs: ReplacePrefabAssetOfPrefabInstance + PrefabReplacingSettings) 
      var settings = new PrefabReplacingSettings {
        objectMatchMode = ObjectMatchMode.ByHierarchy,
        prefabOverridesOptions = PrefabOverridesOptions.KeepAllPossibleOverrides,
        changeRootNameToAssetName = false,
        logInfo = true
      };

      foreach (var scenePath in scenes) {
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        bool modified = false;

        // Gather all prefab instance ROOTS in the scene.
        var roots = scene.GetRootGameObjects();
        var all = new List<GameObject>(1024);
        foreach (var r in roots) Collect(r, all);

        var candidateRoots = new HashSet<GameObject>();
        foreach (var go in all) {
          if (PrefabUtility.GetPrefabInstanceStatus(go) != PrefabInstanceStatus.NotAPrefab) {
            var instRoot = PrefabUtility.GetNearestPrefabInstanceRoot(go);
            if (instRoot != null) candidateRoots.Add(instRoot);
          }
        }

        var movedPathNorm = movedPath.Replace('\\','/');
        foreach (var instRoot in candidateRoots) {
          var src = PrefabUtility.GetCorrespondingObjectFromSource(instRoot);
          var srcPath = AssetDatabase.GetAssetPath(src)?.Replace('\\','/');
          if (string.Equals(srcPath, movedPathNorm)) {
            PrefabUtility.ReplacePrefabAssetOfPrefabInstance(instRoot, livePrefab, settings, InteractionMode.AutomatedAction);
            modified = true;
            instancesReplaced++;
            Debug.Log($"[SwapPrefabBindings] {scenePath}: replaced instance '{instRoot.name}'");
          }
        }

        if (modified) {
          EditorSceneManager.MarkSceneDirty(scene);
          EditorSceneManager.SaveScene(scene);
          scenesChanged++;
        }
      }

      Debug.Log($"[SwapPrefabBindings] Done. Scenes changed: {scenesChanged}, instances replaced: {instancesReplaced}");
    }

    static void Collect(GameObject root, List<GameObject> list) {
      list.Add(root);
      foreach (Transform c in root.transform) Collect(c.gameObject, list);
    }
  }
}
