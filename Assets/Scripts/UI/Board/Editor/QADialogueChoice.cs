#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools
{
    /// <summary>
    /// Headless driver for dialogue choice buttons (L9/L11 branches):
    /// clicks the first or second active Button under the dialogue canvas.
    /// </summary>
    public static class QADialogueChoice
    {
        [MenuItem("AQ/Dev/QA Click Dialogue Choice 1")]
        private static void Choice1() => Click(0);

        [MenuItem("AQ/Dev/QA Click Dialogue Choice 2")]
        private static void Choice2() => Click(1);

        private static void Click(int index)
        {
            if (!Application.isPlaying) { Debug.LogWarning("[QAChoice] Play mode only."); return; }
            var panel = GameObject.Find("DialoguePanel");
            var root = panel != null ? panel.transform.root.gameObject : null;
            if (root == null)
            {
                // Fallback: any canvas containing "Dialogue" in its root name.
                foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
                    if (c.transform.root.name.Contains("Dialogue")) { root = c.transform.root.gameObject; break; }
            }
            if (root == null) { Debug.LogWarning("[QAChoice] No dialogue root found."); return; }

            var buttons = root.GetComponentsInChildren<Button>(false);
            if (index >= buttons.Length)
            {
                Debug.LogWarning($"[QAChoice] Only {buttons.Length} active buttons; wanted index {index}.");
                return;
            }
            Debug.Log($"[QAChoice] Clicking '{buttons[index].name}' ({index + 1} of {buttons.Length}).");
            buttons[index].onClick.Invoke();
        }
    }
}
#endif
