using UnityEngine;
using UnityEngine.UI;
using AQ.App;

public class StartDialogueButton : MonoBehaviour
{
    public DialogueRunner dialogueRunner;
    
    void Start()
    {
        if (dialogueRunner == null)
        {
            dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        }

        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(StartDialogue);
        }
    }

    void StartDialogue()
    {
        if (dialogueRunner != null && dialogueRunner.Graph != null)
        {
            dialogueRunner.Panel.gameObject.SetActive(true);
            dialogueRunner.JumpTo(dialogueRunner.Graph.startId);
        }
        else
        {
            Debug.LogError("DialogueRunner or Graph not assigned!");
        }
    }
}