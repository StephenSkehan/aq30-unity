using UnityEditor;
using UnityEngine;

namespace AQ.Editor.Items
{
    /// <summary>
    /// Ensures all PNGs under Assets/Art/Icons/ are imported as Sprite, 100 PPU.
    /// Run once via: Tools > AQ > Fix Icon Import Settings
    /// Also fires automatically on new imports via AssetPostprocessor.
    /// </summary>
    public class ItemIconImporter : AssetPostprocessor
    {
        private const string IconRoot = "Assets/Art/Icons/";

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(IconRoot)) return;

            var importer = (TextureImporter)assetImporter;
            if (importer.textureType == TextureImporterType.Sprite) return;

            importer.textureType        = TextureImporterType.Sprite;
            importer.spriteImportMode   = SpriteImportMode.Single;
            importer.spritePivot        = new Vector2(0.5f, 0.5f);
            importer.spritePixelsPerUnit = 100f;
            importer.mipmapEnabled      = false;
            importer.filterMode         = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.alphaIsTransparency = true;
        }

        [MenuItem("Tools/AQ/Fix Icon Import Settings")]
        public static void FixAll()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { IconRoot });
            int fixed_ = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                if (importer.textureType == TextureImporterType.Sprite) continue;

                importer.textureType        = TextureImporterType.Sprite;
                importer.spriteImportMode   = SpriteImportMode.Single;
                importer.spritePivot        = new Vector2(0.5f, 0.5f);
                importer.spritePixelsPerUnit = 100f;
                importer.mipmapEnabled      = false;
                importer.filterMode         = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.alphaIsTransparency = true;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                fixed_++;
            }
            Debug.Log($"[ItemIconImporter] Fixed import settings on {fixed_} textures under {IconRoot}");
        }
    }
}
