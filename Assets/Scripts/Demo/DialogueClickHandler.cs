using UnityEngine;
using UnityEngine.EventSystems;
using AQ.App;

/// <summary>
/// Handles click/touch input on the dialogue panel to advance dialogue.
/// Works by programmatically triggering the AdvanceArea button's onClick event.
/// Attach this to the DialoguePanel GameObject.
/// </summary>
public class DialogueClickHandler : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    [Tooltip("Reference to the DialogueController - will auto-find if not assigned")]
    public DialogueController dialogueController;

    [Header("Settings")]
    [Tooltip("Require clicking outside of choice buttons to advance")]
    public bool blockClicksDuringChoices = true;

    [Header("Debug")]
    public bool verboseLogging = true;

    void Start()
    {
        Debug.Log("[DialogueClickHandler] START - Initializing...");
        
        // Auto-find DialogueController if not assigned
        if (dialogueController == null)
        {
            Debug.Log("[DialogueClickHandler] Searching for DialogueController...");
            
            dialogueController = GetComponent<DialogueController>();
            if (dialogueController != null)
                Debug.Log("[DialogueClickHandler] Found DialogueController on same GameObject");
            
            if (dialogueController == null)
            {
                dialogueController = GetComponentInChildren<DialogueController>();
                if (dialogueController != null)
                    Debug.Log("[DialogueClickHandler] Found DialogueController in children");
            }
            
            if (dialogueController == null)
            {
                Debug.LogError("[DialogueClickHandler] Could not find DialogueController! Please assign it manually.");
            }
        }
        else
        {
            Debug.Log("[DialogueClickHandler] DialogueController already assigned");
        }
        
        // Check if Image has Raycast Target enabled
        UnityEngine.UI.Image img = GetComponent<UnityEngine.UI.Image>();
        if (img != null)
        {
            Debug.Log($"[DialogueClickHandler] Image found - Raycast Target: {img.raycastTarget}");
            if (!img.raycastTarget)
            {
                Debug.LogWarning("[DialogueClickHandler] WARNING: Image.raycastTarget is FALSE! Clicks won't be detected. Please enable it.");
            }
        }
        else
        {
            Debug.LogWarning("[DialogueClickHandler] No Image component found! Clicks won't be detected without a raycast target.");
        }
        
        // Check EventSystem
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            Debug.LogError("[DialogueClickHandler] No EventSystem found in scene! UI clicks won't work.");
        }
        else
        {
            Debug.Log("[DialogueClickHandler] EventSystem found: " + eventSystem.name);
        }
        
        Debug.Log("[DialogueClickHandler] Initialization complete");
    }

    /// <summary>
    /// Called when the panel is clicked (Unity Event System)
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("[DialogueClickHandler] ===== CLICK DETECTED =====");
        Debug.Log($"[DialogueClickHandler] Click position: {eventData.position}");
        
        if (dialogueController == null)
        {
            Debug.LogError("[DialogueClickHandler] DialogueController is null! Cannot advance.");
            return;
        }
        Debug.Log("[DialogueClickHandler] DialogueController found");

        // Check if we should block clicks during choices
        if (blockClicksDuringChoices)
        {
            bool choicesActive = AreChoicesActive();
            Debug.Log($"[DialogueClickHandler] Checking choices - Active: {choicesActive}");
            
            if (choicesActive)
            {
                Debug.Log("[DialogueClickHandler] ⚠ Choices are active - click a choice button instead");
                return;
            }
        }

        // Check if AdvanceArea button exists
        if (dialogueController.AdvanceArea == null)
        {
            Debug.LogError("[DialogueClickHandler] AdvanceArea button is null!");
            return;
        }
        Debug.Log("[DialogueClickHandler] AdvanceArea button exists");

        // Check if AdvanceArea is active
        bool isActive = dialogueController.AdvanceArea.gameObject.activeInHierarchy;
        Debug.Log($"[DialogueClickHandler] AdvanceArea active: {isActive}");
        
        if (!isActive)
        {
            Debug.LogWarning("[DialogueClickHandler] ⚠ AdvanceArea button is not active - cannot advance");
            return;
        }

        // Check if button is interactable
        bool isInteractable = dialogueController.AdvanceArea.interactable;
        Debug.Log($"[DialogueClickHandler] AdvanceArea interactable: {isInteractable}");

        // Programmatically click the AdvanceArea button
        Debug.Log("[DialogueClickHandler] ✓ Invoking AdvanceArea.onClick...");
        dialogueController.AdvanceArea.onClick.Invoke();
        Debug.Log("[DialogueClickHandler] ✓ Click invoked successfully");
    }

    /// <summary>
    /// Check if any choice buttons are currently active/visible
    /// </summary>
    bool AreChoicesActive()
    {
        if (dialogueController.ChoiceButtons == null)
        {
            if (verboseLogging)
                Debug.Log("[DialogueClickHandler] ChoiceButtons array is null");
            return false;
        }

        int activeCount = 0;
        foreach (var button in dialogueController.ChoiceButtons)
        {
            if (button != null && button.gameObject.activeInHierarchy)
            {
                activeCount++;
            }
        }

        if (verboseLogging && activeCount > 0)
            Debug.Log($"[DialogueClickHandler] Found {activeCount} active choice buttons");

        return activeCount > 0;
    }

    /// <summary>
    /// Manual test method for inspector button
    /// </summary>
    [ContextMenu("Test Click")]
    public void TestClick()
    {
        Debug.Log("[DialogueClickHandler] Manual test click triggered");
        // Simulate a click
        PointerEventData fakeData = new PointerEventData(EventSystem.current);
        OnPointerClick(fakeData);
    }
}