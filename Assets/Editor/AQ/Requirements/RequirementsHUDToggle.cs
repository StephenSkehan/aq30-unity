#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AQ.Editor.Requirements
{
    public static class RequirementsHUDToggle
    {
        private const string Path = "RequirementsHUD";

        [MenuItem("AQ/Requirements/Hide RequirementsHUD")]
        public static void Hide()
        {
            var go = GameObject.Find(Path);
            if (!go) { Debug.LogWarning("⚠️ RequirementsHUD not found."); return; }
            if (!go.activeSelf)
            {
                Debug.Log("ℹ️ RequirementsHUD already hidden.");
                return;
            }
            Undo.RecordObject(go, "Hide RequirementsHUD");
            go.SetActive(false);
            Debug.Log("🙈 RequirementsHUD hidden.");
        }

        [MenuItem("AQ/Requirements/Show RequirementsHUD")]
        public static void Show()
        {
            var go = GameObject.Find(Path);
            if (!go) { Debug.LogWarning("⚠️ RequirementsHUD not found."); return; }
            if (go.activeSelf)
            {
                Debug.Log("ℹ️ RequirementsHUD already visible.");
                return;
            }
            Undo.RecordObject(go, "Show RequirementsHUD");
            go.SetActive(true);
            Debug.Log("👀 RequirementsHUD shown.");
        }
    }
}
#endif
