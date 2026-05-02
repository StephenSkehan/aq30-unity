using UnityEngine;

namespace AQ.App
{
    public static class NarrativeFlags
    {
        private const string PREFIX = "nar_flag_";

        public static void Set(string flag)
        {
            if (string.IsNullOrEmpty(flag)) return;
            PlayerPrefs.SetInt(PREFIX + flag, 1);
            PlayerPrefs.Save();
        }

        public static bool Has(string flag)
        {
            if (string.IsNullOrEmpty(flag)) return false;
            return PlayerPrefs.GetInt(PREFIX + flag, 0) == 1;
        }

        public static void Clear(string flag)
        {
            if (string.IsNullOrEmpty(flag)) return;
            PlayerPrefs.DeleteKey(PREFIX + flag);
            PlayerPrefs.Save();
        }
    }
}
