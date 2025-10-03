// COMPLETE FILE: Assets/MergeBoardBoot.cs
// REPLACE THE ENTIRE FILE WITH THIS

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-50)]
public sealed class MergeBoardBoot : MonoBehaviour
{
    [Header("Auto-filled")]
    [SerializeField] GridLayoutGroup grid;
    [SerializeField] Component presenter;
    [SerializeField] Component controller;

    [Header("Fallbacks")]
    [SerializeField] float spacingFallback = 2f;
    [SerializeField] bool verboseLogs = true;

    void Reset()
    {
        grid = GetComponent<GridLayoutGroup>();
        presenter = FindSiblingByTypeName(gameObject, "MergeBoardPresenter");
        controller = FindSiblingByTypeName(gameObject, "MergeBoardController");
    }

    void Awake()
    {
        if (!presenter) presenter = FindSiblingByTypeName(gameObject, "MergeBoardPresenter");
        if (!controller) controller = FindSiblingByTypeName(gameObject, "MergeBoardController");
        ApplyGridSpacing();
    }

    void Start()
    {
        ApplyGridSpacing();

        // Check if slots already exist in scene (pre-placed)
        int existingSlots = GetChildCount();
        int expectedSlots = (GetInt(presenter, "Rows") ?? GetInt(controller, "Rows") ?? 9) *
                           (GetInt(presenter, "Columns") ?? GetInt(controller, "Cols") ?? 7);
        bool hasPreplacedSlots = existingSlots >= expectedSlots;

        if (!hasPreplacedSlots)
        {
            // No pre-placed slots, try to build at runtime
            if (GetChildCount() == 0)
            {
                int rows = GetInt(presenter, "Rows") ?? GetInt(controller, "Rows") ?? 0;
                int cols = GetInt(presenter, "Columns") ?? GetInt(presenter, "Cols") ??
                           GetInt(controller, "Cols") ?? GetInt(controller, "Columns") ?? 0;

                var prefab = GetPrefab(controller, new[] { "TilePrefab", "tilePrefab", "SlotPrefab", "slotPrefab" });

                if (rows > 0 && cols > 0 && prefab != null)
                {
                    BuildSlots(rows, cols, prefab);
                }
                else
                {
                    Warn($"Cannot auto-build: rows={rows}, cols={cols}, prefab={(prefab ? prefab.name : "<null>")}. " +
                         $"Use menu: AQ > Board > Rebuild MergeBoard_Demo Scene");
                    return;
                }
            }
        }
        else
        {
            Log($"Found {existingSlots} pre-placed slots, skipping runtime build.");
        }

        // Note: Generator is placed by MergeBoardController.Start() after it initializes the tiles array

        Log($"Boot done. children={GetChildCount()}, grid.spacing={grid?.spacing}");
    }

    // ---------- build / seed helpers ----------

    void BuildSlots(int rows, int cols, GameObject tilePrefab)
    {
        if (!grid) return;

        if (grid.constraint != GridLayoutGroup.Constraint.FixedColumnCount)
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = cols;

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                var go = UnityEngine.Object.Instantiate(tilePrefab);
                go.name = $"slot_{r:00}_{c:00}";
                var rt = go.GetComponent<RectTransform>();
                rt.SetParent(grid.transform, false);
            }
        Log($"Auto-built {rows * cols} slots.");
    }

    void ApplyGridSpacing()
    {
        if (!grid) grid = GetComponent<GridLayoutGroup>();
        if (!grid) return;

        float s = GetFloat(presenter, "Spacing") ?? spacingFallback;
        grid.spacing = new Vector2(s, s);
    }

    // ---------- reflection/util ----------

    Component FindSiblingByTypeName(GameObject go, string typeName)
    {
        if (!go) return null;
        foreach (var m in go.GetComponents<MonoBehaviour>())
            if (m && m.GetType().Name == typeName) return m;
        return null;
    }

    bool TryInvokeAny(Component target, string[] names)
    {
        if (!target) return false;
        foreach (var n in names)
        {
            var mi = target.GetType().GetMethod(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (mi != null)
            {
                try { mi.Invoke(target, null); Log($"Invoked {target.GetType().Name}.{n}()"); return true; }
                catch (Exception e) { Warn($"{target.GetType().Name}.{n} threw: {e.Message}"); }
            }
        }
        return false;
    }

    int? GetInt(Component obj, string name)
    {
        if (!obj) return null;
        var t = obj.GetType();
        var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && (p.PropertyType == typeof(int))) return (int)p.GetValue(obj);
        var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null && (f.FieldType == typeof(int))) return (int)f.GetValue(obj);
        return null;
    }

    float? GetFloat(Component obj, string name)
    {
        if (!obj) return null;
        var t = obj.GetType();
        var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.PropertyType == typeof(float)) return (float)p.GetValue(obj);
        var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null && f.FieldType == typeof(float)) return (float)f.GetValue(obj);
        return null;
    }

    GameObject GetPrefab(Component obj, string[] fieldNames)
    {
        if (!obj) return null;
        var t = obj.GetType();
        foreach (var n in fieldNames)
        {
            // Try both the provided name and lowercase first letter version
            foreach (var name in new[] { n, char.ToLower(n[0]) + n.Substring(1) })
            {
                var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (p != null)
                {
                    var val = p.GetValue(obj);
                    if (val is GameObject go) return go;
                    if (val is Component c) return c ? c.gameObject : null;
                }
                var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (f != null)
                {
                    var val = f.GetValue(obj);
                    if (val is GameObject go) return go;
                    if (val is Component c) return c ? c.gameObject : null;
                }
            }
        }
        return null;
    }

    List<Sprite> GetSpriteList(Component obj, string name)
    {
        if (!obj) return null;
        var t = obj.GetType();
        var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && typeof(IEnumerable<Sprite>).IsAssignableFrom(p.PropertyType))
            return new List<Sprite>((IEnumerable<Sprite>)p.GetValue(obj));
        var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null && typeof(IEnumerable<Sprite>).IsAssignableFrom(f.FieldType))
            return new List<Sprite>((IEnumerable<Sprite>)f.GetValue(obj));
        return null;
    }

    int CountIconsWithSprites()
    {
        if (!grid) return 0;

        int count = 0;
        var t = grid.transform;
        for (int i = 0; i < t.childCount; i++)
        {
            var slot = t.GetChild(i);
            var icon = slot.Find("Icon");
            if (!icon) continue;

            var img = icon.GetComponent<UnityEngine.UI.Image>();
            if (img && img.sprite) count++;
        }
        return count;
    }

    Sprite GetSprite(Component obj, string name)
    {
        if (!obj) return null;
        var t = obj.GetType();
        var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.PropertyType == typeof(Sprite)) return (Sprite)p.GetValue(obj);
        var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null && f.FieldType == typeof(Sprite)) return (Sprite)f.GetValue(obj);
        return null;
    }

    int GetChildCount() => grid ? grid.transform.childCount : 0;

    void Log(string m) { if (verboseLogs) Debug.Log($"{GetType().Name}: {m}"); }
    void Warn(string m) { Debug.LogWarning($"{GetType().Name}: {m}"); }
}
