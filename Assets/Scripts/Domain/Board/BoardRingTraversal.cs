using System;
using System.Collections.Generic;

namespace AQ.Domain.Board
{
    /// <summary>
    /// Manhattan rings around (sr, sc) on a rows×cols grid, 0-indexed.
    /// Order is clockwise from North within each ring; off-board cells are skipped.
    /// This file has NO Unity dependencies and does not change runtime behavior by itself.
    /// </summary>
    public static class BoardRingTraversal
    {
        /// <summary>
        /// Enumerates cells by increasing Manhattan distance from (sr, sc),
        /// ring-by-ring, starting at distance 1 (origin is excluded).
        /// </summary>
        public static IEnumerable<(int r, int c)> Enumerate(int rows, int cols, int sr, int sc)
        {
            if (rows <= 0 || cols <= 0) yield break;
            // Maximum Manhattan distance needed to reach the farthest corner from (sr, sc).
            int maxD = Math.Max(sr, rows - 1 - sr) + Math.Max(sc, cols - 1 - sc);

            for (int d = 1; d <= maxD; d++)
            {
                foreach (var cell in EnumerateRing(rows, cols, sr, sc, d))
                    yield return cell;
            }
        }

        /// <summary>
        /// Enumerates a single ring at exact Manhattan distance d from (sr, sc),
        /// clockwise from North (top, then NE arc → East → SE arc → South → SW arc → West → NW arc).
        /// </summary>
        public static IEnumerable<(int r, int c)> EnumerateRing(int rows, int cols, int sr, int sc, int d)
        {
            if (d <= 0) yield break;
            int r, c;

            // North spoke
            if (Try(rows, cols, sr - d, sc, out r, out c)) yield return (r, c);

            // NE edge (exclusive of spokes): (sr-(d-1), sc+1) .. (sr-1, sc+(d-1))
            for (int k = 1; k < d; k++)
                if (Try(rows, cols, sr - (d - k), sc + k, out r, out c)) yield return (r, c);

            // East spoke
            if (Try(rows, cols, sr, sc + d, out r, out c)) yield return (r, c);

            // SE edge: (sr+1, sc+(d-1)) .. (sr+(d-1), sc+1)
            for (int k = 1; k < d; k++)
                if (Try(rows, cols, sr + k, sc + (d - k), out r, out c)) yield return (r, c);

            // South spoke
            if (Try(rows, cols, sr + d, sc, out r, out c)) yield return (r, c);

            // SW edge: (sr+(d-1), sc-1) .. (sr+1, sc-(d-1))
            for (int k = 1; k < d; k++)
                if (Try(rows, cols, sr + (d - k), sc - k, out r, out c)) yield return (r, c);

            // West spoke
            if (Try(rows, cols, sr, sc - d, out r, out c)) yield return (r, c);

            // NW edge: (sr-1, sc-(d-1)) .. (sr-(d-1), sc-1)
            for (int k = 1; k < d; k++)
                if (Try(rows, cols, sr - k, sc - (d - k), out r, out c)) yield return (r, c);
        }

        private static bool Try(int rows, int cols, int r, int c, out int rr, out int cc)
        {
            rr = r; cc = c;
            return r >= 0 && r < rows && c >= 0 && c < cols;
        }
    }
}
