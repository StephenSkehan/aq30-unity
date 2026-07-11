using System;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace AQ.App.Services.Ads
{
    /// <summary>
    /// Runs Google UMP consent at boot, before any ad request. On iOS this also
    /// drives the published IDFA explainer + Apple's ATT prompt. AdService waits
    /// for ConsentResolved before initializing the ads SDK.
    /// </summary>
    public static class ConsentService
    {
        /// <summary>True once the consent flow finished and ad requests are allowed.</summary>
        public static bool AdsAllowed { get; private set; }

        /// <summary>Fired once per boot when the consent flow completes (either outcome).</summary>
        public static event Action ConsentResolved;

        public static bool PrivacyOptionsRequired
        {
            get
            {
#if UNITY_EDITOR
                return false;
#else
                try
                {
                    return ConsentInformation.PrivacyOptionsRequirementStatus
                           == PrivacyOptionsRequirementStatus.Required;
                }
                catch { return false; }
#endif
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Gather()
        {
#if UNITY_EDITOR
            // UMP has no editor implementation; treat as resolved so the
            // placeholder ads pipeline works in Play mode.
            Resolve(true);
#else
            var request = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false
            };

#if DEVELOPMENT_BUILD
            // Force the EEA form on dev builds so it can be tested from Australia.
            // Physical devices must also be registered: UMP prints this device's
            // hashed ID in the Xcode console on first run — paste it below.
            request.ConsentDebugSettings = new ConsentDebugSettings
            {
                DebugGeography = DebugGeography.EEA,
                TestDeviceHashedIds = new System.Collections.Generic.List<string>
                {
                    // "PASTE-DEVICE-HASH-FROM-CONSOLE-HERE"
                }
            };
#endif

            ConsentInformation.Update(request, updateError =>
            {
                if (updateError != null)
                {
                    // Offline/first-run failure: fall back to whatever the SDK
                    // cached; never block the game on consent infrastructure.
                    Debug.LogWarning($"[Consent] Update failed: {updateError.Message}");
                    Resolve(SafeCanRequestAds());
                    return;
                }

                ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
                {
                    if (formError != null)
                        Debug.LogWarning($"[Consent] Form failed: {formError.Message}");
                    Resolve(SafeCanRequestAds());
                });
            });
#endif
        }

        /// <summary>Settings-panel entry: EEA users can revisit their choices.</summary>
        public static void ShowPrivacyOptions(Action<string> onDone)
        {
#if UNITY_EDITOR
            onDone?.Invoke("Not available in editor.");
#else
            ConsentForm.ShowPrivacyOptionsForm(formError =>
                onDone?.Invoke(formError?.Message));
#endif
        }

        private static bool SafeCanRequestAds()
        {
            try { return ConsentInformation.CanRequestAds(); }
            catch { return false; }
        }

        private static void Resolve(bool adsAllowed)
        {
            AdsAllowed = adsAllowed;
            Debug.Log($"[Consent] Resolved — ads allowed: {adsAllowed}");
            ConsentResolved?.Invoke();
        }
    }
}
