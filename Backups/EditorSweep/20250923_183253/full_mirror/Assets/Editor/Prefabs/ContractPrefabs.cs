#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace AQ.EditorTools
{
    public static class ContractPrefabs
    {
        const string ContractDir = "Assets/Resources/App/UI/Prefabs";
        static readonly string[] ContractNames = { "HUD", "DialoguePanel" };

        [MenuItem("AQ/Prefabs/Ensure Contract Prefabs")]
        public static void EnsureContractPrefabs()
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/App");
            EnsureFolder("Assets/Resources/App/UI");
            EnsureFolder(ContractDir);

            foreach (var name in ContractNames)
                TryWriteContractPrefab(name);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ContractPrefabs] Synced contract prefabs  " + ContractDir);
        }

        static void TryWriteContractPrefab(string prefabName)
        {
            // Find all prefabs with this name under Assets/
            var guids = AssetDatabase.FindAssets($"t:Prefab {prefabName}");
            var paths = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.StartsWith("Assets/"))
                // Rank: prefer authored UI paths, deprioritize quarantine/archives
                .OrderByDescending(p => p.Contains("/UI/"))
                .ThenByDescending(p => p.StartsWith("Assets/UI/"))
                .ThenBy(p => p.Contains("Resources_moved") || p.Contains("Z._quarantine") ? 1 : 0)
                .ThenBy(p => p) // deterministic within same rank
                .ToArray();

            if (paths.Length == 0)
            {
                Debug.LogWarning($"[ContractPrefabs] No prefab named '{prefabName}' found under Assets/.");
                return;
            }

            // Pick the first prefab that has no missing scripts
            string srcPath = null;
            GameObject src = null;
            foreach (var p in paths)
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                if (!go) continue;

                var hasMissing = go.GetComponentsInChildren<MonoBehaviour>(true).Any(c => c == null);
                if (!hasMissing) { srcPath = p; src = go; break; }
            }

            if (src == null)
            {
                Debug.LogWarning($"[ContractPrefabs] All candidates for '{prefabName}' have missing scripts. Checked: {string.Join(", ", paths)}");
                return;
            }

            // Ensure contract dir exists
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/App");
            EnsureFolder("Assets/Resources/App/UI");
            EnsureFolder(ContractDir);

            // Save a clean contract copy
            var temp = Object.Instantiate(src);
            var dstPath = $"{ContractDir}/{prefabName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(temp, dstPath, out bool ok);
            Object.DestroyImmediate(temp);

            if (ok) Debug.Log($"[ContractPrefabs] {prefabName}  {dstPath} (from {srcPath})");
            else Debug.LogWarning($"[ContractPrefabs] Failed to write {dstPath}");
        }


        static void EnsureFolder(string path)
        {
            var parts = path.Split('/');
            var build = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = build + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(build, parts[i]);
                build = next;
            }
        }
    }
}
#endif

