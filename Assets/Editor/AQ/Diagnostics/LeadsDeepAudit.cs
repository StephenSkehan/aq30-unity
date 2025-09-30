#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AQ.EditorTools.Diagnostics
{
    /// <summary>
    /// Read-only, end-to-end audit:
    /// 1) Scene TierSetPopup instances (deep) + prefab assets named "TierSetPopup" (deep) -> missing scripts & structure.
    /// 2) LeadsBarView -> Card Prefab reference and its prefab structure (ActorAnchor/Image, RequirementsRow/Req_1..3).
    /// 3) Lead_* ScriptableObjects: actorSprite presence + requirement tierSprites.
    /// 4) Live cards in scene (Edit or Play): actor presence and requirement icon sprites.
    ///
    /// Never modifies scene or assets.
    /// </summary>
    public static class LeadsDeepAudit
    {
        [MenuItem("AQ/Diagnostics/Leads/Deep Runtime + Assets Audit (Read-Only)")]
        public static void Run()
        {
            Debug.Log("=== [AQ DeepAudit] BEGIN ===");

            AuditSceneTierSetPopups();
            AuditTierSetPopupPrefabs();
            AuditCardPrefabReference();
            AuditLeadAssets();
            AuditLiveCards();

            Debug.Log("=== [AQ DeepAudit] END ===");
        }

        // ---------- (1) Scene TierSetPopup instances ----------
        private static void AuditSceneTierSetPopups()
        {
            var popups = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(go => go.name == "TierSetPopup")
                .Where(go => go.scene.IsValid() && go.scene.isLoaded)
                .ToArray();

            if (popups.Length == 0)
            {
                Debug.Log("[AQ DeepAudit] No TierSetPopup instances in loaded scenes.");
                return;
            }

            Debug.Log($"[AQ DeepAudit] TierSetPopup scene instances: {popups.Length}");
            foreach (var go in popups)
            {
                int missingDeep = CountMissingRecursive(go);
                string comps = DescribeComponents(go);
                Debug.Log($"[AQ DeepAudit] Scene='{go.scene.name}' Path='{PathOf(go.transform)}' Active={go.activeInHierarchy} MissingScripts(deep)={missingDeep}\n  RootComponents: {comps}");

                var grid = go.transform.Find("IconGrid");
                bool hasGrid = grid != null;
                bool gridHasRect = grid is RectTransform;
                var gridLayout = hasGrid ? grid.GetComponent<GridLayoutGroup>() : null;
                int iconChildren = hasGrid ? grid.Cast<Transform>().Count(t => t.name.StartsWith("Icon")) : 0;
                bool hasHighlight = hasGrid && grid.Find("Highlight") != null;

                Debug.Log(
                    $"[AQ DeepAudit] PopupStruct: Grid={(hasGrid ? "OK" : "NO")} " +
                    $"GridRect={(gridHasRect ? "Rect" : "NoRect")} " +
                    $"GridLayout={(gridLayout ? "OK" : "NO")} " +
                    $"Icons={iconChildren} Highlight={(hasHighlight ? "OK" : "NO")}"
                );
            }
        }

        // ---------- (2) Prefab assets named "TierSetPopup" ----------
        private static void AuditTierSetPopupPrefabs()
        {
            var guids = AssetDatabase.FindAssets("TierSetPopup t:prefab");
            if (guids.Length == 0)
            {
                Debug.Log("[AQ DeepAudit] No prefab assets named 'TierSetPopup' found.");
                return;
            }

            Debug.Log($"[AQ DeepAudit] TierSetPopup prefab assets: {guids.Length}");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!root) continue;

                int missingDeep = CountMissingRecursive(root, includeChildren: true);
                string comps = DescribeComponents(root);
                Debug.Log($"[AQ DeepAudit] Prefab='{path}' MissingScripts(deep)={missingDeep}\n  RootComponents: {comps}");

                var t = root.transform;
                var grid = t.Find("IconGrid");
                bool hasGrid = grid != null;
                int iconChildren = hasGrid ? grid.Cast<Transform>().Count(ch => ch.name.StartsWith("Icon")) : 0;
                bool hasHighlight = hasGrid && grid.Find("Highlight") != null;

                Debug.Log(
                    $"[AQ DeepAudit] PrefabStruct: Grid={(hasGrid ? "OK" : "NO")} " +
                    $"Icons={iconChildren} Highlight={(hasHighlight ? "OK" : "NO")}"
                );
            }
        }

        // ---------- (3) LeadsBarView -> Card Prefab and structure ----------
        private static void AuditCardPrefabReference()
        {
            var barView = FindByTypeName("LeadsBarView");
            if (barView == null)
            {
                Debug.LogWarning("[AQ DeepAudit] LeadsBarView not found in scene (by type name).");
                return;
            }

            UnityEngine.Object cardPrefab =
                GetFieldOrProp<UnityEngine.Object>(barView, "cardPrefab", "CardPrefab", "card", "leadCardPrefab");

            if (cardPrefab == null)
            {
                Debug.LogWarning("[AQ DeepAudit] LeadsBarView found but card prefab reference is null or field not recognized.");
                return;
            }

            var assetPath = AssetDatabase.GetAssetPath(cardPrefab);
            var display = string.IsNullOrEmpty(assetPath) ? cardPrefab.name : assetPath;
            Debug.Log($"[AQ DeepAudit] LeadsBarView Card Prefab = {display}");

            var go = cardPrefab as GameObject ?? (!string.IsNullOrEmpty(assetPath) ? AssetDatabase.LoadAssetAtPath<GameObject>(assetPath) : null);
            if (!go)
            {
                Debug.LogWarning("[AQ DeepAudit] Card prefab is not a GameObject.");
                return;
            }

            var t = go.transform;
            bool hasActorAnchor = t.Find("ActorAnchor") != null;
            bool hasActorImage  = t.Find("ActorAnchor/Image")?.GetComponent<Image>() != null;
            bool hasReqRow      = t.Find("RequirementsRow") != null;
            bool hasReq1        = t.Find("RequirementsRow/Req_1") != null;
            bool hasReq2        = t.Find("RequirementsRow/Req_2") != null;
            bool hasReq3        = t.Find("RequirementsRow/Req_3") != null;

            Debug.Log(
                "[AQ DeepAudit] CardPrefabStruct: " +
                $"ActorAnchor={(hasActorAnchor ? "OK" : "NO")} " +
                $"ActorImage={(hasActorImage ? "OK" : "NO")} " +
                $"ReqRow={(hasReqRow ? "OK" : "NO")} " +
                $"Req1={(hasReq1 ? "OK" : "NO")} " +
                $"Req2={(hasReq2 ? "OK" : "NO")} " +
                $"Req3={(hasReq3 ? "OK" : "NO")}"
            );
        }

        // ---------- (4) Lead_* assets ----------
        private static void AuditLeadAssets()
        {
            var guids = AssetDatabase.FindAssets("Lead_ t:ScriptableObject", new[] { "Assets" });
            if (guids.Length == 0)
            {
                Debug.Log("[AQ DeepAudit] No Lead_* ScriptableObjects found.");
                return;
            }

            int withActor = 0, withoutActor = 0, totalReqGroups = 0, groupsWithSprites = 0, groupsWithoutSprites = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (!asset) continue;

                var so = new SerializedObject(asset);
                var actorProp = so.FindProperty("actorSprite");
                var titleProp = so.FindProperty("title");
                var idProp    = so.FindProperty("id");

                string title = titleProp != null ? titleProp.stringValue : "(title?)";
                int id = idProp != null ? idProp.intValue : -1;

                bool hasActor = actorProp != null && actorProp.objectReferenceValue != null;
                if (hasActor) withActor++; else withoutActor++;

                var reqs = so.FindProperty("requirements");
                int reqCount = reqs != null ? reqs.arraySize : 0;

                for (int i = 0; i < reqCount; i++)
                {
                    totalReqGroups++;
                    var elem = reqs.GetArrayElementAtIndex(i);
                    string groupTitle = elem.FindPropertyRelative("groupTitle")?.stringValue ?? "(group?)";
                    var tierSprites = elem.FindPropertyRelative("tierSprites");
                    int spriteCount = tierSprites != null ? tierSprites.arraySize : 0;

                    if (spriteCount > 0) groupsWithSprites++; else groupsWithoutSprites++;

                    Debug.Log($"[AQ DeepAudit] Lead id={id} '{title}': req[{i}] '{groupTitle}' tierSprites={spriteCount}");
                }

                Debug.Log($"[AQ DeepAudit] Lead id={id} '{title}': actor={(hasActor ? "OK" : "NONE")} reqGroups={reqCount}");
            }

            Debug.Log($"[AQ DeepAudit] Lead assets summary: actorSprite OK={withActor}, MISSING={withoutActor}; requirement groups total={totalReqGroups}, with sprites={groupsWithSprites}, without sprites={groupsWithoutSprites}");
        }

        // ---------- (5) Live cards (Edit or Play) ----------
        private static void AuditLiveCards()
        {
            var content = GameObject.Find("Content_Leads")?.transform;
            if (!content)
            {
                Debug.LogWarning("[AQ DeepAudit] Content_Leads not found in scene.");
                return;
            }

            int scanned = 0;
            foreach (Transform card in content)
            {
                bool looksLikeCard = card.Find("Text_Title") || card.Find("RequirementsRow");
                if (!looksLikeCard) continue;

                scanned++;
                string title = card.Find("Text_Title")?.GetComponent<TMP_Text>()?.text ?? "(no title)";
                string idTxt = card.Find("Text_LeadId")?.GetComponent<TMP_Text>()?.text ?? "(no id)";

                var anchor = card.Find("ActorAnchor");
                var actorImg = card.Find("ActorAnchor/Image")?.GetComponent<Image>();
                string actorSprite = (actorImg && actorImg.sprite) ? actorImg.sprite.name : "(null)";
                string actorState = actorImg ? $"α={actorImg.color.a:0.##} {(actorImg.enabled ? "enabled" : "disabled")}" : "(no Image)";

                string[] reqInfo = new string[3];
                for (int i = 1; i <= 3; i++)
                {
                    var icon = card.Find($"RequirementsRow/Req_{i}/Icon")?.GetComponent<Image>();
                    string sName = (icon && icon.sprite) ? icon.sprite.name : "(null)";
                    reqInfo[i - 1] = $"Req_{i}={sName}";
                }

                Debug.Log($"[AQ DeepAudit] LIVE Card '{card.name}' title='{title}' id='{idTxt}' Anchor={(anchor ? "OK" : "NO")} ActorImg={(actorImg ? "OK" : "NO")} sprite={actorSprite} {actorState} | {string.Join(" ", reqInfo)}");
            }

            Debug.Log($"[AQ DeepAudit] Live cards scanned: {scanned}. PlayMode={(Application.isPlaying ? "Yes" : "No")}.");
        }

        // ---------- helpers ----------
        private static int CountMissingRecursive(GameObject root, bool includeChildren = true)
        {
            int count = 0;
            foreach (var c in root.GetComponents<Component>()) if (!c) count++;
            if (includeChildren)
            {
                foreach (Transform ch in root.transform)
                    count += CountMissingRecursive(ch.gameObject, true);
            }
            return count;
        }

        private static string DescribeComponents(GameObject go)
        {
            var list = new List<string>();
            foreach (var c in go.GetComponents<Component>())
            {
                if (!c) { list.Add("(Missing Script)"); continue; }
                list.Add(c.GetType().FullName);
            }
            return string.Join(", ", list);
        }

        private static string PathOf(Transform t)
        {
            var parts = new List<string>();
            while (t != null) { parts.Add(t.name); t = t.parent; }
            parts.Reverse();
            return string.Join("/", parts);
        }

        private static Component FindByTypeName(string simpleName)
        {
            var all = Resources.FindObjectsOfTypeAll<Component>();
            foreach (var c in all)
            {
                if (!c) continue;
                var t = c.GetType();
                if (t.Name == simpleName || t.FullName == simpleName)
                {
                    var go = c.gameObject;
                    if (go.scene.IsValid() && go.scene.isLoaded) return c;
                }
            }
            return null;
        }

        private static T GetFieldOrProp<T>(Component comp, params string[] names) where T : UnityEngine.Object
        {
            var type = comp.GetType();
            foreach (var n in names)
            {
                var f = type.GetField(n, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (f != null && typeof(T).IsAssignableFrom(f.FieldType))
                    return (T)f.GetValue(comp);

                var p = type.GetProperty(n, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (p != null && typeof(T).IsAssignableFrom(p.PropertyType))
                    return (T)p.GetValue(comp);
            }
            return null;
        }
    }
}
#endif
