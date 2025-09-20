// Assembly: AQ.SharedKernel
// File: Runtime/CaseFlow/InMemoryCaseFlowService.cs

using System;
using System.Collections.Generic;

namespace AQ.SharedKernel.CaseFlow
{
    public sealed class InMemoryCaseFlowService : ICaseFlowService
    {
        private EpisodeId _episode;
        private List<string> _steps = new();
        private int _index = 0;

        public CaseFlowState Current => new CaseFlowState(_episode, _index, _steps);

        public void Begin(EpisodeId episode, params string[] steps)
        {
            _episode = episode;
            _steps = new List<string>(steps ?? Array.Empty<string>());
            _index = 0;
        }

        public bool CompleteCurrentStep()
        {
            if (_index >= _steps.Count) return false;
            _index++;
            return true;
        }

        public void Reset() => _index = 0;
    }
}
