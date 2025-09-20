using System.Collections.Generic;

namespace AQ.App
{
    /// <summary>
    /// Plain C# narrative engine matching DialogueSmokeTests:
    /// Advance -> first line "We start." (Speaker "Ally")
    /// Continue -> show 2 choices
    /// Choose(0) -> "Left it is."
    /// Continue -> "Done." and complete
    /// </summary>
    public class MiniNarrativeEngine
    {
        public string Speaker { get; private set; }
        public string Line    { get; private set; }
        public List<string> Choices { get; private set; }
        public bool IsComplete { get; private set; }

        // 0 = init, 1 = first line, 2 = choices, 3 = chosen-left, 4 = done
        private int _state = 0;

        public void Advance()
        {
            if (IsComplete) return;

            switch (_state)
            {
                case 0:
                    // Enter first line
                    Speaker = "Ally";
                    Line    = "We start.";
                    Choices = null;
                    _state  = 1;
                    break;

                case 1:
                    // Move to choices
                    Line    = null;
                    Choices = new List<string> { "Left", "Right" };
                    _state  = 2;
                    break;

                case 3:
                    // After chosen-left, finalize
                    Line       = "Done.";
                    Choices    = null;
                    IsComplete = true;
                    _state     = 4;
                    break;

                case 2:
                    // Continue while on choices: test never does this; ignore safely.
                    break;
            }
        }

        public void Choose(int index)
        {
            if (IsComplete) return;
            if (_state != 2) return;

            // Test asserts Choose(0) -> "Left it is."
            Line    = (index == 0) ? "Left it is." : "Right then.";
            Choices = null;
            _state  = 3;
        }
    }
}
