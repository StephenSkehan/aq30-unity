using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MergeBoardController))]
public class DemoBoardPopulator : MonoBehaviour
{
    [Tooltip("Drop a few Sprites here to visualize the board (e.g., the T1–T3 icons).")]
    public List<Sprite> sampleSprites = new List<Sprite>();

    private MergeBoardController board;

    private void Start()
    {
        board = GetComponent<MergeBoardController>();
        ApplySamples();
    }

    [ContextMenu("Apply Samples")]
    public void ApplySamples()
    {
        if (board == null) board = GetComponent<MergeBoardController>();
        if (board == null || board.Tiles == null) return;

        int s = 0;
        for (int i = 0; i < board.Tiles.Count; i++)
        {
            var tile = board.GetTile(i);
            if (!tile) continue;

            if (sampleSprites.Count > 0 && (i % 6 == 0 || i % 7 == 0))
            {
                var sprite = sampleSprites[s % sampleSprites.Count];
                tile.Bind(sprite, Random.Range(1, 4));
                s++;
            }
            else
            {
                tile.Clear();
            }
        }
    }
}
