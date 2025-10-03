// Assets/Editor/UI/Board/CleanMissingOnBoard.cs
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AQ.Editor.UI.Board
{
    public static class CleanMissingOnBoard
    {
        [MenuItem("AQ/Board/Clean Missing Scripts (Active Scene)")]
        public static void CleanActiveScene()
        {
            int removed = 0;
            var scene = SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                removed += CleanGO(root);
            }
            Debug.Log($"[AQ] CleanMissing: removed {removed} missing scripts from scene.");
        }

        static int CleanGO(GameObject go)
        {
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            foreach (Transform t in go.transform)
                removed += CleanGO(t.gameObject);
            return removed;
        }
    }
}
