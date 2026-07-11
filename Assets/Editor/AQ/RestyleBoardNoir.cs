using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using AQ.App.UI;

namespace AQ.EditorTools
{
    /// <summary>
    /// One-shot board reskin: persists the AQTheme rounded sprite as a real
    /// asset, then applies the noir palette to slot backgrounds, adds a frame
    /// plate behind the grid and sets the camera clear color. Board slots are
    /// found by their slot_RR_CC naming (their types live in Assembly-CSharp,
    /// which this assembly cannot reference).
    /// </summary>
    public static class RestyleBoardNoir
    {
        const string SpritePath = "Assets/Resources/App/UI/aq_rounded.png";

        [MenuItem("AQ/Setup/Restyle Board (Noir)")]
        public static void Restyle()
        {
            var rounded = EnsureRoundedSpriteAsset();
            if (rounded == null) return;

            var slotRe = new Regex(@"^slot_(\d{2})_(\d{2})$");
            Transform boardRoot = null;
            int cells = 0;

            foreach (var img in Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (img.name != "Bg" || img.transform.parent == null) continue;
                var m = slotRe.Match(img.transform.parent.name);
                if (!m.Success) continue;

                int r = int.Parse(m.Groups[1].Value);
                int c = int.Parse(m.Groups[2].Value);

                Undo.RecordObject(img, "Board restyle");
                img.sprite = rounded;
                img.type   = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 2.5f; // small cells need tighter corners
                img.color  = (r + c) % 2 == 0 ? AQTheme.BoardCellA : AQTheme.BoardCellB;
                EditorUtility.SetDirty(img);

                boardRoot = img.transform.parent.parent;
                cells++;
            }

            if (boardRoot == null)
            {
                Debug.LogError("[Board] no slot_RR_CC cells found in the open scene.");
                return;
            }

            AddFramePlate(boardRoot, rounded);

            var cam = Camera.main;
            if (cam != null)
            {
                Undo.RecordObject(cam, "Board restyle");
                cam.clearFlags      = CameraClearFlags.SolidColor;
                cam.backgroundColor = AQTheme.Navy;
                EditorUtility.SetDirty(cam);
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            Debug.Log($"[Board] restyled {cells} cells under '{boardRoot.name}', frame plate + camera set.");
        }

        static void AddFramePlate(Transform boardRoot, Sprite rounded)
        {
            var existing = boardRoot.Find("BoardFrame");
            if (existing != null) Undo.DestroyObjectImmediate(existing.gameObject);

            // boardRoot has a GridLayoutGroup driving its children — the plate
            // must opt out (ignoreLayout) or it gets adopted as a grid cell and
            // shifts every slot by one.
            const float pad = 14f;
            var go = new GameObject("BoardFrame", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            Undo.RegisterCreatedObjectUndo(go, "Board restyle");
            go.transform.SetParent(boardRoot, false);
            go.transform.SetAsFirstSibling(); // render behind the slots
            go.GetComponent<LayoutElement>().ignoreLayout = true;

            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(-pad, -pad);
            rt.offsetMax = new Vector2(pad, pad);

            var img = go.GetComponent<Image>();
            img.sprite = rounded;
            img.type   = Image.Type.Sliced;
            img.color  = AQTheme.BoardFrame;
            img.raycastTarget = false;
        }

        static Sprite EnsureRoundedSpriteAsset()
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            if (sprite != null) return sprite;

            var tex = AQTheme.BuildRoundedTexture(readable: true);
            System.IO.File.WriteAllBytes(SpritePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(SpritePath);

            var importer = (TextureImporter)AssetImporter.GetAtPath(SpritePath);
            importer.textureType         = TextureImporterType.Sprite;
            importer.spriteImportMode    = SpriteImportMode.Single;
            importer.spriteBorder        = new Vector4(40f, 40f, 40f, 40f);
            importer.mipmapEnabled       = false;
            importer.alphaIsTransparency = true;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();

            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            if (sprite == null) Debug.LogError("[Board] failed to create " + SpritePath);
            return sprite;
        }
    }
}
