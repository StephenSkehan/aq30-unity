#if UNITY_EDITOR
using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class RepairTierSetPopupInScene
{
    [MenuItem("AQ/UI/Leads/Repair TierSetPopup (Legacy Script)")]
    public static void Run()
    {
        var hud = GameObject.Find("HUD_Board")?.transform
                  ?? UnityEngine.Object.FindFirstObjectByType<Canvas>()?.transform
                  ?? UnityEngine.Object.FindAnyObjectByType<Transform>();

        if (!hud)
        {
            Debug.LogWarning("[RepairTierSetPopupInScene] No HUD/Canvas found.");
            return;
        }

        var popupT = GameObject.Find("TierSetPopup")?.transform;
        if (!popupT)
        {
            var go = new GameObject("TierSetPopup", typeof(RectTransform), typeof(CanvasGroup));
            popupT = go.transform;
            popupT.SetParent(hud, false);
            var rt = (RectTransform)popupT;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(520, 280);
        }

        // ensure basic contents
        var cg = popupT.GetComponent<CanvasGroup>(); cg.alpha = 0; cg.blocksRaycasts = false; cg.interactable = false;

        TMP_Text Title(Transform parent, string name)
        {
            var t = parent.Find(name) ?? new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).transform;
            t.SetParent(parent, false);
            var rt = (RectTransform)t; rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f); rt.pivot = new Vector2(0.5f,1f);
            rt.anchoredPosition = new Vector2(0, -24); rt.sizeDelta = new Vector2(480, 48);
            var tmp = t.GetComponent<TMP_Text>(); tmp.text = "STAKEOUT FUEL"; tmp.fontSize = 34;
            tmp.textWrappingMode = TextWrappingModes.NoWrap; tmp.overflowMode = TextOverflowModes.Truncate;
            tmp.alignment = TextAlignmentOptions.Midline;
            return tmp;
        }

        var title = Title(popupT, "Title");

        var grid = popupT.Find("IconGrid") ?? new GameObject("IconGrid", typeof(RectTransform), typeof(GridLayoutGroup)).transform;
        grid.SetParent(popupT, false);
        var gridRT = (RectTransform)grid; gridRT.anchorMin = gridRT.anchorMax = new Vector2(0.5f,0.5f); gridRT.pivot = new Vector2(0.5f,0.5f);
        gridRT.sizeDelta = new Vector2(480, 120); gridRT.anchoredPosition = new Vector2(0,10);
        var layout = grid.GetComponent<GridLayoutGroup>(); layout.cellSize = new Vector2(64,64); layout.spacing = new Vector2(12,12);
        layout.constraint = GridLayoutGroup.Constraint.FixedRowCount; layout.constraintCount = 1;

        for (int i=0;i<6;i++)
        {
            var c = grid.Find($"Icon{i+1}") ?? new GameObject($"Icon{i+1}", typeof(RectTransform), typeof(Image)).transform;
            c.SetParent(grid, false);
            ((RectTransform)c).sizeDelta = new Vector2(64,64);
            var img = c.GetComponent<Image>(); img.raycastTarget = false;
        }

        var highlight = grid.Find("Highlight") ?? new GameObject("Highlight", typeof(RectTransform), typeof(Image)).transform;
        highlight.SetParent(grid, false);
        var hImg = highlight.GetComponent<Image>(); hImg.enabled = false;

        var close = popupT.Find("Btn_Close") ?? new GameObject("Btn_Close", typeof(RectTransform), typeof(Image), typeof(Button)).transform;
        close.SetParent(popupT, false);
        var crt2 = (RectTransform)close; crt2.anchorMin = crt2.anchorMax = new Vector2(1,1); crt2.pivot = new Vector2(1,1);
        crt2.sizeDelta = new Vector2(32,32); crt2.anchoredPosition = new Vector2(-8,-8);

        // Try to wire TierSetPopupPresenter if present (reflection)
        var presenterType = FindType("AQ.App.UI.Leads.TierSetPopupPresenter") ?? FindType("TierSetPopupPresenter");
        if (presenterType != null)
        {
            var presenter = popupT.GetComponent(presenterType) ?? popupT.gameObject.AddComponent(presenterType);
            SetPrivate(presenter, "cg", cg);
            SetPrivate(presenter, "titleText", title);
            SetPrivate(presenter, "iconGrid", grid);
            SetPrivate(presenter, "highlight", hImg);
            SetPrivate(presenter, "closeButton", close.GetComponent<Button>());
            Debug.Log("[RepairTierSetPopupInScene] Wired TierSetPopupPresenter.");
        }
        else
        {
            Debug.LogWarning("[RepairTierSetPopupInScene] TierSetPopupPresenter type not found. Popup created but not wired.");
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private static Type FindType(string name)
    {
        var t = Type.GetType(name);
        if (t != null) return t;
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            t = asm.GetType(name);
            if (t != null) return t;
            try { t = asm.GetTypes().FirstOrDefault(x => x.Name == name); if (t != null) return t; }
            catch { }
        }
        return null;
    }

    private static void SetPrivate(object obj, string field, object value)
    {
        if (obj == null) return;
        var f = obj.GetType().GetField(field, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (f != null) f.SetValue(obj, value);
    }
}
#endif
