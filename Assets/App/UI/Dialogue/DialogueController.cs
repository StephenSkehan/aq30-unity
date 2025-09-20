using UnityEngine;
using UnityEngine.UI;
using System;

#if TMP_PRESENT
using TMPro;
#endif

public class DialogueController : MonoBehaviour
{
#if TMP_PRESENT
    public TMP_Text Speaker;
    public TMP_Text Body;
    public TMP_Text[] ChoiceLabels;
#else
    public Text Speaker;
    public Text Body;
    public Text[] ChoiceLabels;
#endif
    public Button AdvanceArea;
    public Button[] ChoiceButtons;

    public event Action AdvanceClicked;
    public event Action<int> ChoiceClicked;

    void Awake(){
        if (AdvanceArea != null) AdvanceArea.onClick.AddListener(() => AdvanceClicked?.Invoke());
        if (ChoiceButtons != null)
            for (int i=0;i<ChoiceButtons.Length;i++){
                int idx = i;
                ChoiceButtons[i].onClick.AddListener(() => ChoiceClicked?.Invoke(idx));
            }
        ShowChoices(false);
    }

    public void BindNode(CaseGraph.Node n){
        if (Speaker) Speaker.text = n != null ? n.speaker : "";
        if (Body)    Body.text    = n != null ? n.line    : "";
        if (n != null && n.choices != null && n.choices.Length > 0){
            ShowChoices(true);
            for (int i=0;i<ChoiceButtons.Length;i++){
                bool on = i < n.choices.Length;
                ChoiceButtons[i].gameObject.SetActive(on);
                if (on && ChoiceLabels != null && i < ChoiceLabels.Length)
                    ChoiceLabels[i].text = n.choices[i].text;
            }
        } else {
            ShowChoices(false);
        }
    }

    void ShowChoices(bool on){
        if (ChoiceButtons == null) return;
        foreach (var b in ChoiceButtons) if (b) b.gameObject.SetActive(on);
        if (AdvanceArea) AdvanceArea.gameObject.SetActive(!on);
    }
}
