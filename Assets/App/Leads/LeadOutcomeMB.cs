using UnityEngine;
using UnityEngine.SceneManagement;
using AQ.App.Audio;
using AQ.App.Economy;
using AQ.App.Overflow;
using AQ.SharedKernel.Economy;

namespace AQ.App.Leads
{
    /// <summary>
    /// Responds to LeadsRuntimeBus.OnLeadActivated to apply a lead's outcome fields:
    ///   - SoftCurrency / EnergyGrant  → WalletLocator grant
    ///   - NarrativeFlags[]            → NarrativeFlags.Set per entry
    ///   - SpawnLeadIds[]              → LeadsRepository.SpawnLead per entry
    ///
    /// Auto-created after each scene load. Kept separate from LeadActivationBridgeMB
    /// so that component can stay in Assembly-CSharp focused on board item consumption.
    /// </summary>
    public sealed class LeadOutcomeMB : MonoBehaviour
    {
        private LeadsRepository _repo;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureExists()
        {
            CreateIfMissing();
            SceneManager.sceneLoaded += (_, __) => CreateIfMissing();
        }

        private static void CreateIfMissing()
        {
            if (FindFirstObjectByType<LeadOutcomeMB>() == null)
                new GameObject("~LeadOutcome").AddComponent<LeadOutcomeMB>();
        }

        private void Start()
        {
            _repo = FindAnyObjectByType<LeadsRepository>();
        }

        private void OnEnable()  => LeadsRuntimeBus.OnLeadActivated += OnLeadActivated;
        private void OnDisable() => LeadsRuntimeBus.OnLeadActivated -= OnLeadActivated;

        private void OnLeadActivated(LeadData lead)
        {
            if (lead == null) return;

            UISfxService.PlayLeadFulfilled();
            GrantRewards(lead);
            ApplyNarrativeFlags(lead);
            SpawnFollowUpLeads(lead);
        }

        private void GrantRewards(LeadData lead)
        {
            var wallet = WalletLocator.Instance;
            if (wallet == null) return;

            if (lead.SoftCurrency > 0) wallet.Grant("lead.outcome", Reward.Soft(lead.SoftCurrency));
            if (lead.EnergyGrant  > 0) wallet.Grant("lead.outcome", Reward.Energy(lead.EnergyGrant));

            if (!string.IsNullOrEmpty(lead.generatorRewardTypeId))
            {
                OverflowBucketService.Push(new OverflowTileData
                {
                    kind   = OverflowKind.Generator,
                    family = lead.generatorRewardTypeId,
                    tier   = lead.generatorRewardTier
                });
            }
        }

        private void ApplyNarrativeFlags(LeadData lead)
        {
            if (lead.NarrativeFlags == null) return;
            foreach (var flag in lead.NarrativeFlags)
                NarrativeFlags.Set(flag);
        }

        private void SpawnFollowUpLeads(LeadData lead)
        {
            if (lead.SpawnLeadIds == null || lead.SpawnLeadIds.Length == 0) return;
            if (_repo == null) _repo = FindAnyObjectByType<LeadsRepository>();
            if (_repo == null || _repo.database == null) return;

            foreach (var id in lead.SpawnLeadIds)
            {
                var spawned = _repo.database.FindById(id);
                if (spawned != null)
                    _repo.SpawnLead(spawned);
                else
                    Debug.LogWarning($"[LeadOutcome] SpawnLeadId '{id}' not found in database for lead '{lead.leadId}'.", this);
            }
        }
    }
}
