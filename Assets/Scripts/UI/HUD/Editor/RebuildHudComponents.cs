#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using AQ.App.UI;          // AQTheme
using AQ.App.UI.HUD;      // ShowEnergyStoreMB

namespace AQ.EditorTools
{
    /// <summary>
    /// Rebuilds the top HUD from the component sprite kit (Gossip-Harbor-slim),
    /// replacing the single baked HUD3_Transparent background. Idempotent: it tears
    /// down its own generated children (name prefix "gen_hud_") and rebuilds, and it
    /// never deletes the runtime-wired widgets (Img_Player, Txt_Value,
    /// Txt_Soft_Currency, Txt_Premium, Txt_Timer, But_Settings) — it only repositions
    /// and recolours them. Real "+" buttons are added and self-wire at runtime via
    /// ShowEnergyStoreMB. Run: AQ/Setup/Rebuild HUD (Components).
    /// </summary>
    public static class RebuildHudComponents
    {
        const string TopBar = "Assets/Art/UI/TopBar/";
        const string Gen    = "gen_hud_";

        // ---- layout (HUDImage local space, center anchor; x:-540..540, y:0 = middle) ----
        const float RowY       = 0f;
        const float PortraitX  = -418f;
        const float FrameSize  = 122f;

        static readonly float[] PillX  = { -210f, 40f, 290f };   // energy, soft, premium
        const float PillW = 160f, PillH = 62f;
        const float IconDX = -95f, IconSize = 54f;
        const float ValueDX = 8f;
        const float PlusDX = 100f, PlusSize = 50f;
        const float SettingsX = 462f, SettingsY = 0f, SettingsSize = 88f;

        [MenuItem("AQ/Setup/Rebuild HUD (Components)")]
        public static void Rebuild()
        {
            var hud = GameObject.Find("HUDImage");
            if (hud == null) { Debug.LogWarning("[HUDRebuild] HUDImage not found in open scene."); return; }
            var hudRt = (RectTransform)hud.transform;

            Undo.RegisterFullObjectHierarchyUndo(hud, "Rebuild HUD");

            // 1) Tear down previously generated children (idempotent re-run).
            for (int i = hudRt.childCount - 1; i >= 0; i--)
            {
                var c = hudRt.GetChild(i);
                if (c.name.StartsWith(Gen)) Object.DestroyImmediate(c.gameObject);
            }

            // 2) Normalize the container: it ships at scale y=0.75 (which squishes
            // every child, incl. the portrait). Bake that into height (320*0.75=240)
            // and reset scale to 1 so the new components render undistorted.
            hudRt.localScale = Vector3.one;
            hudRt.sizeDelta  = new Vector2(hudRt.sizeDelta.x, 240f);

            // Backing: swap the baked HUD3 sprite for a dark rounded panel.
            var rounded = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/App/UI/aq_rounded.png");
            var hudImg = hud.GetComponent<Image>();
            if (hudImg != null && rounded != null)
            {
                hudImg.sprite = rounded;
                hudImg.type   = Image.Type.Sliced;
                hudImg.pixelsPerUnitMultiplier = 1.5f;
                hudImg.color  = AQTheme.Panel;
            }

            // The kit's ui_meter_pill_9s is a square sprite with heavy transparent
            // padding and no usable 9-slice data, so it can't stretch into a clean
            // pill. Build the pill from AQTheme.Rounded (proper 9-slice) instead —
            // same cream body + teal outline the kit intends, crisp at any size.
            var frame  = LoadSpriteSingle(TopBar + "ui_top_avatar_frame.png");
            var icons  = new[]
            {
                LoadSpriteSingle(TopBar + "ui_top_energy.png"),
                LoadSpriteSingle(TopBar + "ui_top_soft.png"),
                LoadSpriteSingle(TopBar + "ui_top_premium.png"),
            };

            // 3) Avatar frame behind the portrait.
            MakeImage(hudRt, Gen + "avatar_frame", frame, Color.white, PortraitX, RowY, FrameSize, FrameSize);

            // 4) Three pills + icons + plus buttons.
            string[] valueNames = { "Txt_Value", "Txt_Soft_Currency", "Txt_Premium" };
            bool[]   plusLive    = { true, false, true }; // energy+, soft(info-only), premium+
            for (int i = 0; i < 3; i++)
            {
                float px = PillX[i];
                // Teal outline underlay + cream body = the kit's pill look, cleanly.
                MakeImage(hudRt, Gen + "pillbg_" + i, AQTheme.Rounded, AQTheme.Teal,  px, RowY, PillW + 6f, PillH + 6f);
                MakeImage(hudRt, Gen + "pill_"   + i, AQTheme.Rounded, AQTheme.Paper, px, RowY, PillW, PillH);
                MakeImage(hudRt, Gen + "icon_"   + i, icons[i], Color.white, px + IconDX, RowY, IconSize, IconSize);
                MakePlus(hudRt, Gen + "plus_" + i, px + PlusDX, RowY, plusLive[i]);
                ReseatValue(hud.transform, valueNames[i], px + ValueDX, RowY);
            }

            // 5) Timer under the energy pill.
            ReseatTimer(hud.transform, "Txt_Timer", PillX[0], RowY - 56f);

            // 6) Portrait + settings gear on top; relabel settings to a gear glyph.
            BringToFront(hud.transform, "Img_Player");
            StyleSettingsGear(hud.transform, "But_Settings");

            EditorUtility.SetDirty(hud);
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("[HUDRebuild] Component HUD rebuilt.");
        }

        // ---------- helpers ----------

        static Sprite LoadSpriteSingle(string path, Vector4 border = default)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool dirty = false;
                if (importer.textureType != TextureImporterType.Sprite)
                { importer.textureType = TextureImporterType.Sprite; dirty = true; }
                if (importer.spriteImportMode != SpriteImportMode.Single)
                { importer.spriteImportMode = SpriteImportMode.Single; dirty = true; }
                if (border != default && importer.spriteBorder != border)
                { importer.spriteBorder = border; dirty = true; }
                if (dirty) importer.SaveAndReimport();
            }
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s == null) Debug.LogWarning($"[HUDRebuild] sprite missing: {path}");
            return s;
        }

        static Image MakeImage(RectTransform parent, string name, Sprite sprite, Color color,
                               float x, float y, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            Center(rt, x, y, w, h);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.type = sprite != null && sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
            img.preserveAspect = sprite != null && sprite.border == Vector4.zero;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        static void MakePlus(RectTransform parent, string name, float x, float y, bool live)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            Center(rt, x, y, PlusSize, PlusSize);

            var img = go.GetComponent<Image>();
            img.sprite = AQTheme.Rounded;
            img.type   = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 0.5f; // corners overrun -> circle
            img.color  = live ? new Color32(60, 140, 235, 255) : new Color32(90, 100, 120, 255);

            var lblGo = new GameObject("Label", typeof(RectTransform));
            lblGo.transform.SetParent(rt, false);
            var lrt = (RectTransform)lblGo.transform;
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero; lrt.offsetMax = new Vector2(0f, -4f);
            var tmp = lblGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "+";
            tmp.enableAutoSizing = false;
            tmp.fontSize = 40f;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            AQTheme.StyleText(tmp, display: true);

            var btn = go.GetComponent<Button>();
            btn.interactable = live;
            if (live) go.AddComponent<ShowEnergyStoreMB>();
        }

        static void ReseatValue(Transform hud, string name, float x, float y)
        {
            var t = hud.Find(name);
            if (t == null) { Debug.LogWarning($"[HUDRebuild] {name} missing"); return; }
            var rt = (RectTransform)t;
            Center(rt, x, y, 150f, 56f);
            var tmp = t.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = AQTheme.Navy;      // dark text reads on the cream pill
                tmp.enableAutoSizing = false;
                tmp.fontSize = 34f;
                tmp.margin = Vector4.zero;     // clear the old full-width layout margins
            }
            rt.SetAsLastSibling();
        }

        static void ReseatTimer(Transform hud, string name, float x, float y)
        {
            var t = hud.Find(name);
            if (t == null) return;
            var rt = (RectTransform)t;
            Center(rt, x, y, 240f, 28f);
            var tmp = t.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = AQTheme.PaperDim;
                tmp.fontSize = 22f;
                tmp.margin = Vector4.zero;
            }
            rt.SetAsLastSibling();
        }

        static void BringToFront(Transform hud, string name)
        {
            var t = hud.Find(name);
            if (t != null) t.SetAsLastSibling();
        }

        static void StyleSettingsGear(Transform hud, string name)
        {
            var t = hud.Find(name);
            if (t == null) return;
            var rt = (RectTransform)t;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(SettingsX, SettingsY);
            rt.sizeDelta = new Vector2(SettingsSize, SettingsSize);

            var img = t.GetComponent<Image>();
            if (img != null) { AQTheme.Round(img, AQTheme.Card); }

            // Hide the "MENU" text label; NunitoSans lacks a ⚙ glyph so we draw a
            // reliable 3-bar settings/menu icon from Images (proper gear sprite is
            // still pending art per the audit).
            var tmp = t.GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = string.Empty;

            for (int i = t.childCount - 1; i >= 0; i--)
            {
                var c = t.GetChild(i);
                if (c.name.StartsWith(Gen + "bar")) Object.DestroyImmediate(c.gameObject);
            }
            for (int i = 0; i < 3; i++)
            {
                var bar = new GameObject(Gen + "bar_" + i, typeof(RectTransform), typeof(Image));
                var brt = (RectTransform)bar.transform;
                brt.SetParent(rt, false);
                Center(brt, 0f, 16f - i * 16f, 44f, 7f);
                var bimg = bar.GetComponent<Image>();
                bimg.sprite = AQTheme.Rounded;
                bimg.type = Image.Type.Sliced;
                bimg.pixelsPerUnitMultiplier = 1f;
                bimg.color = AQTheme.Paper;
                bimg.raycastTarget = false;
            }
            rt.SetAsLastSibling();
        }

        static void Center(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
            rt.localScale = Vector3.one;
        }
    }
}
#endif
