#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AQ.Editor.Art
{
    // Normalizes UI PNG import settings under Assets/Art/UI/**.png
    // Unity 6.x note: sprite mesh type is set via TextureImporterSettings, not TextureImporter.spriteMeshType.
    public sealed class UIImportGuard : AssetPostprocessor
    {
        private static bool IsUiPng(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            var norm = path.Replace("\\", "/");
            return norm.StartsWith("Assets/Art/UI/", System.StringComparison.OrdinalIgnoreCase)
                   && norm.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase);
        }

        void OnPreprocessTexture()
        {
            if (!IsUiPng(assetPath)) return;

            var ti = (TextureImporter)assetImporter;
            bool dirty = false;

            // Core sprite settings
            if (ti.textureType != TextureImporterType.Sprite) { ti.textureType = TextureImporterType.Sprite; dirty = true; }
            if (!ti.alphaIsTransparency) { ti.alphaIsTransparency = true; dirty = true; }
            if (ti.mipmapEnabled) { ti.mipmapEnabled = false; dirty = true; }
            if (ti.filterMode != FilterMode.Bilinear) { ti.filterMode = FilterMode.Bilinear; dirty = true; }
            if (ti.textureCompression != TextureImporterCompression.Uncompressed) { ti.textureCompression = TextureImporterCompression.Uncompressed; dirty = true; }
            if (ti.maxTextureSize != 1024) { ti.maxTextureSize = 1024; dirty = true; }

            // Mesh type (Full Rect) via TextureImporterSettings
            var tis = new TextureImporterSettings();
            ti.ReadTextureSettings(tis);
            if (tis.spriteMeshType != SpriteMeshType.FullRect)
            {
                tis.spriteMeshType = SpriteMeshType.FullRect;
                ti.SetTextureSettings(tis);
                dirty = true;
            }

            if (dirty)
            {
                Debug.Log($"🛡 UIImportGuard normalized: {assetPath}");
            }
        }

        [MenuItem("AQ/Art/Reimport UI Folder (guard settings)")]
        private static void ReimportUi()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art/UI" });
            var paths = guids.Select(AssetDatabase.GUIDToAssetPath)
                             .Where(p => p.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                             .ToArray();

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var p in paths)
                    AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            Debug.Log($"✅ Reimported {paths.Length} UI textures with guard settings.");
        }
    }
}
#endif
