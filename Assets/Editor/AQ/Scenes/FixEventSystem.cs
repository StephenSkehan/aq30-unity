#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AQ.EditorTools.Scenes
{
    public static class FixEventSystem
    {
        [MenuItem("AQ/Scenes/Fix EventSystem (use InputSystemUIInputModule)")]
        public static void Fix()
        {
            var es = Object.FindFirstObjectByType<EventSystem>();
            if (es == null)
            {
                es = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
            }

            // Remove Standalone if present
            var old = es.GetComponent<StandaloneInputModule>();
            if (old) Object.DestroyImmediate(old);

            // Add InputSystemUIInputModule via reflection so we don't require a hard compile-time ref
            var inputSysType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSysType == null)
            {
                Debug.LogError("Input System package not found or not imported. Install Unity Input System.");
                return;
            }
            if (es.GetComponent(inputSysType) == null)
            {
                es.gameObject.AddComponent(inputSysType);
                Debug.Log("[FixEventSystem] Added InputSystemUIInputModule.");
            }
            else
            {
                Debug.Log("[FixEventSystem] InputSystemUIInputModule already present.");
            }
        }
    }
}
#endif
