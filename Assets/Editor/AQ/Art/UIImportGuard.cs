#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AQ.Editor.Art
{
    // Enforces Sprite(2D/UI), Alpha=Transparency, Full Rect, no MipMaps, Bilinear, Uncompressed for UI art.
    public sealed class UIImportGuard : AssetPostprocessor
    {
        private static bool IsUiPng(string path)
            => path.Replace("\\","/").StartsWith("Assets/Art/UI/", System.StringComparison.OrdinalIgnoreCase)
               && path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase);

        void OnPreprocessTexture()
        {
            if (!IsUiPng(assetPath)) return;

            var ti = (TextureImporter)assetImporter;
            bool dirty = false;

            if (ti.textureType != TextureImporterType.Sprite) { ti.textureType = TextureImporterType.Sprite; dirty = true; }
            if (!ti.alphaIsTransparency) { ti.alphaIsTransparency = true; dirty = true; }
            if (ti.mipmapEnabled) { ti.mipmapEnabled = false; dirty = true; }
            if (ti.filterMode != FilterMode.Bilinear) { ti.filterMode = FilterMode.Bilinear; dirty = true; }
            if (ti.textureCompression != TextureImporterCompression.Uncompressed) { ti.textureCompression = TextureImporterCompression.Uncompressed; dirty = true; }
            if (ti.spriteMeshType != SpriteMeshType.FullRect) { ti.spriteMeshType = SpriteMeshType.FullRect; dirty = true; }

            if (dirty) Debug.Log($"🛡 UIImportGuard normalized: {assetPath}");
        }

        [MenuItem("AQ/Art/Reimport UI Folder (guard settings)")]
        private static void ReimportUi()
        {
            var paths = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art/UI" })
                                     .Select(AssetDatabase.GUIDToAssetPath)
                                     .Where(p => p.EndsWith(".png"))
                                     .ToArray();
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var p in paths) AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate);
            }
            finally { AssetDatabase.StopAssetEditing(); }
            Debug.Log($"✅ Reimported {paths.Length} UI textures with guard settings.");
        }
    }
}
#endif
