// Unity 6.x, C# 9 (block-scoped namespaces, not file-scoped)
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.Editor.Scenes
{
    public static class LeadsBarWire
    {
        private const string CanvasName = "Canvas_Board";
        private const string HudBoardPath = "HUD_Board";
        private const string LeadsBarPath = "LeadsBar";
        private const string ScrollName = "ScrollLeads";
        private const string ViewportName = "Viewport";
        private const string ContentName = "Content_Leads";
        private const string LeadCardPrefabPath = "Assets/UI/Prefabs/LeadCardView.prefab";

        // ---------- PUBLIC MENUS ----------

        [MenuItem("AQ/Scenes/Wire Leads Board (HLG Horizontal)")]
        public static void WireLeadsBarHLG()
        {
            var (canvas, hud, leads) = EnsurePaths();
            var (scroll, viewport, content) = EnsureScrollRect(leads, horizontal:true, vertical:false);

            // HLG per v4.4 contract
            var hlg = EnsureComponent<HorizontalLayoutGroup>(content.gameObject);
            hlg.spacing = 24f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            var fitter = EnsureComponent<ContentSizeFitter>(content.gameObject);
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

            // If a GridLayoutGroup exists from a previous run, remove it (contract says HLG)
            var glg = content.GetComponent<GridLayoutGroup>();
            if (glg) Object.DestroyImmediate(glg, true);

            // Optional: seed a few demo cards if prefab present
            SeedDemoLeadCards(content);

            MarkDirtyAndPing(content.gameObject, "✅ Done. LeadsBar wired for HLG (horizontal). Press Play.");

            // Safety: name normalization
            scroll.name = ScrollName;
            viewport.name = ViewportName;
            content.name = ContentName;
        }

        [MenuItem("AQ/Scenes/Wire Leads Board (3-Column Grid)")]
        public static void WireLeadsBarGrid()
        {
            var (canvas, hud, leads) = EnsurePaths();
            var (scroll, viewport, content) = EnsureScrollRect(leads, horizontal:false, vertical:true);

            // Remove HLG + Fitter for grid mode
            var hlg = content.GetComponent<HorizontalLayoutGroup>();
            if (hlg) Object.DestroyImmediate(hlg, true);
            var fitter = content.GetComponent<ContentSizeFitter>();
            if (fitter) Object.DestroyImmediate(fitter, true);

            var glg = EnsureComponent<GridLayoutGroup>(content.gameObject);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;
            glg.cellSize = new Vector2(160, 160);
            glg.spacing = new Vector2(12, 12);
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            glg.childAlignment = TextAnchor.UpperLeft;

            // Optional: seed demo cards
            SeedDemoLeadCards(content);

            MarkDirtyAndPing(content.gameObject, "✅ Done. LeadsBar wired for Grid (3 columns, 160 row, 12 gutters). Press Play.");

            // Safety: name normalization
            scroll.name = ScrollName;
            viewport.name = ViewportName;
            content.name = ContentName;
        }

        [MenuItem("AQ/Audits/Audit LeadsBar Contract")]
        public static void AuditLeadsBarContract()
        {
            var root = GameObject.Find(CanvasName);
            if (!root) { LogFail($"Missing {CanvasName}"); return; }

            var hud = root.transform.Find(HudBoardPath);
            if (!hud) { LogFail($"Missing {CanvasName}/{HudBoardPath}"); return; }

            var leads = hud.Find(LeadsBarPath);
            if (!leads) { LogFail($"Missing {CanvasName}/{HudBoardPath}/{LeadsBarPath}"); return; }

            var scroll = leads.Find(ScrollName);
            var viewport = scroll ? scroll.Find(ViewportName) : null;
            var content = viewport ? viewport.Find(ContentName) : null;

            bool ok = true;
            ok &= Check(scroll, "ScrollRect container");
            ok &= Check(viewport, "Viewport");
            ok &= Check(content, ContentName);

            if (ok)
            {
                var sr = scroll!.GetComponent<ScrollRect>();
                ok &= Check(sr != null, "ScrollRect component");
                ok &= Check(viewport!.GetComponent<Image>() != null, "Viewport has Image");
                ok &= Check(viewport!.GetComponent<Mask>() != null, "Viewport has Mask");

                // Either HLG+Fitter (contract) OR Grid alt
                var hasHLG = content!.GetComponent<HorizontalLayoutGroup>() != null && content.GetComponent<ContentSizeFitter>() != null;
                var hasGrid = content.GetComponent<GridLayoutGroup>() != null;

                if (!hasHLG && !hasGrid)
                {
                    ok = false;
                    Debug.LogError("❌ Content_Leads has neither HLG+Fitter nor GridLayoutGroup.");
                }
                else if (hasHLG)
                {
                    var h = content.GetComponent<HorizontalLayoutGroup>();
                    var f = content.GetComponent<ContentSizeFitter>();
                    ok &= Check(Mathf.Approximately(h.spacing, 24f), "HLG spacing = 24");
                    ok &= Check(!h.childForceExpandWidth && !h.childForceExpandHeight, "HLG force expand off");
                    ok &= Check(h.childControlWidth && h.childControlHeight, "HLG control size W/H on");
                    ok &= Check(f.horizontalFit == ContentSizeFitter.FitMode.PreferredSize, "Fitter horizontal = Preferred");
                    ok &= Check(f.verticalFit == ContentSizeFitter.FitMode.MinSize, "Fitter vertical = Min");
                }
            }

            if (ok) Debug.Log("✅ Audit: LeadsBar matches contract.");
            else Debug.LogError("❌ Audit: LeadsBar violations found (see logs).");
        }

        // Batch entry for PowerShell: -executeMethod AQ.Editor.Scenes.LeadsBarWire.ExecuteWireHLGBatch
        public static void ExecuteWireHLGBatch()
        {
            WireLeadsBarHLG();
            // Save active scene if needed (optional):
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.IsValid() && scene.isDirty) EditorSceneManager.SaveScene(scene);
        }

        // ---------- HELPERS ----------

        private static (GameObject canvas, Transform hud, Transform leads) EnsurePaths()
        {
            var canvas = GameObject.Find(CanvasName);
            if (!canvas)
            {
                canvas = new GameObject(CanvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                var c = canvas.GetComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvas.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }

            var hud = canvas.transform.Find(HudBoardPath);
            if (!hud)
            {
                var go = new GameObject(HudBoardPath, typeof(RectTransform), typeof(VerticalLayoutGroup));
                go.transform.SetParent(canvas.transform, false);
                var vlg = go.GetComponent<VerticalLayoutGroup>();
                vlg.spacing = 0f;
                vlg.childControlHeight = true; // HUD stack manages rows
                vlg.childControlWidth = true;
                vlg.childForceExpandHeight = false;
                vlg.childForceExpandWidth = false;
                hud = go.transform;
            }

            var leads = hud.Find(LeadsBarPath);
            if (!leads)
            {
                var go = new GameObject(LeadsBarPath, typeof(RectTransform), typeof(LayoutElement));
                go.transform.SetParent(hud, false);
                var le = go.GetComponent<LayoutElement>();
                le.minHeight = 220; // sane default; VLG can size this row
                leads = go.transform;
            }

            Stretch(leads.GetComponent<RectTransform>(), vertical:true);

            return (canvas, hud, leads);
        }

        private static (GameObject scroll, RectTransform viewport, RectTransform content) EnsureScrollRect(Transform leads, bool horizontal, bool vertical)
        {
            var scroll = leads.Find(ScrollName)?.gameObject ?? new GameObject(ScrollName, typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scroll.transform.SetParent(leads, false);
            var sr = EnsureComponent<ScrollRect>(scroll);
            sr.horizontal = horizontal;
            sr.vertical = vertical;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.inertia = true;
            sr.decelerationRate = 0.135f;
            sr.scrollSensitivity = 20f;

            // Optional background (fully transparent by default)
            var bg = EnsureComponent<Image>(scroll);
            var bgc = bg.color; bgc.a = 0f; bg.color = bgc;

            var viewport = leads.Find($"{ScrollName}/{ViewportName}") as RectTransform;
            if (!viewport)
            {
                var vpGo = new GameObject(ViewportName, typeof(RectTransform), typeof(Image), typeof(Mask));
                vpGo.transform.SetParent(scroll.transform, false);
                viewport = vpGo.GetComponent<RectTransform>();
                var img = vpGo.GetComponent<Image>(); img.color = new Color(0,0,0,0); // invisible, but required for Mask
                var mask = vpGo.GetComponent<Mask>(); mask.showMaskGraphic = false;
            }

            Stretch(viewport, vertical:true);

            var content = leads.Find($"{ScrollName}/{ViewportName}/{ContentName}") as RectTransform;
            if (!content)
            {
                var cGo = new GameObject(ContentName, typeof(RectTransform));
                cGo.transform.SetParent(viewport, false);
                content = cGo.GetComponent<RectTransform>();
                content.anchorMin = new Vector2(0, 1);
                content.anchorMax = new Vector2(0, 1);
                content.pivot = new Vector2(0, 1);
                content.anchoredPosition = Vector2.zero;
            }

            sr.viewport = viewport;
            sr.content = content;

            Stretch(scroll.GetComponent<RectTransform>(), vertical:true);

            return (scroll, viewport, content);
        }

        private static T EnsureComponent<T>(GameObject go) where T : Component
            => go.GetComponent<T>() ?? go.AddComponent<T>();

        private static void Stretch(RectTransform rt, bool vertical)
        {
            if (!rt) return;
            var aMin = new Vector2(0, vertical ? 0 : rt.anchorMin.y);
            var aMax = new Vector2(1, vertical ? 1 : rt.anchorMax.y);
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void SeedDemoLeadCards(RectTransform content)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(LeadCardPrefabPath);
            if (!prefab) { Debug.Log("ℹ️ LeadCardView prefab not found; skipping demo seed."); return; }

            // Seed 5 demo items only if none exist
            if (content.childCount > 0) return;

            for (int i = 0; i < 5; i++)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, content);
                instance.name = $"LeadCard_{i:00}";
            }
        }

        private static void MarkDirtyAndPing(Object obj, string msg)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorUtility.SetDirty(obj);
            EditorGUIUtility.PingObject(obj);
            Debug.Log(msg);
        }

        private static bool Check(Transform t, string label)
            => Check(t != null, label);

        private static bool Check(bool condition, string label)
        {
            if (!condition) Debug.LogError($"❌ {label}");
            return condition;
        }

        private static void LogFail(string msg) => Debug.LogError($"❌ {msg}");
    }
}
#endif
