#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

namespace AQ.Editor.Leads
{
    public static class DebugDumpFirstCard
    {
        private const string ContentPath = "LeadsBar/Viewport/Content_Leads";

        [MenuItem("AQ/Leads/Debug • Dump first card tree")]
        public static void Run()
        {
            var content = GameObject.Find(ContentPath)?.transform;
            if (!content) { Debug.LogError("❌ Content_Leads not found."); return; }
            if (content.childCount == 0) { Debug.LogWarning("⚠️ No cards under Content_Leads."); return; }

            var root = content.GetChild(0);
            var sb = new StringBuilder();
            Dump(root, 0, sb);
            Debug.Log($"[CardDump]\n{sb}");
        }

        private static void Dump(Transform t, int d, StringBuilder sb)
        {
            string indent = new string(' ', d * 2);
            string extra = "";
            if (t.GetComponent<TextMeshProUGUI>()) extra = " TMP";
            else if (t.GetComponent<Image>()) extra = " Img";
            else if (t.GetComponent<Button>()) extra = " Btn";
            sb.AppendLine($"{indent}- {t.name}{extra}");
            for (int i = 0; i < t.childCount; i++) Dump(t.GetChild(i), d + 1, sb);
        }
    }
}
#endif
