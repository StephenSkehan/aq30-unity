#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

static class QAResetMenuItem
{
    // No-dialog variant for MCP/automation-driven sessions (the modal in the
    // interactive item blocks the editor when driven remotely).
    [MenuItem("AQ/QA Reset + Play (silent)", false, 2)]
    static void QAResetAndPlaySilent() => DoReset();

    [MenuItem("AQ/QA Reset + Play", false, 1)]
    static void QAResetAndPlay()
    {
        if (!EditorUtility.DisplayDialog(
            "QA Full Reset",
            "Clears ALL PlayerPrefs (including FTUE) and board save data, then enters Play Mode.",
            "Reset + Play", "Cancel"))
            return;
        DoReset();
    }

    static void DoReset()
    {
        // Resetting WHILE playing wipes prefs/saves under the live session
        // without rebooting it → zero-wallet limbo with no leads (bitten
        // 2026-07-17 and again 2026-07-18). Exit play first, then reset and
        // re-enter on the next editor tick.
        if (EditorApplication.isPlaying)
        {
            Debug.Log("[QA Reset] Was in play mode — exiting first, reset re-runs on exit.");
            EditorApplication.isPlaying = false;
            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isPlaying) DoReset();
                else EditorApplication.delayCall += DoReset; // one more tick if still tearing down
            };
            return;
        }

        // PlayerPrefs — everything, including aq.ftue.entitlements.v1 and all flags
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Board save files — replicate what BoardSaveSystem.ClearSave() deletes
        // without calling runtime services outside Play Mode.
        // MUST stay in sync with ClearSave(): missing overflow_state.json here
        // let the pocket accumulate one gen_junk per QA cycle (20 stacked
        // generators found in a real save on 2026-07-12).
        var root = Application.persistentDataPath;
        foreach (var path in new[]
        {
            Path.Combine(root, "board_state.json"),
            Path.Combine(root, "board_state.prev.json"),
            Path.Combine(root, "board_state.json.tmp"),
            Path.Combine(root, "overflow_state.json"),
            Path.Combine(root, "generator_registry.json"),
            Path.Combine(root, "locker_state.json"),
        })
        {
            if (File.Exists(path)) File.Delete(path);
        }

        Debug.Log("[QA Reset] PlayerPrefs cleared. Board save cleared. Entering Play Mode.");
        EditorApplication.isPlaying = true;
    }
}
#endif
