#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Our runtime components
using AQ.App.Leads;  // LeadCardView, LeadRequirementItem
using AQ.App.Home;   // HomeResumeCard

namespace AQ.EditorTools.UI
{
    public static class PrefabBuilder
    {
        private const string PrefabDir = "Assets/UI/Prefabs";
        private static Sprite s_UISprite;
        private static Sprite s_UISpriteKnob;
        private static TMP_FontAsset s_DefaultTmpFont;

        [MenuItem("AQ/UI/Build Prefabs/Build All (ReqItem, LeadCardView, HomeResumeCard)")]
        public static void BuildAll()
        {
            EnsureFolder();

            // cache built-in sprites/fonts
            s_UISprite      = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            s_UISpriteKnob  = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            s_DefaultTmpFont = TMP_Settings.defaultFontAsset; // should exist once TMP essentials are imported

            var reqItem = Build_ReqItem();
            var leadCard = Build_LeadCardView(reqItem);
            var homeResume = Build_HomeResumeCard();

            Debug.Log($"[PrefabBuilder] Done.\n - {AssetDatabase.GetAssetPath(reqItem)}\n - {AssetDatabase.GetAssetPath(leadCard)}\n - {AssetDatabase.GetAssetPath(homeResume)}");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("AQ/UI/Build Prefabs/Build ReqItem only")]
        public static void Build_ReqItem_Menu()
        {
            EnsureFolder();
            var p = Build_ReqItem();
            Debug.Log($"[PrefabBuilder] ReqItem -> {AssetDatabase.GetAssetPath(p)}");
        }

        [MenuItem("AQ/UI/Build Prefabs/Build LeadCardView only")]
        public static void Build_LeadCardView_Menu()
        {
            EnsureFolder();
            var reqItem = LoadPrefab("ReqItem.prefab") ?? Build_ReqItem();
            var p = Build_LeadCardView(reqItem);
            Debug.Log($"[PrefabBuilder] LeadCardView -> {AssetDatabase.GetAssetPath(p)}");
        }

        [MenuItem("AQ/UI/Build Prefabs/Build HomeResumeCard only")]
        public static void Build_HomeResumeCard_Menu()
        {
            EnsureFolder();
            var p = Build_HomeResumeCard();
            Debug.Log($"[PrefabBuilder] HomeResumeCard -> {AssetDatabase.GetAssetPath(p)}");
        }

        // ---------- builders ----------

        private static GameObject Build_ReqItem()
        {
            var root = CreateUIPanel("ReqItem", new Vector2(180, 72));

            var layout = root.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.spacing = 8;
            layout.childControlWidth = true; layout.childControlHeight = true;
            layout.childForceExpandWidth = false; layout.childForceExpandHeight = false;
            var fitter = root.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

            var icon = CreateImage("Icon", root.transform, new Vector2(64, 64));
            icon.sprite = s_UISpriteKnob;

            var label = CreateTMP("Label", root.transform, 24, TextAlignmentOptions.MidlineLeft);
            label.enableAutoSizing = true;
            label.fontSizeMin = 18; label.fontSizeMax = 28;
            label.rectTransform.sizeDelta = new Vector2(80, 56);

            var check = CreateImage("Check", root.transform, new Vector2(24, 24));
            check.sprite = s_UISprite;
            check.enabled = false;

            var comp = root.AddComponent<LeadRequirementItem>();
            SetSerialized(comp, "icon", icon);
            SetSerialized(comp, "label", label);
            SetSerialized(comp, "checkmark", check);

            var path = $"{PrefabDir}/ReqItem.prefab";
            var prefab = SavePrefab(root, path);
            return prefab;
        }

        private static GameObject Build_LeadCardView(GameObject reqItemPrefab)
        {
            var root = CreateUIPanel("LeadCard", new Vector2(600, 300));
            var bg = root.GetComponent<Image>(); bg.sprite = s_UISprite;

            var card = root.AddComponent<LeadCardView>();

            // Portrait
            var portrait = CreateImage("Portrait", root.transform, new Vector2(180, 180));
            portrait.rectTransform.anchorMin = new Vector2(0, 1);
            portrait.rectTransform.anchorMax = new Vector2(0, 1);
            portrait.rectTransform.pivot     = new Vector2(0, 1);
            portrait.rectTransform.anchoredPosition = new Vector2(16, -16);
            portrait.sprite = s_UISpriteKnob;

            // Title
            var title = CreateTMP("Text_Title", root.transform, 34, TextAlignmentOptions.TopLeft);
            title.rectTransform.anchorMin = new Vector2(0, 1);
            title.rectTransform.anchorMax = new Vector2(1, 1);
            title.rectTransform.pivot     = new Vector2(0, 1);
            title.rectTransform.anchoredPosition = new Vector2(208, -16);
            title.rectTransform.sizeDelta = new Vector2(-224, 48);
            title.text = "Lead Title";

            // Action tag
            var action = CreateTMP("Text_ActionTag", root.transform, 22, TextAlignmentOptions.TopLeft);
            action.color = new Color(0.75f, 0.85f, 1f, 1f);
            action.rectTransform.anchorMin = new Vector2(0, 1);
            action.rectTransform.anchorMax = new Vector2(1, 1);
            action.rectTransform.pivot     = new Vector2(0, 1);
            action.rectTransform.anchoredPosition = new Vector2(208, -64);
            action.rectTransform.sizeDelta = new Vector2(-224, 40);
            action.text = "SURVEILLANCE";

            // One-liner
            var one = CreateTMP("Text_OneLiner", root.transform, 26, TextAlignmentOptions.TopLeft);
            one.enableAutoSizing = true; one.fontSizeMin = 20; one.fontSizeMax = 28;
            one.rectTransform.anchorMin = new Vector2(0, 1);
            one.rectTransform.anchorMax = new Vector2(1, 1);
            one.rectTransform.pivot     = new Vector2(0, 1);
            one.rectTransform.anchoredPosition = new Vector2(208, -100);
            one.rectTransform.sizeDelta = new Vector2(-224, 70);
            one.text = "Short lead description.";

            // Cost
            var cost = CreateTMP("Text_Cost", root.transform, 22, TextAlignmentOptions.TopRight);
            cost.rectTransform.anchorMin = new Vector2(0, 1);
            cost.rectTransform.anchorMax = new Vector2(1, 1);
            cost.rectTransform.pivot     = new Vector2(1, 0.5f);
            cost.rectTransform.anchoredPosition = new Vector2(-16, -24);
            cost.rectTransform.sizeDelta = new Vector2(220, 40);
            cost.text = "No Cost";

            // Requirements container
            var reqs = new GameObject("Requirements", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            reqs.transform.SetParent(root.transform, false);
            var reqRect = reqs.GetComponent<RectTransform>();
            reqRect.anchorMin = new Vector2(0, 0);
            reqRect.anchorMax = new Vector2(1, 0);
            reqRect.pivot     = new Vector2(0, 0);
            reqRect.anchoredPosition = new Vector2(16, 72);
            reqRect.sizeDelta = new Vector2(-32, 80);
            var rLayout = reqs.GetComponent<HorizontalLayoutGroup>();
            rLayout.padding = new RectOffset(0, 0, 0, 0);
            rLayout.spacing = 12;
            rLayout.childControlWidth = false; rLayout.childControlHeight = true;
            rLayout.childForceExpandWidth = false; rLayout.childForceExpandHeight = false;

            // Badges row
            var badges = new GameObject("Badges", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            badges.transform.SetParent(root.transform, false);
            var bRect = badges.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0, 0);
            bRect.anchorMax = new Vector2(0, 0);
            bRect.pivot     = new Vector2(0, 0);
            bRect.anchoredPosition = new Vector2(16, 16);
            bRect.sizeDelta = new Vector2(120, 32);
            var bLayout = badges.GetComponent<HorizontalLayoutGroup>();
            bLayout.spacing = 8;

            var badgeEvidence = CreateImage("Badge_Evidence", badges.transform, new Vector2(24, 24)); badgeEvidence.sprite = s_UISprite;
            var badgeNewLeads = CreateImage("Badge_NewLeads", badges.transform, new Vector2(24, 24)); badgeNewLeads.sprite = s_UISprite;
            var badgeRewards  = CreateImage("Badge_Rewards",  badges.transform, new Vector2(24, 24)); badgeRewards.sprite = s_UISprite;

            // Proceed button
            var btnGO = new GameObject("Button_Proceed", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(root.transform, false);
            var btnRect = btnGO.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(1, 0);
            btnRect.anchorMax = new Vector2(1, 0);
            btnRect.pivot     = new Vector2(1, 0);
            btnRect.anchoredPosition = new Vector2(-16, 16);
            btnRect.sizeDelta = new Vector2(160, 56);
            btnGO.GetComponent<Image>().sprite = s_UISprite;
            var btnLabel = CreateTMP("Text_Proceed", btnGO.transform, 26, TextAlignmentOptions.Center);
            btnLabel.rectTransform.anchorMin = Vector2.zero;
            btnLabel.rectTransform.anchorMax = Vector2.one;
            btnLabel.rectTransform.offsetMin = Vector2.zero;
            btnLabel.rectTransform.offsetMax = Vector2.zero;
            btnLabel.text = "Proceed";

            // Wire LeadCardView
            SetSerialized(card, "portraitImage", portrait);
            SetSerialized(card, "titleText", title);
            SetSerialized(card, "actionTagText", action);
            SetSerialized(card, "oneLinerText", one);
            SetSerialized(card, "costText", cost);
            SetSerialized(card, "requirementsRoot", reqs.transform);
            SetSerialized(card, "requirementItemPrefab", reqItemPrefab.GetComponent<LeadRequirementItem>());
            SetSerialized(card, "badgeEvidence", badgeEvidence.gameObject);
            SetSerialized(card, "badgeNewLeads", badgeNewLeads.gameObject);
            SetSerialized(card, "badgeRewards",  badgeRewards.gameObject);
            SetSerialized(card, "proceedButton", btnGO.GetComponent<Button>());

            var path = $"{PrefabDir}/LeadCardView.prefab";
            var prefab = SavePrefab(root, path);
            return prefab;
        }

        private static GameObject Build_HomeResumeCard()
        {
            var root = CreateUIPanel("HomeResumeCard", new Vector2(900, 280));
            var bg = root.GetComponent<Image>(); bg.sprite = s_UISprite;

            var comp = root.AddComponent<HomeResumeCard>();

            // Big button filling card
            var btnGO = new GameObject("Button_Resume", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(root.transform, false);
            var rect = btnGO.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            btnGO.GetComponent<Image>().color = new Color(1,1,1,0); // transparent hit area

            var title = CreateTMP("Text_Title", root.transform, 44, TextAlignmentOptions.Center);
            title.rectTransform.anchorMin = new Vector2(0, 0.5f);
            title.rectTransform.anchorMax = new Vector2(1, 0.5f);
            title.rectTransform.anchoredPosition = new Vector2(0, 36);
            title.rectTransform.sizeDelta = new Vector2(0, 60);
            title.text = "Resume Case";

            var sub = CreateTMP("Text_Subline", root.transform, 28, TextAlignmentOptions.Center);
            sub.color = new Color(1,1,1,0.85f);
            sub.rectTransform.anchorMin = new Vector2(0, 0.5f);
            sub.rectTransform.anchorMax = new Vector2(1, 0.5f);
            sub.rectTransform.anchoredPosition = new Vector2(0, -28);
            sub.rectTransform.sizeDelta = new Vector2(0, 50);
            sub.text = "Continue from Leads";

            SetSerialized(comp, "titleText", title);
            SetSerialized(comp, "sublineText", sub);
            SetSerialized(comp, "resumeButton", btnGO.GetComponent<Button>());

            var path = $"{PrefabDir}/HomeResumeCard.prefab";
            var prefab = SavePrefab(root, path);
            return prefab;
        }

        // ---------- helpers ----------

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/UI")) AssetDatabase.CreateFolder("Assets", "UI");
            if (!AssetDatabase.IsValidFolder(PrefabDir))  AssetDatabase.CreateFolder("Assets/UI", "Prefabs");
        }

        private static GameObject CreateUIPanel(string name, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(CanvasRenderer));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            var img = go.GetComponent<Image>();
            img.sprite = s_UISprite;
            img.type   = Image.Type.Sliced;
            return go;
        }

        private static Image CreateImage(string name, Transform parent, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.sprite = s_UISprite;
            img.type   = Image.Type.Sliced;
            return img;
        }

        private static TextMeshProUGUI CreateTMP(string name, Transform parent, float fontSize, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = name;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            if (s_DefaultTmpFont != null) tmp.font = s_DefaultTmpFont;
            return tmp;
        }

        private static GameObject SavePrefab(GameObject root, string path)
        {
            var folder = Path.GetDirectoryName(path);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject LoadPrefab(string name)
        {
            var path = $"{PrefabDir}/{name}";
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        // Set private serialized field by name to avoid manual inspector wiring
        private static void SetSerialized(Object target, string fieldName, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[PrefabBuilder] Property '{fieldName}' not found on {target.name} ({target.GetType().Name}).");
                return;
            }
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
