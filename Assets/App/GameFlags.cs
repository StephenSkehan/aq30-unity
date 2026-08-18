using UnityEngine;

namespace AQ.App
{
    /// <summary>
    /// THE story-flag store (unified 2026-08-18). Historically there were two —
    /// NarrativeFlags (nar_flag_*) and DialogueFlags (dlg_flag_*) — with identical
    /// APIs and identical flag NAMES landing in different PlayerPrefs keys, so a
    /// read through the wrong store compiled fine and silently never matched
    /// (this shipped at least two defects: the silenced hint system, the armed
    /// DropRoller gates). Both old classes now forward here, which makes the two
    /// namespaces one. New code should call GameFlags directly.
    ///
    /// Migration: PlayerPrefs cannot enumerate keys, so legacy flags migrate
    /// lazily — the first Has() that finds a value only under an old prefix
    /// copies it to the unified key. Set() writes the unified key only; Clear()
    /// deletes all three so a cleared flag cannot resurrect from a legacy key.
    /// </summary>
    public static class GameFlags
    {
        private const string Prefix    = "flag_";
        private const string LegacyNar = "nar_flag_";
        private const string LegacyDlg = "dlg_flag_";

        public static void Set(string flag)
        {
            if (string.IsNullOrEmpty(flag)) return;
            PlayerPrefs.SetInt(Prefix + flag, 1);
            PlayerPrefs.Save();
            Debug.Log($"[GameFlags] Set: {flag}");
        }

        public static bool Has(string flag)
        {
            if (string.IsNullOrEmpty(flag)) return false;
            if (PlayerPrefs.GetInt(Prefix + flag, 0) == 1) return true;

            // Lazy legacy migration (see class doc).
            if (PlayerPrefs.GetInt(LegacyNar + flag, 0) == 1 ||
                PlayerPrefs.GetInt(LegacyDlg + flag, 0) == 1)
            {
                PlayerPrefs.SetInt(Prefix + flag, 1);
                PlayerPrefs.Save();
                return true;
            }
            return false;
        }

        public static void Clear(string flag)
        {
            if (string.IsNullOrEmpty(flag)) return;
            PlayerPrefs.DeleteKey(Prefix + flag);
            PlayerPrefs.DeleteKey(LegacyNar + flag);
            PlayerPrefs.DeleteKey(LegacyDlg + flag);
            PlayerPrefs.Save();
            Debug.Log($"[GameFlags] Cleared: {flag}");
        }
    }
}
