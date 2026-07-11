#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

static class QAResetMenuItem
{
    [MenuItem("AQ/QA Reset + Play", false, 1)]
    static void QAResetAndPlay()
    {
        if (!EditorUtility.DisplayDialog(
            "QA Full Reset",
            "Clears ALL PlayerPrefs (including FTUE) and board save data, then enters Play Mode.",
            "Reset + Play", "Cancel"))
            return;

        // PlayerPrefs — everything, including aq.ftue.entitlements.v1 and all flags
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Board save files — replicate what BoardSaveSystem.ClearSave() deletes
        // without calling runtime services outside Play Mode
        var root = Application.persistentDataPath;
        foreach (var path in new[]
        {
            Path.Combine(root, "board_state.json"),
            Path.Combine(root, "board_state.prev.json"),
            Path.Combine(root, "board_state.json.tmp"),
        })
        {
            if (File.Exists(path)) File.Delete(path);
        }

        Debug.Log("[QA Reset] PlayerPrefs cleared. Board save cleared. Entering Play Mode.");
        EditorApplication.isPlaying = true;
    }
}
#endif
