using UnityEngine;

namespace AQ.App.Composition
{
    public interface IMergeService
    {
        // Grid-based merge using Unity's Vector2Int coords
        bool TryMerge(Vector2Int a, Vector2Int b);

        // Overload using explicit ints (x1,y1,x2,y2)
        bool TryMerge(int ax, int ay, int bx, int by);
    }
}
