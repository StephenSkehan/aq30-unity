using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Diagnostic tool to check Canvas configuration issues.
/// Attach to any GameObject and run in Play Mode.
/// </summary>
public class CanvasDiagnostics : MonoBehaviour
{
    void Start()
    {
        Debug.Log("========== CANVAS DIAGNOSTICS ==========");
        
        // Find all canvases in scene
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        Debug.Log($"Found {canvases.Length} Canvas(es) in scene");
        
        foreach (Canvas canvas in canvases)
        {
            DiagnoseCanvas(canvas);
        }
        
        Debug.Log("========================================");
    }
    
    void DiagnoseCanvas(Canvas canvas)
    {
        Debug.Log($"\n--- Canvas: {canvas.gameObject.name} ---");
        Debug.Log($"  Active: {canvas.gameObject.activeInHierarchy}");
        Debug.Log($"  Enabled: {canvas.enabled}");
        Debug.Log($"  Render Mode: {canvas.renderMode}");
        
        // Check GraphicRaycaster
        GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            Debug.LogError($"  ⚠ PROBLEM: Canvas '{canvas.name}' is MISSING GraphicRaycaster component!");
            Debug.LogError($"     FIX: Add Component → Graphic Raycaster to this Canvas");
        }
        else
        {
            Debug.Log($"  ✓ GraphicRaycaster: Present (enabled={raycaster.enabled})");
            if (!raycaster.enabled)
            {
                Debug.LogWarning($"  ⚠ GraphicRaycaster is DISABLED!");
            }
        }
        
        // Check Camera (for non-overlay modes)
        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Camera cam = canvas.worldCamera;
            if (cam == null)
            {
                Debug.LogError($"  ⚠ PROBLEM: Canvas is '{canvas.renderMode}' but has NO Camera assigned!");
                Debug.LogError($"     FIX: Assign Main Camera to Canvas.worldCamera");
            }
            else
            {
                Debug.Log($"  Camera: {cam.name}");
            }
        }
        
        // Check CanvasScaler
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            Debug.Log($"  CanvasScaler: {scaler.uiScaleMode}");
        }
        
        // Check for DialoguePanel child
        Transform dialoguePanel = canvas.transform.Find("DialoguePanel");
        if (dialoguePanel != null)
        {
            Debug.Log($"  ✓ Found DialoguePanel child");
            
            // Check if DialoguePanel is active
            if (!dialoguePanel.gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"  ⚠ DialoguePanel is INACTIVE!");
            }
            
            // Check for Image component with raycast target
            UnityEngine.UI.Image img = dialoguePanel.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                Debug.Log($"  DialoguePanel Image: raycastTarget={img.raycastTarget}");
            }
            else
            {
                Debug.LogWarning($"  ⚠ DialoguePanel has no Image component!");
            }
        }
        else
        {
            Debug.LogWarning($"  DialoguePanel child not found");
        }
    }
}