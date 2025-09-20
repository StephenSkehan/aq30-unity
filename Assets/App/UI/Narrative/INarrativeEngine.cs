namespace AQ.App
{
    public interface INarrativeEngine
    {
        /// <summary>Advance the narrative. If a choice is required, pass the chosen index; else pass -1.</summary>
        void Advance(int choiceIndex = -1);

        /// <summary>Current speaker name (may be null/empty).</summary>
        string Speaker { get; }

        /// <summary>Current line text (may be null if awaiting choice).</summary>
        string Line { get; }

        /// <summary>Current choices (length 0 if none).</summary>
        string[] Choices { get; }

        /// <summary>True if narrative reached an end state.</summary>
        bool IsComplete { get; }
    }
}