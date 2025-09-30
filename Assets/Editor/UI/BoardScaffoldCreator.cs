using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class BoardScaffoldCreator
{
    private const string CanvasName   = "Canvas_Board";
    private const string GridRootName = "Grid_Board";
    private const string FrameName    = "BoardFrame";
    private const string ViewportName = "BoardViewport";
    private const string GridName     = "BoardGrid";

    private const int   kColumns    = 7;
    private const int   kRows       = 9;
    private const float kSpacing    = 2f;   // 2px spacing
    private const float kSideMargin = 24f;
    private const float kBottom     = 48f;
    private const float kFallbackTopInset = 660f;

    private static readonly Color kCellColor = Color.white;

    private static readonly string[] PresenterNames =
    {
        "AQ.UI.MergeBoardPresenter",
        "MergeBoardPresenter"
    };

    [MenuItem("AQ/UI/Board/Rebuild Scaffold (7×9)")]
    public static void CreateBoardScaffold()
    {
        // 1) Canvas
        var canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas == null || canvas.name != CanvasName)
            canvas = GameObject.Find(CanvasName)?.GetComponent<Canvas>();

        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Board Scaffold", $"Could not find '{CanvasName}' in the scene.", "OK");
            Debug.LogError("Board Scaffold: Canvas not found.");
            return;
        }

        var canvasRT = (RectTransform)canvas.transform;

        // 2) Root just below HUD_Board so it lives under the leads row
        var root = canvasRT.Find(GridRootName) as RectTransform;
        if (root == null)
        {
            var go = new GameObject(GridRootName, typeof(RectTransform));
            root = (RectTransform)go.transform;
            root.SetParent(canvasRT, false);
        }
        var hud = canvasRT.Find("HUD_Board") as RectTransform;
        if (hud != null) root.SetSiblingIndex(hud.GetSiblingIndex() + 1);

        // 3) Remove any legacy "BoardPresenter" components that are not our runtime presenter
        foreach (var c in root.GetComponents<Component>())
        {
            if (c == null) continue;
            var tn = c.GetType().Name;
            if (tn == "BoardPresenter" && c.GetType().FullName != "AQ.UI.MergeBoardPresenter")
                Undo.DestroyObjectImmediate(c);
        }

        // 4) Ensure clean hierarchy (wipe unexpected siblings to avoid overlapping grids)
        var frame    = EnsureChild(root, FrameName);
        var viewport = EnsureChild(frame, ViewportName);
        var grid     = EnsureChild(viewport, GridName);

        WipeUnexpectedChildren(root,    new[] { FrameName });
        WipeUnexpectedChildren(frame,   new[] { ViewportName });
        WipeUnexpectedChildren(viewport,new[] { GridName });

        if (viewport.GetComponent<RectMask2D>() == null)
            viewport.gameObject.AddComponent<RectMask2D>();

        var layout = grid.GetComponent<GridLayoutGroup>() ?? grid.gameObject.AddComponent<GridLayoutGroup>();
        layout.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = kColumns;
        layout.startCorner     = GridLayoutGroup.Corner.UpperLeft;
        layout.startAxis       = GridLayoutGroup.Axis.Horizontal;
        layout.childAlignment  = TextAnchor.UpperCenter;
        layout.spacing         = new Vector2(kSpacing, kSpacing);

        // 5) Attach presenter via reflection (no compile-time dependency)
        var presenterType = FindType(PresenterNames);
        if (presenterType == null)
        {
            EditorUtility.DisplayDialog(
                "Board Scaffold",
                "Could not find 'MergeBoardPresenter'. Ensure Assets/UI/Board/MergeBoardPresenter.cs exists (not in an Editor folder).",
                "OK");
            return;
        }

        var presenter = root.GetComponent(presenterType);
        if (presenter == null) presenter = Undo.AddComponent(root.gameObject, presenterType);

        // 6) Compute TopInset from LeadsBar bottom (fallback if missing)
        float topInset = kFallbackTopInset;
        if (hud != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(hud);
            var leadsBar = hud.Find("LeadsBar") as RectTransform;
            if (leadsBar != null)
            {
                var b = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRT, leadsBar);
                float canvasTop   = canvasRT.rect.height * 0.5f;
                float leadsBottom = b.min.y;
                topInset = Mathf.Max(canvasTop - leadsBottom + 8f, 0f);
            }
        }

        // 7) Push config to presenter
        SetMember(presenter, "Columns",       kColumns);
        SetMember(presenter, "Rows",          kRows);
        SetMember(presenter, "Spacing",       kSpacing);
        SetMember(presenter, "SideMargin",    kSideMargin);
        SetMember(presenter, "BottomMargin",  kBottom);
        SetMember(presenter, "TopInset",      topInset);
        SetMember(presenter, "BoardFrame",    frame);
        SetMember(presenter, "BoardViewport", viewport);
        SetMember(presenter, "Grid",          grid);
        SetMember(presenter, "CellColor",     kCellColor);

        // 8) Rebuild EXACTLY 7×9 placeholders (pure white)
        foreach (Transform t in grid) UnityEngine.Object.DestroyImmediate(t.gameObject);

        int count = kColumns * kRows;
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject($"Tile_{i:00}", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(grid, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot     = new Vector2(0.5f, 0.5f);

            var img = go.GetComponent<Image>();
            img.color = kCellColor;        // PURE WHITE
            img.raycastTarget = false;
            // img.sprite left null so it renders as a solid quad.
        }

        // 9) Force layout + appearance
        CallMethod(presenter, "ApplyLayout");
        CallMethod(presenter, "ApplyAppearance");

        EditorUtility.SetDirty(root);
        EditorUtility.SetDirty(frame);
        EditorUtility.SetDirty(viewport);
        EditorUtility.SetDirty(grid);
        if (presenter is Component pc) EditorUtility.SetDirty(pc);

        Debug.Log("Board scaffold created/updated under Canvas.");
    }

    // ---------- helpers ----------

    private static RectTransform EnsureChild(RectTransform parent, string name)
    {
        var child = parent.Find(name) as RectTransform;
        if (child == null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            child = (RectTransform)go.transform;
            child.SetParent(parent, false);
        }
        return child;
    }

    private static void WipeUnexpectedChildren(Transform parent, IEnumerable<string> keepNames)
    {
        var keep = new HashSet<string>(keepNames);
        var toRemove = new List<GameObject>();
        foreach (Transform c in parent)
            if (!keep.Contains(c.name))
                toRemove.Add(c.gameObject);
        foreach (var go in toRemove)
            UnityEngine.Object.DestroyImmediate(go);
    }

    private static Type FindType(string[] names)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try { types = asm.GetTypes(); }
            catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }

            foreach (var t in types)
            {
                foreach (var n in names)
                {
                    if (string.Equals(t.FullName, n, StringComparison.Ordinal) ||
                        string.Equals(t.Name,     n, StringComparison.Ordinal))
                        return t;
                }
            }
        }
        return null;
    }

    private static void SetMember(object target, string name, object value)
    {
        if (target == null) return;
        var t = target.GetType();
        var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.CanWrite) { p.SetValue(target, value); return; }
        var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null) f.SetValue(target, value);
    }

    private static void CallMethod(object target, string name)
    {
        if (target == null) return;
        var m = target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        m?.Invoke(target, null);
    }
}
