// Assembly: AQ.SharedKernel
// File: Runtime/CaseFlow/ICaseFlowService.cs

namespace AQ.SharedKernel.CaseFlow
{
    public interface ICaseFlowService
    {
        CaseFlowState Current { get; }
        void Begin(EpisodeId episode, params string[] steps);
        /// <summary>Advance exactly one step; returns false if already complete.</summary>
        bool CompleteCurrentStep();
        /// <summary>Reset progress (keeps episode & steps).</summary>
        void Reset();
    }
}
