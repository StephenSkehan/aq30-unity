using UnityEngine;

namespace AQ.App
{
    /// <summary>
    /// MonoBehaviour shell the test attaches with AddComponent&lt;DialoguePanel&gt;().
    /// Forwards user input into the bound MiniNarrativeEngine.
    /// </summary>
    public sealed class DialoguePanel : MonoBehaviour
    {
        private MiniNarrativeEngine _engine;

        public void BindEngine(MiniNarrativeEngine engine)
        {
            _engine = engine;
        }

        public void OnContinue()
        {
            _engine?.Advance();
        }

        public void OnChoice(int index)
        {
            _engine?.Choose(index);
        }
    }
}
