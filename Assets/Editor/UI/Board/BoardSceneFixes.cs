using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.Editor.Board
{
    /// <summary>
    /// One-click fixer:
    ///  - Sets MergeBoard GridLayoutGroup.spacing to (2,2)
    ///  - Mirrors 2 into MergeBoardPresenter.Spacing (if present)
    ///  - Removes BoxCollider2D from all slot children under the grid
    /// </summary>
    public static class BoardSceneFixes
    {
        [MenuItem("AQ/Board/Fix Current Scene (Grid + Remove Slot Colliders)")]
        public static void FixGridAndRemoveColliders()
        {
            var grid = FindMergeBoardGrid(out var boardGO);
            if (!grid || !boardGO)
            {
                Debug.LogError("BoardSceneFixes: Could not find a MergeBoard object with a GridLayoutGroup.");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(boardGO, "Fix Grid & Remove Slot Colliders");

            // 1) Normalize grid spacing
            var before = grid.spacing;
            grid.spacing = new Vector2(2f, 2f);
            EditorUtility.SetDirty(grid);
            Debug.Log($"BoardSceneFixes: grid.spacing {before} -> {grid.spacing}");

            // Also mirror into presenter.Spacing if present
            var presenter = boardGO.GetComponent(GetTypeByName("MergeBoardPresenter"));
            if (presenter != null)
            {
                if (SetFloat(presenter, "Spacing", 2f))
                {
                    EditorUtility.SetDirty(presenter);
                    Debug.Log("BoardSceneFixes: Set MergeBoardPresenter.Spacing = 2");
                }
            }

            // 2) Remove BoxCollider2D from slot children
            int removed = 0;
            foreach (Transform child in grid.transform)
            {
                var col = child.GetComponent<BoxCollider2D>();
                if (col != null)
                {
                    Undo.DestroyObjectImmediate(col);
                    removed++;
                }
            }
            Debug.Log($"BoardSceneFixes: Removed {removed} BoxCollider2D components from slots.");

            Debug.Log("BoardSceneFixes: Done. Re-run AQ > Board > Sanity (Strict).");
        }

        // ---------- helpers ----------

        private static GridLayoutGroup FindMergeBoardGrid(out GameObject boardGO)
        {
            boardGO = null;

            // Prefer an object actually named "MergeBoard"
            var named = GameObject.Find("MergeBoard");
            if (named != null)
            {
                var glg = named.GetComponent<GridLayoutGroup>();
                if (glg) { boardGO = named; return glg; }
            }

            // Otherwise, find any GridLayoutGroup that carries a MergeBoardPresenter/Controller on the same GO
            foreach (var glg in UnityEngine.Object.FindObjectsByType<GridLayoutGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var go = glg.gameObject;
                bool hasPresenter = go.GetComponent(GetTypeByName("MergeBoardPresenter")) != null;
                bool hasController = go.GetComponent(GetTypeByName("MergeBoardController")) != null;
                if (hasPresenter || hasController)
                {
                    boardGO = go;
                    return glg;
                }
            }

            return null;
        }

        private static Type GetTypeByName(string typeName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var t in asm.GetTypes())
                    {
                        if (t.Name == typeName) return t;
                    }
                }
                catch { /* ignore reflection-only / dynamic assemblies */ }
            }
            return null;
        }

        private static bool SetFloat(Component obj, string memberName, float value)
        {
            if (!obj) return false;
            var t = obj.GetType();

            var p = t.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && p.CanWrite && p.PropertyType == typeof(float))
            {
                p.SetValue(obj, value);
                return true;
            }

            var f = t.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && f.FieldType == typeof(float))
            {
                f.SetValue(obj, value);
                return true;
            }
            return false;
        }
    }
}
