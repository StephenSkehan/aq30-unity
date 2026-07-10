using UnityEditor;
using UnityEngine;
using AQ.App;

namespace AQ.EditorTools
{
    public static class TestChoiceDialogue
    {
        [MenuItem("AQ/Dev/Test Choice Dialogue (Play Mode)")]
        public static void Run()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[ChoiceTest] Enter Play Mode first.");
                return;
            }

            var runner = Object.FindAnyObjectByType<DialogueRunner>(FindObjectsInactive.Include);
            if (runner == null)
            {
                Debug.LogError("[ChoiceTest] No DialogueRunner in scene.");
                return;
            }

            var g = ScriptableObject.CreateInstance<CaseGraph>();
            g.startId = "T1";
            g.nodes = new[]
            {
                new CaseGraph.Node
                {
                    id = "T1", speaker = "Ally Quinn",
                    line = "Choice UI test. Two doors. Pick one.",
                    choices = new[]
                    {
                        new CaseGraph.Choice { text = "Take the left door",  nextId = "TL" },
                        new CaseGraph.Choice { text = "Take the right door", nextId = "TR" },
                    }
                },
                new CaseGraph.Node
                {
                    id = "TL", speaker = "Ally Quinn",
                    line = "Left door. This branch sets a flag you can verify.",
                    setsFlag = "test.choice.left", nextId = "TC"
                },
                new CaseGraph.Node
                {
                    id = "TR", speaker = "Ally Quinn",
                    line = "Right door. This branch sets a different flag.",
                    setsFlag = "test.choice.right", nextId = "TC"
                },
                new CaseGraph.Node
                {
                    id = "TC", speaker = "Ally Quinn",
                    line = "Branches converge here. Tap to end. (Flags: left=" +
                           "check console after end.)"
                },
            };

            if (runner.Panel != null) runner.Panel.gameObject.SetActive(true);
            runner.BootWithGraph(g);
            Debug.Log("[ChoiceTest] Booted. After the run: DialogueFlags left=" +
                      DialogueFlags.Has("test.choice.left") + " right=" + DialogueFlags.Has("test.choice.right"));
        }
    }
}
