using System.Collections;
using UnityEngine;
using AQ.App;
using AQ.App.Leads;
using AQ.App.Analytics;
using AQ.SharedKernel.CaseFlow;

namespace AQ.App.CaseFlow
{
    /// <summary>
    /// Bridges the lead satisfaction pipeline into CaseFlow step transitions:
    ///   "Board_Active" → "Lead_Ready"  : fires when any lead reaches LeadState.Ready
    ///   "Lead_Ready"   → next step     : fires when the player taps Proceed on a ready lead card
    /// </summary>
    [DefaultExecutionOrder(0)]
    public sealed class CaseFlowLeadBridgeMB : MonoBehaviour
    {
        [Header("Resolution Dialogue")]
        [SerializeField] private CaseGraph       resolutionDialogue;
        [SerializeField] private DialogueRunner  dialogueRunner;

        private LeadsRepository  _repo;
        private LeadsBarView     _bar;
        private ICaseFlowService _svc;

        private const string kBoardActive = "Board_Active";
        private const string kLeadReady   = "Lead_Ready";

        void Start()
        {
            _svc  = CaseFlowLocator.Instance;
            _repo = FindAnyObjectByType<LeadsRepository>();
            _bar  = FindAnyObjectByType<LeadsBarView>();

            if (_repo != null) _repo.LeadsChanged    += OnLeadsChanged;
            if (_bar  != null) _bar.ProceedRequested += OnProceed;

            // Second path: direct state-change notifications (fired by LeadRequirementChecker)
            LeadsRuntimeBus.OnLeadStateChanged += OnLeadStateChanged;

            if (_svc  == null) Debug.LogWarning("[CaseFlowLeadBridge] CaseFlowLocator.Instance is null — add CaseFlowOrchestratorMB to scene.", this);
            if (_repo == null) Debug.LogWarning("[CaseFlowLeadBridge] LeadsRepository not found in scene.", this);
            if (_bar  == null) Debug.LogWarning("[CaseFlowLeadBridge] LeadsBarView not found in scene.", this);

            StartCoroutine(CatchUpNextFrame());
        }

        void OnDestroy()
        {
            if (_repo != null) _repo.LeadsChanged          -= OnLeadsChanged;
            if (_bar  != null) _bar.ProceedRequested       -= OnProceed;
            LeadsRuntimeBus.OnLeadStateChanged             -= OnLeadStateChanged;
        }

        private IEnumerator CatchUpNextFrame()
        {
            yield return null;
            OnLeadsChanged();
        }

        private void OnLeadStateChanged(LeadData lead)
        {
            if (lead == null || lead.RuntimeState != LeadState.Ready) return;
            TryAdvanceFromBoardActive(lead.leadId);
        }

        private void OnLeadsChanged()
        {
            if (_svc == null || _repo == null) return;
            if (CurrentKey() != kBoardActive) return;

            var leads = _repo.CurrentLeads;
            for (int i = 0; i < leads.Count; i++)
            {
                if (leads[i] != null && leads[i].RuntimeState == LeadState.Ready)
                {
                    TryAdvanceFromBoardActive(leads[i].leadId);
                    return;
                }
            }
        }

        private void TryAdvanceFromBoardActive(string leadId)
        {
            if (_svc == null) return;
            if (CurrentKey() != kBoardActive) return;

            if (_svc.CompleteCurrentStep())
                Debug.Log($"[CaseFlowLeadBridge] Lead '{leadId}' Ready → step now '{CurrentKey()}'", this);
        }

        private void OnProceed(LeadData lead)
        {
            if (_svc == null) return;
            if (lead == null || lead.RuntimeState != LeadState.Ready) return;

            // Locker items count toward satisfaction, so a Ready card may need to
            // draw from the Evidence Locker. Confirm before consuming stash.
            int fromLocker = CountLockerDraw(lead);
            if (fromLocker > 0)
            {
                string noun = fromLocker == 1 ? "item" : "items";
                UI.Common.ConfirmPopup.Show(
                    "USE LOCKER ITEMS?",
                    $"{fromLocker} required {noun} will be taken from your Evidence Locker.",
                    "PROCEED",
                    onConfirm: () => DoProceed(lead));
                return;
            }

            DoProceed(lead);
        }

        /// <summary>How many required items the board can't cover and the locker would supply.</summary>
        private static int CountLockerDraw(LeadData lead)
        {
            var checker = LeadRequirementChecker.Instance;
            if (checker == null || lead.requirements == null) return 0;

            // Greedy board-first allocation, sharing counts across duplicate itemIds
            // (mirrors the checker's satisfaction pass).
            var boardTemp = new System.Collections.Generic.Dictionary<string, int>();
            int shortfall = 0;
            foreach (var req in lead.requirements)
            {
                var def = req.itemDefinition;
                if (def == null || string.IsNullOrEmpty(def.itemId)) continue;

                if (!boardTemp.TryGetValue(def.itemId, out int avail))
                    avail = checker.GetBoardCount(def.itemId);

                int needed = req.quantity < 1 ? 1 : req.quantity;
                int fromBoard = Mathf.Min(avail, needed);
                boardTemp[def.itemId] = avail - fromBoard;
                shortfall += needed - fromBoard;
            }
            return shortfall;
        }

        // FTUE first-merge choreography (Assembly-CSharp) drives these two entries.
        // Set for one DoProceed call: the resolution dialogue boots at this node
        // instead of the graph's startId (payoff resumes at N4 after the intro
        // span already played N1–N3).
        private string _dialogueStartOverrideId;

        /// <summary>
        /// Plays a sub-span of a dialogue graph outside the lead-proceed flow,
        /// with the same bar-hide/runner-activate mechanics as a resolution boot.
        /// Used by the FTUE choreography for the L1 intro (nodes 1–3).
        /// </summary>
        public void PlayIntroForFtue(CaseGraph graph, string startNodeId, string endAfterNodeId)
        {
            if (dialogueRunner == null || graph == null)
            {
                Debug.LogWarning("[CaseFlowLeadBridge] PlayIntroForFtue: missing runner or graph.", this);
                return;
            }

            if (_bar != null) _bar.gameObject.SetActive(false);
            dialogueRunner.DialogueEnded += OnDialogueEnded;
            dialogueRunner.gameObject.SetActive(true);
            dialogueRunner.BootWithGraph(graph, startNodeId, endAfterNodeId);
        }

        /// <summary>
        /// Auto-proceed for the FTUE first-merge choreography — the exact card-tap
        /// path, minus the tap, with the resolution dialogue resuming at
        /// <paramref name="dialogueStartNodeId"/>. Skips the locker-draw confirm:
        /// on the FTUE board the requirement is always board-covered.
        /// </summary>
        public void ProceedForFtue(LeadData lead, string dialogueStartNodeId)
        {
            _dialogueStartOverrideId = dialogueStartNodeId;
            try { DoProceed(lead); }
            finally { _dialogueStartOverrideId = null; }
        }

        private void DoProceed(LeadData lead)
        {
            if (_svc == null) return;
            if (lead == null || lead.RuntimeState != LeadState.Ready) return;

            GameAnalytics.LogCardSubmit(lead.leadId);

            // Hold reward/consumption flight FX until the resolution dialogue closes —
            // rewards fire inside BroadcastActivated, the same frame the dialogue opens,
            // and the chips should fly over the restored grid + HUD, not the stage.
            // Released by DialogueStageMB on DialogueClosed.
            UI.FlightFX.SetHold(true);

            // Consume board items and remove lead card — LeadActivationBridgeMB handles this
            // via the bus so AQ.App doesn't need a direct ref to MergeBoardController.
            LeadsRuntimeBus.BroadcastActivated(lead);

            // Advance the narrative step exactly once when the first lead resolves.
            // After that the step stays at Resolution and subsequent leads just boot dialogue.
            if (CurrentKey() == kLeadReady)
            {
                if (_svc.CompleteCurrentStep())
                    Debug.Log($"[CaseFlowLeadBridge] Proceed '{lead?.leadId}' → step now '{CurrentKey()}'", this);
            }

            TryBootDialogue(lead);
        }

        private void TryBootDialogue(LeadData lead)
        {
            if (dialogueRunner == null)
            {
                Debug.LogWarning("[CaseFlowLeadBridge] TryBootDialogue: dialogueRunner is NULL — assign it in the Inspector on this GameObject.", this);
                return;
            }

            if (_bar != null) _bar.gameObject.SetActive(false);
            dialogueRunner.DialogueEnded += OnDialogueEnded;
            dialogueRunner.gameObject.SetActive(true);

            if (lead?.resolutionDialogue != null && !string.IsNullOrEmpty(_dialogueStartOverrideId))
                dialogueRunner.BootWithGraph(lead.resolutionDialogue, _dialogueStartOverrideId, null);
            else if (lead?.resolutionDialogue != null)
                dialogueRunner.BootWithGraph(lead.resolutionDialogue);
            else
                dialogueRunner.BootWithText("Ally Quinn", $"{lead?.title}: Missing dialog data - ID: {lead?.leadId}");
        }

        private void OnDialogueEnded()
        {
            dialogueRunner.DialogueEnded -= OnDialogueEnded;
            if (_bar != null) _bar.gameObject.SetActive(true);
        }

        private string CurrentKey()
        {
            var s = _svc?.Current;
            if (s == null) return null;
            return s.StepIndex < s.Steps.Count ? s.Steps[s.StepIndex] : null;
        }
    }
}
