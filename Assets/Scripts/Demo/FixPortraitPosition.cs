using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editor script to fix Portrait positioning in the dialogue panel.
/// Attach to any GameObject and use the context menu.
/// </summary>
public class FixPortraitPosition : MonoBehaviour
{
    [ContextMenu("Fix Portrait Position")]
    void FixPosition()
    {
        Debug.Log("========== FIXING PORTRAIT POSITION ==========");
        
        // Find Portrait GameObject
        GameObject portrait = GameObject.Find("Portrait");
        
        if (portrait == null)
        {
            // Try alternate search
            Transform dialoguePanel = GameObject.Find("DialoguePanel")?.transform;
            if (dialoguePanel != null)
            {
                portrait = dialoguePanel.Find("Portrait")?.gameObject;
            }
        }
        
        if (portrait == null)
        {
            Debug.LogError("❌ Could not find Portrait GameObject!");
            Debug.LogError("   Make sure Portrait exists as a child of DialoguePanel");
            return;
        }
        
        Debug.Log($"✓ Found Portrait: {portrait.name}");
        
        RectTransform rt = portrait.GetComponent<RectTransform>();
        if (rt == null)
        {
            Debug.LogError("❌ Portrait has no RectTransform!");
            return;
        }
        
        // Show current settings
        Debug.Log($"Current anchor: min={rt.anchorMin}, max={rt.anchorMax}");
        Debug.Log($"Current position: {rt.anchoredPosition}");
        Debug.Log($"Current size: {rt.sizeDelta}");
        
        // Fix to Middle-Left anchor
        rt.anchorMin = new Vector2(0f, 0.5f);  // Left, Middle
        rt.anchorMax = new Vector2(0f, 0.5f);  // Left, Middle
        rt.pivot = new Vector2(0.5f, 0.5f);    // Center pivot
        
        // Position on the left side with padding
        rt.anchoredPosition = new Vector2(150f, 0f);  // 150px from left, centered vertically
        
        // Set size
        rt.sizeDelta = new Vector2(200f, 200f);
        
        Debug.Log($"✓ Fixed anchor: min={rt.anchorMin}, max={rt.anchorMax}");
        Debug.Log($"✓ Fixed position: {rt.anchoredPosition}");
        Debug.Log($"✓ Fixed size: {rt.sizeDelta}");
        
        // Check Image component
        Image img = portrait.GetComponent<Image>();
        if (img == null)
        {
            Debug.LogError("❌ Portrait has no Image component!");
            return;
        }
        
        Debug.Log($"Image sprite: {(img.sprite ? img.sprite.name : "NULL")}");
        Debug.Log($"Image color: {img.color}");
        
        // Ensure visible
        if (img.color.a < 0.1f)
        {
            Debug.LogWarning("⚠ Image alpha was too low, setting to 1");
            img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
        }
        
        // Ensure active
        if (!portrait.activeSelf)
        {
            Debug.LogWarning("⚠ Portrait was inactive, activating...");
            portrait.SetActive(true);
        }
        
        // Check layer order
        Canvas portraitCanvas = portrait.GetComponent<Canvas>();
        if (portraitCanvas != null)
        {
            Debug.LogWarning("⚠ Portrait has a Canvas component - this might cause issues!");
            Debug.LogWarning("   Consider removing the Canvas component from Portrait");
        }
        
        Debug.Log("==========================================");
        Debug.Log("✓ Portrait should now be visible on the left side!");
        Debug.Log("  Enter Play Mode to test.");
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(portrait);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(portrait.scene);
        #endif
    }
    
    [ContextMenu("Debug Portrait Info")]
    void DebugPortraitInfo()
    {
        Debug.Log("========== PORTRAIT DEBUG INFO ==========");
        
        GameObject portrait = GameObject.Find("Portrait");
        if (portrait == null)
        {
            Transform dialoguePanel = GameObject.Find("DialoguePanel")?.transform;
            if (dialoguePanel != null)
            {
                portrait = dialoguePanel.Find("Portrait")?.gameObject;
            }
        }
        
        if (portrait == null)
        {
            Debug.LogError("❌ Could not find Portrait!");
            return;
        }
        
        RectTransform rt = portrait.GetComponent<RectTransform>();
        Debug.Log($"GameObject: {portrait.name}");
        Debug.Log($"Active: {portrait.activeSelf}");
        Debug.Log($"ActiveInHierarchy: {portrait.activeInHierarchy}");
        Debug.Log($"Layer: {LayerMask.LayerToName(portrait.layer)}");
        Debug.Log($"Parent: {(portrait.transform.parent ? portrait.transform.parent.name : "NULL")}");
        
        if (rt != null)
        {
            Debug.Log($"AnchorMin: {rt.anchorMin}");
            Debug.Log($"AnchorMax: {rt.anchorMax}");
            Debug.Log($"Pivot: {rt.pivot}");
            Debug.Log($"AnchoredPosition: {rt.anchoredPosition}");
            Debug.Log($"SizeDelta: {rt.sizeDelta}");
            Debug.Log($"LocalScale: {rt.localScale}");
            Debug.Log($"Rect: {rt.rect}");
            
            // Get world corners
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            Debug.Log($"World Corners:");
            Debug.Log($"  Bottom-Left: {corners[0]}");
            Debug.Log($"  Top-Left: {corners[1]}");
            Debug.Log($"  Top-Right: {corners[2]}");
            Debug.Log($"  Bottom-Right: {corners[3]}");
        }
        
        Image img = portrait.GetComponent<Image>();
        if (img != null)
        {
            Debug.Log($"Image.sprite: {(img.sprite ? img.sprite.name : "NULL")}");
            Debug.Log($"Image.color: {img.color}");
            Debug.Log($"Image.raycastTarget: {img.raycastTarget}");
            Debug.Log($"Image.enabled: {img.enabled}");
        }
        
        Canvas canvas = portrait.GetComponent<Canvas>();
        if (canvas != null)
        {
            Debug.LogWarning($"⚠ Portrait has Canvas component!");
            Debug.Log($"  Canvas.renderMode: {canvas.renderMode}");
            Debug.Log($"  Canvas.sortingOrder: {canvas.sortingOrder}");
        }
        
        Debug.Log("=========================================");
    }
}