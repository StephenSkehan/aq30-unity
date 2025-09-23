#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using AQ.App.CaseFlow; // <-- correct namespace for CaseFlowGateMB / CaseFlowAdvanceOnEventMB

namespace AQ.EditorTools
{
    public static class FixResolutionRootTool
    {
        [MenuItem("AQ/CaseFlow/Fix ResolutionRoot Now")]
        public static void FixNow()
        {
            // 1) Locate ResolutionRoot in the open scene
            var res = Resources.FindObjectsOfTypeAll<RectTransform>()
                               .FirstOrDefault(rt => rt.gameObject.scene.isLoaded &&
                                                     rt.gameObject.name == "ResolutionRoot");
            if (res == null)
            {
                Debug.LogError("[ResolutionUI] Could not find a GameObject named 'ResolutionRoot' in the scene.");
                return;
            }

            // 2) Pick a Canvas – prefer the one that contains DialoguePanel, else any, else create one
            Canvas canvas = null;
            var dialogue = GameObject.Find("DialoguePanel");
            if (dialogue != null) canvas = dialogue.GetComponentInParent<Canvas>(true);

#if UNITY_6000_0_OR_NEWER
            if (canvas == null) canvas = Object.FindAnyObjectByType<Canvas>();
#else
            if (canvas == null) canvas = Object.FindObjectOfType<Canvas>();
#endif
            if (canvas == null)
            {
                var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = go.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

#if UNITY_6000_0_OR_NEWER
                if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
#else
                if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
#endif
                {
                    new GameObject("EventSystem",
                        typeof(UnityEngine.EventSystems.EventSystem),
                        typeof(UnityEngine.EventSystems.StandaloneInputModule));
                }
                Debug.Log("[ResolutionUI] No Canvas found. Created a new Canvas + EventSystem.");
            }

            // 3) Reparent under Canvas and stretch full screen; bring to front
            Undo.SetTransformParent(res, canvas.transform, "Reparent ResolutionRoot under Canvas");
            StretchFullScreen(res);
            res.SetAsLastSibling();

            // Ensure it has a dark background
            var img = res.GetComponent<Image>() ?? Undo.AddComponent<Image>(res.gameObject);
            img.color = new Color(0f, 0f, 0f, 0.63f);

            // 4) Ensure CaseFlow components exist
            var gate = res.GetComponent<CaseFlowGateMB>() ?? Undo.AddComponent<CaseFlowGateMB>(res.gameObject);
            var adv  = res.GetComponent<CaseFlowAdvanceOnEventMB>() ?? Undo.AddComponent<CaseFlowAdvanceOnEventMB>(res.gameObject);

            // Configure gate to show at index==2, polling each frame
            ConfigureGate(gate, requiredIndex: 2, preferEnumName: "AtIndex");

            EditorUtility.SetDirty(res.gameObject);
            Debug.Log("[ResolutionUI] ResolutionRoot repaired: parented under Canvas, stretched, on top, gate=Index==2.");
            Selection.activeGameObject = res.gameObject;
        }

        static void StretchFullScreen(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        // Configure via SerializedObject so we don’t rely on exact field names across refactors
        static void ConfigureGate(MonoBehaviour gate, int requiredIndex, string preferEnumName)
        {
            var so = new SerializedObject(gate);

            var idx   = so.FindProperty("requiredIndex") ?? so.FindProperty("_requiredIndex") ?? so.FindProperty("index");
            var poll  = so.FindProperty("pollEveryFrame") ?? so.FindProperty("_pollEveryFrame");
            var tgt   = so.FindProperty("target") ?? so.FindProperty("_target");
            var mode  = so.FindProperty("mode") ?? so.FindProperty("_mode") ??
                        so.FindProperty("when") ?? so.FindProperty("_when") ??
                        so.FindProperty("gateMode") ?? so.FindProperty("_gateMode");

            if (idx  != null) idx.intValue  = requiredIndex;
            if (poll != null) poll.boolValue = true;
            if (tgt  != null) tgt.objectReferenceValue = null; // default to self if supported

            if (mode != null && mode.propertyType == SerializedPropertyType.Enum)
            {
                var names = mode.enumDisplayNames;
                var pick = System.Array.FindIndex(names, n =>
                    string.Equals(n, preferEnumName, System.StringComparison.OrdinalIgnoreCase) ||
                    n.IndexOf("Index", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    n.IndexOf("Equal", System.StringComparison.OrdinalIgnoreCase) >= 0);
                if (pick >= 0) mode.enumValueIndex = pick;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
