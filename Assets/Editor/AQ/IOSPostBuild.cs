#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace AQ.EditorTools
{
    public static class IOSPostBuild
    {
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS) return;

            var plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            // Standard HTTPS only — exempt from export compliance. With this key
            // present, App Store Connect skips the per-build encryption questionnaire.
            plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);

            // Apple's ATT prompt purpose string (the system dialog's second line).
            // The friendly pre-prompt explainer is the published AdMob IDFA message.
            plist.root.SetString("NSUserTrackingUsageDescription",
                "This lets us show ads that are more relevant to you and keep the game free. Your game progress is never linked to your identity.");

            plist.WriteToFile(plistPath);

            InstallPrivacyManifest(pathToBuiltProject);
        }

        // Apple requires PrivacyInfo.xcprivacy at the app-bundle root for App Store /
        // TestFlight processing when SDKs use required-reason APIs (Firebase + Google
        // Mobile Ads both do). The manifest lives in the repo at
        // BuildResources/iOS/PrivacyInfo.xcprivacy (outside Assets, so it stays out of
        // the asset pipeline); this copies it into the Xcode project and adds it to the
        // main app target's build so it ships at the .app root.
        static void InstallPrivacyManifest(string pathToBuiltProject)
        {
            const string fileName = "PrivacyInfo.xcprivacy";
            string src = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "BuildResources", "iOS", fileName));
            if (!File.Exists(src))
            {
                Debug.LogError($"[IOSPostBuild] {fileName} not found at {src}. " +
                    "App Store / TestFlight upload will be REJECTED for a missing privacy manifest. " +
                    "Restore BuildResources/iOS/PrivacyInfo.xcprivacy before building for submission.");
                return;
            }

            string projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            var proj = new PBXProject();
            proj.ReadFromFile(projPath);
            string targetGuid = proj.GetUnityMainTargetGuid();

            File.Copy(src, Path.Combine(pathToBuiltProject, fileName), overwrite: true);

            if (proj.FindFileGuidByProjectPath(fileName) == null)
            {
                string fileGuid = proj.AddFile(fileName, fileName, PBXSourceTree.Source);
                proj.AddFileToBuild(targetGuid, fileGuid);
            }

            proj.WriteToFile(projPath);
            Debug.Log($"[IOSPostBuild] Installed {fileName} into the app target.");
        }
    }
}
#endif
