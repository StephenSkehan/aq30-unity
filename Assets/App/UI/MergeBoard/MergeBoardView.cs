using System;
using System.Collections.Generic;
using UnityEngine;

namespace AQ.App
{
    /// <summary>
    /// Simple board façade:
    ///  • Tracks a coarse grid position for spawned items
    ///  • Removes items safely (works in EditMode + PlayMode)
    ///  • Spawns items at a grid position (optional prefab)
    ///
    /// NOTE: Does NOT declare ItemView (you already have it).
    /// </summary>
    public class MergeBoardView : MonoBehaviour
    {
        [Header("Layout (optional)")]
        [SerializeField] private RectTransform gridRoot;     // parent for items (can be this)
        [SerializeField] private Vector2 cellSize = new Vector2(100, 100);

        [Header("Spawn (optional)")]
        [SerializeField] private ItemView itemPrefab;        // optional prefab for SpawnItem

        // Some code paths elsewhere call this event after the *first* successful merge.
        public event Action OnFirstMergeCompleted;

        private readonly Dictionary<ItemView, Vector2Int> _gridMap = new Dictionary<ItemView, Vector2Int>();

        private RectTransform GridRootRT
        {
            get
            {
                if (gridRoot != null) return gridRoot;
                gridRoot = GetComponent<RectTransform>();
                return gridRoot;
            }
        }

        /// <summary>Return a best-effort grid coordinate for the given item.</summary>
        public Vector2Int GetGridPos(ItemView item)
        {
            if (item == null) return Vector2Int.zero;

            if (_gridMap.TryGetValue(item, out var pos))
                return pos;

            // Fallback: infer from RectTransform position under the grid root.
            var rt = item.GetComponent<RectTransform>();
            var root = GridRootRT;
            if (rt != null && root != null && rt.parent == root)
            {
                var local = rt.anchoredPosition;
                var cx = cellSize.x <= 0 ? 1f : cellSize.x;
                var cy = cellSize.y <= 0 ? 1f : cellSize.y;
                return new Vector2Int(Mathf.RoundToInt(local.x / cx), Mathf.RoundToInt(local.y / cy));
            }

            return Vector2Int.zero;
        }

        /// <summary>Remove an item from the board. EditMode-safe.</summary>
        public void RemoveItem(ItemView item)
        {
            if (item == null) return;

            _gridMap.Remove(item);

            // Prefer a local safe destroy to avoid hard-depending on any specific DestroyUtil signature.
            SafeDestroy(item.gameObject);
        }

        /// <summary>
        /// Spawn a new item at a grid position. If no prefab is assigned, this is a no-op with a warning.
        /// </summary>
        public ItemView SpawnItem(Vector2Int gridPos, string optionalLabel = null)
        {
            if (itemPrefab == null)
            {
                Debug.LogWarning("[MergeBoardView] No itemPrefab assigned; SpawnItem skipped.");
                return null;
            }

            var root = GridRootRT;
            var instance = Instantiate(itemPrefab, root != null ? root : transform);

            var rt = instance.GetComponent<RectTransform>();
            if (rt != null)
            {
                if (root != null)
                    rt.SetParent(root, worldPositionStays: false);

                rt.anchoredPosition = new Vector2(gridPos.x * cellSize.x, gridPos.y * cellSize.y);
                rt.localScale = Vector3.one;
            }

            _gridMap[instance] = gridPos;
            if (!string.IsNullOrEmpty(optionalLabel))
                TrySetLabel(instance, optionalLabel);

            return instance;
        }

        // --- helpers ---

        private void TrySetLabel(ItemView item, string text)
        {
            var t = item.GetType();
            var prop = t.GetProperty("LabelText",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (prop != null && prop.CanWrite && prop.PropertyType == typeof(string))
            {
                prop.SetValue(item, text);
            }
        }

        private static void SafeDestroy(GameObject go)
        {
            if (go == null) return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEngine.Object.DestroyImmediate(go);
                return;
            }
#endif
            UnityEngine.Object.Destroy(go);
        }

        // Allow external code (like your adapter) to raise "first merge completed" once.
        private bool _firstMergeRaised;
        public void RaiseFirstMergeIfNeeded()
        {
            if (_firstMergeRaised) return;
            _firstMergeRaised = true;
            OnFirstMergeCompleted?.Invoke();
        }
    }
}
