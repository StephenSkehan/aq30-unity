using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AQ.EditorTools
{
    public static class BuildIOS
    {
        // Xcode project generation only; archive + upload happen on the Mac.
        [MenuItem("AQ/Build/Build iOS (Xcode Project)")]
        public static void Build()
        {
            var opts = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Main Merge.unity" },
                locationPathName = "Builds/iOS-b3",
                target = BuildTarget.iOS,
                options = BuildOptions.None // release: no Development, no script debugging
            };

            Debug.Log("[BuildIOS] START -> Builds/iOS-b3");
            BuildReport report = BuildPipeline.BuildPlayer(opts);
            BuildSummary s = report.summary;
            if (s.result == BuildResult.Succeeded)
                Debug.Log($"[BuildIOS] SUCCEEDED in {s.totalTime.TotalMinutes:F1} min, {s.totalSize / (1024 * 1024)} MB, errors={s.totalErrors}, warnings={s.totalWarnings}");
            else
                Debug.LogError($"[BuildIOS] {s.result} after {s.totalTime.TotalMinutes:F1} min, errors={s.totalErrors}");
        }
    }
}
