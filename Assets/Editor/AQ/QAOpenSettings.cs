#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools
{
    /// <summary>Opens the settings modal in Play Mode for headless UI QA.</summary>
    public static class QAOpenSettings
    {
        [MenuItem("AQ/Dev/QA Open Settings (Play Mode)")]
        public static void Open()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[QAOpenSettings] Enter Play Mode first.");
                return;
            }
            AQ.App.UI.Settings.GameControlPanelMB.Show();
            Debug.Log("[QAOpenSettings] Settings panel shown.");
        }
    }
}
#endif
