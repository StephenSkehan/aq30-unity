#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AQ.EditorTools.UI.Leads
{
    /// <summary>
    /// Removes ONLY missing MonoBehaviours from ALL "TierSetPopup" objects in the active scene.
    /// Does not touch anything else.
    /// </summary>
    public static class CleanMissingOnTierSetPopupOnly
    {
        [MenuItem("AQ/UI/Leads/Clean Missing Scripts on TierSetPopup (Scene)")]
        public static void Run()
        {
            var popups = GameObject
                .FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(t => t.name == "TierSetPopup")
                .Select(t => t.gameObject)
                .ToArray();

            if (popups.Length == 0)
            {
                Debug.LogWarning("[AQ CleanPopup] No TierSetPopup objects in scene.");
                return;
            }

            int total = 0;
            foreach (var go in popups)
            {
                int before = CountMissing(go);
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                foreach (Transform c in go.transform)
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(c.gameObject);
                int after = CountMissing(go);
                int removed = before - after;
                total += removed;
                Debug.Log($"[AQ CleanPopup] {go.scene.name}:{PathOf(go.transform)} removed {removed} missing scripts.");
            }

            Debug.Log($"[AQ CleanPopup] Total missing scripts removed: {total}.");
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static int CountMissing(GameObject go)
        {
            int cnt = 0;
            foreach (var c in go.GetComponents<Component>()) if (!c) cnt++;
            foreach (Transform t in go.transform)
                foreach (var c in t.GetComponents<Component>()) if (!c) cnt++;
            return cnt;
        }

        private static string PathOf(Transform t)
        {
            System.Collections.Generic.List<string> parts = new();
            while (t != null) { parts.Add(t.name); t = t.parent; }
            parts.Reverse();
            return string.Join("/", parts);
        }
    }
}
#endif
