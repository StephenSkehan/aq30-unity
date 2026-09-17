using System;
using UnityEngine;

namespace AQ.App.Episodes
{
    public struct EpisodeProgress
    {
        public bool Started;
        public bool Complete;
    }

    /// <summary>
    /// The episode-transition seam. The save system (Assembly-CSharp, which this
    /// assembly cannot reference) registers the handlers at Awake; AQ.App UI
    /// (resolution screen) and the selector popup call through them. Switching
    /// is persist-then-reload: the handler writes the outgoing episode's section
    /// and moves the save's pointer atomically, the caller reloads the scene,
    /// and the normal boot restore path does the rest.
    /// </summary>
    public static class EpisodeFlow
    {
        /// <summary>Registered by BoardSaveSystem: SaveNow with the pointer moved. False = nothing durable changed, do not reload.</summary>
        public static Func<string, bool> SwitchHandler;

        /// <summary>Registered by BoardSaveSystem: started/complete per episode, from the loaded aggregate.</summary>
        public static Func<string, EpisodeProgress> ProgressProvider;

        /// <summary>Registered by the selector popup's auto-installer.</summary>
        public static Action SelectorOpener;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            SwitchHandler = null;
            ProgressProvider = null;
            SelectorOpener = null;
        }

        public static bool TrySwitch(string episodeId) =>
            SwitchHandler != null && SwitchHandler(episodeId);

        public static EpisodeProgress ProgressOf(string episodeId) =>
            ProgressProvider != null ? ProgressProvider(episodeId) : default;

        public static void OpenSelector() => SelectorOpener?.Invoke();

        /// <summary>
        /// The unlock rule (ruling R6: linear season, no replay): a playable
        /// episode is unlocked when it is the first playable entry, or when the
        /// nearest playable entry before it is complete. Reserved slots (no
        /// database) are never unlocked. Pure over its inputs for testability.
        /// </summary>
        public static bool IsUnlocked(EpisodeCatalog catalog, string episodeId, Func<string, EpisodeProgress> progress)
        {
            if (catalog == null || progress == null) return false;
            var entry = catalog.FindById(episodeId);
            if (entry == null || !entry.HasContent) return false;

            EpisodeEntry previousPlayable = null;
            foreach (var e in catalog.Episodes)
            {
                if (e == null || !e.HasContent) continue;
                if (e == entry)
                    return previousPlayable == null || progress(previousPlayable.episodeId).Complete;
                previousPlayable = e;
            }
            return false;
        }

        /// <summary>The next playable, unlocked episode after the given one; null when the season ends here (or content isn't ready).</summary>
        public static EpisodeEntry NextPlayable(EpisodeCatalog catalog, string episodeId, Func<string, EpisodeProgress> progress)
        {
            if (catalog == null) return null;
            var next = catalog.Next(episodeId);
            while (next != null && !next.HasContent)
                next = catalog.Next(next.episodeId);
            return next != null && IsUnlocked(catalog, next.episodeId, progress) ? next : null;
        }
    }
}
