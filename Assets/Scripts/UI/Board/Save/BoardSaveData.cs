// Assets/Scripts/UI/Board/Save/BoardSaveData.cs
using System;
using UnityEngine;

namespace AQ.App.UI.Board
{
    [Serializable]
    public sealed class BoardCellData
    {
        public int r;
        public int c;
        public string kind; // "Empty", "Item", "Generator"
        public int tier;
    }

    [Serializable]
    public sealed class BoardSaveData
    {
        public int schemaVersion = 1;
        public int rows;
        public int cols;
        public BoardCellData[] cells;

        public static readonly BoardSaveData Empty = new BoardSaveData { schemaVersion = 1, rows = 0, cols = 0, cells = Array.Empty<BoardCellData>() };
    }
}
