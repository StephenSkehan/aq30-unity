// Assets/Editor/AQ/Variants/WK3_2_BindB_ToOverlay.cs
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
    public static class WK3_2_BindB_ToOverlay
    {
        // Preferred names (used if present); binder falls back to heuristics if not found.
        private const string PanelName   = "ResolutionPanel";
        private const string TitlePath   = "Title";
        private const string BodyPath    = "Body";
        private const string ButtonPath  = "ContinueButton";
        private const string BulletsPath = "Bullets";
        private const string QuestsPath  = "Quests";

        // ---------- MENUS ----------

        [MenuItem("Tools/AQ/Overlay/Dump Resolution Hierarchy")]
        public static void DumpHierarchy()
        {
            var panel = FindPanelInOpenScenes();
            if (!panel)
            {
                var prefabPath = FindPanelPrefabPath();
                if (string.IsNullOrEmpty(prefabPath))
                {
                    Debug.LogError("[Overlay/Dump] Could not find Resolution panel in scene or prefab.");
                    return;
                }
                var root = PrefabUtility.LoadPrefabContents(prefabPath);
                DumpTransform(root.transform);
                PrefabUtility.UnloadPrefabContents(root);
                Debug.Log("[Overlay/Dump] Dumped prefab hierarchy: " + prefabPath);
                return;
            }
            DumpTransform(panel.transform);
            Debug.Log("[Overlay/Dump] Dumped scene hierarchy (inactive OK).");
        }

        [MenuItem("Tools/AQ/Content Variants/Apply Variant B")]
        public static void Apply()
        {
            // Prefer a SCENE instance (active or inactive)
            var scenePanel = FindPanelInOpenScenes();
            if (scenePanel)
            {
                ApplyToTransform(scenePanel.transform, persistInEditMode: true, context: "Scene");
                return;
            }

            // Otherwise, bind PREFAB (Edit Mode only)
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Variant B",
                    "No scene instance found. Prefab binding is disabled in Play Mode.\n\nStop Play Mode and run again.",
                    "OK");
                return;
            }

            var prefabPath = FindPanelPrefabPath();
            if (string.IsNullOrEmpty(prefabPath))
            {
                EditorUtility.DisplayDialog("Variant B", "Could not find Resolution prefab.", "OK");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            var panel = root.name == PanelName ? root : (root.transform.Find(PanelName)?.gameObject ?? root);
            ApplyToTransform(panel.transform, persistInEditMode: false, context: "Prefab→" + prefabPath);
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            PrefabUtility.UnloadPrefabContents(root);
            EditorUtility.DisplayDialog("Variant B", "Applied to prefab:\n" + prefabPath, "OK");
        }

        [MenuItem("Tools/AQ/Content Variants/Verify Variant B")]
        public static void Verify()
        {
            var tr = FindPanelInOpenScenes()?.transform;
            string ctx = "Scene";
            if (!tr)
            {
                var prefabPath = FindPanelPrefabPath();
                if (string.IsNullOrEmpty(prefabPath))
                {
                    Debug.LogError("[VerifyB] No scene panel or prefab found.");
                    return;
                }
                var root = PrefabUtility.LoadPrefabContents(prefabPath);
                tr = (root.name == PanelName ? root : (root.transform.Find(PanelName)?.gameObject ?? root)).transform;
                ctx = "Prefab→" + prefabPath;
                var res = VerifyOverlay(tr);
                PrefabUtility.UnloadPrefabContents(root);
                Debug.Log("[WK3-2 VerifyB] (" + ctx + ") TitleOk=" + res.titleOk + ", BodyOk=" + res.bodyOk + ", ButtonOk=" + res.buttonOk);
                return;
            }

            var r = VerifyOverlay(tr);
            Debug.Log("[WK3-2 VerifyB] (" + ctx + ") TitleOk=" + r.titleOk + ", BodyOk=" + r.bodyOk + ", ButtonOk=" + r.buttonOk);
        }

        // ---------- CORE ----------

        private static void ApplyToTransform(Transform panel, bool persistInEditMode, string context)
        {
            // Variant B content (swap to content SO later if desired)
            string title  = "CASE RESOLVED";
            string body   = "Thanks for playing! Your instincts are sharp.\nMore mysteries await in Havenbay.";
            string button = "CONTINUE";
            string[] bullets = { "Unlocked: Next Case Teaser", "Evidence archived", "Podcast updated" };
            string[] quests  = { "Return to the Board", "Check Leads Journal", "Visit Ally’s Studio" };
            int soft = 500, energy = 10, premium = 0;

            // Locate targets with resilient finders
            var titleFind  = FindTitleTMP(panel);
            var bodyFind   = FindBodyTMP(panel, titleFind.tmp);
            var buttonFind = FindButtonTMP(panel);

            if (titleFind.tmp != null)  titleFind.tmp.text  = title;
            if (bodyFind.tmp != null)   bodyFind.tmp.text   = body;
            if (buttonFind.tmp != null) buttonFind.tmp.text = button;

            // Optional containers
            string bulletsFoundPath;
            string questsFoundPath;
            ApplyOptionalListText(panel, BulletsPath, bullets, out bulletsFoundPath);
            ApplyOptionalListText(panel, QuestsPath,  quests,  out questsFoundPath);

            // Rewards staging (scene only, tolerant)
            TryStageRewardsOnResolutionMB(soft, energy, premium);

            // Logging (no nested interpolated strings)
            var sb = new StringBuilder();
            sb.Append("[WK3-2 BindB] Applied to ").Append(context).Append(". ");
            sb.Append("Title:").Append(titleFind.path ?? "<auto>").Append(' ');
            sb.Append("Body:").Append(bodyFind.path ?? "<auto>").Append(' ');
            sb.Append("Button:").Append(buttonFind.path ?? "<auto>").Append(' ');
            if (!string.IsNullOrEmpty(bulletsFoundPath)) sb.Append("Bullets:").Append(bulletsFoundPath).Append(' ');
            if (!string.IsNullOrEmpty(questsFoundPath))  sb.Append("Quests:").Append(questsFoundPath).Append(' ');

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

        // ---------- FINDERS (resilient) ----------

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

            // 3) Text-based
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
                var candidates = panel.GetComponentsInChildren<TMP_Text>(true)
                                      .Where(t => t != titleRef)
                                      .OrderBy(t => Mathf.Abs(t.transform.position.y - titleRef.transform.position.y));
                var near = candidates.FirstOrDefault();
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
                              .FirstOrDefault(t => string.Equals((t.text ?? string.Empty).Trim(), "Continue", StringComparison.OrdinalIgnoreCase));
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

        private static void DumpTransform(Transform root)
        {
            void Recurse(Transform t, int depth)
            {
                var comps = t.GetComponents<Component>().Select(c => c ? c.GetType().Name : "null").ToArray();
                Debug.Log(new string(' ', depth * 2) + "- " + t.name + "   [" + string.Join(", ", comps) + "]");
                foreach (Transform c in t) Recurse(c, depth + 1);
            }
            Debug.Log("[Overlay/Dump] Root: " + GetPath(root));
            Recurse(root, 0);
        }
    }
}
#endif
