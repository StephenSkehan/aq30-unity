using UnityEditor;
using UnityEngine;
using AQ.App;

namespace AQ.EditorTools
{
    /// <summary>
    /// Boots a single dialogue node using a newly imported portrait sprite so
    /// the panel rendering can be verified without playing to the lead that
    /// uses it. Edit the two consts when checking a different character.
    /// </summary>
    public static class TestPortraitDialogue
    {
        const string SpritePath = "Assets/Art/Characters/Dot/char_dot_angry_f01.png";
        const string Speaker    = "Dot Ellis";

        [MenuItem("AQ/Dev/Test Portrait Dialogue (Play Mode)")]
        public static void Run()
        {
            if (!Application.isPlaying) { Debug.LogWarning("[PortraitTest] Enter Play Mode first."); return; }

            var runner = Object.FindAnyObjectByType<DialogueRunner>(FindObjectsInactive.Include);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            if (runner == null || sprite == null)
            {
                Debug.LogError($"[PortraitTest] runner={runner != null} sprite={sprite != null} ({SpritePath})");
                return;
            }

            var g = ScriptableObject.CreateInstance<CaseGraph>();
            g.startId = "P1";
            g.nodes = new[]
            {
                new CaseGraph.Node
                {
                    id = "P1", speaker = Speaker,
                    line = "Portrait render test — verify the face, crop and name plate, then tap to close.",
                    portrait = sprite
                }
            };

            if (runner.Panel != null) runner.Panel.gameObject.SetActive(true);
            runner.BootWithGraph(g);
            Debug.Log("[PortraitTest] booted with " + SpritePath);
        }

        // Boots a two-page-long single node so runner pagination can be QA'd.
        [MenuItem("AQ/Dev/Test Long Node Dialogue (Play Mode)")]
        public static void RunLongNode()
        {
            if (!Application.isPlaying) { Debug.LogWarning("[PortraitTest] Enter Play Mode first."); return; }
            var runner = Object.FindAnyObjectByType<DialogueRunner>(FindObjectsInactive.Include);
            if (runner == null) { Debug.LogError("[PortraitTest] no runner"); return; }
            var g = ScriptableObject.CreateInstance<CaseGraph>();
            g.startId = "L1";
            g.nodes = new[]
            {
                new CaseGraph.Node
                {
                    id = "L1", speaker = "Pagination Test",
                    line = "First sentence of a very long node that should not fit on one page of the strip. " +
                           "Second sentence padding the count well past the limit with more words than needed. " +
                           "Third sentence to guarantee we cross two hundred and forty characters comfortably. " +
                           "Fourth sentence that must appear on PAGE TWO if pagination works correctly."
                }
            };
            runner.gameObject.SetActive(true);
            if (runner.Panel != null) runner.Panel.gameObject.SetActive(true);
            runner.BootWithGraph(g);
        }

        // Boots a real Ep1 CaseGraph asset so per-graph stage wiring
        // (stageBackground + lit scrim) can be verified end to end.
        [MenuItem("AQ/Dev/Test E1 Tip Dialogue (Play Mode)")]
        public static void RunTipGraph()
        {
            if (!Application.isPlaying) { Debug.LogWarning("[PortraitTest] Enter Play Mode first."); return; }
            var runner = Object.FindAnyObjectByType<DialogueRunner>(FindObjectsInactive.Include);
            var g = AssetDatabase.LoadAssetAtPath<CaseGraph>("Assets/Content/TheListener/Dialogue/Resolve_E1_Tip.asset");
            if (runner == null || g == null) { Debug.LogError($"[PortraitTest] runner={runner != null} graph={g != null}"); return; }
            Debug.Log($"[PortraitTest] tip graph stageBackground={(g.stageBackground != null ? g.stageBackground.name : "NULL")}");
            runner.gameObject.SetActive(true);
            if (runner.Panel != null) runner.Panel.gameObject.SetActive(true);
            runner.BootWithGraph(g);
        }

        // Ends the running dialogue through the real End() path (JumpTo to a
        // missing node) so DialogueClosed/stage-restore can be QA'd without a tap.
        [MenuItem("AQ/Dev/QA End Dialogue (Play Mode)")]
        public static void EndDialogue()
        {
            if (!Application.isPlaying) { Debug.LogWarning("[PortraitTest] Enter Play Mode first."); return; }
            var runner = Object.FindAnyObjectByType<DialogueRunner>(FindObjectsInactive.Include);
            if (runner == null) { Debug.LogError("[PortraitTest] no DialogueRunner in scene"); return; }
            runner.JumpTo("__qa_end__");
            Debug.Log("[PortraitTest] forced dialogue end");
        }
    }
}
