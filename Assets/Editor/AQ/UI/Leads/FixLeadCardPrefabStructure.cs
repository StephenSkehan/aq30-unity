#if UNITY_EDITOR
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AQ.EditorTools.UI.Leads
{
    /// <summary>
    /// Finds the LeadsBarView in the scene, loads its referenced Card Prefab asset,
    /// and ensures:
    ///   - ActorAnchor child exists with an Image child
    ///   - RequirementsRow exists with Req_1/Req_2/Req_3 each having an Icon (Image) and Tick (Image)
    ///   - Text fields have sane overflow/wrapping (readability tweak only)
    /// Saves the prefab in place. No renames, no other churn.
    /// </summary>
    public static class FixLeadCardPrefabStructure
    {
        [MenuItem("AQ/UI/Leads/Fix Card Prefab Structure (Actor & Req Slots)")]
        public static void Run()
        {
            var barView = FindByTypeName("LeadsBarView");
            if (barView == null)
            {
                Debug.LogWarning("[AQ FixCardPrefab] LeadsBarView not found in scene.");
                return;
            }

            var cardPrefab = GetFieldOrProp<Object>(barView, "cardPrefab", "CardPrefab", "card", "leadCardPrefab");
            if (cardPrefab == null)
            {
                Debug.LogWarning("[AQ FixCardPrefab] LeadsBarView.cardPrefab is null.");
                return;
            }

            string path = AssetDatabase.GetAssetPath(cardPrefab);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("[AQ FixCardPrefab] cardPrefab is not an asset path.");
                return;
            }

            var prefabRoot = PrefabUtility.LoadPrefabContents(path);
            if (!prefabRoot)
            {
                Debug.LogWarning($"[AQ FixCardPrefab] Could not load prefab '{path}'.");
                return;
            }

            var t = prefabRoot.transform;

            // Ensure ActorAnchor/Image
            var anchor = t.Find("ActorAnchor") as RectTransform;
            if (!anchor)
            {
                var go = new GameObject("ActorAnchor", typeof(RectTransform));
                anchor = go.GetComponent<RectTransform>();
                anchor.SetParent(t, false);
                anchor.anchorMin = anchor.anchorMax = new Vector2(0.5f, 1f); // top-center
                anchor.pivot = new Vector2(0.5f, 0f);
                anchor.sizeDelta = new Vector2(96, 96);
                anchor.anchoredPosition = new Vector2(0, 18);
            }

            var imgTr = anchor.Find("Image") as RectTransform;
            if (!imgTr)
            {
                var go = new GameObject("Image", typeof(RectTransform), typeof(Image));
                imgTr = go.GetComponent<RectTransform>();
                imgTr.SetParent(anchor, false);
                imgTr.anchorMin = imgTr.anchorMax = new Vector2(0.5f, 0.5f);
                imgTr.pivot = new Vector2(0.5f, 0.5f);
                imgTr.sizeDelta = new Vector2(96, 96);
                var img = go.GetComponent<Image>();
                var c = img.color; c.a = 1f; img.color = c;
                img.raycastTarget = false;
            }

            // Ensure RequirementsRow with three slots (Req_1..3)
            var reqRow = t.Find("RequirementsRow") as RectTransform;
            if (!reqRow)
            {
                var go = new GameObject("RequirementsRow", typeof(RectTransform));
                reqRow = go.GetComponent<RectTransform>();
                reqRow.SetParent(t, false);
                reqRow.anchorMin = new Vector2(0, 1);
                reqRow.anchorMax = new Vector2(1, 1);
                reqRow.pivot = new Vector2(0.5f, 1f);
                reqRow.sizeDelta = new Vector2(-28, 60); // per spec
                reqRow.anchoredPosition = new Vector2(0, -96);
            }

            EnsureSlot(reqRow, "Req_1");
            EnsureSlot(reqRow, "Req_2");
            EnsureSlot(reqRow, "Req_3");

            // TMP readability touches (safe)
            TweakTMP(t, "Text_Title");
            TweakTMP(t, "Text_Objective");
            TweakTMP(t, "Text_LeadId");

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            AssetDatabase.SaveAssets();

            Debug.Log($"[AQ FixCardPrefab] Repaired structure on '{path}'.");
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static void EnsureSlot(RectTransform row, string name)
        {
            var slot = row.Find(name) as RectTransform;
            if (!slot)
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(Button));
                slot = go.GetComponent<RectTransform>();
                slot.SetParent(row, false);
                slot.anchorMin = new Vector2(0, 0.5f);
                slot.anchorMax = new Vector2(0, 0.5f);
                slot.pivot = new Vector2(0, 0.5f);
                slot.sizeDelta = new Vector2(100, 60);
            }

            if (!slot.Find("Icon"))
            {
                var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                var rt = icon.GetComponent<RectTransform>();
                rt.SetParent(slot, false);
                rt.anchorMin = rt.anchorMax = new Vector2(0, 0.5f);
                rt.pivot = new Vector2(0, 0.5f);
                rt.sizeDelta = new Vector2(48, 48);
                rt.anchoredPosition = new Vector2(8, 0);
                icon.GetComponent<Image>().raycastTarget = false;
            }

            if (!slot.Find("Tick"))
            {
                var tick = new GameObject("Tick", typeof(RectTransform), typeof(Image));
                var rt = tick.GetComponent<RectTransform>();
                rt.SetParent(slot, false);
                rt.anchorMin = rt.anchorMax = new Vector2(1, 0.5f);
                rt.pivot = new Vector2(1, 0.5f);
                rt.sizeDelta = new Vector2(24, 24);
                rt.anchoredPosition = new Vector2(-8, 0);
                var img = tick.GetComponent<Image>();
                img.raycastTarget = false;
                img.enabled = false; // presenter enables when satisfied
            }
        }

        private static void TweakTMP(Transform root, string childName)
        {
            var t = root.Find(childName)?.GetComponent<TMP_Text>();
            if (!t) return;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.overflowMode = TextOverflowModes.Truncate;
            t.raycastTarget = false;
        }

        // reflection helpers
        private static Component FindByTypeName(string simpleName)
        {
            var all = Resources.FindObjectsOfTypeAll<Component>();
            foreach (var c in all)
            {
                if (!c) continue;
                var type = c.GetType();
                if (type.Name == simpleName || type.FullName == simpleName)
                {
                    var go = c.gameObject;
                    if (go.scene.IsValid() && go.scene.isLoaded) return c;
                }
            }
            return null;
        }

        private static T GetFieldOrProp<T>(Component comp, params string[] names) where T : Object
        {
            var type = comp.GetType();
            foreach (var n in names)
            {
                var f = type.GetField(n, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (f != null && typeof(T).IsAssignableFrom(f.FieldType)) return (T)f.GetValue(comp);

                var p = type.GetProperty(n, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (p != null && typeof(T).IsAssignableFrom(p.PropertyType)) return (T)p.GetValue(comp);
            }
            return null;
        }
    }
}
#endif
