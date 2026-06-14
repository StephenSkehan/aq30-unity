using AQ.SharedKernel;

namespace AQ.App.Events
{
    public readonly struct CaseResolvedEvent : IGameEvent
    {
        public readonly string EpisodeId;
        public CaseResolvedEvent(string episodeId) { EpisodeId = episodeId; }
    }
}
