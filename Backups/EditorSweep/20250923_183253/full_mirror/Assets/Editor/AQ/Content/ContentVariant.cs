#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.Content
{
    public static class ContentVariant
    {
        // ===== Entry points =====
        [MenuItem("AQ/Content/Apply Variant A")]
        public static void ApplyVariantA_Menu()
        {
            ApplyVariantA();
            Debug.Log("[ContentVariant] Applied Variant A.");
        }

        public static void ApplyVariantA()
        {
            var root = FindGO("ResolutionRoot");
            if (root == null)
            {
                Debug.LogWarning("[ContentVariant] ResolutionRoot not found in scene.");
                return;
            }

            var panel = FindChildRecursive(root.transform, "ResolutionPanel")?.gameObject ?? root;

            // ----- TEXT CONTENT -----
            var titleObj = FindOrCreateUIText(panel.transform,
                new[] { "Title", "Headline", "Header", "Heading" },
                "Title",
                fontSize: 28,
                color: new Color(0.95f, 0.98f, 1f, 1f));

            var bodyObj = FindOrCreateUIText(panel.transform,
                new[] { "Body", "Subhead", "SubText", "Description" },
                "Body",
                fontSize: 18,
                color: new Color(0.90f, 0.95f, 1f, 0.95f));

            SetText(titleObj, "Case Closed");
            SetText(bodyObj,  "Your investigation cracked the trail wide open.");

            // ----- QUESTS -----
            EnsureQuestList(panel.transform, new[]
            {
                "Investigate new lead at City Hall",
                "Cross-check Marlow’s alibi records",
                "Tag recovered evidence in caseboard"
            });

            // ----- BUTTON + LISTENERS -----
            var button = FindResolveButton(panel.transform);
            if (button == null)
            {
                Debug.LogWarning("[ContentVariant] Resolve/Continue Button not found under ResolutionPanel; wiring skipped.");
            }
            else
            {
                // Ensure button label exists & set copy
                var btnText = button.GetComponentInChildren<Text>(true);
                if (btnText == null)
                {
                    var textGO = new GameObject("Text", typeof(RectTransform));
                    textGO.transform.SetParent(button.transform, false);
                    btnText = textGO.AddComponent<Text>();
                    btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    btnText.alignment = TextAnchor.MiddleCenter;
                    var rt = (RectTransform)textGO.transform;
                    rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                }
                Undo.RecordObject(btnText, "Set Button Text");
                btnText.text = "Continue";
                btnText.fontSize = 20;
                btnText.color = Color.white;
                EditorUtility.SetDirty(btnText);

                // Make sure the two target components exist
                var advance = FindAdvanceComponent();
                if (advance == null)
                    Debug.LogWarning("[ContentVariant] CaseFlowAdvanceOnEventMB not found in scene.");

                var continueMb = EnsureContinueMB(root); // auto-add if missing

                // Wire persistent listeners on the BUTTON (not on the UnityEvent)
                Undo.RecordObject(button, "Wire ResolveButton OnClick");
                ClearPersistent(button.onClick);
                if (advance != null)
                    UnityEventTools.AddPersistentListener(button.onClick, advance.Advance);
                if (continueMb != null)
                    UnityEventTools.AddPersistentListener(button.onClick, continueMb.OnResolve);

                button.interactable = true;
                EditorUtility.SetDirty(button);
            }

            Selection.activeObject = root;
            EditorGUIUtility.PingObject(root);
        }

        // ===== Finds =====
        private static GameObject FindGO(string name)
        {
#if UNITY_2023_1_OR_NEWER
            var tr = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(t => t.name == name);
#else
            var tr = UnityEngine.Object.FindObjectsOfType<Transform>(true)
                .FirstOrDefault(t => t.name == name);
#endif
            return tr ? tr.gameObject : null;
        }

        private static Transform FindChildRecursive(Transform parent, string childName)
        {
            if (parent == null) return null;
            if (parent.name == childName) return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                var c = parent.GetChild(i);
                var hit = FindChildRecursive(c, childName);
                if (hit != null) return hit;
            }
            return null;
        }

        private static Transform FindChildByPath(Transform root, string path)
        {
            if (root == null) return null;
            if (string.IsNullOrEmpty(path)) return root;
            var parts = path.Split('/');
            Transform cur = root;
            foreach (var p in parts)
            {
                if (cur == null) return null;
                var next = Enumerable.Range(0, cur.childCount).Select(i => cur.GetChild(i))
                    .FirstOrDefault(c => c.name == p) ?? FindChildRecursive(cur, p);
                cur = next;
            }
            return cur;
        }

        private static Button FindResolveButton(Transform panel)
        {
            // Prefer explicit names
            var candidates = new[] { "ResolveButton", "ContinueButton", "OK", "Confirm", "Next" };
            foreach (var n in candidates)
            {
                var t = FindChildRecursive(panel, n);
                if (t != null)
                {
                    var b = t.GetComponent<Button>();
                    if (b != null) return b;
                }
            }

            // Name contains keywords
            var all = panel.GetComponentsInChildren<Button>(true);
            var byName = all.FirstOrDefault(b =>
                b.name.IndexOf("resolve", StringComparison.OrdinalIgnoreCase) >= 0 ||
                b.name.IndexOf("continue", StringComparison.OrdinalIgnoreCase) >= 0 ||
                b.name.IndexOf("next", StringComparison.OrdinalIgnoreCase) >= 0);
            if (byName != null) return byName;

            // Fallback: first button
            return all.FirstOrDefault();
        }

        private static AQ.App.CaseFlow.CaseFlowAdvanceOnEventMB FindAdvanceComponent()
        {
#if UNITY_2023_1_OR_NEWER
            return UnityEngine.Object.FindFirstObjectByType<AQ.App.CaseFlow.CaseFlowAdvanceOnEventMB>(FindObjectsInactive.Include);
#else
            return UnityEngine.Object.FindObjectOfType<AQ.App.CaseFlow.CaseFlowAdvanceOnEventMB>();
#endif
        }

        private static AQ.App.CaseFlow.ResolutionContinueMB EnsureContinueMB(GameObject root)
        {
            var existing = root.GetComponent<AQ.App.CaseFlow.ResolutionContinueMB>();
            if (existing != null) return existing;

            Undo.RecordObject(root, "Add ResolutionContinueMB");
            var added = root.AddComponent<AQ.App.CaseFlow.ResolutionContinueMB>();
            EditorUtility.SetDirty(root);
            Debug.Log("[ContentVariant] Added ResolutionContinueMB to ResolutionRoot.");
            return added;
        }

        // ===== UI builders =====
        private static GameObject FindOrCreateUIText(Transform panel, string[] nameCandidates, string defaultName, int fontSize, Color color)
        {
            // Try find by any candidate name
            foreach (var n in nameCandidates)
            {
                var t = FindChildRecursive(panel, n);
                var txt = t ? t.GetComponent<Text>() : null;
                if (txt != null) return txt.gameObject;
            }

            // Create a new Text under panel
            var go = new GameObject(defaultName, typeof(RectTransform));
            go.transform.SetParent(panel, false);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = defaultName == "Title" ? TextAnchor.MiddleCenter : TextAnchor.UpperLeft;

            // Stretch full width
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.offsetMin = new Vector2(20, -40);
            rt.offsetMax = new Vector2(-20, -10);

            Undo.RegisterCreatedObjectUndo(go, "Create UI Text");
            EditorUtility.SetDirty(text);
            return go;
        }

        private static void SetText(GameObject go, string value)
        {
            if (!go) return;
            var t = go.GetComponent<Text>();
            if (!t) return;
            Undo.RecordObject(t, "Set UI Text");
            t.text = value;
            EditorUtility.SetDirty(t);
        }

        private static void EnsureQuestList(Transform panel, string[] items)
        {
            var listT = FindChildRecursive(panel, "QuestList");
            if (listT == null)
            {
                var go = new GameObject("QuestList", typeof(RectTransform));
                go.transform.SetParent(panel, false);

                var layout = go.AddComponent<VerticalLayoutGroup>();
                layout.spacing = 6;
                layout.childControlWidth = true;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;

                listT = go.transform;

                var rt = (RectTransform)listT;
                rt.anchorMin = new Vector2(0, 0f);
                rt.anchorMax = new Vector2(1, 0f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.offsetMin = new Vector2(20, 20);
                rt.offsetMax = new Vector2(-20, -120);

                Undo.RegisterCreatedObjectUndo(go, "Create QuestList");
            }

            // Rebuild items
            Undo.RegisterFullObjectHierarchyUndo(listT.gameObject, "Rebuild QuestList");
            for (int i = listT.childCount - 1; i >= 0; i--)
                Undo.DestroyObjectImmediate(listT.GetChild(i).gameObject);

            foreach (var item in items)
            {
                var row = new GameObject("Quest", typeof(RectTransform));
                row.transform.SetParent(listT, false);
                var txt = row.AddComponent<Text>();
                txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txt.text = "• " + item;
                txt.fontSize = 18;
                txt.alignment = TextAnchor.MiddleLeft;
                txt.color = new Color(0.9f, 0.95f, 1f, 0.95f);
                EditorUtility.SetDirty(txt);
            }

            EditorUtility.SetDirty(listT.gameObject);
        }

        // ===== UnityEvent helpers =======
        private static void ClearPersistent(UnityEngine.Events.UnityEvent evt)
        {
            for (int i = evt.GetPersistentEventCount() - 1; i >= 0; i--)
                UnityEventTools.RemovePersistentListener(evt, i);
        }
    }
}
#endif
