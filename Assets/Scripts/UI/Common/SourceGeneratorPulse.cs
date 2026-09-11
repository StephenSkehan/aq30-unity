using System.Collections;
using UnityEngine;
using AQ.App.UI.Board;

namespace AQ.App.UI.Common
{
    /// <summary>
    /// Pulses the board generator that produces a given item family (SHOW ME
    /// trail, SAS FTUE I3): the visible half of "where does this item come
    /// from". Runs a few seconds so it is still going when the family popup
    /// closes; restores the tile's scale exactly.
    /// </summary>
    public static class SourceGeneratorPulse
    {
        private const float Seconds = 6f;

        public static void PulseFor(MergeBoardController board, string itemFamily)
        {
            if (board == null || string.IsNullOrEmpty(itemFamily)) return;

            var tile = FindSourceGenerator(board, itemFamily);
            if (tile == null) return;

            var go = new GameObject("__SourceGenPulse");
            go.AddComponent<PulseRunner>().Run(tile);
        }

        /// <summary>First board generator whose drop table (any tier) rolls the family.</summary>
        private static BoardTileView FindSourceGenerator(MergeBoardController board, string itemFamily)
        {
            for (int r = 0; r < board.Rows; r++)
                for (int c = 0; c < board.Cols; c++)
                {
                    var v = board.Get(r, c);
                    if (v == null || v.IsEmpty || v.Kind != TileKind.Generator) continue;

                    var so = board.FindGeneratorType(board.GetFamily(v));
                    if (so == null || so.tiers == null) continue;

                    foreach (var tierCfg in so.tiers)
                    {
                        if (tierCfg?.dropTable == null) continue;
                        foreach (var e in tierCfg.dropTable)
                            if (e.type == AQ.App.Generators.DropType.Item && e.itemFamily == itemFamily)
                                return v;
                    }
                }
            return null;
        }

        private sealed class PulseRunner : MonoBehaviour
        {
            public void Run(BoardTileView tile) => StartCoroutine(Pulse(tile));

            private IEnumerator Pulse(BoardTileView tile)
            {
                float t = 0f;
                while (t < Seconds && tile != null && !tile.IsEmpty && tile.itemImage != null)
                {
                    t += Time.unscaledDeltaTime;
                    float phase = (Mathf.Sin(t * Mathf.PI * 2f / 0.9f) + 1f) * 0.5f;
                    tile.itemImage.transform.localScale = Vector3.one * (1f + 0.15f * phase); // +50% (2026-08-22)
                    yield return null;
                }
                if (tile != null && tile.itemImage != null)
                    tile.itemImage.transform.localScale = Vector3.one;
                Destroy(gameObject);
            }
        }
    }
}
