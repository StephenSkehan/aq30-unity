#if UNITY_EDITOR
using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using AQ.App.Leads;  // LeadsBarView, LeadCardView, LeadRequirementsHUD
using AQ.App.HUD;    // TopStatusRow

namespace AQ.EditorTools.Scenes
{
    public static class CaseBoardSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Case/Case_Board_Portrait.unity";
        private const string PrefabLeadCardPath = "Assets/UI/Prefabs/LeadCardView.prefab";
        private static readonly string[] LeadAssetPaths =
        {
            "Assets/AQ_Seed/Leads/Lead_LabSetup.asset",
            "Assets/AQ_Seed/Leads/Lead_Surveillance_KP_CAM_12.asset",
            "Assets/AQ_Seed/Leads/Lead_RecordsPull.asset",
        };

        private static Sprite sUiSprite;
        private static TMP_FontAsset sTmpFont;

        [MenuItem("AQ/Scenes/Build → Case_Board_Portrait")]
        public static void BuildCaseBoardScene()
        {
            EnsureFolders();
            sUiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            sTmpFont  = TMP_Settings.defaultFontAsset;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = "Case_Board_Portrait";

            // 1) Camera
            var cam = UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (!cam)
            {
                var camGo = new GameObject("MainCamera", typeof(Camera));
                cam = camGo.GetComponent<Camera>();
            }
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;

            // 2) EventSystem
            if (!UnityEngine.Object.FindFirstObjectByType<EventSystem>())
            {
                var es = new GameObject("EventSystem", typeof(EventSystem));
                es.AddComponent<StandaloneInputModule>();
            }

            // 3) Canvas root
            var canvas = CreateCanvas("Canvas_Board");
            var hud = CreateEmpty("HUD_Board", canvas.transform);
            StretchRect(hud.GetComponent<RectTransform>());

            // TopBar placeholder
            var topBar = CreatePanel("TopBar", hud.transform, new Vector2(1080, 160));
            AnchorTopStretch(topBar.GetComponent<RectTransform>(), 0, 0, 0, -176);
            topBar.GetComponent<Image>().color = new Color(1,1,1,0.03f);

            // StatusRow
            var statusRow = CreatePanel("StatusRow", hud.transform, new Vector2(1080, 72));
            var srRT = statusRow.GetComponent<RectTransform>();
            AnchorTopStretch(srRT, 0, -176, 0, -248);
            var sr = statusRow.AddComponent<TopStatusRow>();

            var tSolved = CreateTMP("Text_Solved", statusRow.transform, "Solved 0%", 26, TextAlignmentOptions.MidlineLeft);
            var tEv     = CreateTMP("Text_Evidence", statusRow.transform, "Evidence 0", 26, TextAlignmentOptions.MidlineLeft);
            var tLeads  = CreateTMP("Text_Leads", statusRow.transform, "Leads 3", 26, TextAlignmentOptions.MidlineLeft);
            var tLast   = CreateTMP("Text_LastBreakthrough", statusRow.transform, "Last OK --", 26, TextAlignmentOptions.MidlineLeft); // ASCII-safe

            var row = statusRow.AddComponent<HorizontalLayoutGroup>();
            row.padding = new RectOffset(24,24,12,12);
            row.spacing = 24;
            row.childAlignment = TextAnchor.MiddleLeft;
            row.childForceExpandWidth = false; row.childForceExpandHeight = false;

            SetSerialized(sr, "solvedText", tSolved);
            SetSerialized(sr, "evidenceText", tEv);
            SetSerialized(sr, "leadsText", tLeads);
            SetSerialized(sr, "lastBreakthroughText", tLast);

            // LeadsBar
            var leadsBarGO = CreateEmpty("LeadsBar", hud.transform);
            var leadsBarRT = leadsBarGO.GetComponent<RectTransform>(); // was AddComponent<RectTransform>()
            AnchorTopStretch(leadsBarRT, 48, -248, 48, -(248+340)); // height 340
            var leadsBar = leadsBarGO.AddComponent<LeadsBarView>();

            var scroll = CreateScrollView("ScrollLeads", leadsBarGO.transform);
            var scrollRect = scroll.GetComponent<ScrollRect>();
            scrollRect.horizontal = true; scrollRect.vertical = false;
            scrollRect.movementType = ScrollRect.MovementType.Elastic; scrollRect.inertia = true;

            var viewport = scroll.transform.Find("Viewport").GetComponent<RectTransform>();
            var vpImg = viewport.gameObject.AddComponent<Image>();
            vpImg.sprite = sUiSprite; vpImg.color = new Color(1,1,1,0.02f);
            var mask = viewport.gameObject.AddComponent<Mask>(); mask.showMaskGraphic = false;

            var content = viewport.Find("Content").GetComponent<RectTransform>();
            content.name = "Content_Leads";
            var hlg = content.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 24; hlg.childControlWidth = true; hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit   = ContentSizeFitter.FitMode.MinSize;

            SetSerialized(leadsBar, "scrollRect", scrollRect);
            SetSerialized(leadsBar, "contentRoot", content);

            var leadCardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabLeadCardPath);
            if (leadCardPrefab != null)
            {
                var comp = leadCardPrefab.GetComponent<LeadCardView>();
                if (comp != null) SetSerialized(leadsBar, "cardPrefab", comp);
                else Debug.LogWarning($"[CaseBoardBuilder] Prefab at {PrefabLeadCardPath} has no LeadCardView component.");
            }
            else Debug.LogWarning($"[CaseBoardBuilder] Missing prefab: {PrefabLeadCardPath}. Build it with AQ → UI → Build Prefabs.");

            var soArrProp = new SerializedObject(leadsBar).FindProperty("initialLeads");
            soArrProp.arraySize = LeadAssetPaths.Length;
            for (int i = 0; i < LeadAssetPaths.Length; i++)
            {
                var so = AssetDatabase.LoadAssetAtPath<LeadCardSO>(LeadAssetPaths[i]);
                soArrProp.GetArrayElementAtIndex(i).objectReferenceValue = so;
                if (so == null) Debug.LogWarning($"[CaseBoardBuilder] Lead asset missing: {LeadAssetPaths[i]}");
            }
            soArrProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();

            // RequirementsHUD
            var reqHud = CreatePanel("RequirementsHUD", hud.transform, new Vector2(1080, 120));
            var reqRT = reqHud.GetComponent<RectTransform>();
            AnchorTopStretch(reqRT, 48, -(248+340+16), 48, -(248+340+16+120));
            var reqComp = reqHud.AddComponent<LeadRequirementsHUD>();

            var icons = new Image[3]; var labels = new TextMeshProUGUI[3]; var ticks = new Image[3];
            var slots = reqHud.AddComponent<HorizontalLayoutGroup>();
            slots.padding = new RectOffset(16,16,16,16); slots.spacing = 24;
            slots.childAlignment = TextAnchor.MiddleLeft;
            for (int i = 0; i < 3; i++)
            {
                var slot = CreatePanel($"ReqSlot_{i}", reqHud.transform, new Vector2(240, 88));
                var slotLay = slot.AddComponent<HorizontalLayoutGroup>();
                slotLay.spacing = 8; slotLay.childAlignment = TextAnchor.MiddleLeft;
                slotLay.childControlWidth = true; slotLay.childControlHeight = true;
                slotLay.childForceExpandWidth = false; slotLay.childForceExpandHeight = false;

                icons[i] = CreateImage($"ReqIcon_{i}", slot.transform, new Vector2(64,64));
                labels[i] = CreateTMP($"ReqLabel_{i}", slot.transform, "Item", 24, TextAlignmentOptions.MidlineLeft);
                ticks[i]  = CreateImage($"ReqTick_{i}", slot.transform, new Vector2(24,24));
                ticks[i].enabled = false;
            }
            SetArray(reqComp, "icons", icons);
            SetArray(reqComp, "labels", labels);
            SetArray(reqComp, "ticks", ticks);

            // Optional presenter stub object (won't error if class missing)
            var presenterGO = new GameObject("BoardPresenter");
            presenterGO.transform.SetParent(canvas.transform, false);
            var bpType = Type.GetType("AQ.App.CaseFlow.BoardPresenter, Assembly-CSharp");
            if (bpType != null)
            {
                var comp = presenterGO.AddComponent(bpType);
                TrySetField(comp, "leadsBar", leadsBar);
                TrySetField(comp, "topStatusRow", sr);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AssetDatabase.Refresh();

            Debug.Log($"[CaseBoardBuilder] Scene created at {ScenePath}");
        }

        // ---------- helpers ----------

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            if (!AssetDatabase.IsValidFolder("Assets/Scenes/Case")) AssetDatabase.CreateFolder("Assets/Scenes", "Case");
        }

        private static GameObject CreateCanvas(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;

            StretchRect(go.GetComponent<RectTransform>());
            return go;
        }

        private static void StretchRect(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        private static void AnchorTopStretch(RectTransform rt, float left, float top, float right, float bottom)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 size)
        {
            // Do NOT add CanvasRenderer explicitly; Image/TMP will add it automatically.
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.sprite = sUiSprite; img.type = Image.Type.Sliced; img.color = new Color(1,1,1,0.02f);
            return go;
        }

        private static GameObject CreateEmpty(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static GameObject CreateScrollView(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>(); img.color = new Color(1,1,1,0.01f); img.sprite = sUiSprite; img.type = Image.Type.Sliced;

            var viewport = new GameObject("Viewport", typeof(RectTransform));
            viewport.transform.SetParent(go.transform, false);
            var vpRT = viewport.GetComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero; vpRT.anchorMax = Vector2.one; vpRT.offsetMin = Vector2.zero; vpRT.offsetMax = Vector2.zero;

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var ctRT = content.GetComponent<RectTransform>();
            ctRT.anchorMin = new Vector2(0, 1);
            ctRT.anchorMax = new Vector2(0, 1);
            ctRT.pivot     = new Vector2(0, 1);
            ctRT.anchoredPosition = new Vector2(0, 0);
            ctRT.sizeDelta = new Vector2(10, 300);

            var sr = go.GetComponent<ScrollRect>();
            sr.viewport = viewport.GetComponent<RectTransform>();
            sr.content  = content.GetComponent<RectTransform>();

            return go;
        }

        private static Image CreateImage(string name, Transform parent, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.sprite = sUiSprite; img.type = Image.Type.Sliced;
            return img;
        }

        private static TextMeshProUGUI CreateTMP(string name, Transform parent, string text, float size, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = align;
            if (sTmpFont) tmp.font = sTmpFont;
            return tmp;
        }

        private static void SetSerialized(UnityEngine.Object target, string fieldName, UnityEngine.Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null) { Debug.LogWarning($"[CaseBoardBuilder] Field '{fieldName}' not found on {target.GetType().Name}."); return; }
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetArray(UnityEngine.Object target, string fieldName, UnityEngine.Object[] array)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null) { Debug.LogWarning($"[CaseBoardBuilder] Field '{fieldName}' not found."); return; }
            prop.arraySize = array.Length;
            for (int i = 0; i < array.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = array[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void TrySetField(Component comp, string fieldName, UnityEngine.Object value)
        {
            var so = new SerializedObject(comp);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
#endif
