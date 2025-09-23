#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.EditorTools.UI
{
    public static class FixPrefabs_Minimal
    {
        // Paths we care about
        private const string ReqItemPath = "Assets/UI/Prefabs/ReqItem.prefab";
        private const string CardPath    = "Assets/UI/Prefabs/LeadCardView.prefab";

        // Type names we must not hard-reference (to avoid asmdef surprises)
        private const string LeadRequirementItemType = "AQ.App.Leads.LeadRequirementItem";
        private const string LeadCardViewType        = "AQ.App.Leads.LeadCardView";

        // ---------- MENUS ----------
        [MenuItem("AQ/UI/Fix → Minimal Prefab Repair (ReqItem + LeadCard)")]
        public static void RunMinimalFix()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode", "Exit Play Mode first.", "OK");
                return;
            }

            EnsureFolder("Assets/UI/Prefabs");

            bool reqChanged  = FixReqItem();
            bool cardChanged = FixLeadCard();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.MarkAllScenesDirty();

            Debug.Log($"[AQ Fix] Done. ReqItem={(reqChanged ? "CHANGED" : "OK")} LeadCard={(cardChanged ? "CHANGED" : "OK")}. Press Play.");
        }

        [MenuItem("AQ/UI/Fix → Quick Prefab Audit")]
        public static void QuickAudit()
        {
            string req  = System.IO.File.Exists(ReqItemPath) ? ReqItemPath : "(missing)";
            string card = System.IO.File.Exists(CardPath)    ? CardPath    : "(missing)";
            Debug.Log($"[AQ Audit] ReqItem: {req}\n[AQ Audit] LeadCard: {card}");
        }

        // ---------- REQ ITEM ----------
        private static bool FixReqItem()
        {
            // Create prefab shell if missing
            if (!System.IO.File.Exists(ReqItemPath))
            {
                var go = new GameObject("ReqItem", typeof(RectTransform));
                PrefabUtility.SaveAsPrefabAsset(go, ReqItemPath);
                UnityEngine.Object.DestroyImmediate(go);
            }

            var root = PrefabUtility.LoadPrefabContents(ReqItemPath);
            bool changed = false;

            // Remove any Missing Scripts
            changed |= RemoveMissingScriptsRecursive(root);

            root.name = "ReqItem";
            var rt = root.GetComponent<RectTransform>() ?? root.AddComponent<RectTransform>();
            if (rt.sizeDelta != new Vector2(460, 56)) { rt.sizeDelta = new Vector2(460, 56); changed = true; }

            // Children
            var iconGO  = GetOrCreate(root.transform, "Icon",  typeof(RectTransform), typeof(Image), ref changed);
            var labelGO = GetOrCreate(root.transform, "Label", typeof(RectTransform), typeof(TextMeshProUGUI), ref changed);
            var checkGO = GetOrCreate(root.transform, "Check", typeof(RectTransform), typeof(Image), ref changed);

            // Layout
            var h = root.GetComponent<HorizontalLayoutGroup>() ?? root.AddComponent<HorizontalLayoutGroup>();
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childForceExpandWidth = false; h.childForceExpandHeight = false;
            h.childControlWidth = true; h.childControlHeight = true;
            h.spacing = 12; h.padding = new RectOffset(8, 8, 6, 6);

            var iconRT = iconGO.GetComponent<RectTransform>(); if (iconRT.sizeDelta != new Vector2(48, 48)) { iconRT.sizeDelta = new Vector2(48, 48); changed = true; }

            var tmp = labelGO.GetComponent<TextMeshProUGUI>();
            changed |= SetupTMP(tmp, "Requirement", 24, TextAlignmentOptions.MidlineLeft, wrap:false, dim:1f);

            var checkRT = checkGO.GetComponent<RectTransform>(); if (checkRT.sizeDelta != new Vector2(28, 28)) { checkRT.sizeDelta = new Vector2(28, 28); changed = true; }
            var chkImg = checkGO.GetComponent<Image>(); if (chkImg.color != new Color(0.9f, 1f, 0.9f, 1f)) { chkImg.color = new Color(0.9f, 1f, 0.9f, 1f); changed = true; }

            // Ensure LeadRequirementItem exists and fields are wired
            var reqType = FindType(LeadRequirementItemType);
            if (reqType == null)
            {
                EditorUtility.DisplayDialog("Missing type",
                    $"Could not find {LeadRequirementItemType}.\nMake sure your script exists and compiles.", "OK");
                PrefabUtility.UnloadPrefabContents(root);
                return changed;
            }
            var reqComp = root.GetComponent(reqType) ?? root.AddComponent(reqType);
            changed |= SetIfFieldChanged(reqComp, "icon",  iconGO.GetComponent<Image>());
            changed |= SetIfFieldChanged(reqComp, "label", labelGO.GetComponent<TextMeshProUGUI>());
            changed |= SetIfFieldChanged(reqComp, "check", checkGO.GetComponent<Image>());

            PrefabUtility.SaveAsPrefabAsset(root, ReqItemPath);
            PrefabUtility.UnloadPrefabContents(root);
            return changed;
        }

        // ---------- LEAD CARD ----------
        private static bool FixLeadCard()
        {
            if (!System.IO.File.Exists(CardPath))
            {
                EditorUtility.DisplayDialog("Missing prefab",
                    $"Create LeadCard prefab at:\n{CardPath}\nwith a LeadCardView component.", "OK");
                return false;
            }

            var root = PrefabUtility.LoadPrefabContents(CardPath);
            bool changed = false;

            // Remove any Missing Scripts
            changed |= RemoveMissingScriptsRecursive(root);

            root.name = "LeadCardView";

            // Background & sizing
            var img = root.GetComponent<Image>() ?? root.AddComponent<Image>();
            var wantBG = new Color(1, 1, 1, 0.07f);
            if (img.color != wantBG) { img.color = wantBG; changed = true; }

            var le = root.GetComponent<LayoutElement>() ?? root.AddComponent<LayoutElement>();
            if (le.preferredWidth != 540f)  { le.preferredWidth = 540f;  changed = true; }
            if (le.preferredHeight != 300f) { le.preferredHeight = 300f; changed = true; }

            var vpad = root.GetComponent<VerticalLayoutGroup>() ?? root.AddComponent<VerticalLayoutGroup>();
            vpad.childAlignment = TextAnchor.UpperLeft;
            vpad.childControlHeight = true; vpad.childControlWidth = true;
            vpad.childForceExpandHeight = false; vpad.childForceExpandWidth = false;
            vpad.padding = new RectOffset(16, 16, 16, 16);
            vpad.spacing = 8f;

            // Children we expect
            var portrait = GetOrCreate(root.transform, "Portrait", typeof(RectTransform), typeof(Image), ref changed);
            var title    = GetOrCreate(root.transform, "Title",    typeof(RectTransform), typeof(TextMeshProUGUI), ref changed);
            var action   = GetOrCreate(root.transform, "Action",   typeof(RectTransform), typeof(TextMeshProUGUI), ref changed);
            var oneLiner = GetOrCreate(root.transform, "OneLiner", typeof(RectTransform), typeof(TextMeshProUGUI), ref changed);
            var reqs     = GetOrCreate(root.transform, "Requirements", typeof(RectTransform), ref changed);
            var proceed  = GetOrCreate(root.transform, "Proceed",  typeof(RectTransform), typeof(Image), typeof(Button), ref changed);

            // Portrait style
            var prtRT = portrait.GetComponent<RectTransform>();
            if (prtRT.sizeDelta != new Vector2(220, 140)) { prtRT.sizeDelta = new Vector2(220, 140); changed = true; }
            var prtImg = portrait.GetComponent<Image>();
            var prtCol = new Color(1, 1, 1, 0.12f);
            if (prtImg.color != prtCol) { prtImg.color = prtCol; changed = true; }

            // TMP styles
            changed |= SetupTMP(title.GetComponent<TextMeshProUGUI>(),    "Interview: Lena's Neighbor", 28, TextAlignmentOptions.TopLeft, wrap:true,  dim:1f);
            changed |= SetupTMP(action.GetComponent<TextMeshProUGUI>(),   "Action: INTERVIEW",          22, TextAlignmentOptions.TopLeft, wrap:false, dim:0.8f);
            changed |= SetupTMP(oneLiner.GetComponent<TextMeshProUGUI>(), "Get eyes-on account.",       22, TextAlignmentOptions.TopLeft, wrap:true,  dim:0.9f);

            // Requirements container: ensure it's a VerticalLayoutGroup (replace any existing LayoutGroup)
            RemoveAnyLayoutGroup(reqs, ref changed); // <-- convert instead of double-adding
            var reqV = reqs.GetComponent<VerticalLayoutGroup>() ?? reqs.AddComponent<VerticalLayoutGroup>();
            reqV.childAlignment = TextAnchor.UpperLeft;
            reqV.childControlHeight = true; reqV.childControlWidth = true;
            reqV.childForceExpandHeight = false; reqV.childForceExpandWidth = false;
            reqV.spacing = 6f; reqV.padding = new RectOffset(0,0,0,0);
            var csf = reqs.GetComponent<ContentSizeFitter>() ?? reqs.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

            // Proceed button (ensure label)
            var pbImg = proceed.GetComponent<Image>();
            var pbCol = new Color(0.2f, 0.6f, 0.9f, 0.9f);
            if (pbImg.color != pbCol) { pbImg.color = pbCol; changed = true; }
            var pbRT = proceed.GetComponent<RectTransform>();
            if (pbRT.sizeDelta != new Vector2(200, 64)) { pbRT.sizeDelta = new Vector2(200, 64); changed = true; }
            var pbText = proceed.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            if (!pbText)
            {
                var go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                go.transform.SetParent(proceed.transform, false);
                pbText = go.GetComponent<TextMeshProUGUI>();
                changed = true;
            }
            changed |= SetupTMP(pbText, "PROCEED", 22, TextAlignmentOptions.Center, wrap:false, dim:1f);
            var pbTextRT = pbText.GetComponent<RectTransform>();
            pbTextRT.anchorMin = Vector2.zero; pbTextRT.anchorMax = Vector2.one; pbTextRT.offsetMin = Vector2.zero; pbTextRT.offsetMax = Vector2.zero;

            // Wire LeadCardView fields via reflection
            var lcvType = FindType(LeadCardViewType);
            if (lcvType == null)
            {
                EditorUtility.DisplayDialog("Missing type",
                    $"Could not find {LeadCardViewType}.", "OK");
                PrefabUtility.UnloadPrefabContents(root);
                return changed;
            }
            var lcv = root.GetComponent(lcvType) ?? root.AddComponent(lcvType);

            changed |= SetIfFieldChanged(lcv, "portraitImage", portrait.GetComponent<Image>());
            changed |= SetIfFieldChanged(lcv, "titleText",     title.GetComponent<TextMeshProUGUI>());
            changed |= SetIfFieldChanged(lcv, "actionText",    action.GetComponent<TextMeshProUGUI>());
            changed |= SetIfFieldChanged(lcv, "oneLinerText",  oneLiner.GetComponent<TextMeshProUGUI>());
            changed |= SetIfFieldChanged(lcv, "requirementsRoot", reqs);
            changed |= SetIfFieldChanged(lcv, "proceedButton", proceed.GetComponent<Button>());

            // Set requirementItemPrefab reference
            var reqPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ReqItemPath);
            changed |= SetIfFieldChanged(lcv, "requirementItemPrefab", reqPrefab);

            PrefabUtility.SaveAsPrefabAsset(root, CardPath);
            PrefabUtility.UnloadPrefabContents(root);
            return changed;
        }

        // ---------- helpers ----------
        private static Type FindType(string fullName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(fullName, false))
                .FirstOrDefault(t => t != null);
        }

        // Remove ANY existing LayoutGroup on this container before we add Vertical
        private static void RemoveAnyLayoutGroup(GameObject container, ref bool changed)
        {
            if (!container) return;
            var lg = container.GetComponent<LayoutGroup>();
            if (lg)
            {
                UnityEngine.Object.DestroyImmediate(lg, false);
                changed = true;
            }
        }

        // Overload: create empty
        private static GameObject GetOrCreate(Transform parent, string name, ref bool changed)
        {
            var t = parent.Find(name);
            if (t) return t.gameObject;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            changed = true;
            return go;
        }

        // Overload: create with arbitrary component list (params)
        private static GameObject GetOrCreate(Transform parent, string name, ref bool changed, params Type[] components)
        {
            var t = parent.Find(name);
            if (t) return t.gameObject;
            var go = new GameObject(name, components);
            go.transform.SetParent(parent, false);
            changed = true;
            return go;
        }

        // Convenience overloads to match call-sites (types first, ref last)
        private static GameObject GetOrCreate(Transform parent, string name, Type c1, ref bool changed)
            => GetOrCreate(parent, name, ref changed, c1);

        private static GameObject GetOrCreate(Transform parent, string name, Type c1, Type c2, ref bool changed)
            => GetOrCreate(parent, name, ref changed, c1, c2);

        private static GameObject GetOrCreate(Transform parent, string name, Type c1, Type c2, Type c3, ref bool changed)
            => GetOrCreate(parent, name, ref changed, c1, c2, c3);

        private static bool SetupTMP(TextMeshProUGUI tmp, string text, float baseSize, TextAlignmentOptions align, bool wrap, float dim)
        {
            bool changed = false;
            if (tmp.text != text) { tmp.text = text; changed = true; }
            tmp.enableAutoSizing = true;
            float wantMin = Mathf.Max(16, baseSize - 12);
            float wantMax = Mathf.Max(baseSize, baseSize + 10);
            if (Math.Abs(tmp.fontSizeMin - wantMin) > 0.001f) { tmp.fontSizeMin = wantMin; changed = true; }
            if (Math.Abs(tmp.fontSizeMax - wantMax) > 0.001f) { tmp.fontSizeMax = wantMax; changed = true; }
            if (tmp.alignment != align) { tmp.alignment = align; changed = true; }
            var wantWrap = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            if (tmp.textWrappingMode != wantWrap) { tmp.textWrappingMode = wantWrap; changed = true; }
            var wantOv = wrap ? TextOverflowModes.Overflow : TextOverflowModes.Ellipsis;
            if (tmp.overflowMode != wantOv) { tmp.overflowMode = wantOv; changed = true; }
            var wantCol = new Color(dim, dim, dim, 1f);
            if (tmp.color != wantCol) { tmp.color = wantCol; changed = true; }
            return changed;
        }

        private static bool SetIfFieldChanged(Component target, string field, object value)
        {
            var f = target.GetType().GetField(field);
            if (f == null) return false;
            var cur = f.GetValue(target);
            if (!Equals(cur, value))
            {
                f.SetValue(target, value);
                return true;
            }
            return false;
        }

        private static bool RemoveMissingScriptsRecursive(GameObject root)
        {
            int before = CountMissingScripts(root);
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
            foreach (Transform t in root.transform)
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
            int after = CountMissingScripts(root);
            return after < before;
        }

        private static int CountMissingScripts(GameObject go)
        {
            int count = 0;
            var comps = go.GetComponentsInChildren<Component>(true);
            foreach (var c in comps) if (c == null) count++;
            return count;
        }

        private static void EnsureFolder(string path)
        {
            var parts = path.Split('/');
            string cur = "";
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
