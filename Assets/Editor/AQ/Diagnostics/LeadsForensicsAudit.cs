#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AQ.EditorTools.Diagnostics
{
    /// <summary>
    /// Read-only, side-effect-free audit of HUD_Board, LeadsBar, lead cards, and TierSetPopup.
    /// Writes JSON to _audit/ui/ and prints a concise console summary. Never modifies the scene.
    /// </summary>
    public static class LeadsForensicsAudit
    {
        // === PUBLIC MENU ===
        [MenuItem("AQ/Diagnostics/Leads/Run Forensics Audit")]
        public static void Run()
        {
            var report = new RootReport
            {
                timestamp      = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                scene          = SceneManager.GetActiveScene().name,
                unityVersion   = Application.unityVersion,
                tmpVersion     = GetTMPVersion(),
                typesAvailable = DetectTypes(new[]
                {
                    "AQ.App.UI.Leads.LeadCardPresenter",
                    "AQ.App.UI.Leads.RequirementSlotView",
                    "AQ.App.UI.Leads.TierSetPopupPresenter",
                    "LeadCardPresenter", "RequirementSlotView", "TierSetPopupPresenter"
                })
            };

            // Canvas & HUD
            var canvas = GameObject.Find("Canvas_Board");
            report.canvas = canvas ? InspectCanvas(canvas) : null;
            if (!canvas) Warn("Canvas_Board not found.");

            var hud = GameObject.Find("HUD_Board");
            report.hud = hud ? InspectHUD(hud) : null;
            if (!hud) Warn("HUD_Board not found.");

            // LeadsBar & cards
            var leadsBar = GameObject.Find("LeadsBar");
            var content  = GameObject.Find("Content_Leads");
            if (leadsBar && content)
            {
                report.leadsBar = InspectLeadsBar(leadsBar, content, out var cardReports);
                report.cards = cardReports;
            }
            else
            {
                if (!leadsBar) Warn("LeadsBar not found.");
                if (!content)  Warn("Content_Leads not found.");
            }

            // TierSetPopup
            var popup = GameObject.Find("TierSetPopup");
            report.tierSetPopup = popup ? InspectTierSetPopup(popup) : null;
            if (!popup) Info("TierSetPopup not found.");

            // LeadCard prefabs (best-effort)
            report.prefabs = InspectLeadCardPrefabs();

            // --- OUTPUT ---
            var dir  = "_audit/ui";
            System.IO.Directory.CreateDirectory(dir);
            var path = System.IO.Path.Combine(dir, $"leads_forensics_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            var json = JsonUtility.ToJson(report, prettyPrint: true);
            System.IO.File.WriteAllText(path, json);

            // Console summary
            PrintSummary(report, path);
        }

        // === INSPECTORS ===
        private static CanvasReport InspectCanvas(GameObject canvasGo)
        {
            var r = new CanvasReport { name = canvasGo.name };
            var canvas = canvasGo.GetComponent<Canvas>();
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            var rt     = canvasGo.transform as RectTransform;

            r.mode   = canvas ? canvas.renderMode.ToString() : "(no Canvas)";
            if (scaler)
            {
                r.scalerMode    = scaler.uiScaleMode.ToString();
                r.referenceResX = scaler.referenceResolution.x;
                r.referenceResY = scaler.referenceResolution.y;
                r.match         = scaler.matchWidthOrHeight;
            }
            r.rect = RectInfo(rt);
            return r;
        }

        private static HudReport InspectHUD(GameObject hud)
        {
            var r = new HudReport { name = hud.name, rect = RectInfo(hud.transform as RectTransform) };

            var vlg = hud.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                r.vlg = new VlgInfo
                {
                    spacing    = vlg.spacing,
                    childCtrlW = vlg.childControlWidth,
                    childCtrlH = vlg.childControlHeight,
                    expandW    = vlg.childForceExpandWidth,
                    expandH    = vlg.childForceExpandHeight,
                    padding    = PadInfo(vlg.padding)
                };
            }

            // TopBar
            var topBar = hud.transform.Find("TopBar");
            if (topBar)
            {
                r.topBar = new TopBarInfo
                {
                    rect                     = RectInfo(topBar as RectTransform),
                    hlg                      = BuildHlgInfo(topBar.GetComponent<HorizontalLayoutGroup>()),
                    hasBtnHome               = topBar.Find("Btn_Home"),
                    hasAvatarChip            = topBar.Find("AvatarChip"),
                    hasEpisodeChipUnderAvatar= topBar.Find("AvatarChip/EpisodeChip"),
                    hasSpacerFlex            = topBar.Find("Spacer_Flex"),
                    hasMeters = new [] { "Meter_Energy", "Meter_Soft", "Meter_Premium" }
                        .Select(n => (n, present: topBar.Find(n)!=null)).ToList()
                };
            }
            else Warn("TopBar missing under HUD_Board.");

            // StatusRow
            var statusRow = hud.transform.Find("StatusRow");
            if (statusRow)
            {
                var row = new StatusRowInfo { rect = RectInfo(statusRow as RectTransform) };
                row.labels = new List<TextInfo>();
                foreach (var name in new[] { "Text_Solved", "Text_Evidence", "Text_Leads", "Text_LastBreakthrough", "Text_LastOK" })
                {
                    var t = statusRow.Find(name)?.GetComponent<TMP_Text>();
                    if (t) row.labels.Add(TextInfoOf(name, t));
                }
                r.statusRow = row;
            }
            else Warn("StatusRow missing under HUD_Board.");

            // RequirementsHUD
            var reqHud = hud.transform.Find("RequirementsHUD");
            if (reqHud)
            {
                r.requirementsHud = new RequirementsHudInfo
                {
                    rect   = RectInfo(reqHud as RectTransform),
                    active = reqHud.gameObject.activeSelf,
                    image  = reqHud.GetComponent<Image>() ? "Image present" : "(no Image)"
                };
            }

            return r;
        }

        private static LeadsBarReport InspectLeadsBar(GameObject leadsBar, GameObject content, out List<CardReport> cards)
        {
            var r = new LeadsBarReport
            {
                rect        = RectInfo(leadsBar.transform as RectTransform),
                hasScroll   = leadsBar.GetComponentInChildren<ScrollRect>(true) != null
            };

            var scroll = leadsBar.GetComponentInChildren<ScrollRect>(true);
            if (scroll)
            {
                r.scrollH = scroll.horizontal;
                r.scrollV = scroll.vertical;
                r.scrollContentPath = PathOf(scroll.content);
            }

            var viewport = leadsBar.transform.Find("Viewport");
            if (viewport)
            {
                r.viewportHasMask = viewport.GetComponent<RectMask2D>() || viewport.GetComponent<Mask>();
            }

            var hlg = content.GetComponent<HorizontalLayoutGroup>();
            if (hlg)
            {
                r.contentRect    = RectInfo(content.transform as RectTransform);
                r.contentSpacing = hlg.spacing;
                r.contentPadding = PadInfo(hlg.padding);
            }

            // Cards (tolerant: any direct child that looks like a card)
            cards = new List<CardReport>();
            foreach (Transform c in content.transform)
            {
                var card = InspectCard(c);
                if (card != null) cards.Add(card);
            }

            return r;
        }

        private static CardReport InspectCard(Transform t)
        {
            var hasTitle = t.Find("Text_Title");
            var hasReq   = t.Find("RequirementsRow");
            if (!hasTitle && !hasReq) return null;

            var rep = new CardReport
            {
                name           = t.name,
                rect           = RectInfo(t as RectTransform),
                backImage      = t.GetComponent<Image>() != null,
                button         = t.GetComponent<Button>() != null,
                missingScripts = CountMissingScriptsRecursive(t.gameObject),
                background     = ColorHexOr(t.GetComponent<Image>()?.color)
            };

            // Title / Objective / Id
            rep.title     = TextInfoOf("Text_Title",     t.Find("Text_Title")?.GetComponent<TMP_Text>());
            rep.objective = TextInfoOf("Text_Objective", t.Find("Text_Objective")?.GetComponent<TMP_Text>());
            rep.leadId    = TextInfoOf("Text_LeadId",    t.Find("Text_LeadId")?.GetComponent<TMP_Text>());

            // Actor
            var actorImg = t.Find("ActorAnchor/Image")?.GetComponent<Image>();
            rep.actor = new ActorInfo
            {
                hasAnchor = t.Find("ActorAnchor") != null,
                hasImage  = actorImg != null,
                hasSprite = actorImg && actorImg.sprite != null,
                alpha     = actorImg ? actorImg.color.a : 0f
            };

            // Requirements
            var row = t.Find("RequirementsRow");
            if (row)
            {
                rep.requirementsRow = new RequirementsRowReport
                {
                    present = true,
                    req1    = InspectSlot(row, "Req_1"),
                    req2    = InspectSlot(row, "Req_2"),
                    req3    = InspectSlot(row, "Req_3")
                };
            }
            else
            {
                rep.requirementsRow = new RequirementsRowReport { present = false };
            }

            return rep;
        }

        private static RequirementSlotReport InspectSlot(Transform row, string name)
        {
            var s = row.Find(name);
            if (!s) return new RequirementSlotReport { name = name, present = false };

            var icon = s.Find("Icon")?.GetComponent<Image>();
            var tick = s.Find("Tick")?.GetComponent<Image>();

            return new RequirementSlotReport
            {
                name                   = name,
                present                = true,
                hasButton              = s.GetComponent<Button>() != null,
                hasRequirementSlotView = HasComponentNamed(s.gameObject, "RequirementSlotView"),
                hasIcon                = icon != null,
                hasTick                = tick != null,
                tickEnabled            = tick && tick.enabled,
                missingScripts         = CountMissingScriptsRecursive(s.gameObject)
            };
            }

        private static TierSetPopupReport InspectTierSetPopup(GameObject popup)
        {
            var t         = popup.transform;
            var cg        = popup.GetComponent<CanvasGroup>();
            var titleTxt  = FindTMPText(t, "Title"); // UI or 3D TMP, either is OK (read-only)
            var iconGrid  = t.Find("IconGrid");
            var gridLay   = iconGrid ? iconGrid.GetComponent<GridLayoutGroup>() : null;
            var highlight = iconGrid ? iconGrid.Find("Highlight")?.GetComponent<Image>() : null;
            var closeBtn  = t.Find("Btn_Close")?.GetComponent<Button>();

            var r = new TierSetPopupReport
            {
                rect                = RectInfo(t as RectTransform),
                hasCanvasGroup      = cg != null,
                visibleByCG         = cg ? cg.alpha > 0 && cg.blocksRaycasts && cg.interactable : (bool?)null,
                hasTitle            = titleTxt != null,
                titleIsUI           = titleTxt is TextMeshProUGUI,
                titleHasRect        = titleTxt ? titleTxt.transform is RectTransform : (bool?)null,
                hasIconGrid         = iconGrid != null,
                iconGridHasRect     = iconGrid ? iconGrid is RectTransform : (bool?)null,
                iconGridHasLayout   = gridLay != null,
                iconChildren        = iconGrid ? iconGrid.Cast<Transform>().Count(ch => ch.name.StartsWith("Icon")) : 0,
                hasHighlight        = highlight != null,
                hasCloseButton      = closeBtn != null,
                hasPresenterType    = DetectTypes(new[]{"AQ.App.UI.Leads.TierSetPopupPresenter","TierSetPopupPresenter"}).Count > 0,
                presenterOnPopup    = HasComponentNamed(popup, "TierSetPopupPresenter"),
                missingScripts      = CountMissingScriptsRecursive(popup)
            };

            return r;
        }

        private static List<PrefabReport> InspectLeadCardPrefabs()
        {
            var list = new List<PrefabReport>();
            var guids = AssetDatabase.FindAssets("LeadCard t:prefab");
            foreach (var guid in guids)
            {
                var path   = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!prefab) continue;

                var pr = new PrefabReport { path = path, name = prefab.name };
                pr.missingScripts = CountMissingScriptsRecursive(prefab);

                var t = prefab.transform;
                pr.hasTextTitle  = t.Find("Text_Title") != null;
                pr.hasObjective  = t.Find("Text_Objective") != null;
                pr.hasLeadId     = t.Find("Text_LeadId") != null;
                pr.hasReqRow     = t.Find("RequirementsRow") != null;
                pr.hasReq1       = t.Find("RequirementsRow/Req_1") != null;
                pr.hasReq2       = t.Find("RequirementsRow/Req_2") != null;
                pr.hasReq3       = t.Find("RequirementsRow/Req_3") != null;
                pr.hasActorImage = t.Find("ActorAnchor/Image")?.GetComponent<Image>() != null;
                pr.hasPresenter  = HasComponentNamed(prefab, "LeadCardPresenter");

                list.Add(pr);
            }
            return list;
        }

        // === UTIL ===
        private static string GetTMPVersion()
        {
            var asm = typeof(TextMeshProUGUI).Assembly.GetName();
            return $"{asm.Name} {asm.Version}";
        }

        private static List<string> DetectTypes(IEnumerable<string> names)
        {
            var found = new List<string>();
            foreach (var n in names)
            {
                var t = Type.GetType(n);
                if (t != null) { found.Add(t.FullName); continue; }
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        t = asm.GetType(n) ?? asm.GetTypes().FirstOrDefault(x => x.Name == n);
                        if (t != null) { found.Add(t.FullName); break; }
                    }
                    catch { /* some dynamic assemblies throw */ }
                }
            }
            return found.Distinct().ToList();
        }

        private static bool HasComponentNamed(GameObject go, string simpleName)
        {
            foreach (var c in go.GetComponents<Component>())
            {
                if (!c) return true; // missing script counts as "present but broken"
                var type = c.GetType();
                if (type.Name == simpleName || type.FullName == simpleName)
                    return true;
            }
            return false;
        }

        private static string PathOf(Transform t)
        {
            if (!t) return "(null)";
            var path = t.name;
            var p = t.parent;
            while (p != null)
            {
                path = p.name + "/" + path;
                p = p.parent;
            }
            return path;
        }

        private static int CountMissingScriptsRecursive(GameObject go)
        {
            int count = 0;
            foreach (var c in go.GetComponents<Component>()) if (!c) count++;
            foreach (Transform child in go.transform)
                count += CountMissingScriptsRecursive(child.gameObject);
            return count;
        }

        private static string ColorHexOr(Color? c)
        {
            if (c == null) return "(no Image)";
            Color32 cc = (Color)c;
            return $"#{cc.r:X2}{cc.g:X2}{cc.b:X2}{cc.a:X2}";
        }

        private static RectData RectInfo(RectTransform rt)
        {
            if (!rt) return new RectData { hasRectTransform = false };
            return new RectData
            {
                hasRectTransform = true,
                anchorMinX = rt.anchorMin.x, anchorMinY = rt.anchorMin.y,
                anchorMaxX = rt.anchorMax.x, anchorMaxY = rt.anchorMax.y,
                pivotX = rt.pivot.x, pivotY = rt.pivot.y,
                sizeX = rt.rect.size.x, sizeY = rt.rect.size.y,
                posX = rt.anchoredPosition.x, posY = rt.anchoredPosition.y
            };
        }

        private static PaddingInfo PadInfo(RectOffset p)
            => p == null ? null : new PaddingInfo { left = p.left, right = p.right, top = p.top, bottom = p.bottom };

        private static HlgInfo BuildHlgInfo(HorizontalLayoutGroup h)
        {
            if (!h) return null;
            return new HlgInfo
            {
                spacing    = h.spacing,
                childCtrlW = h.childControlWidth,
                childCtrlH = h.childControlHeight,
                expandW    = h.childForceExpandWidth,
                expandH    = h.childForceExpandHeight,
                padding    = PadInfo(h.padding)
            };
        }

        private static TextInfo TextInfoOf(string name, TMP_Text t)
        {
            if (!t) return new TextInfo { name = name, present = false };
            string wrap;
            try { wrap = t.textWrappingMode.ToString(); } catch { wrap = "(unknown)"; }

            return new TextInfo
            {
                name          = name,
                present       = true,
                text          = t.text,
                font          = t.font ? t.font.name : "(null)",
                size          = t.fontSize,
                alignment     = t.alignment.ToString(),
                overflow      = t.overflowMode.ToString(),
                wrapping      = wrap,
                raycastTarget = t.raycastTarget
            };
        }

        private static TMP_Text FindTMPText(Transform parent, string name)
        {
            var tr = parent.Find(name);
            if (!tr) return null;
            return tr.GetComponent<TextMeshProUGUI>() ?? tr.GetComponent<TMP_Text>();
        }

        // === LOGGING ===
        private static void Info(string s) => Debug.Log("[AQ Forensics] " + s);
        private static void Warn(string s) => Debug.LogWarning("[AQ Forensics] " + s);

        private static void PrintSummary(RootReport r, string jsonPath)
        {
            Info($"Scene='{r.scene}' Unity={r.unityVersion} TMP={r.tmpVersion}");

            if (r.canvas != null)
                Info($"Canvas: mode={r.canvas.mode}, scaler={r.canvas.scalerMode} ref=({r.canvas.referenceResX},{r.canvas.referenceResY}) match={r.canvas.match:0.00}");

            if (r.leadsBar != null)
            {
                var lb = r.leadsBar;
                Info($"LeadsBar: ScrollRect H={lb.scrollH} V={lb.scrollV} viewportMask={(lb.viewportHasMask?"OK":"NO")} spacing={lb.contentSpacing:0} padding L{lb.contentPadding?.left}/R{lb.contentPadding?.right}");
                var count = r.cards?.Count ?? 0;
                Info($"Cards found: {count}");
                if (r.cards != null)
                {
                    foreach (var c in r.cards)
                    {
                        var req = c.requirementsRow?.present == true
                            ? $"{Flag(c.requirementsRow.req1)}/{Flag(c.requirementsRow.req2)}/{Flag(c.requirementsRow.req3)}"
                            : "no row";
                        Info($"Card '{c.name}': bg={c.background} title='{c.title?.text}' id='{c.leadId?.text}' actor(sprite={c.actor?.hasSprite}, α={c.actor?.alpha:0.##}) req={req} missingScripts={c.missingScripts}");
                    }
                }
            }

            if (r.tierSetPopup != null)
            {
                var p = r.tierSetPopup;
                Info($"TierSetPopup: title={(p.hasTitle? "OK":"NO")} titleUI={(p.titleIsUI? "UI":"TMP")} rect={(p.titleHasRect==true?"Rect":"NoRect")} grid={(p.hasIconGrid? "OK":"NO")} gridRect={(p.iconGridHasRect==true?"Rect":"NoRect")} icons={p.iconChildren} presenterType={(p.hasPresenterType?"seen":"none")} presenterOnObj={(p.presenterOnPopup?"yes":"no")} missingScripts={p.missingScripts}");
            }

            Info($"Types available: [{string.Join(", ", r.typesAvailable)}]");
            Info($"JSON written: {jsonPath}");
        }

        private static string Flag(RequirementSlotReport s)
            => (!s.present ? "-" : (s.hasButton && s.hasIcon && s.hasTick ? "✓" : "⚠"));

        // === DTOs ===
        [Serializable] private class RootReport
        {
            public string timestamp;
            public string scene;
            public string unityVersion;
            public string tmpVersion;
            public List<string> typesAvailable;
            public CanvasReport canvas;
            public HudReport hud;
            public LeadsBarReport leadsBar;
            public List<CardReport> cards;
            public TierSetPopupReport tierSetPopup;
            public List<PrefabReport> prefabs;
        }

        [Serializable] private class CanvasReport
        {
            public string name;
            public string mode;
            public string scalerMode;
            public float referenceResX, referenceResY, match;
            public RectData rect;
        }

        [Serializable] private class HudReport
        {
            public string name;
            public RectData rect;
            public VlgInfo vlg;
            public TopBarInfo topBar;
            public StatusRowInfo statusRow;
            public RequirementsHudInfo requirementsHud;
        }

        [Serializable] private class TopBarInfo
        {
            public RectData rect;
            public HlgInfo hlg;
            public bool hasBtnHome;
            public bool hasAvatarChip;
            public bool hasEpisodeChipUnderAvatar;
            public bool hasSpacerFlex;
            public List<(string meter, bool present)> hasMeters;
        }

        [Serializable] private class StatusRowInfo
        {
            public RectData rect;
            public List<TextInfo> labels;
        }

        [Serializable] private class RequirementsHudInfo
        {
            public RectData rect;
            public bool active;
            public string image;
        }

        [Serializable] private class LeadsBarReport
        {
            public RectData rect;
            public bool hasScroll;
            public bool scrollH, scrollV;
            public string scrollContentPath;
            public bool viewportHasMask;
            public RectData contentRect;
            public float contentSpacing;
            public PaddingInfo contentPadding;
        }

        [Serializable] private class CardReport
        {
            public string name;
            public RectData rect;
            public bool backImage;
            public bool button;
            public int  missingScripts;
            public string background;
            public TextInfo title, objective, leadId;
            public ActorInfo actor;
            public RequirementsRowReport requirementsRow;
        }

        [Serializable] private class ActorInfo
        {
            public bool hasAnchor;
            public bool hasImage;
            public bool hasSprite;
            public float alpha;
        }

        [Serializable] private class RequirementsRowReport
        {
            public bool present;
            public RequirementSlotReport req1, req2, req3;
        }

        [Serializable] private class RequirementSlotReport
        {
            public string name;
            public bool present;
            public bool hasButton;
            public bool hasRequirementSlotView;
            public bool hasIcon;
            public bool hasTick;
            public bool tickEnabled;
            public int  missingScripts;
        }

        [Serializable] private class TierSetPopupReport
        {
            public RectData rect;
            public bool hasCanvasGroup;
            public bool? visibleByCG;
            public bool hasTitle;
            public bool titleIsUI;
            public bool? titleHasRect;
            public bool hasIconGrid;
            public bool? iconGridHasRect;
            public bool iconGridHasLayout;
            public int iconChildren;
            public bool hasHighlight;
            public bool hasCloseButton;
            public bool hasPresenterType;
            public bool presenterOnPopup;
            public int  missingScripts;
        }

        [Serializable] private class PrefabReport
        {
            public string path;
            public string name;
            public int    missingScripts;
            public bool   hasTextTitle, hasObjective, hasLeadId;
            public bool   hasReqRow, hasReq1, hasReq2, hasReq3;
            public bool   hasActorImage, hasPresenter;
        }

        [Serializable] private class RectData
        {
            public bool hasRectTransform;
            public float anchorMinX, anchorMinY, anchorMaxX, anchorMaxY;
            public float pivotX, pivotY;
            public float sizeX, sizeY;
            public float posX, posY;
        }

        [Serializable] private class VlgInfo
        {
            public float spacing;
            public bool childCtrlW, childCtrlH, expandW, expandH;
            public PaddingInfo padding;
        }

        [Serializable] private class HlgInfo
        {
            public float spacing;
            public bool childCtrlW, childCtrlH, expandW, expandH;
            public PaddingInfo padding;
        }

        [Serializable] private class PaddingInfo { public int left, right, top, bottom; }

        [Serializable] private class TextInfo
        {
            public string name;
            public bool   present;
            public string text;
            public string font;
            public float  size;
            public string alignment;
            public string overflow;
            public string wrapping;
            public bool   raycastTarget;
        }
    }
}
#endif
