using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools
{
    public static class ConfigureGmaSettings
    {
        // AdMob app ID for "Ally Quinn: True Crime Merge" (iOS). Ad unit IDs live
        // in AdService; this ID lands in Info.plist as GADApplicationIdentifier.
        private const string IosAppId = "ca-app-pub-3693698575346187~7326234905";

        [MenuItem("AQ/Setup/Configure Google Mobile Ads App ID")]
        public static void Configure()
        {
            // Reflection because GoogleMobileAdsSettings' accessibility varies by
            // plugin version.
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                .FirstOrDefault(t => t.FullName == "GoogleMobileAds.Editor.GoogleMobileAdsSettings");

            if (type == null)
            {
                Debug.LogError("[GMA] GoogleMobileAdsSettings type not found — is the plugin imported?");
                return;
            }

            var load = type.GetMethod("LoadInstance",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var settings = load?.Invoke(null, null) as ScriptableObject;
            if (settings == null)
            {
                Debug.LogError("[GMA] Could not load GoogleMobileAdsSettings instance.");
                return;
            }

            var prop = type.GetProperty("GoogleMobileAdsIOSAppId",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop == null || !prop.CanWrite)
            {
                Debug.LogError("[GMA] GoogleMobileAdsIOSAppId property not found/writable.");
                return;
            }

            prop.SetValue(settings, IosAppId);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Debug.Log($"[GMA] iOS App ID set: {IosAppId}");
        }
    }
}
