using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;   // <-- needed
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools
{
    public static class FixBoardDemoGates
    {
        [MenuItem("AQ/CaseFlow/Fix Board Demo Gates Now")]
        public static void Run()
        {
            var minigame   = GameObject.Find("Minigame_Scrub");
            var resolution = GameObject.Find("ResolutionRoot");

            if (!minigame)   Debug.LogWarning("Minigame_Scrub not found");
            if (!resolution) Debug.LogWarning("ResolutionRoot not found");

            // 1) Gate config: Minigame @ step 1, Resolution @ step 2
            SetGate(minigame,   gateMode:"AtIndex", requiredIndex:1);
            SetGate(resolution, gateMode:"AtIndex", requiredIndex:2);

            // 2) Resolution overlay shouldn't block clicks if it shows early
            if (resolution)
            {
                var img = resolution.GetComponentInChildren<Image>(true);
                if (img) img.raycastTarget = false;

                var cg = resolution.GetComponent<CanvasGroup>();
                if (cg) cg.blocksRaycasts = false;

                // Keep it hidden until step 2
                resolution.SetActive(false);

                // And make sure it's on top when shown
                var overlayCanvas = resolution.GetComponent<Canvas>() ?? resolution.AddComponent<Canvas>();
                overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                overlayCanvas.overrideSorting = true;
                overlayCanvas.sortingOrder = 9000;
            }

            // 3) Make the minigame button definitely clickable
            if (minigame)
            {
                var mgCanvas = minigame.GetComponentInChildren<Canvas>(true) ?? minigame.AddComponent<Canvas>();
                mgCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                mgCanvas.overrideSorting = true;
                mgCanvas.sortingOrder = 100;
            }

            // 4) Save/mark scene dirty so changes persist
            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();

            Debug.Log("[FixBoardDemoGates] Applied: Minigame=step 1, Resolution=step 2; click-through safeguards set.");
        }

        static void SetGate(GameObject go, string gateMode, int requiredIndex)
        {
            if (!go) return;
            var gate = go.GetComponents<Component>().FirstOrDefault(c => c && c.GetType().Name == "CaseFlowGateMB");
            if (!gate) { Debug.LogWarning($"[{go?.name}] CaseFlowGateMB not found"); return; }

            var t = gate.GetType();

            // mode (enum)
            var modeField = t.GetField("mode", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)
                          ?? t.GetField("Mode", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
            if (modeField != null && modeField.FieldType.IsEnum)
                modeField.SetValue(gate, Enum.Parse(modeField.FieldType, gateMode));

            var modeProp = t.GetProperty("Mode", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
            if (modeProp != null && modeProp.PropertyType.IsEnum && modeProp.CanWrite)
                modeProp.SetValue(gate, Enum.Parse(modeProp.PropertyType, gateMode));

            // requiredIndex (int)
            var idxField = t.GetField("requiredIndex", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)
                        ?? t.GetField("RequiredIndex", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
            if (idxField != null && idxField.FieldType == typeof(int))
                idxField.SetValue(gate, requiredIndex);

            var idxProp = t.GetProperty("RequiredIndex", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
            if (idxProp != null && idxProp.PropertyType == typeof(int) && idxProp.CanWrite)
                idxProp.SetValue(gate, requiredIndex);
        }
    }
}
