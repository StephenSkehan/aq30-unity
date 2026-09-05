using System.Collections.Generic;
using AQ.App;
using AQ.App.Leads;
using NUnit.Framework;
using UnityEngine;

namespace AQ.Tests.EditMode
{
    /// <summary>
    /// The Agency Bridge (2026-08-24). Before this existed, a player choice could set a
    /// persistent flag through dialogue but no flag could change which leads exist:
    /// LeadsRepository gated on RequiredLeadIds alone and LeadOutcomeMB spawned every id
    /// in SpawnLeadIds unconditionally, so every agency map that promised "this decision
    /// changes which leads spawn" was fiction.
    ///
    /// LeadData now carries requiresFlag/forbidsFlag, read from the unified GameFlags
    /// store. The authoring pattern is a sibling pair: a parent lead spawns BOTH children,
    /// one requiring the choice flag and one forbidding it, and the player's decision
    /// decides which of the two they actually get.
    ///
    /// Coverage per the robustness rules: gate open, gate shut, flag set before spawn,
    /// flag set after spawn, and the restore path (a save that predates the gate must be
    /// re-evaluated, because the spawn-time check is the optimization and the state-scan
    /// is the guarantee).
    /// </summary>
    public class LeadFlagGateTests
    {
        private const string ChoiceFlag = "test_agency_choice";
        private const string OtherFlag  = "test_agency_other";

        private LeadsRepository _repo;
        private GameObject      _go;
        private readonly List<LeadData> _made = new List<LeadData>();

        [SetUp]
        public void SetUp()
        {
            GameFlags.Clear(ChoiceFlag);
            GameFlags.Clear(OtherFlag);
            _go = new GameObject("LeadsRepoUnderTest");
            _repo = _go.AddComponent<LeadsRepository>();
        }

        [TearDown]
        public void TearDown()
        {
            GameFlags.Clear(ChoiceFlag);
            GameFlags.Clear(OtherFlag);
            foreach (var l in _made) if (l != null) Object.DestroyImmediate(l);
            _made.Clear();
            if (_go != null) Object.DestroyImmediate(_go);
        }

        private LeadData MakeLead(string id, string requires = "", string forbids = "")
        {
            var lead = ScriptableObject.CreateInstance<LeadData>();
            lead.leadId = id;
            lead.title = id;
            lead.requiresFlag = requires;
            lead.forbidsFlag = forbids;
            lead.requirements = System.Array.Empty<LeadRequirement>();
            lead.RequiredLeadIds = System.Array.Empty<string>();
            lead.RuntimeState = LeadState.Blocked;
            _made.Add(lead);
            return lead;
        }

        // ---- the pure gate ----

        [Test]
        public void UngatedLead_IsAlwaysOpen()
        {
            var lead = MakeLead("ungated");
            Assert.IsTrue(LeadsRepository.FlagGateOpen(lead),
                "A lead with neither requiresFlag nor forbidsFlag must be unaffected by the gate.");
        }

        [Test]
        public void RequiresFlag_ShutUntilFlagIsSet()
        {
            var lead = MakeLead("needs", requires: ChoiceFlag);
            Assert.IsFalse(LeadsRepository.FlagGateOpen(lead), "requiresFlag unset must keep the gate shut.");

            GameFlags.Set(ChoiceFlag);
            Assert.IsTrue(LeadsRepository.FlagGateOpen(lead), "requiresFlag set must open the gate.");
        }

        [Test]
        public void ForbidsFlag_OpenUntilFlagIsSet()
        {
            var lead = MakeLead("avoids", forbids: ChoiceFlag);
            Assert.IsTrue(LeadsRepository.FlagGateOpen(lead), "forbidsFlag unset must leave the gate open.");

            GameFlags.Set(ChoiceFlag);
            Assert.IsFalse(LeadsRepository.FlagGateOpen(lead), "forbidsFlag set must shut the gate.");
        }

        [Test]
        public void SiblingPair_ExactlyOneIsEverOpen()
        {
            var tookIt  = MakeLead("branch_a", requires: ChoiceFlag);
            var leftIt  = MakeLead("branch_b", forbids:  ChoiceFlag);

            Assert.IsFalse(LeadsRepository.FlagGateOpen(tookIt));
            Assert.IsTrue(LeadsRepository.FlagGateOpen(leftIt));

            GameFlags.Set(ChoiceFlag);

            Assert.IsTrue(LeadsRepository.FlagGateOpen(tookIt));
            Assert.IsFalse(LeadsRepository.FlagGateOpen(leftIt));
        }

        [Test]
        public void BothConditions_RequireSetAndForbidClear()
        {
            var lead = MakeLead("both", requires: ChoiceFlag, forbids: OtherFlag);

            Assert.IsFalse(LeadsRepository.FlagGateOpen(lead), "neither condition met");

            GameFlags.Set(ChoiceFlag);
            Assert.IsTrue(LeadsRepository.FlagGateOpen(lead), "required set, forbidden clear");

            GameFlags.Set(OtherFlag);
            Assert.IsFalse(LeadsRepository.FlagGateOpen(lead), "forbidden flag must veto a met requirement");
        }

        // ---- spawn-time behaviour ----

        [Test]
        public void SpawnLead_FlagSetBeforeSpawn_IsPlayable()
        {
            GameFlags.Set(ChoiceFlag);
            var lead = MakeLead("open_at_spawn", requires: ChoiceFlag);

            _repo.SpawnLead(lead);

            Assert.AreEqual(LeadState.Ready, lead.RuntimeState,
                "A zero-requirement lead whose gate is open spawns Ready.");
        }

        [Test]
        public void SpawnLead_GateShut_StaysBlockedButIsTracked()
        {
            var lead = MakeLead("shut_at_spawn", requires: ChoiceFlag);

            _repo.SpawnLead(lead);

            Assert.AreEqual(LeadState.Blocked, lead.RuntimeState,
                "A shut gate must not hand the player the lead. Blocked leads are never activated, " +
                "so the unchosen sibling cannot advance case progress.");
        }

        [Test]
        public void FlagSetAfterSpawn_ReSpawnOpensTheLead()
        {
            var lead = MakeLead("late_flag", requires: ChoiceFlag);
            _repo.SpawnLead(lead);
            Assert.AreEqual(LeadState.Blocked, lead.RuntimeState);

            GameFlags.Set(ChoiceFlag);
            _repo.SpawnLead(lead);

            Assert.AreEqual(LeadState.Ready, lead.RuntimeState,
                "A flag arriving after the spawn must still be able to open the lead.");
        }

        [Test]
        public void FlagNeverSet_LeadNeverOpens()
        {
            var lead = MakeLead("never", requires: ChoiceFlag);
            _repo.SpawnLead(lead);
            _repo.SpawnLead(lead);

            Assert.AreEqual(LeadState.Blocked, lead.RuntimeState);
        }

        // ---- the restore path is the guarantee, not the spawn-time check ----

        [Test]
        public void Restore_ReEvaluatesTheGate_ASavePredatingItStillOpens()
        {
            var lead = MakeLead("restored", requires: ChoiceFlag);
            _repo.SpawnLead(lead);
            Assert.AreEqual(LeadState.Blocked, lead.RuntimeState);

            // The player made the choice, then the app died before the lead was re-spawned.
            GameFlags.Set(ChoiceFlag);

            _repo.ApplySavedStates(new List<LeadsRepository.LeadSaveState>
            {
                new LeadsRepository.LeadSaveState
                {
                    LeadId = "restored",
                    RuntimeState = (int)LeadState.Blocked,
                    SatisfiedRequirements = System.Array.Empty<bool>(),
                    Activated = false,
                },
            });

            Assert.AreNotEqual(LeadState.Blocked, lead.RuntimeState,
                "The restore-time scan must re-evaluate the flag gate, or a choice made before a " +
                "crash is silently lost and the lead can never open.");
        }

        [Test]
        public void Restore_GateStillShut_LeadStaysBlocked()
        {
            var lead = MakeLead("still_shut", requires: ChoiceFlag);
            _repo.SpawnLead(lead);

            _repo.ApplySavedStates(new List<LeadsRepository.LeadSaveState>
            {
                new LeadsRepository.LeadSaveState
                {
                    LeadId = "still_shut",
                    RuntimeState = (int)LeadState.Blocked,
                    SatisfiedRequirements = System.Array.Empty<bool>(),
                    Activated = false,
                },
            });

            Assert.AreEqual(LeadState.Blocked, lead.RuntimeState,
                "Restore must not heal a flag-gated lead into Ready just because it has no requirements.");
        }
    }
}
