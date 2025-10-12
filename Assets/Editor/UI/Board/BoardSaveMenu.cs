// Assets/Editor/UI/Board/BoardSaveMenu.cs
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AQ.Editor.UI.Board
{
    public static class BoardSaveMenu
    {
        [MenuItem("AQ/Board/Clear Saved Board State")]
        public static void Clear()
        {
            var path = Path.Combine(Application.persistentDataPath, "board_state.json");
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log("[Save] Cleared board_state.json");
            }
            else
            {
                Debug.Log("[Save] No board_state.json to clear.");
            }
        }

        [MenuItem("AQ/Board/Open Persistent Data Folder")]
        public static void OpenFolder()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }
    }
}
#endif
