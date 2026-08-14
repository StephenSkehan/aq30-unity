using System.Collections.Generic;
using UnityEngine;
using AQ.SharedKernel.Economy;

namespace AQ.App.UI.EvidenceBoard
{
    /// <summary>
    /// Runtime state for location history (DossierService pattern): which paid
    /// details are unlocked per location. Detail 0 is free and implicit at
    /// discovery. Prefs aq.locations.state, spend reason "location".
    /// </summary>
    public static class LocationService
    {
        private const string PrefsKey = "aq.locations.state";

        [System.Serializable] private class DTO { public List<string> keys = new(); public List<int> paid = new(); }

        private static Dictionary<string, int> _paid;

        /// <summary>Total visible details: the free one plus purchases.</summary>
        public static int UnlockedCount(string key)
        {
            EnsureLoaded();
            return 1 + (_paid.TryGetValue(key, out var n) ? n : 0);
        }

        public static bool TryUnlockNext(string key)
        {
            EnsureLoaded();
            var def = LocationCatalog.Get(key);
            if (def == null) return false;

            int next = UnlockedCount(key);
            if (next >= def.details.Length) return false;

            int price  = def.details[next].price;
            var wallet = Economy.WalletLocator.Instance;
            if (wallet == null || !wallet.TrySpend(Currency.Soft, price, "location")) return false;

            _paid[key] = (_paid.TryGetValue(key, out var n) ? n : 0) + 1;
            Save();

            Analytics.AnalyticsLocator.Instance?.LogEvent("location_detail_unlocked",
                new Dictionary<string, object>
                {
                    ["location"] = key,
                    ["detail_index"] = next,
                    ["price"] = price,
                    ["balance_after"] = wallet.Get(Currency.Soft),
                });
            return true;
        }

        public static void ResetAll()
        {
            _paid = new Dictionary<string, int>();
            PlayerPrefs.DeleteKey(PrefsKey);
            PlayerPrefs.Save();
        }

        private static void EnsureLoaded()
        {
            if (_paid != null) return;
            _paid = new Dictionary<string, int>();
            try
            {
                var json = PlayerPrefs.GetString(PrefsKey, string.Empty);
                if (string.IsNullOrEmpty(json)) return;
                var dto = JsonUtility.FromJson<DTO>(json);
                if (dto?.keys == null) return;
                for (int i = 0; i < dto.keys.Count && i < dto.paid.Count; i++)
                    _paid[dto.keys[i]] = dto.paid[i];
            }
            catch { /* fresh state */ }
        }

        private static void Save()
        {
            var dto = new DTO();
            foreach (var kv in _paid) { dto.keys.Add(kv.Key); dto.paid.Add(kv.Value); }
            PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(dto));
            PlayerPrefs.Save();
        }
    }
}
