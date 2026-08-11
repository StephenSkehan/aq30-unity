using System;
using System.Collections.Generic;
using AQ.App;
using AQ.App.Overflow;
using AQ.App.UI.Specials;
using AQ.SharedKernel.Economy;
using UnityEngine;

namespace AQ.App.UI.Dossiers
{
    /// <summary>
    /// Ally's Case Files — progressive character bios bought with CaseCash
    /// (design: SAS/feature-dossiers-v1.md, all rulings Stephen 2026-08-11).
    /// Variable fact counts per character; only the NEXT locked fact is ever
    /// surfaced. Rewards are items and Case Kit specials only (no energy, no
    /// ingots). Final facts additionally gate on episode resolution.
    /// Persistence: PlayerPrefs (QA Reset's DeleteAll clears it).
    /// </summary>
    public static class DossierService
    {
        private const string PrefsKey  = "aq.dossiers.state";
        private const string CloseFlag = "aq.lead.e1_close.seen";

        [Serializable] private class DTO { public List<string> keys = new(); public List<int> counts = new(); }

        private static Dictionary<string, int> _unlocked;

        public static event Action Changed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _unlocked = null;

        public static bool Has(string key) =>
            !string.IsNullOrEmpty(key) && DossierCatalog.All.ContainsKey(key);

        public static int UnlockedCount(string key)
        {
            EnsureLoaded();
            return _unlocked.TryGetValue(key, out var n) ? n : 0;
        }

        public static DossierFact NextFact(string key)
        {
            if (!Has(key)) return null;
            var def = DossierCatalog.All[key];
            int n = UnlockedCount(key);
            return n < def.facts.Count ? def.facts[n] : null;
        }

        public static bool IsComplete(string key) =>
            Has(key) && UnlockedCount(key) >= DossierCatalog.All[key].facts.Count;

        /// <summary>The next fact exists but is sealed behind episode resolution.</summary>
        public static bool NextIsSealed(string key)
        {
            var fact = NextFact(key);
            return fact != null && fact.gatedOnEpisodeClose && !DialogueFlags.Has(CloseFlag);
        }

        public static bool TryUnlockNext(string key)
        {
            var fact = NextFact(key);
            if (fact == null || NextIsSealed(key)) return false;

            var wallet = Economy.WalletLocator.Instance;
            if (wallet == null || !wallet.TrySpend(Currency.Soft, fact.price, "dossier")) return false;

            EnsureLoaded();
            int index = UnlockedCount(key);
            _unlocked[key] = index + 1;
            Save();

            GrantReward(fact.reward);
            Analytics.GameAnalytics.LogDossierFact(key, index, fact.price, wallet.Get(Currency.Soft));

            if (IsComplete(key))
            {
                var def = DossierCatalog.All[key];
                if (def.completion != null) GrantReward(def.completion);
                if (!string.IsNullOrEmpty(def.completionCassette))
                {
                    SpecialItemsService.GrantCassette(def.completionCassette);
                    Common.ToastService.Show("dossier_cas", "A cassette slips out of the file.", 3f);
                }
            }

            Changed?.Invoke();
            return true;
        }

        private static void GrantReward(DossierReward reward)
        {
            if (reward == null) return;
            switch (reward.kind)
            {
                case DossierRewardKind.Item:
                    // A reward can outrun board discovery (e.g. a T6 flute) —
                    // it is in Ally's file now, so it counts as seen.
                    Common.ItemDiscoveryService.Mark(reward.family, reward.tier);
                    OverflowBucketService.Push(new OverflowTileData
                    {
                        kind   = OverflowKind.Item,
                        family = reward.family,
                        tier   = reward.tier
                    });
                    FlightFX.FlyToOverflow();
                    break;
                case DossierRewardKind.Special:
                    SpecialItemsService.Grant(reward.special); // Case Kit toast rides the tray bridge
                    break;
            }
        }

        /// <summary>QA seam.</summary>
        public static void ResetAll()
        {
            _unlocked = new Dictionary<string, int>();
            PlayerPrefs.DeleteKey(PrefsKey);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        // ---- persistence ----

        private static void EnsureLoaded()
        {
            if (_unlocked != null) return;
            _unlocked = new Dictionary<string, int>();
            try
            {
                var dto = JsonUtility.FromJson<DTO>(PlayerPrefs.GetString(PrefsKey, string.Empty));
                if (dto?.keys != null)
                    for (int i = 0; i < dto.keys.Count && i < dto.counts.Count; i++)
                        _unlocked[dto.keys[i]] = dto.counts[i];
            }
            catch { /* fresh state */ }
        }

        private static void Save()
        {
            var dto = new DTO();
            foreach (var kv in _unlocked) { dto.keys.Add(kv.Key); dto.counts.Add(kv.Value); }
            PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(dto));
            PlayerPrefs.Save();
        }
    }
}
