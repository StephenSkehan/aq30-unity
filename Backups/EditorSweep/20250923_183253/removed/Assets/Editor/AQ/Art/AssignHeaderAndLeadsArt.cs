#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections.Generic;

namespace AQ.EditorTools.Art
{
    public static class AssignHeaderAndLeadsArt
    {
        // --- SOURCE PATHS ---
        const string TOPBAR = "Assets/Art/UI/TopBar/";
        const string LEADS  = "Assets/Art/UI/Leads/";
        const string PREFABS_LEADCARD = "Assets/UI/Prefabs/LeadCardView.prefab";

        // Top bar sprites
        const string SPR_ENERGY   = TOPBAR + "ui_top_energy.png";
        const string SPR_SOFT     = TOPBAR + "ui_top_soft.png";
        const string SPR_PREMIUM  = TOPBAR + "ui_top_premium.png";
        const string SPR_HOME     = TOPBAR + "ui_home.png";
        const string SPR_HOME_BG  = TOPBAR + "ui_home_bg_9s.png";
        const string SPR_EP_CHIP  = TOPBAR + "ui_meter_pill_9s.png";
        const string SPR_AV_FRAME = TOPBAR + "ui_top_avatar_frame.png";
        const string SPR_AV_POR   = TOPBAR + "ui_top_avatar_portrait_02.png";

        // Leads sprites
        const string SPR_LEAD_BG  = LEADS  + "lead_card_bg_9s.png";
        const string SPR_ACT_INT  = LEADS  + "act_interview.png";

        // 9-slice borders
        static readonly Vector4 BORDER_HOME_BG = new(36,36,36,36);
        static readonly Vector4 BORDER_EP_CHIP = new(36,24,36,24);
        static readonly Vector4 BORDER_LEAD_BG = new(48,40,48,40);

        // ---------- MENUS ----------
        [MenuItem("AQ/Art/Assign Header Icons")]
        public static void AssignHeader() => DoAssignHeader();

        [MenuItem("AQ/Art/Assign Leads Card Art")]
        public static void AssignLeads()  => DoAssignLeads();

        [MenuItem("AQ/Art/Assign ALL (Header + Leads)")]
        public static void AssignAll() { DoAssignHeader(); DoAssignLeads(); }

        [MenuItem("AQ/Art/Set 9-Slice Borders")]
        public static void SetBorders()
        {
            int changed = 0;
            changed += EnsureSpriteHasBorder(SPR_HOME_BG, BORDER_HOME_BG);
            changed += EnsureSpriteHasBorder(SPR_EP_CHIP,  BORDER_EP_CHIP);
            changed += EnsureSpriteHasBorder(SPR_LEAD_BG,  BORDER_LEAD_BG);
            AssetDatabase.SaveAssets();
            Debug.Log($"[AQ Art] 9-slice borders applied: {changed} asset(s).");
        }

        // ---------- IMPLEMENTATION ----------
        static void DoAssignHeader()
        {
            var canvas = GameObject.Find("Canvas_Board");
            if (!canvas) { Warn("Canvas_Board not found."); return; }

            var topBar = FindByHints(canvas.transform, "TopBar");
            if (!topBar) { Warn("TopBar not found under Canvas_Board."); return; }

            // Hints for owners + child image names
            string imgHints = "Img_Icon|Icon|Glyph|Image|Art|Sprite";

            // Energy / Soft / Premium
            FuzzyAssign(topBar, "Energy|Meter_Energy|EnergyMeter", imgHints, SPR_ENERGY,  preserve:true);
            FuzzyAssign(topBar, "Soft|Coin|Gold|Meter_Soft",       imgHints, SPR_SOFT,    preserve:true);
            FuzzyAssign(topBar, "Premium|Gem|Diamond|Ruby",        imgHints, SPR_PREMIUM, preserve:true);

            // Episode chip bg (sliced)
            FuzzyAssign(topBar, "Episode|Chip|Pill", "", SPR_EP_CHIP, asSliced:true, includeOwnerImage:true);

            // Avatar frame + portrait
            FuzzyAssign(topBar, "Avatar|Portrait|Profile|AvatarChip", "", SPR_AV_FRAME, includeOwnerImage:true);
            FuzzyAssign(topBar, "Avatar|Portrait|Profile|AvatarChip", "Img_Avatar|Avatar|Image", SPR_AV_POR, preserve:true);

            // Home glyph + optional BG child named BG / Back / Plate
            var homeOwner = FindByHints(topBar, "Home|Btn_Home|Button_Home");
            var homeGlyph = FindImageBelow(homeOwner, "Glyph|Icon|Image|Art") ?? homeOwner?.GetComponent<Image>();
            SetSprite(homeGlyph, SPR_HOME, preserve:true);
            var homeBG = FindImageBelow(homeOwner, "BG|Back|Plate");
            SetSprite(homeBG, SPR_HOME_BG, asSliced:true, raycast:false);

            Debug.Log("[AQ Art] Header icons assigned.");
        }

        static void DoAssignLeads()
        {
            if (!File.Exists(PREFABS_LEADCARD))
            {
                Warn($"LeadCardView prefab not found at {PREFABS_LEADCARD}");
                return;
            }
            BackupFile(PREFABS_LEADCARD);

            var root = PrefabUtility.LoadPrefabContents(PREFABS_LEADCARD);
            int changes = 0;

            // Root bg or child "BG"
            var rootImg = root.GetComponent<Image>();
            if (rootImg)
            {
                rootImg.sprite = LoadSprite(SPR_LEAD_BG);
                rootImg.type   = Image.Type.Sliced;
                EditorUtility.SetDirty(rootImg);
                changes++;
            }

            var bgChild = FindByHints(root.transform, "BG|Background");
            if (bgChild && bgChild.TryGetComponent(out Image bgImg))
            {
                bgImg.sprite = LoadSprite(SPR_LEAD_BG);
                bgImg.type   = Image.Type.Sliced;
                EditorUtility.SetDirty(bgImg);
                changes++;
            }

            // Optional action image
            var actImgTf = FindByHints(root.transform, "Img_Action|ActionIcon|Icon");
            if (actImgTf && actImgTf.TryGetComponent(out Image actImg))
            {
                actImg.sprite = LoadSprite(SPR_ACT_INT); // default, swapped at runtime
                actImg.preserveAspect = true;
                EditorUtility.SetDirty(actImg);
                changes++;
            }

            PrefabUtility.SaveAsPrefabAsset(root, PREFABS_LEADCARD);
            PrefabUtility.UnloadPrefabContents(root);

            SetBorders();
            Debug.Log($"[AQ Art] Leads card art assigned. Changes: {changes}");
        }

        // ---------- Search helpers ----------
        static Transform FindByHints(Transform scope, string hints)
        {
            if (!scope || string.IsNullOrEmpty(hints)) return null;
            string[] keys = hints.Split('|');
            var stack = new Stack<Transform>();
            stack.Push(scope);
            while (stack.Count > 0)
            {
                var t = stack.Pop();
                foreach (var k in keys)
                    if (t.name.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0)
                        return t;
                for (int i = 0; i < t.childCount; i++) stack.Push(t.GetChild(i));
            }
            return null;
        }

        static Image FindImageBelow(Transform owner, string imageNameHints)
        {
            if (!owner) return null;
            if (!string.IsNullOrEmpty(imageNameHints))
            {
                string[] keys = imageNameHints.Split('|');
                var stack = new Stack<Transform>();
                stack.Push(owner);
                while (stack.Count > 0)
                {
                    var t = stack.Pop();
                    foreach (var k in keys)
                        if (t.name.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0 && t.TryGetComponent(out Image named)) return named;
                    for (int i = 0; i < t.childCount; i++) stack.Push(t.GetChild(i));
                }
            }
            // fallback: first Image under owner
            return owner.GetComponentInChildren<Image>(true);
        }

        static void FuzzyAssign(Transform topBar, string ownerHints, string imgHints, string spritePath,
                                bool preserve=false, bool asSliced=false, bool raycast=true, bool includeOwnerImage=false)
        {
            var owner = FindByHints(topBar, ownerHints);
            if (!owner) { Warn($"Owner not found for {Path.GetFileName(spritePath)} (hints: {ownerHints})"); return; }

            Image img = includeOwnerImage ? owner.GetComponent<Image>() : null;
            if (!img) img = FindImageBelow(owner, imgHints);
            SetSprite(img, spritePath, preserve, asSliced, raycast);
        }

        // ---------- Setters / IO ----------
        static void SetSprite(Image img, string spritePath, bool preserve=false, bool asSliced=false, bool raycast=true)
        {
            if (!img) { Warn($"Target Image missing for {Path.GetFileName(spritePath)}"); return; }
            var sp = LoadSprite(spritePath);
            if (!sp) return;
            img.sprite = sp;
            img.raycastTarget = raycast;
            if (asSliced) img.type = Image.Type.Sliced;
            if (preserve) img.preserveAspect = true;
            EditorUtility.SetDirty(img);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        static Sprite LoadSprite(string path)
        {
            var sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (!sp) Warn($"Sprite missing: {path}");
            return sp;
        }

        static int EnsureSpriteHasBorder(string assetPath, Vector4 borderLBRT)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (!tex) { Warn($"Texture not found: {assetPath}"); return 0; }
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            if (importer == null) { Warn($"Importer not found: {assetPath}"); return 0; }
            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
            if (importer.spriteBorder != borderLBRT)                 { importer.spriteBorder = borderLBRT;               changed = true; }
            if (!importer.alphaIsTransparency)                       { importer.alphaIsTransparency = true;              changed = true; }
            if (changed) { importer.SaveAndReimport(); return 1; }
            return 0;
        }

        static void BackupFile(string assetPath)
        {
            try {
                var full = Path.GetFullPath(assetPath);
                var dir  = Path.GetDirectoryName(full);
                var name = Path.GetFileNameWithoutExtension(full);
                var ext  = Path.GetExtension(full);
                var stamp= DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var bak  = Path.Combine(dir!, $"{name}.bak_{stamp}{ext}");
                File.Copy(full, bak, false);
                Debug.Log($"[AQ Art] Backed up prefab -> {bak}");
            } catch (Exception ex) { Warn($"Backup failed for {assetPath}: {ex.Message}"); }
        }

        static void Warn(string msg) => Debug.LogWarning($"[AQ Art] {msg}");
    }
}
#endif
