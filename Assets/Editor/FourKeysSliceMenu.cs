// Assembly: editor (Assets/Editor)
// File: Assets/Editor/FourKeysSliceMenu.cs
// Purpose: AQ > Dev Boot Episode: pick which EpisodeCatalog entry the scene
//          boots into (EpisodeBootOverride). Replaces the Four Keys slice
//          toggle: the slice is now a real catalog entry (fk01), booted through
//          the normal episode path instead of a database swap.

using AQ.App.Episodes;
using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools
{
    public static class DevBootEpisodeMenu
    {
        private const string Root       = "AQ/Dev Boot Episode/";
        private const string FollowSave = Root + "Follow Save (default)";
        private const string Listener   = Root + "The Listener (ep01)";
        private const string FourKeys   = Root + "Four Keys Ch1 (fk01)";
        private const string LegacySliceKey = "aq.dev.fk_slice";

        [MenuItem(FollowSave, false, 1)] private static void SetFollowSave() => Set("");
        [MenuItem(Listener,   false, 2)] private static void SetListener()   => Set("ep01");
        [MenuItem(FourKeys,   false, 3)] private static void SetFourKeys()   => Set("fk01");

        [MenuItem(FollowSave, true)] private static bool ValidateFollowSave() => Check(FollowSave, "");
        [MenuItem(Listener,   true)] private static bool ValidateListener()   => Check(Listener, "ep01");
        [MenuItem(FourKeys,   true)] private static bool ValidateFourKeys()   => Check(FourKeys, "fk01");

        private static void Set(string id)
        {
            EditorPrefs.SetString(EpisodeBootOverride.PrefKey, id);
            EditorPrefs.DeleteKey(LegacySliceKey);
            Debug.Log("[DevBootEpisode] " + (string.IsNullOrEmpty(id)
                ? "cleared: the save's pointer decides the boot episode."
                : "next Play boots '" + id + "' (its own save section; QA Reset clears it like any episode)."));
        }

        private static bool Check(string path, string id)
        {
            Menu.SetChecked(path, EditorPrefs.GetString(EpisodeBootOverride.PrefKey, "") == id);
            return true;
        }
    }
}
