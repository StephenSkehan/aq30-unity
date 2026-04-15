using UnityEngine;
using UnityEngine.UI;
using AQ.App;

/// <summary>
/// Diagnostic tool to check portrait setup.
/// Attach to any GameObject and run in Play Mode after starting dialogue.
/// </summary>
public class PortraitDiagnostics : MonoBehaviour
{
    [Header("Manual Test")]
    public Sprite testSprite;
    
    void Start()
    {
        // Run diagnostics after a short delay (let dialogue system boot)
        Invoke("RunDiagnostics", 1f);
    }
    
    [ContextMenu("Run Portrait Diagnostics")]
    void RunDiagnostics()
    {
        Debug.Log("========== PORTRAIT DIAGNOSTICS ==========");
        
        // Find DialogueController
        DialogueController controller = FindFirstObjectByType<DialogueController>();
        
        if (controller == null)
        {
            Debug.LogError("❌ No DialogueController found in scene!");
            return;
        }
        
        Debug.Log($"✓ Found DialogueController on: {controller.gameObject.name}");
        
        // Check portraitImage field
        if (controller.portraitImage == null)
        {
            Debug.LogError("❌ DialogueController.portraitImage is NULL!");
            Debug.LogError("   FIX: Select DialoguePanel → DialogueController → Assign Portrait GameObject to 'Portrait Image' field");
            return;
        }
        
        Debug.Log($"✓ Portrait Image assigned: {controller.portraitImage.gameObject.name}");
        
        // Check if Portrait GameObject is active
        if (!controller.portraitImage.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("⚠ Portrait GameObject is INACTIVE!");
            Debug.Log("   Attempting to activate...");
            controller.portraitImage.gameObject.SetActive(true);
        }
        
        Debug.Log($"✓ Portrait is active: {controller.portraitImage.gameObject.activeInHierarchy}");
        
        // Check current sprite
        if (controller.portraitImage.sprite == null)
        {
            Debug.LogWarning("⚠ Portrait Image has NO sprite assigned!");
            Debug.Log("   This is normal before dialogue starts.");
            
            if (testSprite != null)
            {
                Debug.Log("   Assigning test sprite...");
                controller.portraitImage.sprite = testSprite;
                controller.portraitImage.gameObject.SetActive(true);
                Debug.Log("✓ Test sprite assigned! Check the Portrait in Game view.");
            }
        }
        else
        {
            Debug.Log($"✓ Portrait sprite: {controller.portraitImage.sprite.name}");
        }
        
        // Check RectTransform
        RectTransform rt = controller.portraitImage.GetComponent<RectTransform>();
        Debug.Log($"  Portrait size: {rt.rect.width} x {rt.rect.height}");
        Debug.Log($"  Portrait position: {rt.anchoredPosition}");
        
        // Check if Portrait is visible on screen
        if (rt.rect.width <= 0 || rt.rect.height <= 0)
        {
            Debug.LogWarning("⚠ Portrait has ZERO size! It won't be visible.");
        }
        
        // Check portraitAnimator
        if (controller.portraitAnimator == null)
        {
            Debug.LogWarning("⚠ Portrait Animator is NULL (emotions won't work)");
        }
        else
        {
            Debug.Log($"✓ Portrait Animator assigned: {controller.portraitAnimator.gameObject.name}");
            Debug.Log($"  Animator controller: {controller.portraitAnimator.runtimeAnimatorController?.name ?? "NONE"}");
        }
        
        // Find DialogueRunner
        DialogueRunner runner = FindFirstObjectByType<DialogueRunner>();
        if (runner != null && runner.Graph != null)
        {
            Debug.Log($"✓ DialogueRunner found with graph: {runner.Graph.name}");
            
            // Check first node for portrait
            if (runner.Graph.nodes != null && runner.Graph.nodes.Length > 0)
            {
                var firstNode = runner.Graph.nodes[0];
                if (firstNode.portrait == null)
                {
                    Debug.LogWarning($"⚠ First node '{firstNode.id}' has NO portrait sprite!");
                    Debug.LogWarning("   FIX: Select CaseGraph asset → Expand Nodes → Assign portrait sprite to each node");
                }
                else
                {
                    Debug.Log($"✓ First node '{firstNode.id}' has portrait: {firstNode.portrait.name}");
                }
            }
        }
        
        Debug.Log("==========================================");
    }
    
    [ContextMenu("Manually Set Test Sprite")]
    void SetTestSprite()
    {
        if (testSprite == null)
        {
            Debug.LogError("Please assign a test sprite in the inspector first!");
            return;
        }
        
        DialogueController controller = FindFirstObjectByType<DialogueController>();
        if (controller != null && controller.portraitImage != null)
        {
            controller.portraitImage.sprite = testSprite;
            controller.portraitImage.gameObject.SetActive(true);
            Debug.Log($"✓ Set test sprite: {testSprite.name}");
        }
    }
}