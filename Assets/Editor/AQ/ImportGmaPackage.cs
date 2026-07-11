using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools
{
    public static class ImportGmaPackage
    {
        [MenuItem("AQ/Setup/Import Google Mobile Ads Package")]
        public static void Import()
        {
            const string path =
                @"C:\Users\User\AppData\Local\Temp\claude\C--users-user-dev-aq30-unity\79da2a79-17c1-44f5-a9bf-0af6ae54d6ad\scratchpad\GoogleMobileAds-v11.2.0.unitypackage";
            AssetDatabase.ImportPackage(path, false);
            Debug.Log("[GMA] Import started: " + path);
        }
    }
}
