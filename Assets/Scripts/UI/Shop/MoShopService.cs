using System;
using System.Collections.Generic;
using AQ.App;
using AQ.App.Locker;
using AQ.App.Overflow;
using AQ.App.UI.Board;
using AQ.SharedKernel.Economy;
using UnityEngine;

namespace AQ.App.UI.Shop
{
    [Serializable]
    public class MoShopOffer
    {
        public string family;      // item family, or generatorTypeId when isGenerator
        public int    tier;        // 0-based
        public int    price;
        public bool   bought;
        public bool   isGenerator;
    }

    [Serializable]
    public class MoShopStateDTO
    {
        public string dateKey;
        public List<MoShopOffer> offers = new();
    }

    /// <summary>
    /// Mo's Back Room — the CaseCash shop (design: SAS/feature-shop-casecash-v1.md,
    /// all five rulings Stephen 2026-08-07). Daily stock, reset at local midnight:
    /// up to 3 DISCOVERED T2–T4 items at 20 CC per T1-equivalent, plus one
    /// duplicate generator of an owned type at 400 CC. Purchases deliver to the
    /// Stash. No CC→energy path: bought generators still cost energy to tap, and
    /// item prices sit ~4.7x above the Schedule B earn yardstick.
    /// </summary>
    public static class MoShopService
    {
        private const string PrefsKey       = "aq.moshop.state";
        private const string UnlockFlag     = "aq.lead.e1_pod1.seen"; // L5 (Stephen-ruled)
        private const int    PricePerT1eq   = 20;  // Stephen-ruled 2026-08-07 (sketch draft had 12)
        private const int    GeneratorPrice = 400;
        private const int    ItemSlots      = 3;

        private static MoShopStateDTO _state;

        public static bool Unlocked =>
            (Config.FeatureFlagsRuntime.Current == null || Config.FeatureFlagsRuntime.Current.MoShop) &&
            DialogueFlags.Has(UnlockFlag);

        public static IReadOnlyList<MoShopOffer> Offers
        {
            get { EnsureStock(); return _state.offers; }
        }

        /// <summary>QA seam: clears state so the next Offers read restocks.</summary>
        public static void ResetStock()
        {
            _state = null;
            PlayerPrefs.DeleteKey(PrefsKey);
            PlayerPrefs.Save();
        }

        public static bool TryBuy(MoShopOffer offer)
        {
            if (offer == null || offer.bought) return false;

            var wallet = Economy.WalletLocator.Instance;
            if (wallet == null || !wallet.TrySpend(Currency.Soft, offer.price, "mo_shop")) return false;

            OverflowBucketService.Push(new OverflowTileData
            {
                kind   = offer.isGenerator ? OverflowKind.Generator : OverflowKind.Item,
                family = offer.family,
                tier   = offer.tier
            });
            UI.FlightFX.FlyToOverflow();

            offer.bought = true;
            Save();

            Analytics.GameAnalytics.LogShopPurchase(
                (offer.isGenerator ? "gen:" : "item:") + offer.family + ":t" + (offer.tier + 1),
                offer.price, wallet.Get(Currency.Soft));
            return true;
        }

        // ---- stock ----

        private static void EnsureStock()
        {
            if (_state == null) Load();
            string today = DateTime.Now.ToString("yyyyMMdd");
            if (_state != null && _state.dateKey == today) return;
            Regenerate(today);
        }

        private static void Regenerate(string dateKey)
        {
            _state = new MoShopStateDTO { dateKey = dateKey };
            var board = UnityEngine.Object.FindFirstObjectByType<MergeBoardController>();
            if (board == null) { Save(); return; }

            // Deterministic per day — same stock across relaunches.
            var rng = new System.Random(dateKey.GetHashCode() ^ 0x5A5A);

            // Items: DISCOVERED tiers only, T2–T4 band. Discovery already implies
            // the family is live for this player, and the shop can never spoil a
            // silhouette in the family view.
            var pool = new List<Items.ItemDefinitionSO>();
            foreach (var d in board.ItemDefinitions)
                if (d != null && d.tier >= 1 && d.tier <= 3 &&
                    Common.ItemDiscoveryService.IsDiscovered(d.family, d.tier))
                    pool.Add(d);

            for (int n = 0; n < ItemSlots && pool.Count > 0; n++)
            {
                int i = rng.Next(pool.Count);
                var d = pool[i];
                pool.RemoveAt(i);
                _state.offers.Add(new MoShopOffer
                {
                    family = d.family,
                    tier   = d.tier,
                    price  = PricePerT1eq << d.tier // 20 CC per T1eq: T2=40, T3=80, T4=160
                });
            }

            // Generator: duplicate of any owned type (board + locker + Stash),
            // any type incl. the Field Kit (Stephen-ruled).
            var owned = new List<string>(board.GeneratorTypeIdsOnBoard());
            for (int i = 0; i < EvidenceLockerService.Count; i++)
            {
                var e = EvidenceLockerService.GetAt(i);
                if (e.kind == OverflowKind.Generator && !owned.Contains(e.family)) owned.Add(e.family);
            }
            foreach (var e in OverflowBucketService.Items)
                if (e.kind == OverflowKind.Generator && !owned.Contains(e.family)) owned.Add(e.family);

            if (owned.Count > 0)
                _state.offers.Add(new MoShopOffer
                {
                    family      = owned[rng.Next(owned.Count)],
                    tier        = 0,
                    price       = GeneratorPrice,
                    isGenerator = true
                });

            Save();
        }

        // ---- persistence (PlayerPrefs; QA Reset's DeleteAll clears it) ----

        private static void Load()
        {
            var raw = PlayerPrefs.GetString(PrefsKey, string.Empty);
            if (string.IsNullOrEmpty(raw)) { _state = new MoShopStateDTO(); return; }
            try { _state = JsonUtility.FromJson<MoShopStateDTO>(raw) ?? new MoShopStateDTO(); }
            catch { _state = new MoShopStateDTO(); }
        }

        private static void Save()
        {
            PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(_state));
            PlayerPrefs.Save();
        }
    }
}
