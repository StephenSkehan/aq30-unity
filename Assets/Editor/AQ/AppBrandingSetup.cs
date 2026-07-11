using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace AQ.EditorTools
{
    public static class AppBrandingSetup
    {
        private const string IconPath   = "Assets/Art/UI/AppIcon_1024.png";
        private const string SplashPath = "Assets/Art/UI/Splash_Portrait.png";

        [MenuItem("AQ/Setup/Apply App Icon + Splash")]
        public static void Apply()
        {
            ConfigureIconImporter();
            ConfigureSplashImporter();

            var iconTex = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            if (iconTex == null) { Debug.LogError($"[Branding] icon not found: {IconPath}"); return; }

            var splashSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SplashPath);
            if (splashSprite == null) { Debug.LogError($"[Branding] splash sprite not found: {SplashPath}"); return; }

            // Default icon — Unity auto-generates all iOS sizes from this.
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { iconTex }, IconKind.Any);

            PlayerSettings.SplashScreen.show = true;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            PlayerSettings.SplashScreen.backgroundPortrait = splashSprite;
            PlayerSettings.SplashScreen.background = splashSprite;
            PlayerSettings.SplashScreen.blurBackgroundImage = false;
            // Matches the art's darkest tone so letterboxing on unusual aspects is invisible.
            PlayerSettings.SplashScreen.backgroundColor = new Color32(6, 10, 20, 255);

            AssetDatabase.SaveAssets();
            Debug.Log("[Branding] Default icon + splash applied.");
        }

        private static void ConfigureIconImporter()
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(IconPath);
            if (importer == null) return;
            // Icons are consumed at build time by the editor, not rendered in-game:
            // keep the source uncompressed so generated sizes stay clean.
            importer.textureType = TextureImporterType.Default;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static void ConfigureSplashImporter()
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(SplashPath);
            if (importer == null) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
    }
}
