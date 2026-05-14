using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace AQ.App.Overflow
{
    public enum OverflowKind { Item, Generator }

    [Serializable]
    public struct OverflowTileData
    {
        public OverflowKind kind;
        /// <summary>For Item: the item family id. For Generator: the generatorTypeId.</summary>
        public string family;
        /// <summary>0-based tier.</summary>
        public int tier;
    }

    /// <summary>
    /// FILO stack for items the player has won, gifted, or purchased but not yet placed on the board.
    /// Persists across sessions via overflow_state.json.
    /// </summary>
    public static class OverflowBucketService
    {
        private static readonly List<OverflowTileData> _stack = new();

        public static event Action BucketChanged;

        public static int Count => _stack.Count;
        public static bool IsEmpty => _stack.Count == 0;

        public static OverflowTileData? Peek()
            => _stack.Count > 0 ? _stack[_stack.Count - 1] : (OverflowTileData?)null;

        public static void Push(OverflowTileData tile)
        {
            _stack.Add(tile);
            BucketChanged?.Invoke();
            Save();
        }

        public static OverflowTileData? Pop()
        {
            if (_stack.Count == 0) return null;
            var top = _stack[_stack.Count - 1];
            _stack.RemoveAt(_stack.Count - 1);
            BucketChanged?.Invoke();
            Save();
            return top;
        }

        public static void Clear()
        {
            _stack.Clear();
            BucketChanged?.Invoke();
            Save();
        }

        public static void Load()
        {
            _stack.Clear();
            var p = FilePath;
            if (!File.Exists(p)) return;
            try
            {
                var dto = JsonUtility.FromJson<DTO>(File.ReadAllText(p, Encoding.UTF8));
                if (dto?.items != null) _stack.AddRange(dto.items);
                BucketChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[OverflowBucket] load failed: {ex.Message}");
            }
        }

        private static string FilePath
            => System.IO.Path.Combine(Application.persistentDataPath, "overflow_state.json");

        private static void Save()
        {
            try
            {
                File.WriteAllText(FilePath, JsonUtility.ToJson(new DTO { items = new List<OverflowTileData>(_stack) }), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[OverflowBucket] save failed: {ex.Message}");
            }
        }

        [Serializable]
        private class DTO { public List<OverflowTileData> items; }
    }
}
