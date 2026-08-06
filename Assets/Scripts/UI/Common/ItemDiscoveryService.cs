using System.Collections.Generic;
using AQ.App.UI.Board;
using UnityEngine;

namespace AQ.App.UI.Common
{
    /// <summary>
    /// Tracks which (family, tier) items the player has ever seen on the board —
    /// drives the silhouette gating in ItemFamilyPopup. Fed by the board's static
    /// OnItemCreated (fires on spawn, merge, overflow placement, and the post-
    /// restore re-fire pass, so save restores mark their contents too).
    /// Persists via PlayerPrefs; QA Reset's DeleteAll clears it with the rest.
    /// </summary>
    public static class ItemDiscoveryService
    {
        private const string PrefsKey = "aq.discovered.items";

        private static readonly HashSet<string> _seen = new();
        private static bool _loaded;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _seen.Clear();
            _loaded = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            EnsureLoaded();
            MergeBoardController.OnItemCreated -= Mark; // domain-reload-off safety
            MergeBoardController.OnItemCreated += Mark;
        }

        public static bool IsDiscovered(string family, int tier)
        {
            EnsureLoaded();
            return _seen.Contains(Key(family, tier));
        }

        public static void Mark(string family, int tier)
        {
            if (string.IsNullOrEmpty(family)) return;
            EnsureLoaded();
            if (!_seen.Add(Key(family, tier))) return;
            PlayerPrefs.SetString(PrefsKey, string.Join(",", _seen));
            PlayerPrefs.Save();
        }

        private static string Key(string family, int tier) => $"{family}:{tier}";

        private static void EnsureLoaded()
        {
            if (_loaded) return;
            _loaded = true;
            var raw = PlayerPrefs.GetString(PrefsKey, string.Empty);
            if (string.IsNullOrEmpty(raw)) return;
            foreach (var k in raw.Split(','))
                if (!string.IsNullOrEmpty(k)) _seen.Add(k);
        }
    }
}
