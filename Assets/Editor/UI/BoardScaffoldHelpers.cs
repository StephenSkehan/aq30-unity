#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class BoardScaffoldHelpers
{
    [MenuItem("AQ/UI/Board/Fix & Reveal Grid (7x9)")]
    public static void FixReveal()
    {
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (!canvas)
        {
            Debug.LogError("No Canvas in scene. Open a scene with Canvas_Board.");
            return;
        }

        // Find or create the scaffold
        var scaffold = GameObject.Find("BoardScaffold");
        if (!scaffold)
        {
            var creatorType = typeof(BoardScaffoldCreator);
            var method = creatorType.GetMethod(
                "CreateBoardScaffold",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
            );
            if (method != null) method.Invoke(null, null);
            scaffold = GameObject.Find("BoardScaffold");
        }

        if (!scaffold)
        {
            Debug.LogError("Could not find or create BoardScaffold.");
            return;
        }

        // Target parent: HUD_Board if present, else Canvas root
        Transform targetParent = null;
        var hud = GameObject.Find("HUD_Board");
        targetParent = hud ? hud.transform : canvas.transform;

        scaffold.transform.SetParent(targetParent, false);
        scaffold.name = "BoardScaffold";

        var rt = scaffold.GetComponent<RectTransform>();
        if (!rt) rt = scaffold.AddComponent<RectTransform>();

        // Stretch within HUD_Board with a small margin
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = new Vector2(24f, 24f);
        rt.offsetMax = new Vector2(-24f, -24f);

        // Draw on top of siblings
        scaffold.transform.SetAsLastSibling();

        // Make sure the tiles are actually visible
        foreach (var img in scaffold.GetComponentsInChildren<Image>(true))
        {
            if (img == null) continue;

            // Any reasonable name match for slot/backers
            var n = img.gameObject.name.ToLowerInvariant();
            if (n.Contains("slot") || n.Contains("bg") || n.Contains("tile"))
            {
                var c = img.color;
                if (c.a < 0.12f) { c.a = 0.12f; img.color = c; } // bump alpha so you can see it
                img.raycastTarget = false; // prevent stealing pointer from real board later
            }
        }

        Selection.activeObject = scaffold;
        EditorGUIUtility.PingObject(scaffold);
        Debug.Log("BoardScaffold placed under HUD_Board and revealed.");
    }
}
#endif
