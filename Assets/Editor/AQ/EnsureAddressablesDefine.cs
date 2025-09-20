// Assets/Editor/AQ/EnsureAddressablesDefine.cs
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace AQ.EditorTools
{
    /// <summary>
    /// Keeps the ADDRESSABLES scripting define in sync with the presence of the Addressables package.
    /// Updated for Unity 2022+/6.x: uses PlayerSettings.Get/SetScriptingDefineSymbols(NamedBuildTarget,...).
    /// </summary>
    [InitializeOnLoad]
    public sealed class EnsureAddressablesDefine : IPreprocessBuildWithReport
    {
        const string Define = "ADDRESSABLES";
        const string PackageName = "com.unity.addressables";

        // Update immediately on domain load
        static EnsureAddressablesDefine()
        {
            TrySyncDefines(verbose:false);
        }

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            TrySyncDefines(verbose:true);
        }

        static void TrySyncDefines(bool verbose)
        {
#if UNITY_2021_2_OR_NEWER
            bool hasAddressables = HasPackage(PackageName);
            var targets = GetCommonNamedTargets();

            foreach (var nbt in targets)
            {
                var defines = PlayerSettings.GetScriptingDefineSymbols(nbt);
                var list = new HashSet<string>(defines.Split(';', StringSplitOptions.RemoveEmptyEntries));

                bool changed = false;
                if (hasAddressables && !list.Contains(Define))
                {
                    list.Add(Define);
                    changed = true;
                }
                else if (!hasAddressables && list.Contains(Define))
                {
                    list.Remove(Define);
                    changed = true;
                }

                if (changed)
                {
                    var joined = string.Join(";", list);
                    PlayerSettings.SetScriptingDefineSymbols(nbt, joined);
                    if (verbose)
                        UnityEngine.Debug.Log($"[EnsureAddressablesDefine] {(hasAddressables ? "Added" : "Removed")} {Define} for {nbt} → {joined}");
                }
            }
#endif
        }

        static bool HasPackage(string id)
        {
#if UNITY_2021_2_OR_NEWER
            try
            {
                // This is inexpensive; finds by name
                var info = UnityEditor.PackageManager.PackageInfo.FindForPackageName(id);
                return info != null;
            }
            catch { return false; }
#else
            return false;
#endif
        }

        static IEnumerable<NamedBuildTarget> GetCommonNamedTargets()
        {
            // Cover the typical platforms used in this project.
            yield return NamedBuildTarget.Android;
            yield return NamedBuildTarget.iOS;
            yield return NamedBuildTarget.Standalone;
            yield return NamedBuildTarget.WebGL;
        }
    }
}
#endif
