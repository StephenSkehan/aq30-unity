#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools
{
    /// <summary>
    /// One-line state probes for remote/MCP QA — replaces full get_gameobject
    /// hierarchy dumps (10-15k tokens each) with targeted single-line logs.
    /// Add a case here whenever a debugging session needs a value repeatedly.
    /// </summary>
    public static class QAStateProbes
    {
        [MenuItem("AQ/Dev/Probe Stage Layout")]
        public static void ProbeStageLayout()
        {
            var panel = GameObject.Find("DialoguePanel");
            if (panel == null) { Debug.Log("[QAProbe] DialoguePanel not found/active"); return; }

            var portrait = panel.transform.Find("Portrait") as RectTransform;
            var body = panel.transform.Find("Body") as RectTransform;
            var speaker = panel.transform.Find("Speaker") as RectTransform;

            if (portrait != null)
                Debug.Log($"[QAProbe] Portrait pos={portrait.anchoredPosition} anchors=({portrait.anchorMin},{portrait.anchorMax}) size={portrait.sizeDelta}");
            if (body != null)
            {
                var t = body.GetComponent<Text>();
                Debug.Log($"[QAProbe] Body anchors=({body.anchorMin},{body.anchorMax}) font={(t ? t.fontSize : -1)} overflow={(t ? t.verticalOverflow.ToString() : "?")} text=\"{(t ? t.text : "")}\"");
            }
            if (speaker != null)
            {
                var t = speaker.GetComponent<Text>();
                Debug.Log($"[QAProbe] Speaker text=\"{(t ? t.text : "")}\"");
            }
        }

        [MenuItem("AQ/Dev/Probe Board State")]
        public static void ProbeBoardState()
        {
            var grid = GameObject.Find("MergeBoard");
            var layout = grid ? grid.GetComponent<GridLayoutGroup>() : null;
            if (layout != null)
                Debug.Log($"[QAProbe] Grid cell={layout.cellSize} spacing={layout.spacing}");

            var scrim = Object.FindAnyObjectByType<AQ.App.UI.BackgroundScrimMB>();
            if (scrim != null)
                Debug.Log($"[QAProbe] Scrim opacity={scrim.opacity:F2}");

            foreach (var name in new[] { "Txt_Value", "Txt_Soft_Currency", "Txt_Premium" })
            {
                var go = GameObject.Find(name);
                var tmp = go ? go.GetComponent<TMPro.TMP_Text>() : null;
                if (tmp != null) Debug.Log($"[QAProbe] {name}=\"{tmp.text}\"");
            }
        }
    }
}
#endif
