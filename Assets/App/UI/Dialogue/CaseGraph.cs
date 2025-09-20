using UnityEngine;
using System;

[CreateAssetMenu(fileName="CaseGraph", menuName="AQ/CaseGraph", order=20)]
public class CaseGraph : ScriptableObject
{
    [Serializable] public class Choice { public string text; public string nextId; }

    [Serializable] public class Node {
        public string id;
        public string speaker;
        [TextArea(2,5)] public string line;
        public string nextId;                // for linear nodes
        public Choice[] choices;             // if length>0, use choices instead of nextId
    }

    public string startId = "START";
    public Node[] nodes;

    public Node Get(string nodeId) {
        if (nodes == null) return null;
        for (int i=0;i<nodes.Length;i++) if (nodes[i].id == nodeId) return nodes[i];
        return null;
    }
}
