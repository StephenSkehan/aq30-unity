using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace AQ.EditorTools
{
    public static class AssignAppIcons
    {
        private const string IconDir = "Assets/Art/AppIcon";

        [MenuItem("AQ/Setup/Assign App Icons")]
        public static void Assign()
        {
            Debug.Log("[AppIcons] START");
            try
            {
                var bySize = LoadIcons();
                Debug.Log($"[AppIcons] loaded {bySize.Count} source textures");
                if (bySize.Count == 0)
                {
                    Debug.LogError($"[AppIcons] No textures found in {IconDir}.");
                    return;
                }

                // iOS only. Android is parked, and its adaptive kind wants a
                // background/foreground pair rather than one full-bleed image.
                int slots = AssignPlatform(NamedBuildTarget.iOS, bySize);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[AppIcons] DONE, filled {slots} slots");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AppIcons] THREW: {e.GetType().Name}: {e.Message}");
            }
        }

        private static int AssignPlatform(NamedBuildTarget target, Dictionary<int, Texture2D> bySize)
        {
            int filled = 0;
            PlatformIconKind[] kinds = PlayerSettings.GetSupportedIconKinds(target);
            Debug.Log($"[AppIcons] {target.TargetName}: {kinds.Length} icon kinds");
            foreach (PlatformIconKind kind in kinds)
            {
                PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(target, kind);
                Debug.Log($"[AppIcons]   kind {kind} -> {icons.Length} slots");
                foreach (PlatformIcon icon in icons)
                {
                    Texture2D tex = Nearest(bySize, icon.width);
                    if (tex == null) continue;
                    // Layer count is per-kind and non-negotiable; assigning the wrong
                    // number throws and aborts the whole pass.
                    icon.SetTextures(Enumerable.Repeat(tex, icon.maxLayerCount).ToArray());
                    filled++;
                }
                PlayerSettings.SetPlatformIcons(target, kind, icons);
            }
            return filled;
        }

        // Prefer an exact size; otherwise the smallest source at or above the slot,
        // so Unity only ever downscales.
        private static Texture2D Nearest(Dictionary<int, Texture2D> bySize, int want)
        {
            if (bySize.TryGetValue(want, out Texture2D exact)) return exact;
            int up = bySize.Keys.Where(k => k > want).DefaultIfEmpty(-1).Min();
            if (up > 0) return bySize[up];
            int down = bySize.Keys.Where(k => k < want).DefaultIfEmpty(-1).Max();
            return down > 0 ? bySize[down] : null;
        }

        private static Dictionary<int, Texture2D> LoadIcons()
        {
            var map = new Dictionary<int, Texture2D>();
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { IconDir }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                int us = name.LastIndexOf('_');
                if (us < 0 || !int.TryParse(name.Substring(us + 1), out int size)) continue;

                EnsureImportSettings(path);
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex != null) map[size] = tex;
            }
            return map;
        }

        // Icons must not be compressed or NPOT-scaled or Unity bakes artefacts into
        // the generated asset catalog.
        private static void EnsureImportSettings(string path)
        {
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) return;

            bool dirty = false;
            if (imp.textureType != TextureImporterType.Default) { imp.textureType = TextureImporterType.Default; dirty = true; }
            if (imp.npotScale != TextureImporterNPOTScale.None) { imp.npotScale = TextureImporterNPOTScale.None; dirty = true; }
            if (imp.mipmapEnabled) { imp.mipmapEnabled = false; dirty = true; }
            if (imp.alphaSource != TextureImporterAlphaSource.None) { imp.alphaSource = TextureImporterAlphaSource.None; dirty = true; }
            if (imp.textureCompression != TextureImporterCompression.Uncompressed) { imp.textureCompression = TextureImporterCompression.Uncompressed; dirty = true; }

            if (dirty) imp.SaveAndReimport();
        }
    }
}
