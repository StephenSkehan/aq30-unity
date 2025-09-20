using UnityEngine;
using AQ.App.Composition;

namespace AQ.App.UI.MergeBoard
{
    public sealed class MergeServiceStub : IMergeService
    {
        public bool TryMerge(Vector2Int a, Vector2Int b)
        {
            // Stub: always "succeeds" for now. Replace with domain calls later.
            return true;
        }

        public bool TryMerge(int ax, int ay, int bx, int by)
        {
            // Stub route for tests/adapter shims
            return true;
        }
    }
}
