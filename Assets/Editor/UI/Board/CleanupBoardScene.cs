// Assets/Editor/UI/Board/CleanupBoardScene.cs
#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace AQ.Editor.UI.Board
{
    public static class CleanupBoardScene
    {
        [MenuItem("AQ/Board/Remove MergeDomainDriver + Missing Scripts")]
        public static void Run()
        {
            int removedMissing = 0, removedDrivers = 0;

            foreach (var go in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                removedMissing += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);

            var t = Type.GetType("AQ.App.UI.Board.MergeDomainDriver, Assembly-CSharp");
            if (t != null)
            {
                foreach (var c in UnityEngine.Object.FindObjectsByType(t, FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    UnityEngine.Object.DestroyImmediate(c);
                    removedDrivers++;
                }
            }

            Debug.Log($"[AQ] Cleanup: removed {removedMissing} missing-script stubs, {removedDrivers} MergeDomainDriver components.");
        }
    }
}
#endif
