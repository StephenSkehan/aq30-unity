// Assets/Editor/UI/Board/Audit_MergeBoardScene.cs
// Read-only scene auditor for MergeBoard_Demo.
// Menu: AQ ► Board ► Audit Current Scene
// Prints a copy/paste friendly report to the Console.

#if UNITY_EDITOR
using System;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

static class Audit_MergeBoardScene
{
    [MenuItem("AQ/Board/Audit Current Scene", false, 100)]
    private static void Audit()
    {
        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        var sb = new StringBuilder();
        var stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        Line(sb, $"=== SCENE AUDIT: {scene.name} @ {stamp} ===");

        // Summary
        Line(sb, $"Root objects: {roots.Length}");
        Line(sb, "");

        foreach (var root in roots.OrderBy(g => g.name))
        {
            DumpGameObject(sb, root, 0);
        }

        // Split into chunks so Unity console shows everything.
        WriteBigToConsole(sb.ToString());
    }

    // ─────────────────────────────────────────────────────────────────────────────

    private static void DumpGameObject(StringBuilder sb, GameObject go, int depth)
    {
        var pad = new string(' ', depth * 2);
        var rt = go.GetComponent<RectTransform>();
        Line(sb, $"{pad}- {go.name}  (active={go.activeInHierarchy}, layer={LayerMask.LayerToName(go.layer)}, tag={go.tag})");

        if (rt != null)
        {
            Line(sb, $"{pad}  RectTransform: anchorMin={Fmt(rt.anchorMin)} anchorMax={Fmt(rt.anchorMax)} pos={Fmt(rt.anchoredPosition)} sizeDelta={Fmt(rt.sizeDelta)} pivot={Fmt(rt.pivot)}");
        }

        // Common interesting components
        var grid = go.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            Line(sb, $"{pad}  GridLayoutGroup: cellSize={Fmt(grid.cellSize)} spacing={Fmt(grid.spacing)} startCorner={grid.startCorner} startAxis={grid.startAxis} childAlign={grid.childAlignment} constraint={grid.constraint} count={grid.constraintCount}");
            var warn = (grid.spacing != new Vector2(2, 2)) ? "  [WARN spacing!=2,2]" : "";
            Line(sb, $"{pad}    Padding: L{grid.padding.left} T{grid.padding.top} R{grid.padding.right} B{grid.padding.bottom}{warn}");
        }

        var img = go.GetComponent<Image>();
        if (img != null)
        {
            var sName = img.sprite ? img.sprite.name : "None";
            Line(sb, $"{pad}  Image: sprite='{sName}' type={img.type} preserveAspect={img.preserveAspect} color={Fmt(img.color)}");
        }

        // Dump MergeBoard bits WITHOUT taking hard compile-time dependency (use type name match).
        foreach (var c in go.GetComponents<Component>())
        {
            if (c == null) continue;
            var tn = c.GetType().FullName ?? c.GetType().Name;

            if (tn.EndsWith("MergeBoardController"))
            {
                DumpMergeBoardController(sb, c, pad);
            }
            else if (tn.EndsWith("MergeBoardPresenter"))
            {
                DumpMergeBoardPresenter(sb, c, pad);
            }
            else if (
                !(c is Transform) &&
                !(c is RectTransform) &&
                !(c is GridLayoutGroup) &&
                !(c is Image) &&
                !(c is CanvasRenderer))
            {
                // List other component types briefly
                Line(sb, $"{pad}  Component: {tn}");
            }
        }

        // Recurse
        for (int i = 0; i < go.transform.childCount; i++)
        {
            DumpGameObject(sb, go.transform.GetChild(i).gameObject, depth + 1);
        }
    }

    private static void DumpMergeBoardController(StringBuilder sb, Component c, string pad)
    {
        Line(sb, $"{pad}  ▶ MergeBoardController");
        var so = new SerializedObject(c);
        so.Update();

        // Helper to print a property if it exists
        void P(string prop, string label = null)
        {
            var sp = so.FindProperty(prop);
            if (sp == null) return;
            label ??= prop;

            switch (sp.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    Line(sb, $"{pad}    {label}: {(sp.objectReferenceValue ? sp.objectReferenceValue.name : "None")}");
                    break;
                case SerializedPropertyType.Integer:
                    Line(sb, $"{pad}    {label}: {sp.intValue}");
                    break;
                case SerializedPropertyType.Vector2Int:
                    Line(sb, $"{pad}    {label}: ({sp.vector2IntValue.x},{sp.vector2IntValue.y})");
                    break;
                default:
                    Line(sb, $"{pad}    {label}: {sp.ToString()}");
                    break;
            }
        }

        // Common field names (try both camelCase & PascalCase where relevant)
        P("boardRoot", "BoardRoot");
        P("grid", "Grid");
        P("tilePrefab", "TilePrefab");

        P("rows", "Rows");       P("Rows", "Rows");
        P("cols", "Cols");       P("Cols", "Cols");

        P("generatorStart", "GeneratorStart"); P("GeneratorStart", "GeneratorStart");
        P("generatorSprite", "GeneratorSprite"); P("GeneratorSprite", "GeneratorSprite");

        // Icons array
        var icons = so.FindProperty("icons"); if (icons == null) icons = so.FindProperty("Icons");
        if (icons != null && icons.isArray)
        {
            Line(sb, $"{pad}    Icons: count={icons.arraySize}");
            for (int i = 0; i < icons.arraySize; i++)
            {
                var e = icons.GetArrayElementAtIndex(i);
                var name = e.objectReferenceValue ? e.objectReferenceValue.name : "None";
                Line(sb, $"{pad}      [{i}] {name}");
            }
        }

        // Weights array
        var weights = so.FindProperty("weights"); if (weights == null) weights = so.FindProperty("Weights");
        if (weights != null && weights.isArray)
        {
            Line(sb, $"{pad}    Weights: count={weights.arraySize}");
            var vals = Enumerable.Range(0, weights.arraySize)
                                 .Select(i => weights.GetArrayElementAtIndex(i).intValue.ToString());
            Line(sb, $"{pad}      {string.Join(", ", vals)}");
        }
    }

    private static void DumpMergeBoardPresenter(StringBuilder sb, Component c, string pad)
    {
        Line(sb, $"{pad}  ▶ MergeBoardPresenter");
        var so = new SerializedObject(c);
        so.Update();

        void P(string prop, string label = null)
        {
            var sp = so.FindProperty(prop);
            if (sp == null) return;
            label ??= prop;

            switch (sp.propertyType)
            {
                case SerializedPropertyType.Integer:
                    Line(sb, $"{pad}    {label}: {sp.intValue}");
                    break;
                case SerializedPropertyType.Float:
                    Line(sb, $"{pad}    {label}: {sp.floatValue}");
                    break;
                case SerializedPropertyType.Color:
                    Line(sb, $"{pad}    {label}: {Fmt(sp.colorValue)}");
                    break;
                case SerializedPropertyType.ObjectReference:
                    Line(sb, $"{pad}    {label}: {(sp.objectReferenceValue ? sp.objectReferenceValue.name : "None")}");
                    break;
                default:
                    Line(sb, $"{pad}    {label}: {sp.ToString()}");
                    break;
            }
        }

        // Likely fields on presenter
        P("columns", "Columns");   P("Columns", "Columns");
        P("rows", "Rows");         P("Rows", "Rows");
        P("spacing", "Spacing");   P("Spacing", "Spacing");
        P("sideMargin", "SideMargin"); P("bottomMargin", "BottomMargin"); P("topInset", "TopInset");
        P("grid", "Grid");         P("boardFrame", "BoardFrame"); P("boardViewport", "BoardViewport");
        P("cellColor", "CellColor");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    private static void Line(StringBuilder sb, string s) => sb.AppendLine(s);

    private static string Fmt(Vector2 v) => $"({v.x:0.##},{v.y:0.##})";
    private static string Fmt(Color c) => $"rgba({Mathf.RoundToInt(c.r*255)},{Mathf.RoundToInt(c.g*255)},{Mathf.RoundToInt(c.b*255)},{c.a:0.##})";

    private static void WriteBigToConsole(string text)
    {
        const int chunk = 7000; // Unity console truncates very long strings
        if (text.Length <= chunk) { Debug.Log(text); return; }
        int i = 0; int n = 1 + (text.Length / chunk);
        while (i < text.Length)
        {
            var len = Mathf.Min(chunk, text.Length - i);
            Debug.Log($"[audit chunk {((i / chunk) + 1)}/{n}]\n" + text.Substring(i, len));
            i += len;
        }
    }
}
#endif
