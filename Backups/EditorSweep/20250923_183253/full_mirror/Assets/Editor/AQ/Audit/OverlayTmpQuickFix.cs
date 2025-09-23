using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class OverlayTmpQuickFix
{
    [MenuItem("AQ/WK3-1/Convert Overlay Labels To TMP")]
    public static void Convert()
    {
        Transform FindDeep(Transform p, string n)
        {
            if (!p) return null;
            foreach (var t in p.GetComponentsInChildren<Transform>(true)) if (t.name == n) return t;
            return null;
        }
        var root = Resources.FindObjectsOfTypeAll<GameObject>()
            .FirstOrDefault(g => g && g.name=="ResolutionRoot" && g.scene.IsValid());
        if (!root) { Debug.LogWarning("ResolutionRoot not found."); return; }
        var panel = FindDeep(root.transform, "ResolutionPanel");
        var title = FindDeep(panel, "TitleText");
        var body  = FindDeep(panel, "BodyText");
        var btn   = FindDeep(panel, "ResolveButton");

	void SwapToTMP(Transform t, float size, Color32 col)
	{
   	 if (!t) return;

   	 // 1. Kill the legacy Text first
   	 var utext = t.GetComponent<Text>();
    	string txt = utext ? utext.text : "";
    	if (utext) Object.DestroyImmediate(utext, true);

    	// 2. Add TMP if missing
    	var tmpType = System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
    	var tmp = t.GetComponent(tmpType);
   	 if (!tmp) tmp = t.gameObject.AddComponent(tmpType);

    	// 3. Set properties safely (float, not double)
    	var textProp = tmpType.GetProperty("text");
    	var sizeProp = tmpType.GetProperty("fontSize");
    	var colProp  = tmpType.GetProperty("color");

    	textProp?.SetValue(tmp, string.IsNullOrEmpty(txt) ? t.name : txt);
    	sizeProp?.SetValue(tmp, size);    // note: size is float
    	colProp?.SetValue(tmp, (Color)col);
	}


        // Convert Title & Body on their own objects
        SwapToTMP(title, 60, new Color32(255,255,255,255));
        SwapToTMP(body,  34, new Color32(0xF2,0xF5,0xFA,255));

        // Convert ResolveButton label (child)
        if (btn)
        {
            var label = btn.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name=="Text") ?? btn;
            SwapToTMP(label, 34, new Color32(0x0B,0x0E,0x14,255));
        }

        Debug.Log("Overlay labels converted to TMP.");
    }
}
