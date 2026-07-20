#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using AQ.App;
using AQ.App.Leads;
using AQ.App.UI.Board;

namespace AQ.UI.Board.EditorTools
{
    /// <summary>
    /// Headless QA drivers for the FTUE first-merge choreography. Status is
    /// log-based (console pipeline can go stale — read Editor.log if needed).
    /// Drive the sequence with: QA Reset + Play → Status → QA End Dialogue
    /// (Play Mode) → QA Merge First Pair → Status.
    /// </summary>
    public static class QAFtueChoreo
    {
        [MenuItem("AQ/Dev/QA FTUE Choreo Status")]
        public static void Status()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[QAFtueChoreo] Enter Play Mode first.");
                return;
            }

            int stage = PlayerPrefs.GetInt(FTUEFirstMergeChoreographyMB.StageKey, 0);
            bool present = GameObject.Find("FTUEFirstMergeChoreography") != null;

            var board = Object.FindAnyObjectByType<MergeBoardController>();
            int t1 = 0, t2 = 0;
            if (board != null && board.GridReady)
                for (int r = 0; r < board.Rows; r++)
                    for (int c = 0; c < board.Cols; c++)
                    {
                        var v = board.Get(r, c);
                        if (v == null || v.Kind != TileKind.Item) continue;
                        var id = board.GetItemId(v);
                        if (id == "audio_investigation_t1") t1++;
                        else if (id == "audio_investigation_t2") t2++;
                    }

            string leadState = "absent";
            var repo = Object.FindAnyObjectByType<LeadsRepository>();
            if (repo != null)
                foreach (var lead in repo.CurrentLeads)
                    if (lead != null && lead.leadId == "e1_tip")
                        leadState = lead.RuntimeState.ToString();

            var runner = Object.FindAnyObjectByType<DialogueRunner>(FindObjectsInactive.Include);
            bool dialogueOpen = runner != null && runner.gameObject.activeInHierarchy &&
                                runner.Panel != null && runner.Panel.gameObject.activeInHierarchy;
            string node = runner != null ? runner.GetCurrentNodeId() : "-";

            Debug.Log($"[QAFtueChoreo] stage={stage} choreoActive={present} " +
                      $"audioT1onBoard={t1} audioT2onBoard={t2} e1_tip={leadState} " +
                      $"dialogueOpen={dialogueOpen} dialogueNode={node}");
        }

        /// <summary>
        /// Drives DialogueRunner.OnAdvance (the real tap path — typing skip,
        /// pagination, node-range end included). Call repeatedly to tap through.
        /// </summary>
        [MenuItem("AQ/Dev/QA Advance Dialogue Once")]
        public static void AdvanceDialogueOnce()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[QAFtueChoreo] Enter Play Mode first.");
                return;
            }

            var runner = Object.FindAnyObjectByType<DialogueRunner>(FindObjectsInactive.Include);
            if (runner == null || !runner.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[QAFtueChoreo] No active DialogueRunner.");
                return;
            }

            var m = typeof(DialogueRunner).GetMethod("OnAdvance",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            m.Invoke(runner, null);
            Debug.Log($"[QAFtueChoreo] Advanced — node now '{runner.GetCurrentNodeId()}'.");
        }

        [MenuItem("AQ/Dev/QA FTUE Choreo Reset Stage Flag")]
        public static void ResetStageFlag()
        {
            PlayerPrefs.DeleteKey(FTUEFirstMergeChoreographyMB.StageKey);
            PlayerPrefs.Save();
            Debug.Log("[QAFtueChoreo] Stage flag cleared (full QA Reset still recommended for a clean run).");
        }
    }
}
#endif
