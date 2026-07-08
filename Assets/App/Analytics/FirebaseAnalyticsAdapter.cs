// Assembly: AQ.App
// Purpose: IAnalytics backend over Firebase Analytics. Installed by FirebaseBootstrap
// only after FirebaseApp dependency check succeeds.

using System.Collections.Generic;
using Firebase.Analytics;

namespace AQ.App.Analytics
{
    public sealed class FirebaseAnalyticsAdapter : IAnalytics
    {
        public void LogEvent(string name, IDictionary<string, object> parameters = null)
        {
            if (parameters == null || parameters.Count == 0)
            {
                FirebaseAnalytics.LogEvent(name);
                return;
            }

            var list = new List<Parameter>(parameters.Count);
            foreach (var kv in parameters)
            {
                switch (kv.Value)
                {
                    case null:        list.Add(new Parameter(kv.Key, string.Empty)); break;
                    case int i:       list.Add(new Parameter(kv.Key, i)); break;
                    case long l:      list.Add(new Parameter(kv.Key, l)); break;
                    case float f:     list.Add(new Parameter(kv.Key, f)); break;
                    case double d:    list.Add(new Parameter(kv.Key, d)); break;
                    case bool b:      list.Add(new Parameter(kv.Key, b ? 1 : 0)); break;
                    default:          list.Add(new Parameter(kv.Key, kv.Value.ToString())); break;
                }
            }
            FirebaseAnalytics.LogEvent(name, list.ToArray());
        }

        public void SetUserProperty(string name, string value)
        {
            FirebaseAnalytics.SetUserProperty(name, value);
        }
    }
}
