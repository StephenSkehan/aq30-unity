// Assets/Editor/AQ/Leads/AddLeadsRepositoryMenu.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using AQ.App.Leads;

namespace AQ.Editor.Leads
{
    internal static class AddLeadsRepositoryMenu
    {
        [MenuItem("AQ/Leads/Add Leads Repository to Scene", priority = 10)]
        private static void AddRepoToScene()
        {
            var existing = FindRepo();
            if (existing != null)
            {
                Selection.activeObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing);
                Debug.Log("[AQ] LeadsRepository already present in scene.", existing);
                return;
            }

            var go = new GameObject("LeadsRepository");
            Undo.RegisterCreatedObjectUndo(go, "Add LeadsRepository");
            var repo = go.AddComponent<LeadsRepository>();
            Selection.activeObject = go;
            EditorGUIUtility.PingObject(go);
            Debug.Log("[AQ] Created LeadsRepository in scene.", repo);
        }

        [MenuItem("AQ/Leads/Probe Runtime", priority = 50)]
        private static void ProbeRuntime()
        {
            var repo = FindRepo();
            if (repo == null)
            {
                Debug.LogWarning("[AQ] Probe: No LeadsRepository found in scene.");
                return;
            }
            Debug.Log($"[AQ] Probe: Found LeadsRepository on '{repo.gameObject.name}'.", repo);
        }

        private static LeadsRepository FindRepo()
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<LeadsRepository>();
#else
            // Fallback for older Unity versions
            return Object.FindObjectOfType<LeadsRepository>();
#endif
        }
    }
}
#endif
