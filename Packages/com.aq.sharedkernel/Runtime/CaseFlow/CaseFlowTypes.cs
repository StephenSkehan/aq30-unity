// Assembly: AQ.SharedKernel
// File: Runtime/CaseFlow/CaseFlowTypes.cs

using System;
using System.Collections.Generic;

namespace AQ.SharedKernel.CaseFlow
{
    public readonly struct EpisodeId
    {
        public string Value { get; }
        public EpisodeId(string value) { Value = value ?? string.Empty; }
        public override string ToString() => Value;
    }

    public sealed class CaseFlowState
    {
        public EpisodeId Episode { get; }
        public int StepIndex { get; }
        public IReadOnlyList<string> Steps { get; }
        public bool IsComplete => StepIndex >= Steps.Count;

        public CaseFlowState(EpisodeId episode, int stepIndex, IReadOnlyList<string> steps)
        {
            Episode = episode;
            StepIndex = stepIndex;
            Steps = steps ?? Array.Empty<string>();
        }
    }
}
