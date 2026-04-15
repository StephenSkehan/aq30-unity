namespace AQ.App
{
    /// <summary>
    /// Single place for the rule: a choice is visible if it has no requiresFlag,
    /// or if that flag is currently set.
    /// </summary>
    public static class ChoiceFilter
    {
        public static CaseGraph.Choice[] GetAvailable(CaseGraph.Choice[] choices)
        {
            if (choices == null || choices.Length == 0)
                return System.Array.Empty<CaseGraph.Choice>();

            var result = new System.Collections.Generic.List<CaseGraph.Choice>(choices.Length);
            foreach (var c in choices)
            {
                if (string.IsNullOrEmpty(c.requiresFlag) || DialogueFlags.Has(c.requiresFlag))
                    result.Add(c);
            }
            return result.ToArray();
        }
    }
}
