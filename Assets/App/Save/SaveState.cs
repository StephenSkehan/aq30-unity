using System;

namespace AQ.App.Save
{
    [Serializable]
    public sealed class SaveState
    {
        public int LastLevelIndex { get; set; }
        public string BoardHash { get; set; } = string.Empty;
    }
}
