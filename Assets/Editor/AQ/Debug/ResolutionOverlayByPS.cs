#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools
{
    /// <summary>
    /// Forces Resolution overlay visible & clickable and adds a debug log to the Resolve button.
    /// Safe for batchmode (PowerShell) and menu use.
    /// </summary>
    public static class ResolutionOverlayByPS
    {
        // Adjust if your scene is different, or open the scene manually first.
        private const string DefaultScenePath = "Assets/Scenes/WK2_BoardDemo.unity";

        [MenuItem("AQ/Debug/Resolution/Setup Overlay For Debug (PS)")]
        public static void SetupOverlayForDebugMenu() => SetupOverlayForDebug();

        [MenuItem("AQ/Debug/Resolution/Revert Overlay Debug (PS)")]
        public static void RevertOverlayDebugMenu() => RevertOverlayDebug();

        /// <summary>
        /// Ensures CanvasGroup(alpha=1, interactable, blocksRaycasts) on ResolutionRoot and
        /// adds a persistent OnClick listener that logs when Resolve is pressed.
        /// </summary>
        public static void SetupOverlayForDebug()
        {
            // Open scene if needed
            var active = EditorSceneManager.GetActiveScene();
            if (!active.isLoaded || string.IsNullOrEmpty(active.path))
            {
                try { EditorSceneManager.OpenScene(DefaultScenePath, OpenSceneMode.Single); }
                catch { Debug.LogWarning($"[PS] Could not open default scene at '{DefaultScenePath}'. Using active scene."); }
            }

            var root = GameObject.Find("ResolutionRoot");
            if (root == null)
            {
                Debug.LogWarning("[PS] ResolutionRoot not found in the open scene.");
                return;
            }

            // Make overlay visible & interactive
            var cg = root.GetComponent<CanvasGroup>() ?? Undo.AddComponent<CanvasGroup>(root);
            Undo.RecordObject(cg, "Set CanvasGroup on ResolutionRoot");
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
            EditorUtility.SetDirty(cg);

            // Ensure we have a logger component to call
            var logger = root.GetComponent<AQ.App.CaseFlow.DebugLogOnResolveMB>()
                        ?? Undo.AddComponent<AQ.App.CaseFlow.DebugLogOnResolveMB>(root);

            // Find the Resolve button
            var btn = FindResolveButton(root);
            if (btn == null)
            {
                Debug.LogWarning("[PS] Resolve button not found under ResolutionRoot.");
            }
            else
            {
                // Wire a persistent log listener once
                if (!HasPersistentListener(btn, logger, nameof(AQ.App.CaseFlow.DebugLogOnResolveMB.LogNow)))
                {
                    UnityEventTools.AddPersistentListener(btn.onClick, logger.LogNow);
                    EditorUtility.SetDirty(btn);
                }
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[PS] Overlay debug wiring complete (CanvasGroup alpha=1, log listener added).");
        }

        /// <summary>Removes the debug log listener and helper component (optional clean-up).</summary>
        public static void RevertOverlayDebug()
        {
            var root = GameObject.Find("ResolutionRoot");
            if (root == null)
            {
                Debug.LogWarning("[PS] ResolutionRoot not found in the open scene.");
                return;
            }

            var btn = FindResolveButton(root);
            var logger = root.GetComponent<AQ.App.CaseFlow.DebugLogOnResolveMB>();

            if (btn != null && logger != null)
            {
                for (int i = btn.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                {
                    if (btn.onClick.GetPersistentTarget(i) == logger &&
                        btn.onClick.GetPersistentMethodName(i) == nameof(AQ.App.CaseFlow.DebugLogOnResolveMB.LogNow))
                    {
                        UnityEventTools.RemovePersistentListener(btn.onClick, i);
                    }
                }
                EditorUtility.SetDirty(btn);
            }

            if (logger != null)
                Undo.DestroyObjectImmediate(logger);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[PS] Overlay debug wiring reverted.");
        }

        // --- helpers ---

        private static Button FindResolveButton(GameObject root)
        {
            // Prefer the expected path
            var t = root.transform.Find("ResolutionPanel/ResolveButton");
            if (t) return t.GetComponent<Button>();

            // Fallback: any child named *Resolve*
            return root.GetComponentsInChildren<Button>(true)
                       .FirstOrDefault(b => b.name.Contains("Resolve"));
        }

        private static bool HasPersistentListener(Button btn, Object target, string method)
        {
            int n = btn.onClick.GetPersistentEventCount();
            for (int i = 0; i < n; i++)
            {
                if (btn.onClick.GetPersistentTarget(i) == target &&
                    btn.onClick.GetPersistentMethodName(i) == method)
                    return true;
            }
            return false;
        }
    }
}
#endif
