// Assembly: AQ.Editor
// Menu utilities to reset/bump the FTUE entitlement flag safely.

using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools.FTUE
{
    public static class FTUEEntitlementTools
    {
        private const string DefaultKey = "aq.ftue.entitlements.v1";

        [MenuItem("AQ/FTUE/Clear Entitlement Flag")]
        public static void ClearFlag()
        {
            PlayerPrefs.DeleteKey(DefaultKey);
            PlayerPrefs.Save();
            Debug.Log($"[FTUE] Cleared PlayerPrefs key '{DefaultKey}'. Next Play will re-grant entitlements.");
        }

        [MenuItem("AQ/FTUE/Bump Entitlement Version")]
        public static void BumpVersion()
        {
            // E.g., switch the script in scene to use v2 later; this helper just tells you the next key.
            var next = "aq.ftue.entitlements.v2";
            Debug.Log($"[FTUE] Suggested next key: '{next}'. Update FTUEEntitlements.playerPrefsKey to re-grant.");
        }
    }
}
