#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;

namespace AQ.Editor.Leads
{
    public static class FixTMPOverflow_Leads
    {
        private const string ContentPath = "LeadsBar/Viewport/Content_Leads";

        [MenuItem("AQ/Leads/Fix TMP Overflow (Truncate, clamp)")]
        public static void Run()
        {
            var content = GameObject.Find(ContentPath)?.transform;
            if (!content) { Debug.LogError("❌ Content_Leads not found."); return; }

            Undo.IncrementCurrentGroup();
            int n = 0;
            foreach (var tmp in content.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                Undo.RecordObject(tmp, "TMP overflow clamp");
#if UNITY_2022_2_OR_NEWER
                // Titles: NoWrap; Microcopy: Normal wrap – both will truncate if overflow
                if (tmp.name == "Text_Title") tmp.textWrappingMode = TextWrappingModes.NoWrap;
                else                           tmp.textWrappingMode = TextWrappingModes.Normal;
#else
                if (tmp.name == "Text_Title") tmp.enableWordWrapping = false;
                else                           tmp.enableWordWrapping = true;
#endif
                tmp.overflowMode = TextOverflowModes.Truncate;
                n++;
            }
            Debug.Log($"✅ TMP overflow fixed on {n} label(s) under Content_Leads.");
        }
    }
}
#endif
