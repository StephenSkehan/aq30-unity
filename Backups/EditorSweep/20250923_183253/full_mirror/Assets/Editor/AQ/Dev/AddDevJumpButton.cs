using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
#if TMP_PRESENT || UNITY_TEXTMESHPRO || TEXTMESHPRO_UGUI
using TMPro;
#endif

namespace AQ.EditorTools.Dev
{
    public static class AddDevJumpButton
    {
        [MenuItem("AQ/Dev/Add 'Go To Merge Board' Button")]
        public static void Run()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play Mode", "Exit Play Mode first.", "OK");
                return;
            }

            // ---- 1) Canvas (create if missing)
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (!canvas)
            {
                var goCanvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = goCanvas.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = goCanvas.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;

                Undo.RegisterCreatedObjectUndo(goCanvas, "Create Canvas");
            }

            // ---- 2) EventSystem (prefer Input System UI module)
            if (!Object.FindFirstObjectByType<EventSystem>())
            {
#if ENABLE_INPUT_SYSTEM
                var esGo = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
#else
                var esGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
#endif
                Undo.RegisterCreatedObjectUndo(esGo, "Create EventSystem");
            }

            // ---- 3) Button root + QuickNavButton (namespace-free component)
            var btnGO = new GameObject(
                "Btn_GoToMergeBoard",
                typeof(RectTransform), typeof(Image), typeof(Button), typeof(QuickNavButton)
            );
            Undo.RegisterCreatedObjectUndo(btnGO, "Create Dev Jump Button");
            btnGO.transform.SetParent(canvas.transform, false);

            // Place top-right
            var rt = (RectTransform)btnGO.transform;
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot     = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-24, -24);
            rt.sizeDelta = new Vector2(280, 96);

            btnGO.GetComponent<Image>().color = new Color(0.12f, 0.48f, 0.75f, 0.9f);

            // ---- 4) Label (TMP if available, else legacy Text)
#if TMP_PRESENT || UNITY_TEXTMESHPRO || TEXTMESHPRO_UGUI
            var labelGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGO.transform.SetParent(btnGO.transform, false);
            var tmpro = labelGO.GetComponent<TextMeshProUGUI>();
            tmpro.text = "Go To Merge Board";
            tmpro.alignment = TextAlignmentOptions.Center;
            tmpro.enableAutoSizing = true;
            tmpro.fontSizeMin = 18;
            tmpro.fontSizeMax = 48;
            var lrt = (RectTransform)labelGO.transform;
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
#else
            var labelGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            labelGO.transform.SetParent(btnGO.transform, false);
            var txt = labelGO.GetComponent<Text>();
            txt.text = "Go To Merge Board";
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            var lrt = (RectTransform)labelGO.transform;
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
#endif

            // ---- 5) Persistent onClick wiring → QuickNavButton.GoNow()
            var qnb = btnGO.GetComponent<QuickNavButton>();
            qnb.sceneName = "Case_Board_Portrait";

            var btn = btnGO.GetComponent<Button>();
            while (btn.onClick.GetPersistentEventCount() > 0)
                UnityEventTools.RemovePersistentListener(btn.onClick, 0);
            UnityEventTools.AddPersistentListener(btn.onClick, qnb.GoNow);

            EnsureSceneInBuild("Assets/Scenes/Case/Case_Board_Portrait.unity");

            Selection.activeGameObject = btnGO;
            Debug.Log("[AQ Dev] Added 'Go To Merge Board' button (top-right) and wired onClick.");
        }

        private static void EnsureSceneInBuild(string assetPath)
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(assetPath);
            if (!sceneAsset) return;

            foreach (var s in EditorBuildSettings.scenes)
                if (s.path == assetPath) return;

            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes)
            {
                new EditorBuildSettingsScene(assetPath, true)
            };
            EditorBuildSettings.scenes = list.ToArray();
            Debug.Log("[AQ Dev] Added to Build Settings: " + assetPath);
        }
    }
}
