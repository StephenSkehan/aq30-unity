using System;
using System.Collections.Generic;

[Serializable]
public class SaveBlob
{
    public int Version = 1;
    public string ActiveThemeName;          // persisted by name via registry
    public string DialogueNodeId;           // current node id
    public BoardState Board = new BoardState();

    [Serializable]
    public class BoardState
    {
        public List<Item> Items = new List<Item>();
        [Serializable] public class Item { public string Id; public float X; public float Y; public string Type; }
    }
}
