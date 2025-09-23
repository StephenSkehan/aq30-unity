using UnityEditor; using UnityEngine; using UnityEngine.UI;
#if TMP_PRESENT
using TMPro;
#endif

public static class DialoguePanelPrefabMaker
{
    const string Path="Assets/UI/Dialogue/DialoguePanel.prefab";

    [MenuItem("AQ/Prefabs/Make DialoguePanel")]
    public static void Make(){
        // Canvas root for the panel
        var canvas = new GameObject("DialogueCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var c = canvas.GetComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080,1920);
        scaler.matchWidthOrHeight = 0.5f;

        // Panel root (taller than before for readability)
        var panel = new GameObject("DialoguePanel", typeof(RectTransform), typeof(Image), typeof(DialogueController));
        panel.transform.SetParent(canvas.transform, false);
        var prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.05f,0.06f);
        prt.anchorMax = new Vector2(0.95f,0.42f);
        prt.offsetMin = prt.offsetMax = Vector2.zero;
        var bg = panel.GetComponent<Image>(); bg.color = new Color(0,0,0,0.75f); bg.raycastTarget = true;

        // Speaker
#if TMP_PRESENT
        var spkGO = new GameObject("Speaker", typeof(RectTransform), typeof(TMP_Text));
        var spk = spkGO.GetComponent<TMP_Text>(); spk.fontSize = 40; spk.enableAutoSizing = true; spk.fontSizeMin = 28; spk.fontSizeMax = 44;
        spk.color = Color.white; spk.enableWordWrapping = false;
#else
        var spkGO = new GameObject("Speaker", typeof(RectTransform), typeof(Text));
        var spk = spkGO.GetComponent<Text>(); spk.fontSize = 36; spk.text = "Speaker"; spk.color = Color.white;
#endif
        spkGO.transform.SetParent(panel.transform,false);
        var srt = spkGO.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0.03f,0.64f); srt.anchorMax = new Vector2(0.97f,0.95f);
        srt.offsetMin = srt.offsetMax = Vector2.zero;

        // Body
#if TMP_PRESENT
        var bodyGO = new GameObject("Body", typeof(RectTransform), typeof(TMP_Text));
        var body = bodyGO.GetComponent<TMP_Text>(); body.fontSize = 30; body.enableAutoSizing = true; body.fontSizeMin = 26; body.fontSizeMax = 40;
        body.color = Color.white; body.enableWordWrapping = true;
#else
        var bodyGO = new GameObject("Body", typeof(RectTransform), typeof(Text));
        var body = bodyGO.GetComponent<Text>(); body.fontSize = 28; body.text = "Line goes here..."; body.color = Color.white; body.horizontalOverflow = HorizontalWrapMode.Wrap; body.verticalOverflow = VerticalWrapMode.Truncate;
#endif
        bodyGO.transform.SetParent(panel.transform,false);
        var brt = bodyGO.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.03f,0.22f); brt.anchorMax = new Vector2(0.97f,0.62f);
        brt.offsetMin = brt.offsetMax = Vector2.zero;

        // Advance full-area button (active when no choices)
        var advGO = new GameObject("AdvanceArea", typeof(RectTransform), typeof(Button), typeof(Image));
        advGO.transform.SetParent(panel.transform,false);
        var art = advGO.GetComponent<RectTransform>();
        art.anchorMin = Vector2.zero; art.anchorMax = Vector2.one; art.offsetMin = art.offsetMax = Vector2.zero;
        var advImg = advGO.GetComponent<Image>(); advImg.color = new Color(1,1,1,0f); advImg.raycastTarget = true;

        // 2 choice buttons, bigger for thumb accuracy
        Button[] btns = new Button[2];
#if TMP_PRESENT
        TMP_Text[] labels = new TMP_Text[2];
#else
        UnityEngine.UI.Text[] labels = new UnityEngine.UI.Text[2];
#endif
        for(int i=0;i<2;i++){
            var btnGO = new GameObject("Choice_"+i, typeof(RectTransform), typeof(Button), typeof(Image));
            btnGO.transform.SetParent(panel.transform,false);
            var rt = btnGO.GetComponent<RectTransform>();
            // bottom row split in two
            rt.anchorMin = new Vector2(0.03f + 0.49f*i, 0.05f);
            rt.anchorMax = new Vector2(0.48f + 0.49f*i, 0.19f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var bimg = btnGO.GetComponent<Image>(); bimg.color = new Color(1,1,1,0.12f);

            var labelGO = new GameObject("Label", typeof(RectTransform)
#if TMP_PRESENT
                , typeof(TMP_Text)
#else
                , typeof(UnityEngine.UI.Text)
#endif
            );
            labelGO.transform.SetParent(btnGO.transform,false);
            var lrt = labelGO.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0.08f,0.15f); lrt.anchorMax = new Vector2(0.92f,0.85f);
            lrt.offsetMin = lrt.offsetMax = Vector2.zero;
#if TMP_PRESENT
            var ltxt = labelGO.GetComponent<TMP_Text>(); ltxt.fontSize = 22; ltxt.enableAutoSizing = true; ltxt.fontSizeMin = 18; ltxt.fontSizeMax = 26; ltxt.alignment = TextAlignmentOptions.Midline; ltxt.color = Color.white; labels[i]=ltxt;
#else
            var ltxt = labelGO.GetComponent<UnityEngine.UI.Text>(); ltxt.fontSize = 22; ltxt.alignment = TextAnchor.MiddleCenter; ltxt.color = Color.white; labels[i]=ltxt;
#endif
            btns[i] = btnGO.GetComponent<Button>();
            btnGO.SetActive(false); // controller toggles
        }

        // Hook controller
        var ctl = panel.GetComponent<DialogueController>();
        ctl.AdvanceArea = advGO.GetComponent<Button>();
        ctl.ChoiceButtons = btns;
        ctl.ChoiceLabels = labels;
#if TMP_PRESENT
        ctl.Speaker = spk; ctl.Body = body;
#else
        ctl.Speaker = spk; ctl.Body = body;
#endif

        if(!AssetDatabase.IsValidFolder("Assets/UI")) AssetDatabase.CreateFolder("Assets","UI");
        if(!AssetDatabase.IsValidFolder("Assets/UI/Dialogue")) AssetDatabase.CreateFolder("Assets/UI","Dialogue");
        var prefab = PrefabUtility.SaveAsPrefabAsset(canvas, Path);
        Object.DestroyImmediate(canvas);
        Debug.Log("[Dialogue] Wrote "+Path+" (large fonts + autosize)");
        Selection.activeObject = prefab;
    }
}
