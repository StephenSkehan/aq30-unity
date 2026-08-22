using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace AQ.App.Overflow
{
    public enum OverflowKind { Item, Generator, Special } // Special: family holds the SpecialId name

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
    /// Player-facing name: THE STASH (Stephen-ruled 2026-08-06).
    /// Persistence: folded into BoardSaveSystem's atomic aggregate at schema 0.8.0 so a
    /// crash can never separate a stash transfer from its board/wallet half (the same
    /// rule the Evidence Locker adopted at 0.7.0). The pre-0.8.0 overflow_state.json is
    /// read once at boot for migration and deleted after the first aggregate save.
    /// </summary>
    public static class OverflowBucketService
    {
        private static readonly List<OverflowTileData> _stack = new();

        public static event Action BucketChanged;

        /// <summary>A REWARD landed in the stash (lead outcome, dossier grant) —
        /// consumed by the advise popup so awards never vanish into the bucket
        /// unannounced (Stephen-ruled 2026-08-22). Deliberate transfers (shop
        /// purchases, board spares) do not raise it.</summary>
        public static event Action<OverflowTileData> RewardArrived;

        /// <summary>Push that announces itself as a reward.</summary>
        public static void Push(OverflowTileData tile, bool announce)
        {
            Push(tile);
            if (announce) RewardArrived?.Invoke(tile);
        }

        public static int Count => _stack.Count;
        public static bool IsEmpty => _stack.Count == 0;

        /// <summary>Read-only view of queued tiles (Mo's Back Room scans it for owned generator types).</summary>
        public static IReadOnlyList<OverflowTileData> Items => _stack;

        public static OverflowTileData? Peek()
            => _stack.Count > 0 ? _stack[_stack.Count - 1] : (OverflowTileData?)null;

        public static void Push(OverflowTileData tile)
        {
            _stack.Add(tile);
            BucketChanged?.Invoke();
        }

        public static OverflowTileData? Pop()
        {
            if (_stack.Count == 0) return null;
            var top = _stack[_stack.Count - 1];
            _stack.RemoveAt(_stack.Count - 1);
            BucketChanged?.Invoke();
            return top;
        }

        public static void Clear()
        {
            _stack.Clear();
            BucketChanged?.Invoke();
            DeleteLegacyFile();
        }

        // --------------- Aggregate hooks (BoardSaveSystem) ---------------

        /// <summary>Snapshot of the stack for the save aggregate.</summary>
        public static List<OverflowTileData> ExportState()
            => new List<OverflowTileData>(_stack);

        /// <summary>Replaces the stack with the aggregate's saved state.</summary>
        public static void ImportState(List<OverflowTileData> items)
        {
            _stack.Clear();
            if (items != null) _stack.AddRange(items);
            BucketChanged?.Invoke();
        }

        /// <summary>Order-sensitive content hash, for the aggregate's change detection.</summary>
        public static int StateHash()
        {
            unchecked
            {
                int h = 23;
                foreach (var t in _stack)
                {
                    h = h * 31 + (int)t.kind;
                    h = h * 31 + (t.family != null ? t.family.GetHashCode() : 0);
                    h = h * 31 + t.tier;
                }
                return h;
            }
        }

        // --------------- Legacy file (pre-0.8.0 migration) ---------------

        /// <summary>
        /// Boot-time migration read of the pre-0.8.0 overflow_state.json. If the
        /// aggregate save is schema 0.8.0+, BoardSaveSystem's ImportState replaces
        /// whatever this loaded.
        /// </summary>
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
                Debug.LogWarning($"[OverflowBucket] legacy load failed: {ex.Message}");
            }
        }

        /// <summary>Removes the pre-0.8.0 standalone file once the aggregate owns the state.</summary>
        public static void DeleteLegacyFile()
        {
            try
            {
                if (File.Exists(FilePath)) File.Delete(FilePath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[OverflowBucket] legacy delete failed: {ex.Message}");
            }
        }

        private static string FilePath
            => System.IO.Path.Combine(Application.persistentDataPath, "overflow_state.json");

        [Serializable]
        private class DTO { public List<OverflowTileData> items; }
    }
}
