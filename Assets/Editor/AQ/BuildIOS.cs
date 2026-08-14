using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AQ.EditorTools
{
    public static class BuildIOS
    {
        private const string OutPath = "Builds/iOS-b6";

        // Xcode project generation only; archive + upload happen on the Mac.
        [MenuItem("AQ/Build/Build iOS (Xcode Project)")]
        public static void Build()
        {
            // Derive the build number from the output path and set it via the
            // editor API. Disk edits to ProjectSettings.asset are INVISIBLE to
            // an open editor (memory copy wins) — b6 first stamped 5 that way.
            var m = System.Text.RegularExpressions.Regex.Match(OutPath, @"b(\d+)$");
            if (m.Success) PlayerSettings.iOS.buildNumber = m.Groups[1].Value;

            var opts = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Main Merge.unity" },
                locationPathName = OutPath,
                target = BuildTarget.iOS,
                options = BuildOptions.None, // release: no Development, no script debugging
                // Settings > Debug tab in TestFlight builds (Stephen-ruled 2026-08-13:
                // RESET is useful to testers, CRASH TEST checks Crashlytics liveness).
                // REMOVE this line for the App Store submission build.
                extraScriptingDefines = new[] { "AQ_DEBUG_TAB" }
            };

            Debug.Log($"[BuildIOS] START -> {OutPath} (buildNumber {PlayerSettings.iOS.buildNumber})");
            BuildReport report = BuildPipeline.BuildPlayer(opts);
            BuildSummary s = report.summary;
            if (s.result == BuildResult.Succeeded)
                Debug.Log($"[BuildIOS] SUCCEEDED in {s.totalTime.TotalMinutes:F1} min, {s.totalSize / (1024 * 1024)} MB, errors={s.totalErrors}, warnings={s.totalWarnings}");
            else
                Debug.LogError($"[BuildIOS] {s.result} after {s.totalTime.TotalMinutes:F1} min, errors={s.totalErrors}");
        }
    }
}
