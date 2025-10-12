// Assets/Editor/UI/Board/Cleanup_RemoveMergeDomainDriver.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

static class Cleanup_RemoveMergeDomainDriver {
    [MenuItem("AQ/Board/Cleanup: Remove MergeDomainDriver + Missing Scripts")]
    static void Run() {
        // remove all "missing script" stubs too
        int missing = 0, drivers = 0;
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            missing += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);

        var t = System.Type.GetType("AQ.App.UI.Board.MergeDomainDriver, Assembly-CSharp");
        if (t != null) {
            foreach (var c in Object.FindObjectsByType(t, FindObjectsInactive.Include, FindObjectsSortMode.None)) {
                Object.DestroyImmediate(c); drivers++;
            }
        }
        Debug.Log($"[AQ] Cleanup removed: {drivers} MergeDomainDriver, {missing} missing-script components.");
    }
}
#endif
