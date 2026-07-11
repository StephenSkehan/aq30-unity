using UnityEngine;
using UnityEngine.SceneManagement;
using AQ.App.Leads;
using AQ.App.UI.Board;

namespace AQ.App
{
    /// <summary>
    /// Responds to LeadsRuntimeBus.OnLeadActivated (fired by CaseFlowLeadBridgeMB when
    /// the player taps Proceed on a Ready lead card) to:
    ///   1. Remove the lead from the repository so the card disappears from the UI.
    ///   2. Consume all required board items, firing OnItemRemoved per item so
    ///      LeadRequirementChecker can re-evaluate remaining leads.
    ///
    /// Lives in Assembly-CSharp so it can hold a direct reference to MergeBoardController
    /// without creating a circular dependency with the AQ.App assembly.
    /// Auto-created after each scene load via RuntimeInitializeOnLoadMethod.
    /// </summary>
    public sealed class LeadActivationBridgeMB : MonoBehaviour
    {
        [SerializeField] private MergeBoardController _board;
        [SerializeField] private LeadsRepository      _repo;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureExists()
        {
            CreateIfMissing();
            // RuntimeInitializeOnLoadMethod fires only on the FIRST scene load.
            // Hook sceneLoaded so subsequent LoadScene calls (e.g. game reset) also recreate us.
            SceneManager.sceneLoaded += (_, __) => CreateIfMissing();
        }

        private static void CreateIfMissing()
        {
            if (FindFirstObjectByType<LeadActivationBridgeMB>() == null)
                new GameObject("~LeadActivationBridge").AddComponent<LeadActivationBridgeMB>();
        }

        private void Start()
        {
            if (!_board) _board = FindAnyObjectByType<MergeBoardController>();
            if (!_repo)  _repo  = FindAnyObjectByType<LeadsRepository>();
        }

        private void OnEnable()  => LeadsRuntimeBus.OnLeadActivated += OnLeadActivated;
        private void OnDisable() => LeadsRuntimeBus.OnLeadActivated -= OnLeadActivated;

        private void OnLeadActivated(LeadData lead)
        {
            if (_repo == null) _repo = FindAnyObjectByType<LeadsRepository>();
            _repo?.RemoveLead(lead);
            ConsumeItems(lead);
        }

        private void ConsumeItems(LeadData lead)
        {
            if (lead?.requirements == null || _board == null) return;

            foreach (var req in lead.requirements)
            {
                var def = req.itemDefinition;
                if (def == null) continue;

                int needed = req.quantity < 1 ? 1 : req.quantity;
                for (int i = 0; i < needed; i++)
                {
                    if (!_board.TryClearItem(def.family, def.tier))
                        Debug.LogWarning($"[LeadActivation] Item family='{def.family}' tier={def.tier} not found on board for lead '{lead.leadId}' (needed {needed}, cleared {i}).", this);
                }
            }
        }
    }
}
