#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

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
        }
    }
}
#endif
