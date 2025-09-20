using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.Variants
{
    public static class WK3_2_RuntimeProbe
    {
        [MenuItem("AQ/WK3-2/Probe Variant A (runtime, assumes overlay is visible)")]
        public static void Probe()
        {
            string ReadText(Transform t)
            {
                if (!t) return null;
                var tmp = t.GetComponentsInChildren<Component>(true).FirstOrDefault(c => c && c.GetType().Name=="TextMeshProUGUI");
                if (tmp != null) return (string)tmp.GetType().GetProperty("text")?.GetValue(tmp);
                var ugui = t.GetComponentInChildren<Text>(true);
                return ugui ? ugui.text : null;
            }
            Transform FindDeep(Transform p, string n)
            {
                if (!p) return null;
                foreach (var t in p.GetComponentsInChildren<Transform>(true)) if (t.name==n) return t;
                return null;
            }

            var root = GameObject.Find("ResolutionRoot");
            if (!root) { Debug.LogWarning("Overlay not visible. Enter Play Mode, trigger the scrub diamond to show it, then run this."); return; }

            var panel = FindDeep(root.transform, "ResolutionPanel");
            var title = ReadText(FindDeep(panel,"TitleText"));
            var body  = ReadText(FindDeep(panel,"BodyText"));
            var q0    = ReadText(FindDeep(panel,"Quest_0"));
            var q1    = ReadText(FindDeep(panel,"Quest_1"));
            var q2    = ReadText(FindDeep(panel,"Quest_2"));
            var rbtn  = FindDeep(panel,"ResolveButton");
            var btxt  = ReadText(rbtn) ?? ReadText(FindDeep(rbtn,"Text"));

            var dir = Path.Combine(Directory.GetCurrentDirectory(), "_audit", "wk3_variants_runtime");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, "variantA_runtime_snapshot_manual.txt");
            File.WriteAllLines(path, new[]{
                "=== WK3-2 Variant A Runtime Snapshot (manual) ===",
                $"Title: {title}",
                $"Body : {body}",
                $"Quest_0: {q0}",
                $"Quest_1: {q1}",
                $"Quest_2: {q2}",
                $"Button : {btxt}",
            });

            Debug.Log($"WK3-2 (runtime) manual snapshot: {path}");
        }
    }
}
