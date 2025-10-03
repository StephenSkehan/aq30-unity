// Assets/Editor/UI/Board/AddCollidersToSlots.cs
// Editor-only utilities to ADD/REMOVE BoxCollider2D on board slots.
// Note: For UI buttons, colliders are not required for the EventSystem.
// These tools are here so you can clean them up in one click if desired.

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.Editor.Board
{
    public static class AddCollidersToSlots
    {
        private static readonly Regex SlotNameRx = new Regex(@"^slot_\d{2}_\d{2}$", RegexOptions.Compiled);

        [MenuItem("AQ/Board/Slots/Remove BoxCollider2D from Slots")]
        private static void RemoveCollidersFromSlots()
        {
            var grid = FindGrid(out string why);
            if (grid == null)
            {
                Debug.LogError("RemoveCollidersFromSlots: " + why);
                return;
            }

            int removed = 0;
            foreach (Transform child in grid.transform)
            {
                if (!LooksLikeSlot(child)) continue;
                var col = child.GetComponent<BoxCollider2D>();
                if (col != null)
                {
                    UnityEngine.Object.DestroyImmediate(col, true);
                    removed++;
                }
            }
            Debug.Log($"Removed {removed} BoxCollider2D components from slots.");
        }

        [MenuItem("AQ/Board/Slots/Add BoxCollider2D to Slots")]
        private static void AddCollidersToSlotsMenu()
        {
            var grid = FindGrid(out string why);
            if (grid == null)
            {
                Debug.LogError("AddCollidersToSlots: " + why);
                return;
            }

            int added = 0;
            foreach (Transform child in grid.transform)
            {
                if (!LooksLikeSlot(child)) continue;
                var col = child.GetComponent<BoxCollider2D>();
                if (col == null)
                {
                    col = Undo.AddComponent<BoxCollider2D>(child.gameObject);
                    col.isTrigger = true;
                    added++;
                }
            }
            Debug.Log($"Added {added} BoxCollider2D components to slots.");
        }

        // ---------- Helpers ----------

        private static GridLayoutGroup FindGrid(out string reasonIfNull)
        {
            reasonIfNull = string.Empty;
            // Prefer current selection
            if (Selection.activeGameObject != null)
            {
                var sel = Selection.activeGameObject;
                var g = sel.GetComponentInParent<GridLayoutGroup>();
                if (g != null) return g;
            }
            // Fallback: any grid named "MergeBoard" or the first GridLayoutGroup
            foreach (var g in FindAll<GridLayoutGroup>())
            {
                if (g != null && g.gameObject.name.Equals("MergeBoard", StringComparison.Ordinal))
                    return g;
            }
            var any = FindOne<GridLayoutGroup>();
            if (any != null) return any;

            reasonIfNull = "Could not locate a GridLayoutGroup (try selecting Canvas_Board/MergeBoard or a child slot).";
            return null;
        }

        private static bool LooksLikeSlot(Transform t)
        {
            if (t == null) return false;
            if (SlotNameRx.IsMatch(t.name)) return true;
            // also treat as slot if it has a Button or BoardTileView
            if (t.GetComponent<Button>() != null) return true;

            // avoid a hard reference to runtime assembly: check by full name
            foreach (var c in t.GetComponents<Component>())
            {
                if (c == null) continue;
                var fn = c.GetType().FullName;
                if (fn == "AQ.App.UI.Board.BoardTileView") return true;
            }
            return false;
        }

        // Version-safe finders (no name collisions with Unity APIs)
        private static T FindOne<T>() where T : UnityEngine.Object
        {
#if UNITY_2023_1_OR_NEWER
            return UnityEngine.Object.FindFirstObjectByType<T>();
#else
            return UnityEngine.Object.FindObjectOfType<T>();
#endif
        }

        private static T[] FindAll<T>() where T : UnityEngine.Object
        {
#if UNITY_2023_1_OR_NEWER
            return UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
    #if UNITY_2020_1_OR_NEWER
            return UnityEngine.Object.FindObjectsOfType<T>(true);
    #else
            return UnityEngine.Object.FindObjectsOfType<T>();
    #endif
#endif
        }
    }
}
#endif
