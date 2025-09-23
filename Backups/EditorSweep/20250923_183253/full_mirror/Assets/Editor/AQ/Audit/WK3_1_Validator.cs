// Editor-only validation for WK3-1 that:
// - finds inactive objects (Resources.FindObjectsOfTypeAll)
// - avoids direct TMPro references (checks type name "TextMeshProUGUI")
// - scans ALL components so TMP is detected even if not first
// - prints a concise evidence report

using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.Audit
{
    public static class WK3_1_Validator
    {
        [MenuItem("AQ/WK3-1/Validate Overlay")]
        public static void ValidateMenu() => Validate();

        public static void Validate()
        {
            // helpers that include inactive scene objects
            GameObject FindRoot(string name)
            {
                return Resources.FindObjectsOfTypeAll<GameObject>()
                    .FirstOrDefault(g => g && g.name == name && g.scene.IsValid());
            }

            Transform FindChildDeep(Transform parent, string childName)
            {
                if (!parent) return null;
                foreach (var t in parent.GetComponentsInChildren<Transform>(true))
                    if (t && t.name == childName) return t;
                return null;
            }

            // Detect TMP by type-name (no hard TMPro reference)
            static bool HasTMPOn(Transform t)
            {
                if (!t) return false;
                var comps = t.GetComponents<Component>();
                return comps != null && comps.Any(c => c && c.GetType().Name == "TextMeshProUGUI");
            }

            static bool HasTMPInChildren(Transform t)
            {
                if (!t) return false;
                var comps = t.GetComponentsInChildren<Component>(true);
                return comps != null && comps.Any(c => c && c.GetType().Name == "TextMeshProUGUI");
            }

            static string BoolStr(bool b) => b ? "True" : "False";
            static string NameOrNull(Object o) => o ? o.name : "null";

            // ----- locate overlay (including inactive) -----
            var root  = FindRoot("ResolutionRoot");
            var panel = FindChildDeep(root ? root.transform : null, "ResolutionPanel");
            var title = FindChildDeep(panel, "TitleText");
            var body  = FindChildDeep(panel, "BodyText");
            var btn   = FindChildDeep(panel, "ResolveButton");

            bool overlayPresent = root && panel && title && body && btn;

            // TMP checks (labels must be TMP on Title/Body; TMP present under ResolveButton)
            bool usesTMP = false;
            if (overlayPresent)
            {
                bool titleTMP = HasTMPOn(title);
                bool bodyTMP  = HasTMPOn(body);
                bool btnTMP   = HasTMPInChildren(btn); // label can be on a child
                usesTMP = titleTMP && bodyTMP && btnTMP;
            }

            // Resolve wiring (persistent listeners)
            bool resolveWired = false;
            if (btn)
            {
                var button = btn.GetComponent<Button>();
                if (button)
                {
                    int n = button.onClick.GetPersistentEventCount();
                    for (int i = 0; i < n; i++)
                    {
                        var tgt = button.onClick.GetPersistentTarget(i);
                        var m   = button.onClick.GetPersistentMethodName(i);
                        if (tgt != null && m == "OnResolve") { resolveWired = true; break; }
                    }
                }
            }

            // Layout sanity: button below body (works even if inactive, uses world pos)
            bool buttonBelowBody = false;
            if (btn && body)
            {
                var br  = btn.GetComponent<RectTransform>();
                var bdy = body.GetComponent<RectTransform>();
                if (br && bdy) buttonBelowBody = br.position.y < bdy.position.y;
            }

            // Diamond → Advance (also find inactive)
            var miniRoot = FindRoot("Minigame_Scrub");
            var diamond  = FindChildDeep(miniRoot ? miniRoot.transform : null, "Button");
            bool diamondAdvances = false;
            if (diamond)
            {
                var sb = diamond.GetComponent<Button>();
                if (sb)
                {
                    int n = sb.onClick.GetPersistentEventCount();
                    for (int i = 0; i < n; i++)
                    {
                        var tgt = sb.onClick.GetPersistentTarget(i);
                        var m   = sb.onClick.GetPersistentMethodName(i);
                        if (tgt != null && m == "Advance") { diamondAdvances = true; break; }
                    }
                }
            }

            // Output
            var msg =
                "WK3-1 VALIDATION → " +
                "OverlayPresent=" + BoolStr(overlayPresent) + ", " +
                "UsesTMP=" + BoolStr(usesTMP) + ", " +
                "ResolveOnClick=" + BoolStr(resolveWired) + ", " +
                "ButtonBelowBody=" + BoolStr(buttonBelowBody) + ", " +
                "ScrubDiamondAdvances=" + BoolStr(diamondAdvances) + "\n" +
                "[evidence] root=" + NameOrNull(root) + " active=" + BoolStr(root && root.activeInHierarchy) + "; " +
                "panel=" + NameOrNull(panel ? panel.gameObject : null) + " active=" + BoolStr(panel && panel.gameObject.activeInHierarchy) + "; " +
                "title=" + NameOrNull(title ? title.gameObject : null) + "; " +
                "body=" + NameOrNull(body ? body.gameObject : null) + "; " +
                "btn="  + NameOrNull(btn   ? btn.gameObject   : null) + "\n" +
                "[diamond] miniRoot=" + NameOrNull(miniRoot) + " active=" + BoolStr(miniRoot && miniRoot.activeInHierarchy) + "; " +
                "button=" + NameOrNull(diamond ? diamond.gameObject : null);

            Debug.Log(msg);
        }
    }
}
