using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Diagnostic tool to see what UI elements are blocking clicks.
/// Attach this to ANY GameObject in the scene.
/// </summary>
public class ClickDebugger : MonoBehaviour
{
    void Update()
    {
        // On mouse click, show what UI elements are under the cursor
        if (Input.GetMouseButtonDown(0))
        {
            DebugClickTarget();
        }
    }

    void DebugClickTarget()
    {
        Debug.Log("========== CLICK DEBUG ==========");
        
        // Check if we have an EventSystem
        if (EventSystem.current == null)
        {
            Debug.LogError("No EventSystem found!");
            return;
        }

        // Create pointer data
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        // Raycast to find all UI elements under cursor
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        Debug.Log($"Mouse Position: {Input.mousePosition}");
        Debug.Log($"Found {results.Count} UI elements under cursor:");

        if (results.Count == 0)
        {
            Debug.LogWarning("No UI elements detected under cursor!");
            Debug.LogWarning("This means clicks are going to the 3D world instead of UI");
        }

        // List all UI elements in order (top to bottom)
        for (int i = 0; i < results.Count; i++)
        {
            RaycastResult result = results[i];
            GameObject go = result.gameObject;
            
            // Build full hierarchy path
            string path = GetFullPath(go);
            
            // Check for click handlers
            bool hasClickHandler = go.GetComponent<IPointerClickHandler>() != null;
            bool hasButton = go.GetComponent<UnityEngine.UI.Button>() != null;
            bool hasImage = go.GetComponent<UnityEngine.UI.Image>() != null;
            
            string handlers = "";
            if (hasClickHandler) handlers += "[IPointerClickHandler] ";
            if (hasButton) handlers += "[Button] ";
            if (hasImage)
            {
                var img = go.GetComponent<UnityEngine.UI.Image>();
                handlers += $"[Image:RaycastTarget={img.raycastTarget}] ";
            }

            Debug.Log($"  [{i}] {path} {handlers}");
        }

        Debug.Log("=================================");
    }

    string GetFullPath(GameObject go)
    {
        string path = go.name;
        Transform current = go.transform.parent;
        
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        
        return path;
    }
}