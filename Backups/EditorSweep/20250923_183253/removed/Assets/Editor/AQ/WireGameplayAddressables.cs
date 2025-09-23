using UnityEngine;
using System.Linq;
// using UnityEditor; // BUT no using UnityEditor.AddressableAssets
using System;
using UnityEditor;

static class WireGameplayAddressables
{
    [MenuItem("AQ/Content/Wire Addressables (safe)")]
    static void Wire()
    {
        var asm = typeof(Editor).Assembly; // get editor asm domain
        // Better: AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Unity.Addressables.Editor")
        var addrAsm = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetTypes().Any(t => t.FullName == "UnityEditor.AddressableAssets.Settings.AddressableAssetSettings"));
        if (addrAsm == null) { Debug.LogWarning("Addressables editor not found."); return; }

        var settingsType = addrAsm.GetType("UnityEditor.AddressableAssets.Settings.AddressableAssetSettings");
        var defaultObjType = addrAsm.GetType("UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject");
        var settingsProp = defaultObjType.GetProperty("Settings");
        var settings = settingsProp.GetValue(null);
        if (settings == null) { Debug.LogWarning("No AddressableAssetSettings."); return; }

        // From here, use reflection to call the few methods you need
        //   no literal "UnityEditor.AddressableAssets" appears in source.
    }
}



