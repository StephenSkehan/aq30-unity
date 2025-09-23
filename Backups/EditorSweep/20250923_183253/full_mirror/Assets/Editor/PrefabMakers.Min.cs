using UnityEditor;
using UnityEngine;
using System.IO;

namespace AQ.BuildTools
{
    public static class SimplePrefabMakers
    {
        // Seed contract: Assets/Resources/App/UI/Prefabs/HUD.prefab
        public static void CreateHUDPrefab()
        {
            const string path = "Assets/Resources/App/UI/Prefabs/HUD.prefab";
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var go = new GameObject("HUD");
            try
            {
                PrefabUtility.SaveAsPrefabAsset(go, path, out bool ok);
                if (!ok) throw new System.Exception("SaveAsPrefabAsset failed: " + path);
                AssetDatabase.SaveAssets(); AssetDatabase.ImportAsset(path); AssetDatabase.Refresh();
                Debug.Log("[SimplePrefabMakers] Created " + path);
            }
            finally { Object.DestroyImmediate(go); }
        }

        // Seed contract: Assets/App/UI/Narrative/Resources/App/UI/Narrative/Prefabs/DialoguePanel.prefab
        public static void CreateDialoguePanelPrefab()
        {
            const string path = "Assets/App/UI/Narrative/Resources/App/UI/Narrative/Prefabs/DialoguePanel.prefab";
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var go = new GameObject("DialoguePanel");
            try
            {
                PrefabUtility.SaveAsPrefabAsset(go, path, out bool ok);
                if (!ok) throw new System.Exception("SaveAsPrefabAsset failed: " + path);
                AssetDatabase.SaveAssets(); AssetDatabase.ImportAsset(path); AssetDatabase.Refresh();
                Debug.Log("[SimplePrefabMakers] Created " + path);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
