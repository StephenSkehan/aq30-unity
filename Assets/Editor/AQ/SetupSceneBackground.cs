#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using AQ.App.UI;   // BackgroundScrimMB

namespace AQ.EditorTools
{
    /// <summary>
    /// Assigns the Rivermouth night background to the scene's Background image and
    /// inserts a tunable dark scrim between it and the board. Idempotent — re-running
    /// reassigns the sprite and reuses the existing scrim (preserving its tuned
    /// opacity). Run: AQ/Setup/Setup Rivermouth Background.
    /// </summary>
    public static class SetupSceneBackground
    {
        const string BgPath   = "Assets/Art/UI/Backgrounds/bg_rivermouth_night.png";
        const string ScrimName = "gen_bg_scrim";

        [MenuItem("AQ/Setup/Setup Rivermouth Background")]
        public static void Setup()
        {
            // 1) Ensure the PNG imports as a UI sprite.
            var importer = AssetImporter.GetAtPath(BgPath) as TextureImporter;
            if (importer != null)
            {
                bool dirty = false;
                if (importer.textureType != TextureImporterType.Sprite)
                { importer.textureType = TextureImporterType.Sprite; dirty = true; }
                if (importer.spriteImportMode != SpriteImportMode.Single)
                { importer.spriteImportMode = SpriteImportMode.Single; dirty = true; }
                if (importer.maxTextureSize < 2048)
                { importer.maxTextureSize = 2048; dirty = true; }
                if (dirty) importer.SaveAndReimport();
            }
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BgPath);
            if (sprite == null) { Debug.LogWarning($"[SceneBg] sprite missing: {BgPath}"); return; }

            // 2) Assign to the Background image (full-bleed fill).
            var bg = GameObject.Find("Background");
            if (bg == null) { Debug.LogWarning("[SceneBg] 'Background' GameObject not found."); return; }
            var bgImg = bg.GetComponent<Image>();
            if (bgImg == null) { Debug.LogWarning("[SceneBg] 'Background' has no Image."); return; }
            Undo.RecordObject(bgImg, "Set background sprite");
            bgImg.sprite = sprite;
            bgImg.type = Image.Type.Simple;
            bgImg.preserveAspect = false;   // fill the screen; the art is portrait-ish so stretch is minimal
            bgImg.color = Color.white;
            EditorUtility.SetDirty(bgImg);

            // 3) Scrim: full-screen dark overlay directly above Background, below the board.
            var parent = bg.transform.parent; // GameRoot
            Transform scrimT = parent != null ? parent.Find(ScrimName) : null;
            GameObject scrim;
            if (scrimT != null)
            {
                scrim = scrimT.gameObject;   // reuse (keeps tuned opacity)
            }
            else
            {
                scrim = new GameObject(ScrimName, typeof(RectTransform), typeof(Image), typeof(BackgroundScrimMB));
                scrim.transform.SetParent(parent, false);
            }

            var rt = (RectTransform)scrim.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
            // Sit right after Background (above it), still below the board grid + HUD.
            rt.SetSiblingIndex(bg.transform.GetSiblingIndex() + 1);

            var img = scrim.GetComponent<Image>();
            if (img.sprite == null) img.sprite = AQTheme.Rounded; // any sprite; scrim is a flat fill
            img.type = Image.Type.Sliced;
            img.raycastTarget = false;

            var mb = scrim.GetComponent<BackgroundScrimMB>();
            if (mb == null) mb = scrim.AddComponent<BackgroundScrimMB>();
            // opacity left as-is if reused; default (0.4) applies on first create.

            EditorUtility.SetDirty(scrim);
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            Debug.Log($"[SceneBg] Rivermouth background assigned; scrim '{ScrimName}' at sibling {rt.GetSiblingIndex()} opacity={mb.opacity}.");
        }
    }
}
#endif
