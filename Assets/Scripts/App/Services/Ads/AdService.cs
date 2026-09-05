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

        private const string DayKey     = "aq.ads.energy.day";
        private const string CountKey   = "aq.ads.energy.count";
        // Energy earned from a completed ad but not yet granted to the wallet.
        // Written the moment the SDK reports the reward earned, cleared once the
        // grant is saved — so a kill/crash between "watched the ad" and "closed
        // the ad" can no longer eat the reward (granted on next boot instead).
        private const string PendingKey = "aq.ads.energy.pending";

        public static AdService Instance { get; private set; }
        public static event Action AvailabilityChanged;

        private RewardedAd _rewarded;
        private bool _initialized;
        private bool _loading;
        // Set by the SDK's earn callback (thread-unknown); consumed on the main
        // thread in Update(), which persists the PendingKey marker.
        private volatile bool _earnedUnpersisted;

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

            // The ads SDK must not start before the UMP/ATT consent flow resolves.
            if (ConsentService.AdsAllowed)
                InitializeAds();
            else
                ConsentService.ConsentResolved += OnConsentResolved;

            // Recover rewards earned in a session that died before the grant
            // landed. Must wait for wallet restore — restore is set-to-saved and
            // would wipe an earlier grant.
            AQ.App.UI.Board.BoardSaveSystem.WalletRestoreCompleted += GrantPendingReward;
        }

        private void OnDestroy()
        {
            ConsentService.ConsentResolved -= OnConsentResolved;
            AQ.App.UI.Board.BoardSaveSystem.WalletRestoreCompleted -= GrantPendingReward;
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (!_earnedUnpersisted) return;
            _earnedUnpersisted = false;
            RolloverIfNewDay();
            PlayerPrefs.SetInt(PendingKey, PlayerPrefs.GetInt(PendingKey, 0) + EnergyPerAd);
            PlayerPrefs.SetInt(CountKey, PlayerPrefs.GetInt(CountKey, 0) + 1);
            PlayerPrefs.Save();
        }

        private void GrantPendingReward()
        {
            int pending = PlayerPrefs.GetInt(PendingKey, 0);
            if (pending <= 0) return;

            // Clear the marker ONLY after the grant landed AND persisted — a
            // null wallet (or failed save) with an unconditional clear silently
            // ate the earned reward, defeating the recovery. The marker survives
            // for the next restore instead.
            var wallet = WalletLocator.Instance;
            if (wallet == null) return;

            wallet.Grant("ad.energy.recovered", EcoReward.Energy(pending));
            if (!AQ.App.UI.Board.BoardSaveSystem.SaveNow()) return;

            PlayerPrefs.SetInt(PendingKey, 0);
            PlayerPrefs.Save();
            AnalyticsLocator.Instance?.LogEvent("ad_rewarded",
                new System.Collections.Generic.Dictionary<string, object>
                { ["placement"] = "energy", ["recovered"] = true });
        }

        private void OnConsentResolved()
        {
            ConsentService.ConsentResolved -= OnConsentResolved;
            if (ConsentService.AdsAllowed) InitializeAds();
            else AvailabilityChanged?.Invoke(); // button stays greyed
        }

        private void InitializeAds()
        {
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
                    if (_earnedUnpersisted)
                    {
                        // Update() never ran between earn and close (Unity is
                        // paused behind the fullscreen ad on iOS, so this is the
                        // COMMON path, not the fallback — a kill while the ad is
                        // still up therefore loses the reward; granting blind at
                        // boot would let players farm energy by force-quitting
                        // ads, so that narrow loss is accepted). Take over the
                        // bookkeeping, marker included, so the failure paths
                        // below still have crash insurance.
                        _earnedUnpersisted = false;
                        RolloverIfNewDay();
                        PlayerPrefs.SetInt(CountKey, PlayerPrefs.GetInt(CountKey, 0) + 1);
                        PlayerPrefs.SetInt(PendingKey, PlayerPrefs.GetInt(PendingKey, 0) + EnergyPerAd);
                        PlayerPrefs.Save();
                    }

                    var wallet = WalletLocator.Instance;
                    if (wallet != null)
                    {
                        wallet.Grant("ad.energy", EcoReward.Energy(EnergyPerAd));
                        // Clear the marker ONLY once the grant is on disk: on a
                        // failed save (or a crash before it) the marker re-grants
                        // at next boot instead of the reward vanishing.
                        if (AQ.App.UI.Board.BoardSaveSystem.SaveNow())
                        {
                            PlayerPrefs.SetInt(PendingKey, 0);
                            PlayerPrefs.Save();
                        }
                        AnalyticsLocator.Instance?.LogEvent("ad_rewarded",
                            new System.Collections.Generic.Dictionary<string, object> { ["placement"] = "energy" });
                    }
                    // wallet null: marker survives; GrantPendingReward recovers it.
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

            // The earn callback's thread is SDK-defined — only touch the volatile
            // flag here; Update() (main thread) persists the pending marker.
            _rewarded.Show(_ => { rewarded = true; _earnedUnpersisted = true; });
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
