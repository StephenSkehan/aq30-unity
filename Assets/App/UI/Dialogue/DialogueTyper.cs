using UnityEngine;
using System.Collections;
#if TMP_PRESENT
using TMPro;
#endif
using UnityEngine.UI;

public class DialogueTyper : MonoBehaviour
{
    public float charsPerSecond = 45f;
    public float punctuationPause = 0.06f;

#if TMP_PRESENT
    TMP_Text tmp;
#endif
    Text ugui;
    Coroutine typing;
    string fullText = "";
    int shown = 0;
    bool isTyping = false;

    public bool IsTyping => isTyping;

    void Awake(){
#if TMP_PRESENT
        tmp = GetComponent<TMP_Text>();
#endif
        ugui = GetComponent<Text>();
    }

    public void SetInstant(string text){
        StopTyping();
        fullText = text ?? "";
        shown = fullText.Length;
        ApplyVisible(shown);
        isTyping = false;
    }

    public void StartTyping(string text){
        StopTyping();
        fullText = text ?? "";
        shown = 0;
        ApplyVisible(0);
        typing = StartCoroutine(TypeRoutine());
    }

    public void Skip(){
        if(!isTyping) return;
        StopTyping();
        SetInstant(fullText);
    }

    public void StopTyping(){
        if(typing != null){ StopCoroutine(typing); typing = null; }
        isTyping = false;
    }

    IEnumerator TypeRoutine(){
        isTyping = true;
        if(string.IsNullOrEmpty(fullText)){ isTyping = false; yield break; }
        float baseDelay = (charsPerSecond > 0f) ? (1f / charsPerSecond) : 0f;
        while(shown < fullText.Length){
            shown++;
            ApplyVisible(shown);
            char c = fullText[shown-1];
            float delay = baseDelay;
            if(c=='.' || c=='!' || c=='?' ) delay += punctuationPause;
            float t = 0f;
            while(t < delay){ t += Time.unscaledDeltaTime; yield return null; }
        }
        isTyping = false;
        typing = null;
    }

    void ApplyVisible(int count){
#if TMP_PRESENT
        if(tmp){
            if(tmp.text != fullText) tmp.text = fullText;
            tmp.maxVisibleCharacters = count;
            return;
        }
#endif
        if(ugui){
            ugui.text = (count >= fullText.Length) ? fullText : fullText.Substring(0, count);
        }
    }

    void OnEnable(){
#if TMP_PRESENT
        if(tmp){ tmp.maxVisibleCharacters = shown; }
#endif
    }
}
