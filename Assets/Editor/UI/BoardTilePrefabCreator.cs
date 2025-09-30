#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class BoardTilePrefabCreator
{
    [MenuItem("AQ/UI/Create/Board Tile Prefab")]
    public static void CreateBoardTilePrefab()
    {
        const string folder = "Assets/UI/Prefabs";
        const string path = folder + "/board_tile_slot.prefab";
        Directory.CreateDirectory(folder);

        // Root
        var root = new GameObject("board_tile_slot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        var rt = (RectTransform)root.transform;
        rt.sizeDelta = new Vector2(96, 96);

        var rootImg = root.GetComponent<Image>();
        rootImg.color = new Color(1, 1, 1, 0.03f); // subtle hit area tint
        var btn = root.GetComponent<Button>();
        btn.transition = Selectable.Transition.ColorTint;

        // BG
        var bgGO = new GameObject("Bg", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bgGO.transform.SetParent(root.transform, false);
        var bgRT = (RectTransform)bgGO.transform;
        bgRT.anchorMin = new Vector2(0, 0);
        bgRT.anchorMax = new Vector2(1, 1);
        bgRT.offsetMin = new Vector2(4, 4);
        bgRT.offsetMax = new Vector2(-4, -4);
        var bgImg = bgGO.GetComponent<Image>();
        bgImg.color = new Color(1, 1, 1, 0.08f);
        bgImg.type = Image.Type.Sliced; // 9-slice if a sprite is assigned later

        // Item
        var itemGO = new GameObject("Item", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        itemGO.transform.SetParent(root.transform, false);
        var itemRT = (RectTransform)itemGO.transform;
        itemRT.anchorMin = new Vector2(0.1f, 0.1f);
        itemRT.anchorMax = new Vector2(0.9f, 0.9f);
        itemRT.offsetMin = Vector2.zero;
        itemRT.offsetMax = Vector2.zero;
        var itemImg = itemGO.GetComponent<Image>();
        itemImg.preserveAspect = true;
        itemImg.raycastTarget = false;

        // Highlight (off)
        var hlGO = new GameObject("Highlight", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        hlGO.transform.SetParent(root.transform, false);
        var hlRT = (RectTransform)hlGO.transform;
        hlRT.anchorMin = new Vector2(0, 0);
        hlRT.anchorMax = new Vector2(1, 1);
        hlRT.offsetMin = Vector2.zero;
        hlRT.offsetMax = Vector2.zero;
        var hlImg = hlGO.GetComponent<Image>();
        hlImg.color = new Color(0.2f, 0.8f, 1f, 0.18f);
        hlImg.raycastTarget = false;
        hlImg.enabled = false;

        // Count badge
        var badgeGO = new GameObject("Badge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        badgeGO.transform.SetParent(root.transform, false);
        var badgeRT = (RectTransform)badgeGO.transform;
        badgeRT.anchorMin = new Vector2(1, 0);
        badgeRT.anchorMax = new Vector2(1, 0);
        badgeRT.anchoredPosition = new Vector2(-10, 10);
        badgeRT.sizeDelta = new Vector2(28, 22);
        var badgeImg = badgeGO.GetComponent<Image>();
        badgeImg.color = new Color(0, 0, 0, 0.6f);

        var txtGO = new GameObject("Count", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        txtGO.transform.SetParent(badgeGO.transform, false);
        var txtRT = (RectTransform)txtGO.transform;
        txtRT.anchorMin = new Vector2(0, 0);
        txtRT.anchorMax = new Vector2(1, 1);
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;
        var tmp = txtGO.GetComponent<TextMeshProUGUI>();
        tmp.text = "1";
        tmp.fontSize = 18;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;

        // ---- Add BoardTileView via reflection and wire its private fields ----
        var boardTileType = FindTypeByName("BoardTileView");
        Component view = null;
        if (boardTileType != null)
        {
            view = root.AddComponent(boardTileType);

            var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            boardTileType.GetField("button", flags)?.SetValue(view, btn);
            boardTileType.GetField("bg", flags)?.SetValue(view, bgImg);
            boardTileType.GetField("itemImage", flags)?.SetValue(view, itemImg);
            boardTileType.GetField("highlight", flags)?.SetValue(view, hlImg);
            boardTileType.GetField("countLabel", flags)?.SetValue(view, tmp);
        }
        else
        {
            Debug.LogWarning("BoardTileView type not found. Prefab will be created without it; add manually later.");
        }

        // Save prefab
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        UnityEngine.Object.DestroyImmediate(root);
        EditorGUIUtility.PingObject(prefab);
        Debug.Log($"Created prefab: {path}");
    }

    private static Type FindTypeByName(string typeName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(typeName);
            if (t != null) return t;
            var tt = asm.GetTypes().FirstOrDefault(x => x.Name == typeName);
            if (tt != null) return tt;
        }
        return null;
    }
}
#endif
