// WK3-2 Variant A verification without hard TMPro dependency.
// - Tries to call AQ.EditorTools.Content.ContentVariant.ApplyVariantA_Menu if it exists.
// - Reads Title/Body/Quest lines and reward-ish numbers via reflection.
// - Dumps a snapshot file for audit evidence.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.Variants
{
    public static class WK3_2_VariantA_Verifier
    {
        [MenuItem("AQ/WK3-2/Verify Variant A (snapshot)")]
        public static void VerifyVariantA()
        {
            // 0) Try to (re)apply Variant A via known static method if it exists.
            TryInvoke("AQ.EditorTools.Content.ContentVariant", "ApplyVariantA_Menu");

            // 1) Find overlay (including inactive)
            var root = FindRoot("ResolutionRoot");
            var panel = FindDeep(root ? root.transform : null, "ResolutionPanel");
            var titleT = FindDeep(panel, "TitleText");
            var bodyT  = FindDeep(panel, "BodyText");
            var btnT   = FindDeep(panel, "ResolveButton");

            if (!root || !panel || !titleT || !bodyT || !btnT)
            {
                Debug.LogError("WK3-2: Overlay not found (ResolutionRoot/ResolutionPanel/{TitleText,BodyText,ResolveButton}).");
                return;
            }

            // 2) Read Title, Body, Quest_0..2, Button label (via TMP reflection or UGUI fallback)
            string title = ReadText(titleT);
            string body  = ReadText(bodyT);
            string q0 = ReadText(FindDeep(panel, "Quest_0"));
            string q1 = ReadText(FindDeep(panel, "Quest_1"));
            string q2 = ReadText(FindDeep(panel, "Quest_2"));
            string btn = ReadText(btnT) ?? ReadText(FindDeep(btnT, "Text"));

            // 3) Simple “A looks applied” sanity checks
            bool nonPlaceholder(string s) => !string.IsNullOrWhiteSpace(s) && s.Trim() != "TitleText" && s.Trim() != "BodyText";
            bool questsOk = new[] { q0, q1, q2 }.Any(s => !string.IsNullOrWhiteSpace(s)); // at least 1 quest
            bool bulletsOk = new[] { q0, q1, q2 }.Where(s => !string.IsNullOrWhiteSpace(s)).All(s => s.TrimStart().StartsWith("•"));
            var rewardsLine = new[] { title, body, q0, q1, q2, btn }.FirstOrDefault(s => (s ?? "").IndexOf("Energy", StringComparison.OrdinalIgnoreCase) >= 0 || (s ?? "").IndexOf("Soft", StringComparison.OrdinalIgnoreCase) >= 0);
            var numbers = rewardsLine != null ? Regex.Matches(rewardsLine, @"[+-]?\d+").Cast<Match>().Select(m => int.Parse(m.Value)).ToArray() : Array.Empty<int>();
            bool rewardsOk = numbers.Any(n => n != 0);

            // 4) Snapshot evidence
            var sb = new StringBuilder();
            sb.AppendLine("=== WK3-2 Variant A Snapshot ===");
            sb.AppendLine($"When: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Scene: {(root ? root.scene.path : "(unknown)")}");
            sb.AppendLine($"Title: {title}");
            sb.AppendLine($"Body : {body}");
            sb.AppendLine($"Quest_0: {q0}");
            sb.AppendLine($"Quest_1: {q1}");
            sb.AppendLine($"Quest_2: {q2}");
            sb.AppendLine($"Button: {btn}");
            sb.AppendLine($"RewardsLine: {rewardsLine}");
            sb.AppendLine($"Numbers: {string.Join(", ", numbers)}");
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "_audit", "wk3_variants", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, "variantA_snapshot.txt");
            File.WriteAllText(path, sb.ToString());

            // 5) Verdict
            bool verdict =
                nonPlaceholder(title) &&
                nonPlaceholder(body)  &&
                questsOk &&
                bulletsOk &&
                rewardsOk;

            Debug.Log($"WK3-2 VERIFY A → " +
                      $"TitleOk={nonPlaceholder(title)}, BodyOk={nonPlaceholder(body)}, " +
                      $"QuestsOk={questsOk}, BulletsOk={bulletsOk}, RewardsOk={rewardsOk} " +
                      $"\nSnapshot: {path}");
        }

        // --- helpers ---
        static GameObject FindRoot(string name) =>
            Resources.FindObjectsOfTypeAll<GameObject>()
                     .FirstOrDefault(g => g && g.name == name && g.scene.IsValid());

        static Transform FindDeep(Transform parent, string childName)
        {
            if (!parent) return null;
            foreach (var t in parent.GetComponentsInChildren<Transform>(true))
                if (t && t.name == childName) return t;
            return null;
        }

        static string ReadText(Transform t)
        {
            if (!t) return null;

            // First try TMP (no hard ref): look for TextMeshProUGUI and read 'text' via reflection
            var tmpType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            if (tmpType != null)
            {
                var tmp = t.GetComponent(tmpType);
                if (tmp != null)
                {
                    var p = tmpType.GetProperty("text");
                    if (p != null) return p.GetValue(tmp) as string;
                }
                // also check children labels (e.g., button)
                var tmpChild = t.GetComponentsInChildren<Component>(true)?.FirstOrDefault(c => c && c.GetType().Name == "TextMeshProUGUI");
                if (tmpChild != null)
                {
                    var p = tmpChild.GetType().GetProperty("text");
                    if (p != null) return p.GetValue(tmpChild) as string;
                }
            }

            // Fallback: legacy UGUI Text
            var ugui = t.GetComponent<Text>();
            if (ugui) return ugui.text;
            var uguiChild = t.GetComponentInChildren<Text>(true);
            if (uguiChild) return uguiChild.text;

            return null;
        }

        static void TryInvoke(string typeName, string methodName)
        {
            var t = Type.GetType(typeName);
            var m = t?.GetMethod(methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (m != null)
            {
                m.Invoke(null, null);
                Debug.Log($"WK3-2: Invoked {typeName}.{methodName}()");
            }
            else
            {
                // It’s okay if this doesn’t exist; we still snapshot what’s present.
                Debug.Log($"WK3-2: {typeName}.{methodName} not found (skipping apply).");
            }
        }
    }
}
