using UnityEditor;
using UnityEngine;
using System.IO;

namespace AQ.BuildTools
{
    public static class SimpleHUDMaker
    {
        // Headless entry point (no MenuItem attribute)
        public static void CreateHUDPrefab()
        {
            const string assetPath = "Assets/Resources/App/UI/Prefabs/HUD.prefab";
            var dir = Path.GetDirectoryName(assetPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var root = new GameObject("HUD");
            try
            {
                PrefabUtility.SaveAsPrefabAsset(root, assetPath, out bool ok);
                if (!ok) throw new System.Exception("SaveAsPrefabAsset failed: " + assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                Debug.Log("[SimpleHUDMaker] Created " + assetPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}


