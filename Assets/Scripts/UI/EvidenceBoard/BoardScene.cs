// Assembly: Assembly-CSharp
// File: Assets/Scripts/UI/EvidenceBoard/BoardScene.cs
// Purpose: One replayable scene on the evidence board. The board pins WHO
//          (portraits) and WHERE (stage backdrops) and lets the player replay
//          the scene. The Listener's scenes are resolved leads (their own
//          resolution dialogue); a package episode's scenes are completed
//          packages (their beat dialogue). Both map onto this.

using AQ.App;
using UnityEngine;

namespace AQ.App.UI.EvidenceBoard
{
    public sealed class BoardScene
    {
        public string id;
        public string title;
        public CaseGraph graph;
        public Sprite actorPortrait;

        public BoardScene(string id, string title, CaseGraph graph, Sprite actorPortrait)
        {
            this.id = id;
            this.title = title;
            this.graph = graph;
            this.actorPortrait = actorPortrait;
        }
    }
}
