#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools
{
    public static class ResolutionUiBuilder
    {
        [MenuItem("AQ/CaseFlow/Create Resolution UI")]
        public static void CreateResolutionUi()
        {
            // 1) Find or create a Canvas. Prefer the one that contains DialoguePanel.
            var canvas = FindPreferredCanvas();
            if (canvas == null)
            {
                canvas = CreateCanvasWithEventSystem();
                Debug.Log("[ResolutionUI] No Canvas found. Created a new Canvas + EventSystem.");
            }

            // 2) Avoid duplicating an existing UI ResolutionRoot
            var existingUi = canvas.GetComponentsInChildren<RectTransform>(true)
                                   .FirstOrDefault(rt => rt.gameObject.name == "ResolutionRoot");
            if (existingUi != null)
            {
                Selection.activeGameObject = existingUi.gameObject;
                Debug.Log("[ResolutionUI] Found existing UI ResolutionRoot under Canvas; selecting it.");
                return;
            }

            // 3) Build ResolutionRoot UI container (full-screen overlay)
            var resolutionRoot = CreateUiObject("ResolutionRoot", canvas.transform);
            StretchFullScreen(resolutionRoot);
            var bg = resolutionRoot.gameObject.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.63f); // semi-transparent black

            // 4) Add CaseFlow components without assuming field/enums
            var gate = resolutionRoot.gameObject.AddComponent<AQ.App.CaseFlow.CaseFlowGateMB>();
            var adv  = resolutionRoot.gameObject.AddComponent<AQ.App.CaseFlow.CaseFlowAdvanceOnEventMB>();

            // Try to set typical serialized fields safely (whatever exists on your GateMB)
            TryConfigureGate(gate, requiredIndex: 2, preferEnumName: "AtIndex");

            // 5) Create a centered panel with title/body/button (UGUI only)
            var panel = CreateUiObject("ResolutionPanel", resolutionRoot);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(640, 360);
            panel.anchoredPosition = Vector2.zero;

            var img = panel.gameObject.AddComponent<Image>();
            img.color = new Color(0.12f, 0.12f, 0.18f, 0.92f);

            var vlg = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.spacing = 16;
            vlg.childControlHeight = vlg.childControlWidth = false;
            vlg.childForceExpandHeight = vlg.childForceExpandWidth = false;

            // Title (UGUI Text)
            var title = CreateUiText(panel, "TitleText", "Case Solved!", 48);

            // Body (UGUI Text)
            var body = CreateUiText(panel, "BodyText", "Thanks, Ally. Continue to wrap up.", 28);

            // Continue button
            var btnRT  = CreateUiObject("ContinueButton", panel);
            btnRT.sizeDelta = new Vector2(220, 64);
            var btnImg = btnRT.gameObject.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.6f, 0.9f, 1f);
            var button = btnRT.gameObject.AddComponent<Button>();

            var btnLabel = CreateUiText(btnRT, "Label", "Continue", 30);
            // Stretch label to button rect
            var lblRT = btnLabel.GetComponent<RectTransform>();
            lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one;
            lblRT.offsetMin = Vector2.zero; lblRT.offsetMax = Vector2.zero;

            // Wire button → Advance() (persistent)
            UnityEventTools.AddPersistentListener(button.onClick, adv.Advance);

            // 6) Save a prefab for reuse
            EnsureFolder("Assets/App/UI/Resolution");
            var prefabPath = "Assets/App/UI/Resolution/ResolutionRoot.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(resolutionRoot.gameObject, prefabPath, InteractionMode.AutomatedAction);
            Debug.Log($"[ResolutionUI] Created and saved prefab at {prefabPath}. Gated at CaseFlow index 2. Button wired to Advance().");

            Selection.activeGameObject = resolutionRoot.gameObject;
        }

        // ---- Gate configuration that adapts to your serialized field names ----
        static void TryConfigureGate(AQ.App.CaseFlow.CaseFlowGateMB gate, int requiredIndex, string preferEnumName)
        {
            var so = new SerializedObject(gate);

            // Common field names we try in order
            var requiredIndexProp = so.FindProperty("requiredIndex") ?? so.FindProperty("_requiredIndex") ?? so.FindProperty("index");
            var pollEveryFrameProp = so.FindProperty("pollEveryFrame") ?? so.FindProperty("_pollEveryFrame");
            var targetProp         = so.FindProperty("target") ?? so.FindProperty("_target");

            // Possible enum fields for mode-like behavior
            var modeProp = so.FindProperty("mode") ??
                           so.FindProperty("_mode") ??
                           so.FindProperty("gateMode") ??
                           so.FindProperty("_gateMode");

            if (requiredIndexProp != null) requiredIndexProp.intValue = requiredIndex;
            if (pollEveryFrameProp != null) pollEveryFrameProp.boolValue = true;
            if (targetProp != null) targetProp.objectReferenceValue = null;

            if (modeProp != null && modeProp.propertyType == SerializedPropertyType.Enum)
            {
                // Choose enum value by display name if present, otherwise leave default
                var names = modeProp.enumDisplayNames;
                var idx = System.Array.FindIndex(names, n =>
                    string.Equals(n, preferEnumName, System.StringComparison.OrdinalIgnoreCase) ||
                    n.IndexOf("Index", System.StringComparison.OrdinalIgnoreCase) >= 0);
                if (idx >= 0) modeProp.enumValueIndex = idx;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(gate);
        }

        // ---- Canvas / UI helpers ----
        static Canvas FindPreferredCanvas()
        {
            // Prefer Canvas parenting DialoguePanel (your scene has DialoguePanel at root)
            var dialoguePanel = GameObject.Find("DialoguePanel");
            if (dialoguePanel != null)
            {
                var inParent = dialoguePanel.GetComponentInParent<Canvas>(true);
                if (inParent != null) return inParent;
            }

            // Unity 6.2+ API (no obsolete warning)
            var any = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).FirstOrDefault();
            return any;
        }

        static Canvas CreateCanvasWithEventSystem()
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;

            var es = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (es == null)
            {
                var esGO = new GameObject("EventSystem",
                    typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.EventSystems.StandaloneInputModule));
                Undo.RegisterCreatedObjectUndo(esGO, "Create EventSystem");
            }

            Undo.RegisterCreatedObjectUndo(go, "Create Canvas");
            return c;
        }

        static RectTransform CreateUiObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            go.transform.SetParent(parent, false);
            return rt;
        }

        static Text CreateUiText(Transform parent, string name, string text, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            go.transform.SetParent(parent, false);

            var t = go.GetComponent<Text>();
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;

            // Unity 6: Arial.ttf is gone → use LegacyRuntime.ttf; fallback to an OS font if needed.
            Font builtin = null;
            try { builtin = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { /* ignore */ }
            if (builtin == null)
            {
                try { builtin = Font.CreateDynamicFontFromOSFont("Arial", fontSize); } catch { /* ignore */ }
            }
            t.font = builtin;

            return t;
        }

        static void StretchFullScreen(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static void EnsureFolder(string folder)
        {
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        }
    }
}
#endif
