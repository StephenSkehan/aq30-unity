#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools.Leads
{
    using AQ.App.Leads;

    public static class LeadsSeeder
    {
        private const string SeedPath = "Assets/AQ_Seed/Leads";
        private static void EnsureDir() { if (!Directory.Exists(SeedPath)) Directory.CreateDirectory(SeedPath); }

        [MenuItem("AQ/Leads/Seed FTUE Lead Assets")]
        public static void SeedFtueLeads()
        {
            EnsureDir();

            CreateLead("Lead_LabSetup",
                "Lab Setup (Priya)", "Set up scanner and laptop.",
                LeadActionType.LabRequest, 0,
                new [] { Req("CleanFingerprint", 2), Req("CrimeScenePhoto", 0), Req("ForensicLaptop", 3) },
                outcomes: LeadOutcomeHint.Evidence | LeadOutcomeHint.NewLeads);

            CreateLead("Lead_Surveillance_KP_CAM_12",
                "Surveillance — KP-CAM-12", "Scrub the footage for a clue.",
                LeadActionType.Surveillance, 1,
                new [] { Req("ChargedRecorder", 2) },
                outcomes: LeadOutcomeHint.Evidence);

            CreateLead("Lead_RecordsPull",
                "Records Pull — Plate", "Run the partial plate.",
                LeadActionType.RecordsPull, 1,
                new [] { Req("DMVQueryTool", 1) },
                outcomes: LeadOutcomeHint.NewLeads | LeadOutcomeHint.Rewards);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Seeded FTUE Lead assets under " + SeedPath);
        }

        private static LeadRequirement Req(string id, int tier) => new LeadRequirement
        {
            ItemId = id,
            MinTier = tier,
            ConsumeOnProceed = true,
            Label = id
        };

        private static void CreateLead(string id, string title, string oneLiner, LeadActionType type, int energy, LeadRequirement[] reqs, LeadOutcomeHint outcomes)
        {
            var asset = ScriptableObject.CreateInstance<LeadCardSO>();
            asset.LeadCardId = id;
            asset.Title = title;
            asset.OneLiner = oneLiner;
            asset.ActionType = type;
            asset.EnergyCost = energy;
            asset.Requirements = reqs;
            asset.OutcomeHints = outcomes;

            var path = $"{SeedPath}/{id}.asset";
            AssetDatabase.CreateAsset(asset, path);
        }
    }
}
#endif
