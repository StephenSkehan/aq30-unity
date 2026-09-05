// Assembly: AQ.App
// File: Assets/App/FTUE/FtueChoreographyConfig.cs
// Purpose: Per-episode data for the first-card FTUE choreography
//          (FTUEFirstMergeChoreographyMB, Assembly-CSharp). The choreography
//          used to hard-code The Listener (lead e1_tip, intro nodes E1_L1_N1
//          to N3, a seeded Audio T1 pair merging to Audio T2). Episode identity
//          is data (CLAUDE.md): the EpisodeCatalog entry references one of
//          these, and a null reference means the built-in Listener defaults,
//          so the shipped episode plays exactly as before.
//
// Two modes, chosen by seedCount:
//   seedCount >= 2  the guided action is a MERGE of pre-seeded items
//                   (The Listener: two Audio T1 on the board, merge to T2).
//   seedCount == 0  the guided action is the first GENERATOR TAP (Four Keys
//                   chapter 1: package 1 asks one Audio T1; the tap lands at
//                   ~0:13 after the ident and the death line, structure v2.2).
//                   The drop is made deterministic: if the tap did not yield
//                   seedItemId, the choreography places one.

using UnityEngine;

namespace AQ.App.FTUE
{
    [CreateAssetMenu(fileName = "FtueChoreography", menuName = "AQ/FTUE/Choreography Config", order = 30)]
    public sealed class FtueChoreographyConfig : ScriptableObject
    {
        [Header("The guided first card")]
        [Tooltip("leadId of the card the choreography guides to Ready and auto-proceeds.")]
        public string leadId;

        [Header("Intro (plays up front, before the guided action)")]
        [Tooltip("Graph to play as the intro. Null = the lead's own resolutionDialogue (Listener style: a node span of the payoff graph).")]
        public CaseGraph introGraph;
        [Tooltip("Start node of the intro span. Empty = the graph's startId.")]
        public string introStartNodeId;
        [Tooltip("End the intro after this node. Empty = play the graph to its end.")]
        public string introEndAfterNodeId;

        [Header("Payoff (after the guided action)")]
        [Tooltip("Node to resume the lead's resolution dialogue at when auto-proceeding (Listener: E1_L1_N4). Empty = no per-lead dialogue (package episodes: the package beat is the payoff).")]
        public string payoffStartNodeId;
        [Tooltip("Package whose beat the intro already showed in full. Its completion pays and marks seen without re-presenting. Empty = none.")]
        public string prePlayedPackageId;

        [Header("Board seeding and guide targets")]
        [Tooltip("Item family placed before the guide (e.g. audio_investigation).")]
        public string seedFamily = "audio_investigation";
        [Tooltip("Tier index of the seed item (0 = T1).")]
        public int seedTier = 0;
        [Tooltip("How many seed items to guarantee on the board. 2 = guided merge; 0 = guided generator tap.")]
        public int seedCount = 2;
        [Tooltip("Item id the guide pulses / the tap must yield (e.g. audio_investigation_t1).")]
        public string seedItemId = "audio_investigation_t1";
        [Tooltip("Merged goal item id (merge mode only). Empty in tap mode.")]
        public string targetItemId = "audio_investigation_t2";

        public bool GuidesGeneratorTap => seedCount <= 0;

        /// <summary>The Listener's shipped choreography, as constants were before this config existed.</summary>
        public static FtueChoreographyConfig ListenerDefaults()
        {
            var c = CreateInstance<FtueChoreographyConfig>();
            c.name               = "ListenerDefaults";
            c.leadId             = "e1_tip";
            c.introGraph         = null;
            c.introStartNodeId   = "E1_L1_N1";
            c.introEndAfterNodeId = "E1_L1_N3";
            c.payoffStartNodeId  = "E1_L1_N4";
            c.prePlayedPackageId = "";
            c.seedFamily         = "audio_investigation";
            c.seedTier           = 0;
            c.seedCount          = 2;
            c.seedItemId         = "audio_investigation_t1";
            c.targetItemId       = "audio_investigation_t2";
            return c;
        }
    }
}
