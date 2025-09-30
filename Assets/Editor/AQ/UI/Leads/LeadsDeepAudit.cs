#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.UI.Leads
{
    public static class LeadsDeepAudit
    {
        private struct CardReport
        {
            public string Name;
            public string Title;
            public string Objective;
            public string LeadId;
            public string BgTint;
            public bool TmpOk;
            public bool ActorHasSprite;
            public float ActorAlpha;
            public bool Req1Ok, Req2Ok, Req3Ok;
            public bool SlotsClickable;
            public bool HasMissingScripts;
        }

        [MenuItem("AQ/UI/Leads/Audit Lead Cards (Deep)")]
        public static void Audit()
        {
            var hud = GameObject.Find("HUD_Board");
            var leadsBar = GameObject.Find("LeadsBar");
            var content = GameObject.Find("Content_Leads");

            if (!hud || !leadsBar || !content)
            {
                Debug.LogWarning("[AQ UI Leads] ✗ Expected objects missing. Need HUD_Board, LeadsBar, Content_Leads in the active scene.");
                return;
            }

            // 1) HUD frame checks
            var scroll = leadsBar.GetComponentInChildren<ScrollRect>(true);
            var viewport = leadsBar.transform.Find("Viewport");
            var maskOk = viewport && (viewport.GetComponent<Mask>() || viewport.GetComponent<RectMask2D>());
            var scrollOk = scroll && scroll.horizontal && !scroll.vertical;

            var hlg = content.GetComponent<HorizontalLayoutGroup>();
            var spacingOk = hlg && Mathf.Approximately(hlg.spacing, 24f);

            Info($"HUD frame: ScrollRect H={scroll?.horizontal} V={scroll?.vertical}  Viewport mask={(maskOk ? "OK" : "NO")}  HLG spacing={(hlg ? hlg.spacing.ToString("0") : "n/a")} {(spacingOk ? "✓" : "⚠")}");

            // 2) EpisodeChip under AvatarChip (paranoia check)
            var avatarChip = GameObject.Find("AvatarChip");
            var ep = avatarChip ? avatarChip.transform.Find("EpisodeChip") : null;
            Info($"EpisodeChip {(ep ? "✓ present (direct child)" : "✗ missing")}.");

            // 3) Card reports
            var reports = new List<CardReport>();
            foreach (Transform t in content.transform)
            {
                if (!t.name.StartsWith("Card", StringComparison.OrdinalIgnoreCase)) continue;

                var rep = new CardReport { Name = t.name };

                var title = t.Find("Text_Title")?.GetComponent<TMP_Text>();
                var obj   = t.Find("Text_Objective")?.GetComponent<TMP_Text>();
                var id    = t.Find("Text_LeadId")?.GetComponent<TMP_Text>();
                rep.Title     = title ? title.text : "(none)";
                rep.Objective = obj   ? obj.text   : "(none)";
                rep.LeadId    = id    ? id.text    : "(none)";

                // TMP sanity (Truncate + no raycast)
                bool tmpOk = true;
                foreach (var tmp in t.GetComponentsInChildren<TMP_Text>(true))
                {
                    var isTrunc = tmp.overflowMode == TextOverflowModes.Truncate;
                    var noRay   = !tmp.raycastTarget;
                    if (!isTrunc || !noRay) { tmpOk = false; break; }
                }
                rep.TmpOk = tmpOk;

                // Background tint classification
                var bg = t.GetComponent<Image>();
                rep.BgTint = bg ? ClassifyTint(bg.color) : "(no Image)";

                // Actor state
                var actorImg = t.Find("ActorAnchor/Image")?.GetComponent<Image>();
                rep.ActorHasSprite = actorImg && actorImg.sprite != null;
                rep.ActorAlpha = actorImg ? actorImg.color.a : 0f;

                // Requirement slots
                bool[] slotOk = new bool[3];
                bool allClickable = true;
                bool missingScripts = false;

                for (int i = 1; i <= 3; i++)
                {
                    var slot = t.Find($"RequirementsRow/Req_{i}");
                    if (!slot) { slotOk[i-1] = false; continue; }

                    // Expected children
                    bool hasIcon = slot.Find("Icon");
                    bool hasTick = slot.Find("Tick");
                    bool hasView = slot.GetComponent(typeof(Component)) && slot.GetComponent("RequirementSlotView") != null;
                    bool hasBtn  = slot.GetComponent<Button>() != null;

                    slotOk[i-1] = hasIcon && hasTick && hasBtn;
                    allClickable &= hasBtn;

                    // Missing (Mono) detection
                    var comps = slot.GetComponents<Component>();
                    foreach (var c in comps) if (!c) { missingScripts = true; break; }
                }

                rep.Req1Ok = slotOk[0]; rep.Req2Ok = slotOk[1]; rep.Req3Ok = slotOk[2];
                rep.SlotsClickable = allClickable;

                // any missing scripts anywhere under the card?
                foreach (var c in t.GetComponentsInChildren<Component>(true))
                {
                    if (!c) { missingScripts = true; break; }
                }
                rep.HasMissingScripts = missingScripts;

                reports.Add(rep);
            }

            // 4) Print summary
            if (reports.Count == 0)
            {
                Debug.LogWarning("[AQ UI Leads] No cards found under Content_Leads.");
                return;
            }

            Info($"Cards found: {reports.Count}");
            foreach (var r in reports)
            {
                var reqStr = $"{Flag(r.Req1Ok)}/{Flag(r.Req2Ok)}/{Flag(r.Req3Ok)}";
                var actor  = r.ActorHasSprite ? $"sprite α={r.ActorAlpha:0.##}" : $"no-sprite α={r.ActorAlpha:0.##}";
                var tmp    = r.TmpOk ? "TMP ✓" : "TMP ⚠";

                Debug.Log($"[AQ UI Leads] {r.Name}: Title='{r.Title}'  Id='{r.LeadId}'  Bg={r.BgTint}  {tmp}  Actor={actor}  ReqSlots={reqStr}  Clickable={(r.SlotsClickable? "✓":"⚠")}  MissingScripts={(r.HasMissingScripts? "⚠":"✓")}");
            }

            // 5) Save JSON snapshot for offline reference
            var json = JsonUtility.ToJson(new Wrapper { items = reports }, true);
            var dir = "_audit/ui"; System.IO.Directory.CreateDirectory(dir);
            var path = System.IO.Path.Combine(dir, $"lead_cards_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            System.IO.File.WriteAllText(path, json);
            Info($"Wrote snapshot: {path}");

            // Mark scene dirty only if we ever add repair actions (currently read-only)
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static string ClassifyTint(Color c)
        {
            // loose buckets for white / eggshell / pale-green; tolerates small drift
            var hex = ColorUtility.ToHtmlStringRGB(c);
            // quick distances in RGB
            Vector3 v = new Vector3(c.r, c.g, c.b);
            float dWhite = Dist(v, new Vector3(1f, 1f, 1f));
            float dEgg   = Dist(v, new Vector3(0xF7/255f, 0xF3/255f, 0xE7/255f));
            float dGreen = Dist(v, new Vector3(0xE6/255f, 0xF4/255f, 0xEA/255f));
            if (dWhite <= dEgg && dWhite <= dGreen) return $"white #{hex}";
            if (dEgg   <= dWhite && dEgg   <= dGreen) return $"eggshell #{hex}";
            if (dGreen <= dWhite && dGreen <= dEgg)   return $"pale-green #{hex}";
            return $"other #{hex}";
        }
        private static float Dist(Vector3 a, Vector3 b){ var d=a-b; return d.magnitude; }

        [Serializable] private class Wrapper { public List<CardReport> items; }

        private static string Flag(bool b) => b ? "✓" : "✗";
        private static void Info(string s) => Debug.Log("[AQ UI Leads] " + s);
    }
}
#endif
