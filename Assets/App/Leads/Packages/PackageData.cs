// Assembly: AQ.App
// File: Assets/App/Leads/Packages/PackageData.cs
// Purpose: The lead package: the unit of story progress (Stephen-ruled 2026-08-31).
//          A package bundles 1 to 5 member lead cards; fulfilling ALL of them
//          shows the package's story beat and pays the package's rewards.
//          Member cards are ordinary LeadData assets and carry no rewards and
//          no resolution dialogue of their own (structure v2.2 Part G).

using System;
using UnityEngine;

namespace AQ.App.Leads.Packages
{
    public enum PackageBeatType
    {
        EvidenceTurn = 0,
        CharacterFact = 1,
        AllyLine = 2,
        ArtCaption = 3
    }

    [CreateAssetMenu(fileName = "Package", menuName = "AQ/Leads/Package", order = 11)]
    public sealed class PackageData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Package id, e.g. fk_p01_04. Recorded in flags; never rename after ship.")]
        public string packageId;

        [Tooltip("Chapter number 1..10. Display and analytics.")]
        public int chapter = 1;

        [Tooltip("Working title. Placeholder until Stephen rules copy.")]
        public string title = "";

        [Header("Members")]
        [Tooltip("The leadIds of the member cards, bar order. 1 to 5 entries.")]
        public string[] memberCardIds = Array.Empty<string>();

        [Header("Beat")]
        public PackageBeatType beatType = PackageBeatType.EvidenceTurn;

        [Tooltip("Dialogue beat (evidence turns, character facts, Ally lines). Optional if caption-only.")]
        public CaseGraph beatDialogue;

        [Tooltip("Art for an art-with-caption beat. Optional.")]
        public Sprite beatArt;

        [TextArea(1, 4)]
        [Tooltip("Caption shown with beatArt, or a one-line beat when no dialogue exists. Placeholder until ruled.")]
        public string beatCaption = "";

        [Header("Package rewards (members pay 0; Part G assumption 4)")]
        public int softCurrency;
        public int energyGrant;
        public int premiumGrant;

        [Tooltip("SpecialId granted on beat dismissal (SkeletonKey, BoxKnife, ...). Empty = none.")]
        public string specialRewardId = "";
        public int specialRewardCount = 1;

        // ---- Flags (GameFlags keys; ride the save aggregate) ----
        // Rule 5 discipline: beat_seen is set only after the beat presentation is
        // dismissed; beat_paid guards reward idempotence so a crash between the
        // two re-shows the beat but never double-pays.
        public string BeatSeenFlag => "pkg." + packageId + ".beat_seen";
        public string BeatPaidFlag => "pkg." + packageId + ".beat_paid";
        // Set by the FTUE choreography after it has shown this package's beat as
        // the episode intro; the presenter then pays and marks seen on completion
        // without presenting the same lines twice. Rule 5: set after display.
        public string BeatPrePlayedFlag => "pkg." + packageId + ".beat_preplayed";
    }
}
