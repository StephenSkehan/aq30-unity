using System;
using GoogleMobileAds.Api;
using UnityEngine;
using AQ.App.Analytics;
using AQ.App.Economy;
using AQ.SharedKernel.Economy;
using EcoReward = AQ.SharedKernel.Economy.Reward;

namespace AQ.App.Services.Ads
{
    /// <summary>
    /// Rewarded-video-only ad layer (economy sheet: no interstitials, no banners).
    /// Watch-for-Energy: +20 energy, max 5 views/day, daily reset at local midnight.
    /// Uses Google's published test ad units except in real iOS release builds.
    /// </summary>
    public sealed class AdService : MonoBehaviour
    {
        public const int EnergyPerAd = 20;
        public const int DailyCap    = 5;

#if UNITY_IOS && !UNITY_EDITOR && !DEVELOPMENT_BUILD
        private const string RewardedEnergyUnitId = "ca-app-pub-3693698575346187/4436347689";
#else
        // Google's public iOS rewarded test unit — never serves real ads.
        private const string RewardedEnergyUnitId = "ca-app-pub-3940256099942544/1712485313";
#endif

        private const string DayKey   = "aq.ads.energy.day";
        private const string CountKey = "aq.ads.energy.count";

        public static AdService Instance { get; private set; }
        public static event Action AvailabilityChanged;

        private RewardedAd _rewarded;
        private bool _initialized;
        private bool _loading;

        public bool AdReady => _initialized && _rewarded != null && _rewarded.CanShowAd() && ViewsLeftToday > 0;

        public int ViewsLeftToday
        {
            get
            {
                RolloverIfNewDay();
                return Mathf.Max(0, DailyCap - PlayerPrefs.GetInt(CountKey, 0));
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInstall()
        {
            if (Instance != null) return;
            var go = new GameObject("__AdService");
            DontDestroyOnLoad(go);
            go.AddComponent<AdService>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            MobileAds.Initialize(_ =>
            {
                _initialized = true;
                LoadRewarded();
            });
        }

        public void ShowRewardedEnergy(Action<bool> onDone)
        {
            if (!AdReady)
            {
                onDone?.Invoke(false);
                return;
            }

            AnalyticsLocator.Instance?.LogEvent("ad_show_attempt",
                new System.Collections.Generic.Dictionary<string, object> { ["placement"] = "energy" });

            bool rewarded = false;
            _rewarded.OnAdFullScreenContentClosed += () =>
            {
                if (rewarded)
                {
                    WalletLocator.Instance?.Grant("ad.energy", EcoReward.Energy(EnergyPerAd));
                    RolloverIfNewDay();
                    PlayerPrefs.SetInt(CountKey, PlayerPrefs.GetInt(CountKey, 0) + 1);
                    PlayerPrefs.Save();
                    AnalyticsLocator.Instance?.LogEvent("ad_rewarded",
                        new System.Collections.Generic.Dictionary<string, object> { ["placement"] = "energy" });
                }
                onDone?.Invoke(rewarded);
                LoadRewarded();
                AvailabilityChanged?.Invoke();
            };
            _rewarded.OnAdFullScreenContentFailed += _ =>
            {
                onDone?.Invoke(false);
                LoadRewarded();
                AvailabilityChanged?.Invoke();
            };

            _rewarded.Show(_ => rewarded = true);
        }

        private void LoadRewarded()
        {
            if (_loading) return;
            _loading = true;

            _rewarded?.Destroy();
            _rewarded = null;

            RewardedAd.Load(RewardedEnergyUnitId, new AdRequest(), (ad, error) =>
            {
                _loading = false;
                if (error != null || ad == null)
                {
                    Debug.LogWarning($"[Ads] Rewarded load failed: {error}");
                    AvailabilityChanged?.Invoke();
                    return;
                }
                _rewarded = ad;
                AvailabilityChanged?.Invoke();
            });
        }

        private static void RolloverIfNewDay()
        {
            var now = DateTime.Now;
            int today  = now.Year * 10000 + now.Month * 100 + now.Day;
            int stored = PlayerPrefs.GetInt(DayKey, 0);
            if (stored == today) return;

            PlayerPrefs.SetInt(DayKey, today);
            PlayerPrefs.SetInt(CountKey, 0);
            PlayerPrefs.Save();
        }
    }
}
