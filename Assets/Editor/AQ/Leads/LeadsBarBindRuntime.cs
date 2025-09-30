#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

namespace AQ.Editor.Leads
{
    public static class LeadsBarBindRuntime
    {
        private const string LeadsBarPath = "LeadsBar";
        private const string ViewportName = "Viewport";
        private const string ContentName  = "Content_Leads";

        [MenuItem("AQ/Leads/Bind Runtime (LeadsBarView) + Clean Demo (robust)")]
        public static void Run()
        {
            // --- Locate our UI targets ---
            var leadsBar = GameObject.Find(LeadsBarPath);
            if (!leadsBar) { Debug.LogError("❌ LeadsBar not found."); return; }

            var viewportTF = leadsBar.transform.Find(ViewportName) as RectTransform;
            var contentTF  = viewportTF ? viewportTF.Find(ContentName) as RectTransform : null;
            var sr         = leadsBar.GetComponent<ScrollRect>();

            if (!viewportTF || !contentTF || !sr)
            {
                Debug.LogError("❌ Expected structure missing. Run 'AQ → Leads → Conform to Audit (ScrollRect on LeadsBar)'.");
                return;
            }

            // --- Find LeadsBarView type across ALL assemblies ---
            var lbvType = FindTypeByName("LeadsBarView");
            if (lbvType == null)
            {
                Debug.LogWarning("⚠️ Could not locate type 'LeadsBarView' in any loaded assembly. Is the script compiled/loaded?");
            }
            else
            {
                // Find instances in the scene (active or inactive, all scenes)
                var instances = Resources.FindObjectsOfTypeAll(lbvType)
                                         .OfType<Component>()
                                         .Where(c => c != null && c.gameObject.scene.IsValid())
                                         .ToArray();

                // Prefer the one attached to LeadsBar, else bind all we found.
                if (instances.Length == 0)
                {
                    // If design intends the component to live on LeadsBar, add it.
                    var view = Undo.AddComponent(leadsBar, lbvType);
                    BindView(view, contentTF, viewportTF, sr);
                    Debug.Log("ℹ️ Added and bound LeadsBarView on LeadsBar.");
                }
                else
                {
                    int bound = 0;
                    foreach (var view in instances)
                    {
                        bound += BindView(view, contentTF, viewportTF, sr) ? 1 : 0;
                    }
                    Debug.Log(bound > 0
                        ? $"✅ Bound {bound} LeadsBarView instance(s)."
                        : "ℹ️ LeadsBarView already bound (no changes).");
                }
            }

            // --- Clean any leftover blank/demo overflow (keep max 3) ---
            int removed = 0;
            for (int i = contentTF.childCount - 1; i >= 0; i--)
            {
                var ch = contentTF.GetChild(i);
                if (ch.name == "LeadCard_Blank") { Undo.DestroyObjectImmediate(ch.gameObject); removed++; }
            }
            int desired = 3;
            for (int i = contentTF.childCount - 1; i >= desired; i--)
            {
                Undo.DestroyObjectImmediate(contentTF.GetChild(i).gameObject);
                removed++;
            }

            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();
            if (removed > 0) Debug.Log($"🧹 Removed {removed} demo object(s).");
        }

        private static Type FindTypeByName(string simpleName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var type = asm.GetTypes().FirstOrDefault(t => t.Name == simpleName);
                    if (type != null) return type;
                }
                catch { /* ignore reflection-only/dirty assemblies */ }
            }
            return null;
        }

        private static bool BindView(Component view, RectTransform content, RectTransform viewport, ScrollRect sr)
        {
            if (!view) return false;
            var so = new SerializedObject(view);
            bool changed = false;

            // Try common serialized field names
            changed |= TryAssign(so, "contentRoot",  content);
            changed |= TryAssign(so, "m_ContentRoot",content);
            changed |= TryAssign(so, "content",      content);
            changed |= TryAssign(so, "contentTf",    content);

            changed |= TryAssign(so, "viewport",     viewport);
            changed |= TryAssign(so, "m_Viewport",   viewport);
            changed |= TryAssign(so, "viewportTf",   viewport);

            changed |= TryAssign(so, "scrollRect",   sr);
            changed |= TryAssign(so, "m_ScrollRect", sr);

            if (changed)
            {
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(view);
            }
            return changed;
        }

        private static bool TryAssign(SerializedObject so, string propName, UnityEngine.Object value)
        {
            var p = so.FindProperty(propName);
            if (p == null) return false;
            if (p.objectReferenceValue == value) return false;
            p.objectReferenceValue = value;
            return true;
        }
    }
}
#endif
