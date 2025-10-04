// Assets/Scripts/UI/Board/FX/BoardFxObserver.cs
using System.Collections.Generic;
using UnityEngine;

namespace AQ.App.UI.Board
{
    /// <summary>
    /// Detects tile state changes and triggers FX:
    ///  - Spawn: empty -> non-empty
    ///  - Merge: one emptied, the other same kind tier+1
    ///  - Swap:  two non-empty tiles exchanged (kind,tier)
    ///  - Move:  one emptied, the other received the same (kind,tier)
    /// Robust to init order: waits until the controller grid is ready.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MergeBoardController))]
    [RequireComponent(typeof(BoardFxPlayer))]
    public sealed class BoardFxObserver : MonoBehaviour
    {
        MergeBoardController ctrl;
        BoardFxPlayer fx;

        struct Snap
        {
            public TileKind kind;
            public int tier;

            public static Snap Of(BoardTileView v)
            {
                if (!v) return default;
                return new Snap { kind = v.Kind, tier = v.Tier };
            }

            public bool Equals(Snap other) => kind == other.kind && tier == other.tier;
            public bool IsEmpty => kind == TileKind.Empty;
        }

        Snap[,] prev;
        bool initialized;

        void Awake()
        {
            ctrl = GetComponent<MergeBoardController>();
            fx   = GetComponent<BoardFxPlayer>();
        }

        void LateUpdate()
        {
            if (!initialized)
            {
                if (GridReady())
                {
                    Capture();
                    initialized = true;
                    // Debug.Log("[FX] Observer initialized (grid detected).");
                }
                else
                {
                    return; // wait for controller to finish binding grid
                }
            }

            DetectAndPlay();
        }

        bool GridReady()
        {
            if (ctrl == null) return false;
            // rows/cols exist, but grid may not yet be built. Probe safely.
            try
            {
                if (ctrl.Rows <= 0 || ctrl.Cols <= 0) return false;
                var v = ctrl.Get(0, 0); // will throw or return null if grid not built
                return v != null;
            }
            catch
            {
                return false;
            }
        }

        void Capture()
        {
            prev = new Snap[ctrl.Rows, ctrl.Cols];
            for (int r = 0; r < ctrl.Rows; r++)
                for (int c = 0; c < ctrl.Cols; c++)
                    prev[r, c] = Snap.Of(ctrl.Get(r, c));
        }

        void DetectAndPlay()
        {
            var changed = new List<(int r, int c, Snap before, Snap after)>(4);

            for (int r = 0; r < ctrl.Rows; r++)
            for (int c = 0; c < ctrl.Cols; c++)
            {
                var v = ctrl.Get(r, c);
                var after  = Snap.Of(v);
                var before = prev[r, c];
                if (!before.Equals(after))
                    changed.Add((r, c, before, after));
            }

            if (changed.Count == 0)
                return;

            if (changed.Count == 1)
            {
                // SPAWN: empty -> non-empty
                var (r, c, before, after) = changed[0];
                if (before.IsEmpty && !after.IsEmpty)
                    fx.PlaySpawn(ctrl.Get(r, c));
            }
            else if (changed.Count == 2)
            {
                var a = changed[0];
                var b = changed[1];
                var va = ctrl.Get(a.r, a.c);
                var vb = ctrl.Get(b.r, b.c);

                bool aEmptied = a.after.IsEmpty && !a.before.IsEmpty;
                bool bEmptied = b.after.IsEmpty && !b.before.IsEmpty;

                if (aEmptied ^ bEmptied)
                {
                    // Exactly one tile emptied -> MERGE or MOVE
                    var from = aEmptied ? va : vb;
                    var into = aEmptied ? vb : va;

                    var beforeInto = aEmptied ? b.before : a.before;
                    var afterInto  = aEmptied ? b.after  : a.after;
                    var beforeFrom = aEmptied ? a.before : b.before;

                    bool merge = (afterInto.kind == beforeInto.kind) && (afterInto.tier == beforeInto.tier + 1);
                    if (merge)
                        fx.PlayMerge(from, into, null);
                    else
                    {
                        bool moved = (afterInto.kind == beforeFrom.kind) && (afterInto.tier == beforeFrom.tier);
                        if (moved)
                            fx.PlayMove(into);
                    }
                }
                else
                {
                    // SWAP: both non-empty before & after; payloads exchanged (kind,tier)
                    bool nonEmptyBefore = !a.before.IsEmpty && !b.before.IsEmpty;
                    bool nonEmptyAfter  = !a.after.IsEmpty  && !b.after.IsEmpty;

                    bool swapped = nonEmptyBefore && nonEmptyAfter &&
                                   a.before.kind == b.after.kind && a.before.tier == b.after.tier &&
                                   b.before.kind == a.after.kind && b.before.tier == a.after.tier;

                    if (swapped)
                        fx.PlaySwap(va, vb);
                }
            }

            // Refresh snapshot after handling
            Capture();
        }
    }
}
