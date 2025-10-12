// Assets/Editor/UI/Board/AddBoardTileViewsUtility.cs
// Editor-only utility to ensure each slot has a Button + BoardTileView and the expected child structure.

#if UNITY_EDITOR
using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.Editor.Board
{
    public static class AddBoardTileViewsUtility
    {
        private static readonly Regex SlotNameRx = new Regex(@"^slot_\d{2}_\d{2}$", RegexOptions.Compiled);

        [MenuItem("AQ/Board/Slots/Add BoardTileView to Slots")]
        private static void AddBoardTileViewsToSlots()
        {
            var grid = FindGrid(out string why);
            if (grid == null)
            {
                Debug.LogError("AddBoardTileViewsToSlots: " + why);
                return;
            }

            int updated = 0;
            foreach (Transform child in grid.transform)
            {
                if (!LooksLikeSlot(child)) continue;
                updated += EnsureSlot(child.gameObject) ? 1 : 0;
            }
            Debug.Log($"Slots updated: {updated}.");
        }

        // ---------- Slot shaping ----------

        private static bool EnsureSlot(GameObject slot)
        {
            bool changed = false;

            // Button
            var button = slot.GetComponent<Button>();
            if (button == null)
            {
                Undo.AddComponent<Button>(slot);
                changed = true;
            }

            // BoardTileView (by full type name to avoid hard ref if assembly moved)
            if (!HasComponentByFullName(slot, "AQ.App.UI.Board.BoardTileView"))
            {
                var type = GetTypeByFullName("AQ.App.UI.Board.BoardTileView");
                if (type != null)
                {
                    Undo.AddComponent(slot, type);
                    changed = true;
                }
            }

            // Children: Bg, Item, Highlight, Badge/Count, Icon (non-destructive: only create if missing)
            EnsureImageChild(slot.transform, "Bg",   preserveAspect: false, color: new Color(1, 1, 1, 1), ref changed, stretch: true);
            EnsureImageChild(slot.transform, "Item", preserveAspect: false, color: new Color(1, 1, 1, 1), ref changed, stretch: true);
            EnsureImageChild(slot.transform, "Highlight", preserveAspect: false, color: new Color(0.2f, 0.8f, 1f, 0.18f), ref changed, stretch: true);

            var badge = EnsureImageChild(slot.transform, "Badge", preserveAspect: false, color: new Color(0, 0, 0, 0.6f), ref changed, stretch: false);
            if (badge != null)
            {
                var count = badge.Find("Count");
                if (count == null)
                {
                    var go = new GameObject("Count");
                    Undo.RegisterCreatedObjectUndo(go, "Create Count");
                    count = go.transform;
                }
                if (count.parent != badge)
                {
                    Undo.SetTransformParent(count, badge, "Move Count under Badge");
                }

                var rt = EnsureRect(count);
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.anchoredPosition = Vector2.zero; rt.sizeDelta = Vector2.zero;

                if (!HasComponentByFullName(count.gameObject, "TMPro.TextMeshProUGUI"))
                {
                    var tmpType = GetTypeByFullName("TMPro.TextMeshProUGUI");
                    if (tmpType != null)
                    {
                        Undo.AddComponent(count.gameObject, tmpType);
                        changed = true;
                    }
                }

                var badgeRT = badge.GetComponent<RectTransform>();
                badgeRT.anchorMin = new Vector2(1, 0);
                badgeRT.anchorMax = new Vector2(1, 0);
                badgeRT.anchoredPosition = new Vector2(-10, 10);
                badgeRT.sizeDelta = new Vector2(28, 22);
            }

            var icon = EnsureImageChild(slot.transform, "Icon", preserveAspect: true, color: Color.white, ref changed, stretch: true);
            if (icon != null)
            {
                var rt = icon.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(-2, -2);
            }

            return changed;
        }

        // ---------- Helpers ----------

        private static RectTransform EnsureRect(Transform t)
        {
            var rt = t.GetComponent<RectTransform>();
            if (rt == null) rt = Undo.AddComponent<RectTransform>(t.gameObject);
            return rt;
        }

        // NOTE: required params (including ref) come BEFORE the optional.
        private static Transform EnsureImageChild(
            Transform parent,
            string name,
            bool preserveAspect,
            Color color,
            ref bool changed,
            bool stretch = true)
        {
            var child = parent.Find(name);
            if (child == null)
            {
                child = new GameObject(name).transform;
                Undo.RegisterCreatedObjectUndo(child.gameObject, "Create child");
                child.SetParent(parent, false);
                changed = true;
            }

            var rt = EnsureRect(child);
            if (stretch)
            {
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.anchoredPosition = Vector2.zero; rt.sizeDelta = Vector2.zero;
            }

            var img = child.GetComponent<Image>();
            if (img == null)
            {
                img = Undo.AddComponent<Image>(child.gameObject);
                changed = true;
            }
            img.preserveAspect = preserveAspect;
            img.color = color;

            return child;
        }

        private static bool LooksLikeSlot(Transform t)
        {
            if (t == null) return false;
            if (SlotNameRx.IsMatch(t.name)) return true;
            if (t.GetComponent<Button>() != null) return true;

            foreach (var c in t.GetComponents<Component>())
            {
                if (c == null) continue;
                if (c.GetType().FullName == "AQ.App.UI.Board.BoardTileView") return true;
            }
            return false;
        }

        private static Type GetTypeByFullName(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = asm.GetType(fullName, false);
                if (type != null) return type;
            }
            return null;
        }

        private static bool HasComponentByFullName(GameObject go, string fullName)
        {
            foreach (var c in go.GetComponents<Component>())
            {
                if (c == null) continue;
                if (c.GetType().FullName == fullName) return true;
            }
            return false;
        }

        private static GridLayoutGroup FindGrid(out string reasonIfNull)
        {
            reasonIfNull = string.Empty;
            if (Selection.activeGameObject != null)
            {
                var sel = Selection.activeGameObject;
                var g = sel.GetComponentInParent<GridLayoutGroup>();
                if (g != null) return g;
            }

            foreach (var g in FindAll<GridLayoutGroup>())
            {
                if (g != null && g.gameObject.name.Equals("MergeBoard", StringComparison.Ordinal))
                    return g;
            }
            var any = FindOne<GridLayoutGroup>();
            if (any != null) return any;

            reasonIfNull = "Could not locate a GridLayoutGroup (select Canvas_Board/MergeBoard or a child slot).";
            return null;
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
