using System;
using UnityEngine;

namespace AQ.App
{
    /// <summary>
    /// ScriptableObject defining dialogue graph structure with support for:
    /// - Linear and branching dialogue
    /// - Character portraits and emotions
    /// - Conditional nodes (flag requirements)
    /// - Voice acting clips
    /// - Flag setting on node visit
    /// </summary>
    [CreateAssetMenu(fileName = "CaseGraph", menuName = "AQ/CaseGraph", order = 20)]
    public class CaseGraph : ScriptableObject
    {
        [Serializable]
        public class Choice
        {
            public string text;
            public string nextId;
            
            [Tooltip("Optional flag requirement for this choice to appear")]
            public string requiresFlag;
        }

        [Serializable]
        public class Node
        {
            [Header("Core Data")]
            public string id;
            public string speaker;
            [TextArea(2, 5)] public string line;

            [Header("Visual")]
            [Tooltip("Character portrait sprite to display")]
            public Sprite portrait;
            
            [Tooltip("Emotion/expression for this line")]
            public EmotionType emotion = EmotionType.Neutral;

            [Header("Audio")]
            [Tooltip("Voice acting clip for this line")]
            public AudioClip voiceClip;
            
            [Tooltip("If true, block advancement until audio finishes playing")]
            public bool waitForAudio = false;

            [Header("Conditions")]
            [Tooltip("Required flag for this node to display (e.g., 'has_evidence_001')")]
            public string requiresFlag;
            
            [Tooltip("If true and flag missing, automatically skip to nextId")]
            public bool skipIfFlagMissing = true;

            [Header("Actions")]
            [Tooltip("Flag to set when this node is visited (e.g., 'talked_to_ally')")]
            public string setsFlag;

            [Header("Flow")]
            [Tooltip("Next node ID for linear progression")]
            public string nextId;
            
            [Tooltip("Choice array - if length > 0, uses choices instead of nextId")]
            public Choice[] choices;
        }

        public enum EmotionType
        {
            Neutral,
            Happy,
            Sad,
            Angry,
            Surprised,
            Worried,
            Confused
        }

        [Header("Graph Configuration")]
        public string startId = "START";

        [Header("Stage")]
        [Tooltip("Optional scene backdrop shown while this dialogue runs. Empty = keep the board's current background.")]
        public Sprite stageBackground;
        
        [Header("Nodes")]
        public Node[] nodes;

        /// <summary>
        /// Get node by ID. Returns null if not found.
        /// </summary>
        public Node Get(string nodeId)
        {
            if (nodes == null || string.IsNullOrEmpty(nodeId)) return null;
            
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].id == nodeId)
                    return nodes[i];
            }
            
            return null;
        }

        /// <summary>
        /// Validate graph structure in editor.
        /// </summary>
        private void OnValidate()
        {
            if (nodes == null) return;

            // Check for duplicate IDs
            var ids = new System.Collections.Generic.HashSet<string>();
            foreach (var node in nodes)
            {
                if (string.IsNullOrEmpty(node.id)) continue;
                
                if (!ids.Add(node.id))
                {
                    Debug.LogWarning($"[CaseGraph] Duplicate node ID: {node.id}", this);
                }
            }
        }
    }
}