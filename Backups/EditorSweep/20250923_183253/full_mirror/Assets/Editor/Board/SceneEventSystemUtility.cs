using UnityEditor; using UnityEngine; using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public static class SceneEventSystemUtility {
    [MenuItem("AQ/Board/Ensure EventSystem (match project input)")]
    public static void Ensure(){
        var es = Object.FindFirstObjectByType<EventSystem>() ?? new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();

        // Remove both modules; we will add the right one
        var legacy = es.GetComponent<StandaloneInputModule>();
        if(legacy) Object.DestroyImmediate(legacy);
#if ENABLE_INPUT_SYSTEM
        var newMod = es.GetComponent<InputSystemUIInputModule>();
        if(!newMod) es.gameObject.AddComponent<InputSystemUIInputModule>();
        Debug.Log("[Board] Using InputSystemUIInputModule.");
#else
        if(!es.GetComponent<StandaloneInputModule>()) es.gameObject.AddComponent<StandaloneInputModule>();
        Debug.Log("[Board] Using StandaloneInputModule (legacy).");
#endif
    }
}
