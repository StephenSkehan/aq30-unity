#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using AQ.App.Merge;

namespace AQ.Editor.Icons
{
    public static class MergeIconCatalogBuilder
    {
        private const string CatalogAssetPath = "Assets/Data/MergeChains/MergeIconCatalog.asset";

        private static readonly string[] PngGlobs =
        {
            "Assets/Art/UI/Icons/MergeChains/**/master/*.png"
        };

        private static readonly string[] AtlasGlobs =
        {
            "Assets/Art/UI/Icons/MergeChains/_Atlases/*.spriteatlas"
        };

        [MenuItem("AQ/Merge/Rebuild Icon Catalog (Demo)")]
        public static void RebuildCatalog()
        {
            // ensure folder exists
            var dir = Path.GetDirectoryName(CatalogAssetPath)?.Replace('\\','/') ?? "Assets/Data/MergeChains";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }

            var catalog = AssetDatabase.LoadAssetAtPath<MergeIconCatalog>(CatalogAssetPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<MergeIconCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogAssetPath);
            }

            var entries = new List<MergeIconCatalog.Entry>();
            var used = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

            // 1) sprites from atlases (preferred)
            foreach (var atlasPath in Glob(AtlasGlobs))
            {
                var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
                if (atlas == null) continue;

                // dynamic buffer growth to fetch all sprites
                var size = 256;
                var buffer = new Sprite[size];
                var count = atlas.GetSprites(buffer);
                while (count == size)
                {
                    size *= 2;
                    buffer = new Sprite[size];
                    count = atlas.GetSprites(buffer);
                }

                for (int i = 0; i < count; i++)
                {
                    var s = buffer[i];
                    if (!s) continue;
                    var key = s.name; // sprite name as ID
                    if (used.Add(key))
                    {
                        entries.Add(new MergeIconCatalog.Entry { id = key, sprite = s });
                    }
                }
            }

            // 2) standalone PNGs (fallback)
            foreach (var pngPath in Glob(PngGlobs))
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
                if (!sprite) continue;

                var id = Path.GetFileNameWithoutExtension(pngPath);
                if (used.Add(id))
                {
                    entries.Add(new MergeIconCatalog.Entry { id = id, sprite = sprite });
                }
            }

            // write & save
            entries = entries.OrderBy(e => e.id, System.StringComparer.OrdinalIgnoreCase).ToList();
            Undo.RegisterCompleteObjectUndo(catalog, "Rebuild Merge Icon Catalog");
            catalog.SetEntries(entries);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();

            Debug.Log($"[AQ] Icon Catalog rebuilt: {CatalogAssetPath} (entries={entries.Count})");
        }

        private static IEnumerable<string> Glob(IEnumerable<string> patterns)
        {
            foreach (var pattern in patterns)
            {
                var root = RootOfGlob(pattern);
                var searchRoots = Directory.Exists(root) ? new[] { root } : new[] { "Assets" };

                foreach (var guid in AssetDatabase.FindAssets("", searchRoots))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (PathMatchesGlob(path, pattern))
                        yield return path;
                }
            }
        }

        private static string RootOfGlob(string pattern)
        {
            pattern = pattern.Replace('\\','/');
            int star = pattern.IndexOfAny(new[] { '*', '?' });
            if (star < 0) return Path.GetDirectoryName(pattern) ?? "Assets";

            var prefix = pattern.Substring(0, star);
            var slash = prefix.LastIndexOf('/');
            if (slash >= 0) prefix = prefix.Substring(0, slash);
            if (string.IsNullOrEmpty(prefix)) prefix = "Assets";
            return prefix;
        }

        // very small glob matcher to cover our patterns
        private static bool PathMatchesGlob(string path, string glob)
        {
            path = path.Replace('\\','/').ToLowerInvariant();
            glob = glob.Replace('\\','/').ToLowerInvariant();

            // normalize common "**/" usage
            glob = glob.Replace("**/", "");

            if (glob.EndsWith("*.png"))
                return path.EndsWith(".png") && path.Contains(glob.Replace("*.png", ""));
            if (glob.EndsWith(".spriteatlas"))
                return path.EndsWith(".spriteatlas") && path.Contains(glob.Replace("*.spriteatlas", ""));

            return path.Contains(glob);
        }
    }
}
#endif
