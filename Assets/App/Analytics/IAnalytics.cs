// Assembly: AQ.App
// Purpose: App-layer analytics abstraction (no Firebase dependency).

using System.Collections.Generic;

namespace AQ.App.Analytics
{
    public interface IAnalytics
    {
        /// <summary>Log an analytics event with optional parameters.</summary>
        void LogEvent(string name, IDictionary<string, object> parameters = null);

        /// <summary>Set a user property (optional, adapter may ignore).</summary>
        void SetUserProperty(string name, string value);
    }
}
