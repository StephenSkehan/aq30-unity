// Assembly: AQ.App
// Purpose: Initialize Firebase once at boot; on success swap the analytics
// backend from DebugLogAnalytics to Firebase and enable Crashlytics collection.

using Firebase;
using Firebase.Crashlytics;
using Firebase.Extensions;
using UnityEngine;

namespace AQ.App.Analytics
{
    public static class FirebaseBootstrap
    {
        public static bool Ready { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.Result != DependencyStatus.Available)
                {
                    // DebugLogAnalytics stays installed; game runs without telemetry.
                    Debug.LogWarning($"[Firebase] Dependencies unavailable: {task.Result}");
                    return;
                }

                Crashlytics.ReportUncaughtExceptionsAsFatal = true;
                Firebase.Analytics.FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                AnalyticsLocator.Set(new FirebaseAnalyticsAdapter());
                Ready = true;
                Debug.Log("[Firebase] Initialized — analytics backend swapped to Firebase.");
            });
        }
    }
}
