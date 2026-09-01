// Assembly: editor (Assets/Editor)
// File: Assets/Editor/FourKeysSliceMenu.cs
// Purpose: Toggle the Four Keys chapter 1 slice (see FourKeysSliceBootstrap).

using AQ.App.Leads.Packages;
using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools
{
    public static class FourKeysSliceMenu
    {
        private const string MenuPath = "AQ/Four Keys Slice/Enabled";

        [MenuItem(MenuPath)]
        private static void Toggle()
        {
            bool on = PlayerPrefs.GetInt(FourKeysSliceBootstrap.PrefKey, 0) == 1;
            PlayerPrefs.SetInt(FourKeysSliceBootstrap.PrefKey, on ? 0 : 1);
            PlayerPrefs.Save();
            Debug.Log("[FourKeysSlice] " + (on ? "DISABLED (The Listener plays)" : "ENABLED (chapter 1 slice plays on next Play)"));
        }

        [MenuItem(MenuPath, true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, PlayerPrefs.GetInt(FourKeysSliceBootstrap.PrefKey, 0) == 1);
            return true;
        }
    }
}
