#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools.Variants
{
    /// <summary>
    /// Lightweight verifier that checks Resolution overlay text fields exist and are non-empty
    /// after applying each variant. Logs a compact summary line per variant.
    /// </summary>
    public static class VariantVerifier
    {
        [MenuItem("AQ/Variants/Verify/Verify Variant A")]
        public static void VerifyA() => VerifyVariant("A");

        [MenuItem("AQ/Variants/Verify/Verify Variant B")]
        public static void VerifyB() => VerifyVariant("B");

        [MenuItem("AQ/Variants/Verify/Verify Variant C")]
        public static void VerifyC() => VerifyVariant("C");

        [MenuItem("AQ/Variants/Verify/Verify All (A,B,C)")]
        public static void VerifyAll()
        {
            VerifyVariant("A");
            VerifyVariant("B");
            VerifyVariant("C");
        }

        private static void VerifyVariant(string which)
        {
            // Apply variant via menus (ensures same path user will click)
            switch (which)
            {
                case "A": VariantMenus.ApplyA(); break;
                case "B": VariantMenus.ApplyB(); break;
                case "C": VariantMenus.ApplyC(); break;
            }

            var results = new List<string>();
            var ok = true;

            foreach (var root in FindAllOverlayRoots())
            {
                var path = root.gameObject.scene.name + "/ResolutionRoot";
                var panel = root.Find("ResolutionPanel");
                if (panel == null) { results.Add($"{path}: MISSING ResolutionPanel"); ok = false; continue; }

                // Required TMP nodes
                var title = panel.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
                var body  = panel.Find("BodyText")?.GetComponent<TextMeshProUGUI>();
                var q0    = panel.Find("Quest_0")?.GetComponent<TextMeshProUGUI>();
                var q1    = panel.Find("Quest_1")?.GetComponent<TextMeshProUGUI>();
                var q2    = panel.Find("Quest_2")?.GetComponent<TextMeshProUGUI>();
                var btnT  = panel.Find("ResolveButton/Text")?.GetComponent<TextMeshProUGUI>();

                bool titleOk = title && !string.IsNullOrWhiteSpace(title.text);
                bool bodyOk  = body  && !string.IsNullOrWhiteSpace(body.text);
                bool q0Ok    = q0    && !string.IsNullOrWhiteSpace(q0.text);
                bool q1Ok    = q1    && !string.IsNullOrWhiteSpace(q1.text);
                bool q2Ok    = q2    && !string.IsNullOrWhiteSpace(q2.text);
                bool btnOk   = btnT  && !string.IsNullOrWhiteSpace(btnT.text);

                ok &= titleOk && bodyOk && q0Ok && q1Ok && q2Ok && btnOk;

                results.Add($"{path}: Title={titleOk}, Body={bodyOk}, Qs=({q0Ok},{q1Ok},{q2Ok}), Button={btnOk}");
            }

            var color = ok ? "green" : "red";
            Debug.Log($"<b><color={color}>[Verify/{which}] {(ok ? "PASS" : "FAIL")}</color></b> → " + string.Join(" | ", results));
        }

        private static IEnumerable<Transform> FindAllOverlayRoots()
        {
            // Include inactive objects in loaded scenes
            return Resources.FindObjectsOfTypeAll<Transform>()
                .Where(t => t && t.name == "ResolutionRoot" && t.gameObject.scene.IsValid() && t.gameObject.scene.isLoaded);
        }
    }
}
#endif
