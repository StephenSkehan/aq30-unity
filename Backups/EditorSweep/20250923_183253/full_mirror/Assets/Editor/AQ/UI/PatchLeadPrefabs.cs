#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.EditorTools.UI
{
    public static class PatchLeadPrefabs
    {
        private const string CardPrefabPath = "Assets/UI/Prefabs/LeadCardView.prefab";
        private const string ReqPrefabPath  = "Assets/UI/Prefabs/ReqItem.prefab";
        private const string LeadCardViewTypeName = "AQ.App.Leads.LeadCardView";
        private const string RequirementItemTypeName = "AQ.App.Leads.LeadRequirementItem";

        [MenuItem("AQ/UI/Fix → Rebuild LeadCard & ReqItem prefabs")]
        public static void Run()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode", "Exit Play Mode first.", "OK");
                return;
            }

            // ---------- 1) Build / fix ReqItem.prefab ----------
            EnsureFolder("Assets/UI/Prefabs");
            var reqAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ReqPrefabPath);
            if (reqAsset == null)
            {
                var temp = new GameObject("ReqItem", typeof(RectTransform));
                PrefabUtility.SaveAsPrefabAsset(temp, ReqPrefabPath);
                Object.DestroyImmediate(temp);
            }

            {
                var root = PrefabUtility.LoadPrefabContents(ReqPrefabPath);
                root.name = "ReqItem";

                var rt = root.GetComponent<RectTransform>() ?? root.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(460, 56);

                var icon  = GetOrCreate(root.transform, "Icon",  typeof(RectTransform), typeof(Image));
                var label = GetOrCreate(root.transform, "Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                var check = GetOrCreate(root.transform, "Check", typeof(RectTransform), typeof(Image));

                var h = root.GetComponent<HorizontalLayoutGroup>() ?? root.AddComponent<HorizontalLayoutGroup>();
                h.childAlignment = TextAnchor.MiddleLeft;
                h.childForceExpandWidth = false; h.childForceExpandHeight = false;
                h.childControlWidth = true;  h.childControlHeight = true;
                h.spacing = 12; h.padding = new RectOffset(8, 8, 6, 6);

                var iconRT = icon.GetComponent<RectTransform>(); iconRT.sizeDelta = new Vector2(48, 48);

                var tmp = label.GetComponent<TextMeshProUGUI>();
                SetupTMP(tmp, "Requirement", 24, TextAlignmentOptions.MidlineLeft, wrap:false, dim:1f);

                var checkRT = check.GetComponent<RectTransform>(); checkRT.sizeDelta = new Vector2(28, 28);
                var chkImg = check.GetComponent<Image>(); chkImg.color = new Color(0.9f, 1f, 0.9f, 1f);

                // Add the requirement component we just created
                var reqType = System.Type.GetType(RequirementItemTypeName + ", Assembly-CSharp")
                             ?? System.Type.GetType(RequirementItemTypeName);
                if (reqType == null)
                {
                    EditorUtility.DisplayDialog("Missing type",
                        $"Could not find requirement component type:\n- {RequirementItemTypeName}\n\n" +
                        "Make sure Assets/App/Leads/LeadRequirementItem.cs exists and compiles.", "OK");
                    PrefabUtility.UnloadPrefabContents(root);
                    return;
                }
                var reqComp = root.GetComponent(reqType) ?? root.AddComponent(reqType);
                SetIfFieldExists(reqComp, "icon",  icon.GetComponent<Image>());
                SetIfFieldExists(reqComp, "label", label.GetComponent<TextMeshProUGUI>());
                SetIfFieldExists(reqComp, "check", check.GetComponent<Image>());

                PrefabUtility.SaveAsPrefabAsset(root, ReqPrefabPath);
                PrefabUtility.UnloadPrefabContents(root);
                Debug.Log("[PatchLeadPrefabs] Rebuilt ReqItem.prefab.");
            }

            // ---------- 2) Build / fix LeadCardView.prefab ----------
            var cardGo = AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath);
            if (cardGo == null)
            {
                EditorUtility.DisplayDialog("Missing prefab",
                    $"Create a basic card prefab at:\n{CardPrefabPath}\nwith a LeadCardView component.", "OK");
                return;
            }

            {
                var root = PrefabUtility.LoadPrefabContents(CardPrefabPath);
                root.name = "LeadCardView";

                var bgImg = root.GetComponent<Image>() ?? root.AddComponent<Image>();
                bgImg.color = new Color(1, 1, 1, 0.07f);

                var le = root.GetComponent<LayoutElement>() ?? root.AddComponent<LayoutElement>();
                le.preferredWidth = 540f;
                le.preferredHeight = 300f;

                var padding = root.GetComponent<VerticalLayoutGroup>() ?? root.AddComponent<VerticalLayoutGroup>();
                padding.childAlignment = TextAnchor.UpperLeft;
                padding.childControlHeight = true; padding.childControlWidth = true;
                padding.childForceExpandHeight = false; padding.childForceExpandWidth = false;
                padding.padding = new RectOffset(16, 16, 16, 16);
                padding.spacing = 8f;

                var portrait = GetOrCreate(root.transform, "Portrait", typeof(RectTransform), typeof(Image));
                var title    = GetOrCreate(root.transform, "Title", typeof(RectTransform), typeof(TextMeshProUGUI));
                var action   = GetOrCreate(root.transform, "Action", typeof(RectTransform), typeof(TextMeshProUGUI));
                var oneLiner = GetOrCreate(root.transform, "OneLiner", typeof(RectTransform), typeof(TextMeshProUGUI));
                var reqs     = GetOrCreate(root.transform, "Requirements", typeof(RectTransform));
                var proceed  = GetOrCreate(root.transform, "Proceed", typeof(RectTransform), typeof(Image), typeof(Button));

                portrait.GetComponent<RectTransform>().sizeDelta = new Vector2(220, 140);
                portrait.GetComponent<Image>().color = new Color(1,1,1,0.12f);

                SetupTMP(title.GetComponent<TextMeshProUGUI>(), "Interview: Lena's Neighbor", 28, TextAlignmentOptions.TopLeft, wrap:true);
                SetupTMP(action.GetComponent<TextMeshProUGUI>(), "Action: INTERVIEW", 22, TextAlignmentOptions.TopLeft, wrap:false, dim:0.8f);
                SetupTMP(oneLiner.GetComponent<TextMeshProUGUI>(), "Get eyes-on account of the night.", 22, TextAlignmentOptions.TopLeft, wrap:true, dim:0.9f);

                var v = reqs.GetComponent<VerticalLayoutGroup>() ?? reqs.gameObject.AddComponent<VerticalLayoutGroup>();
                v.childAlignment = TextAnchor.UpperLeft;
                v.childControlHeight = true; v.childControlWidth = true;
                v.childForceExpandHeight = false; v.childForceExpandWidth = false;
                v.spacing = 6f; v.padding = new RectOffset(0,0,0,0);
                var csf = reqs.GetComponent<ContentSizeFitter>() ?? reqs.gameObject.AddComponent<ContentSizeFitter>();
                csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

                var pb = proceed.GetComponent<Button>();
                proceed.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.9f, 0.9f);
                var pbText = proceed.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
                if (!pbText)
                {
                    var go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                    go.transform.SetParent(proceed.transform, false);
                    var t = go.GetComponent<TextMeshProUGUI>();
                    SetupTMP(t, "PROCEED", 22, TextAlignmentOptions.Center, wrap:false);
                    var tRT = go.GetComponent<RectTransform>();
                    tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one; tRT.offsetMin = Vector2.zero; tRT.offsetMax = Vector2.zero;
                }
                proceed.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 64);

                var cardType = System.Type.GetType(LeadCardViewTypeName + ", Assembly-CSharp") ??
                               System.Type.GetType(LeadCardViewTypeName);
                if (cardType == null)
                {
                    EditorUtility.DisplayDialog("Missing type", $"Could not find {LeadCardViewTypeName}.", "OK");
                    PrefabUtility.UnloadPrefabContents(root);
                    return;
                }
                var lcv = root.GetComponent(cardType) ?? root.AddComponent(cardType);

                SetIfFieldExists(lcv, "portraitImage", portrait.GetComponent<Image>());
                SetIfFieldExists(lcv, "titleText",     title.GetComponent<TextMeshProUGUI>());
                SetIfFieldExists(lcv, "actionText",    action.GetComponent<TextMeshProUGUI>());
                SetIfFieldExists(lcv, "oneLinerText",  oneLiner.GetComponent<TextMeshProUGUI>());
                SetIfFieldExists(lcv, "requirementsRoot", reqs);
                SetIfFieldExists(lcv, "proceedButton", proceed.GetComponent<Button>());

                var reqPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ReqPrefabPath);
                SetIfFieldExists(lcv, "requirementItemPrefab", reqPrefab);

                PrefabUtility.SaveAsPrefabAsset(root, CardPrefabPath);
                PrefabUtility.UnloadPrefabContents(root);
                Debug.Log("[PatchLeadPrefabs] Normalized LeadCardView.prefab (background, layout, Requirements, wiring).");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("[PatchLeadPrefabs] ✅ Done. Play the board scene to see readable cards and requirement rows.");
        }

        // ---------- helpers ----------
        private static GameObject GetOrCreate(Transform parent, string name, params System.Type[] components)
        {
            var t = parent.Find(name);
            if (t) return t.gameObject;
            var go = new GameObject(name, components);
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void SetupTMP(TextMeshProUGUI tmp, string text, float baseSize, TextAlignmentOptions align, bool wrap, float dim = 1f)
        {
            tmp.text = text;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = Mathf.Max(16, baseSize - 12);
            tmp.fontSizeMax = Mathf.Max(baseSize, baseSize + 10);
            tmp.alignment = align;
            tmp.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            tmp.overflowMode = wrap ? TextOverflowModes.Overflow : TextOverflowModes.Ellipsis;
            tmp.color = new Color(dim, dim, dim, 1f);
        }

        private static void SetIfFieldExists(Component target, string field, object value)
        {
            if (target == null) return;
            var f = target.GetType().GetField(field);
            if (f != null) f.SetValue(target, value);
        }

        private static void EnsureFolder(string path)
        {
            var parts = path.Split('/');
            var cur = "";
            for (int i = 0; i < parts.Length; i++)
            {
                cur = i == 0 ? parts[0] : $"{cur}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(cur))
                {
                    var parent = System.IO.Path.GetDirectoryName(cur).Replace("\\", "/");
                    var leaf = System.IO.Path.GetFileName(cur);
                    AssetDatabase.CreateFolder(parent, leaf);
                }
            }
        }
    }
}
#endif
