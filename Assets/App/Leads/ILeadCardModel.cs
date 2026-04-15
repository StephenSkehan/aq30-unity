using UnityEngine;

namespace AQ.App.Leads
{
    /// <summary>
    /// Contract between LeadCardView and the data it renders.
    /// Implement this on any type that can be bound to a lead card.
    /// </summary>
    public interface ILeadCardModel
    {
        string Title { get; }
        string Subtitle { get; }
        string ActionTag { get; }
        Sprite ActorPortrait { get; }
        LeadRequirement[] Requirements { get; }
        bool CanProceed { get; }
    }
}
