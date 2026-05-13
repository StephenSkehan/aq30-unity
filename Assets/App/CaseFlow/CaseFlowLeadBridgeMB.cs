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

            GameAnalytics.LogCardSubmit(lead.leadId);

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

            if (lead?.resolutionDialogue != null)
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
