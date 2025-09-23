// Assets/Editor/AQ/Variants/WK3_2_BindA_ToOverlay.cs
#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.EditorTools.Variants
{
    public static class WK3_2_BindA_ToOverlay
    {
        // Keep these aligned with B/C so all binders behave the same.
        private const string PanelName   = "ResolutionPanel";
        private const string TitlePath   = "Title";
        private const string BodyPath    = "Body";
        private const string ButtonPath  = "ContinueButton";
        private const string BulletsPath = "Quests";   // overlay typically uses Quest_0..2 under a container

        [MenuItem("Tools/AQ/Content Variants/Apply Variant A")]
        public static void Apply()
        {
            // Prefer binding a SCENE instance (active or inactive)
            var scenePanel = FindPanelInOpenScenes();
            if (scenePanel)
            {
                ApplyToTransform(scenePanel.transform, persistInEditMode: true, context: "Scene");
                return;
            }

            // Fallback to prefab binding (not allowed in Play Mode)
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Variant A",
                    "No scene instance found. Prefab binding is disabled in Play Mode.\n\nStop Play Mode and run again.",
                    "OK");
                return;
            }

            var prefabPath = FindPanelPrefabPath();
            if (string.IsNullOrEmpty(prefabPath))
            {
                EditorUtility.DisplayDialog("Variant A", "Could not find Resolution prefab.", "OK");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            var panel = root.name == PanelName ? root : (root.transform.Find(PanelName)?.gameObject ?? root);
            ApplyToTransform(panel.transform, persistInEditMode: false, context: "Prefab→" + prefabPath);
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            PrefabUtility.UnloadPrefabContents(root);
            EditorUtility.DisplayDialog("Variant A", "Applied to prefab:\n" + prefabPath, "OK");
        }

        [MenuItem("Tools/AQ/Content Variants/Verify Variant A")]
        public static void Verify()
        {
            var tr = FindPanelInOpenScenes()?.transform;
            string ctx = "Scene";
            if (!tr)
            {
                var prefabPath = FindPanelPrefabPath();
                if (string.IsNullOrEmpty(prefabPath))
                {
                    Debug.LogError("[VerifyA] No scene panel or prefab found.");
                    return;
                }
                var root = PrefabUtility.LoadPrefabContents(prefabPath);
                tr = (root.name == PanelName ? root : (root.transform.Find(PanelName)?.gameObject ?? root)).transform;
                ctx = "Prefab→" + prefabPath;
                var res = VerifyOverlay(tr);
                PrefabUtility.UnloadPrefabContents(root);
                Debug.Log("[WK3-2 VerifyA] (" + ctx + ") TitleOk=" + res.titleOk + ", BodyOk=" + res.bodyOk + ", ButtonOk=" + res.buttonOk);
                return;
            }

            var r = VerifyOverlay(tr);
            Debug.Log("[WK3-2 VerifyA] (" + ctx + ") TitleOk=" + r.titleOk + ", BodyOk=" + r.bodyOk + ", ButtonOk=" + r.buttonOk);
        }

        // ---------- CORE ----------

        private static void ApplyToTransform(Transform panel, bool persistInEditMode, string context)
        {
            // Variant A copy (from your earlier A binder)
            string title  = "Case Closed";
            string body   = "Your investigation cracked the trail wide open.";
            string button = "CONTINUE";
            string[] quests = {
                "• Investigate new lead at City Hall",
                "• Cross-check Marlow’s alibi records",
                "• Tag recovered evidence in caseboard"
            };
            int soft = 500, energy = 10, premium = 0;

            // Find targets with resilient heuristics (same as B/C)
            var (titleTMP, titlePath) = FindTitleTMP(panel);
            var (bodyTMP,  bodyPath ) = FindBodyTMP(panel, titleTMP);
            var (btnTMP,   btnPath  ) = FindButtonTMP(panel);

            if (titleTMP) titleTMP.text = title;
            if (bodyTMP)  bodyTMP.text  = body;
            if (btnTMP)   btnTMP.text   = button;

            // Optional quest/bullets container (writes when present)
            string questsFoundPath;
            ApplyOptionalListText(panel, BulletsPath, quests, out questsFoundPath);

            // Stage default rewards on the ResolutionContinueMB (scene only; tolerant)
            TryStageRewardsOnResolutionMB(soft, energy, premium);

            // Logging
            var sb = new StringBuilder();
            sb.Append("[WK3-2 BindA] Applied to ").Append(context).Append(". ");
            sb.Append("Title:").Append(titlePath ?? "<auto>").Append(' ');
            sb.Append("Body:").Append(bodyPath ?? "<auto>").Append(' ');
            sb.Append("Button:").Append(btnPath ?? "<auto>").Append(' ');
            if (!string.IsNullOrEmpty(questsFoundPath)) sb.Append("Quests:").Append(questsFoundPath).Append(' ');

            if (persistInEditMode && !EditorApplication.isPlaying)
            {
                EditorUtility.SetDirty(panel.gameObject);
                var scene = panel.gameObject.scene;
                if (scene.IsValid()) EditorSceneManager.MarkSceneDirty(scene);
                sb.Append("(scene marked dirty)");
            }
            else if (EditorApplication.isPlaying)
            {
                sb.Append("(PLAY MODE: changes are transient)");
            }

            Debug.Log(sb.ToString());
        }

        // ---------- FINDERS (shared pattern as B/C) ----------

        private static (TMP_Text tmp, string path) FindTitleTMP(Transform panel)
        {
            // 1) Preferred explicit path
            var tr = panel.Find(TitlePath);
            if (tr != null)
            {
                var t = tr.GetComponent<TMP_Text>();
                if (t != null) return (t, GetPath(t.transform));
            }

            // 2) Name-based
            var byName = panel.GetComponentsInChildren<TMP_Text>(true)
                              .FirstOrDefault(t => t.name.IndexOf("title", StringComparison.OrdinalIgnoreCase) >= 0);
            if (byName != null) return (byName, GetPath(byName.transform));

            // 3) Text-based (historical default)
            var byText = panel.GetComponentsInChildren<TMP_Text>(true)
                              .FirstOrDefault(t => string.Equals((t.text ?? string.Empty).Trim(), "Case Closed", StringComparison.OrdinalIgnoreCase));
            if (byText != null) return (byText, GetPath(byText.transform));

            // 4) Heuristic: largest font size
            var biggest = panel.GetComponentsInChildren<TMP_Text>(true).OrderByDescending(t => t.fontSize).FirstOrDefault();
            return (biggest, biggest != null ? GetPath(biggest.transform) : null);
        }

        private static (TMP_Text tmp, string path) FindBodyTMP(Transform panel, TMP_Text titleRef)
        {
            // 1) Preferred explicit path
            var tr = panel.Find(BodyPath);
            if (tr != null)
            {
                var t = tr.GetComponent<TMP_Text>();
                if (t != null) return (t, GetPath(t.transform));
            }

            // 2) Below titleRef and not the same
            if (titleRef != null)
            {
                var near = panel.GetComponentsInChildren<TMP_Text>(true)
                                .Where(t => t != titleRef)
                                .OrderBy(t => Mathf.Abs(t.transform.position.y - titleRef.transform.position.y))
                                .FirstOrDefault();
                if (near != null) return (near, GetPath(near.transform));
            }

            // 3) Name-based
            var byName = panel.GetComponentsInChildren<TMP_Text>(true)
                              .FirstOrDefault(t => t.name.IndexOf("body", StringComparison.OrdinalIgnoreCase) >= 0);
            if (byName != null) return (byName, GetPath(byName.transform));

            // 4) Fallback: second-largest font
            var list = panel.GetComponentsInChildren<TMP_Text>(true).OrderByDescending(t => t.fontSize).ToList();
            if (list.Count > 1) return (list[1], GetPath(list[1].transform));
            return (null, null);
        }

        private static (TMP_Text tmp, string path) FindButtonTMP(Transform panel)
        {
            // 1) Preferred path
            var tr = panel.Find(ButtonPath);
            if (tr != null)
            {
                var t = tr.GetComponentInChildren<TMP_Text>(true);
                if (t != null) return (t, GetPath(t.transform));
            }

            // 2) Any Button under panel with TMP child
            var btn = panel.GetComponentsInChildren<Button>(true).FirstOrDefault();
            if (btn != null)
            {
                var t = btn.GetComponentInChildren<TMP_Text>(true);
                if (t != null) return (t, GetPath(t.transform));
            }

            // 3) Any TMP saying "Continue"
            var byText = panel.GetComponentsInChildren<TMP_Text>(true)
                              .FirstOrDefault(t => string.Equals((t.text ?? string.Empty).Trim(), "Continue", StringComparison.OrdinalIgnoreCase)
                                                || string.Equals((t.text ?? string.Empty).Trim(), "CONTINUE", StringComparison.OrdinalIgnoreCase));
            if (byText != null) return (byText, GetPath(byText.transform));

            // 4) Last resort: any TMP
            var any = panel.GetComponentsInChildren<TMP_Text>(true).LastOrDefault();
            return (any, any != null ? GetPath(any.transform) : null);
        }

        // ---------- VERIFY / UTILS ----------

        private static (bool titleOk, bool bodyOk, bool buttonOk) VerifyOverlay(Transform panel)
        {
            var title = FindTitleTMP(panel).tmp;
            var body  = FindBodyTMP(panel, title).tmp;
            var btn   = FindButtonTMP(panel).tmp;

            bool okTitle = (title != null) && !string.IsNullOrWhiteSpace(title.text);
            bool okBody  = (body  != null) && !string.IsNullOrWhiteSpace(body.text);
            bool okBtn   = (btn   != null) && !string.IsNullOrWhiteSpace(btn.text);
            return (okTitle, okBody, okBtn);
        }

        private static void ApplyOptionalListText(Transform parent, string containerName, string[] values, out string foundPath)
        {
            foundPath = null;
            var container = parent.Find(containerName);
            if (container == null || values == null) return;

            int i = 0;
            foreach (Transform child in container)
            {
                var t = child.GetComponent<TMP_Text>();
                if (t != null)
                {
                    t.text = (i < values.Length && !string.IsNullOrEmpty(values[i])) ? values[i] : t.text;
                    if (foundPath == null) foundPath = GetPath(container);
                }
                i++;
            }
        }

        private static string GetPath(Transform t)
        {
            string p = t.name;
            while (t.parent != null) { t = t.parent; p = t.name + "/" + p; }
            return p;
        }

        // Finds panel in ANY open scene, even if inactive (ignores Project assets)
        private static GameObject FindPanelInOpenScenes()
        {
            foreach (var tr in Resources.FindObjectsOfTypeAll<Transform>())
            {
                var go = tr.gameObject;
                if (!go.scene.IsValid()) continue;            // skip assets
                if (EditorUtility.IsPersistent(go)) continue;  // skip prefabs
                if (go.name == PanelName) return go;

                // Fallback: object with runtime MB
                bool hasRC = go.GetComponents<Component>()
                               .Any(c => c != null && c.GetType().FullName == "AQ.App.CaseFlow.ResolutionContinueMB");
                if (hasRC) return go;
            }
            return null;
        }

        private static string FindPanelPrefabPath()
        {
            // Try exact name first
            var guids = AssetDatabase.FindAssets("t:Prefab " + PanelName);
            if (guids != null && guids.Length > 0)
                return AssetDatabase.GUIDToAssetPath(guids[0]);

            // Fallback: contains both "Resolution" and "Panel"
            guids = AssetDatabase.FindAssets("t:Prefab Resolution");
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name.IndexOf("Panel", StringComparison.OrdinalIgnoreCase) >= 0)
                    return path;
            }
            return null;
        }

        // Scene-only reward staging; tolerant to missing fields/properties
        private static void TryStageRewardsOnResolutionMB(int soft, int energy, int premium)
        {
            var rc = Resources.FindObjectsOfTypeAll<Component>()
                              .FirstOrDefault(c => c != null
                                                && c.gameObject.scene.IsValid()
                                                && c.GetType().FullName == "AQ.App.CaseFlow.ResolutionContinueMB");
            if (rc == null) return;

            TrySetMember(rc, "defaultSoft",    soft);
            TrySetMember(rc, "defaultEnergy",  energy);
            TrySetMember(rc, "defaultPremium", premium);
            if (!EditorApplication.isPlaying) EditorUtility.SetDirty(rc);
        }

        private static bool TrySetMember(object target, string memberName, int value)
        {
            var t = target.GetType();
            var f = t.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null && f.FieldType == typeof(int)) { f.SetValue(target, value); return true; }
            var p = t.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (p != null && p.CanWrite && p.PropertyType == typeof(int)) { p.SetValue(target, value); return true; }
            return false;
        }
    }
}
#endif
