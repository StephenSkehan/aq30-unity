#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System;
using System.Linq;

namespace AQ.Editor.Leads
{
    public static class LeadsBarDesignMode
    {
        private const string ContentPath = "LeadsBar/Viewport/Content_Leads";

        [MenuItem("AQ/Leads/Design Mode/Enable (disable LeadsBarView & purge runtime children)")]
        public static void Enable()
        {
            Toggle(false, purgeRuntimeChildren: true);
        }

        [MenuItem("AQ/Leads/Design Mode/Disable (re-enable LeadsBarView)")]
        public static void Disable()
        {
            Toggle(true, purgeRuntimeChildren: false);
        }

        private static void Toggle(bool enableRuntime, bool purgeRuntimeChildren)
        {
            var lbvType = FindTypeByName("LeadsBarView");
            if (lbvType == null)
            {
                Debug.LogWarning("⚠️ Could not locate type 'LeadsBarView' in loaded assemblies. Nothing toggled.");
            }
            else
            {
                var views = Resources.FindObjectsOfTypeAll(lbvType)
                        .OfType<Component>()
                        .Where(c => c != null && c.gameObject.scene.IsValid())
                        .ToArray();

                int changed = 0;
                foreach (var view in views)
                {
                    var beh = view as Behaviour;
                    if (!beh) continue;
                    if (beh.enabled == enableRuntime) continue;
                    Undo.RecordObject(beh, "Toggle LeadsBarView");
                    beh.enabled = enableRuntime;
                    EditorUtility.SetDirty(beh);
                    changed++;
                }
                Debug.Log(changed > 0
                    ? $"✅ {(enableRuntime ? "Re-enabled" : "Disabled")} {changed} LeadsBarView component(s)."
                    : $"ℹ️ LeadsBarView already {(enableRuntime ? "enabled" : "disabled")}.");
            }

            if (purgeRuntimeChildren)
            {
                var content = GameObject.Find(ContentPath)?.transform;
                if (content)
                {
                    int removed = 0;
                    // Keep anything that looks like our demo cards (has V/Text_Title child).
                    for (int i = content.childCount - 1; i >= 0; i--)
                    {
                        var ch = content.GetChild(i);
                        bool isDemo = ch.Find("V/Text_Title") != null;
                        if (!isDemo)
                        {
                            Undo.DestroyObjectImmediate(ch.gameObject);
                            removed++;
                        }
                    }
                    if (removed > 0) Debug.Log($"🧹 Removed {removed} runtime-injected object(s) from Content_Leads.");
                }
            }

            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();
        }

        private static Type FindTypeByName(string simpleName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var t = asm.GetTypes().FirstOrDefault(x => x.Name == simpleName);
                    if (t != null) return t;
                } catch { }
            }
            return null;
        }
    }
}
#endif
