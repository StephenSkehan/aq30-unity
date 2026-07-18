using System;
using UnityEngine;

namespace AQ.App.Dev
{
    /// <summary>
    /// Master switch for on-screen debug info (CaseFlow/leads status line +
    /// dev test buttons). Toggled from Settings > Debug (dev builds only);
    /// compiled to always-off in release. Default OFF.
    /// </summary>
    public static class DebugInfoToggle
    {
        private const string Key = "aq.debug.info";

        public static event Action Changed;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public static bool Show
        {
            get => PlayerPrefs.GetInt(Key, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(Key, value ? 1 : 0);
                PlayerPrefs.Save();
                Changed?.Invoke();
            }
        }
#else
        public static bool Show { get => false; set { } }
#endif
    }
}
