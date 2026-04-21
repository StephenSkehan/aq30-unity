using System;
using System.Collections.Generic;

[Serializable]
public class SaveBlob
{
    public int Version = 2;
    public string ActiveThemeName;          // persisted by name via registry
    public string DialogueNodeId;           // current node id
    public BoardState Board = new BoardState();
    public List<LeadSaveEntry> Leads = new List<LeadSaveEntry>();

    [Serializable]
    public class BoardState
    {
        public List<Item> Items = new List<Item>();
        [Serializable] public class Item { public string Id; public float X; public float Y; public string Type; }
    }

    /// <summary>
    /// Runtime satisfaction state for a single lead. Stored separately from the
    /// LeadData ScriptableObject so play progress is never baked into art assets.
    /// </summary>
    [Serializable]
    public class LeadSaveEntry
    {
        public string LeadId;
        /// <summary>Bitmask matching ActiveLeadState.RequirementMask (up to 8 requirements).</summary>
        public byte RequirementMask;
        /// <summary>LeadState enum stored as string to survive enum reordering.</summary>
        public string State;
    }
}
