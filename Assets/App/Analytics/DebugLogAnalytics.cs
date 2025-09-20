// Assembly: AQ.App
// Purpose: Default no-op-ish analytics that writes to the Console. Safe for Editor and CI.

using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AQ.App.Analytics
{
    public sealed class DebugLogAnalytics : IAnalytics
    {
        public void LogEvent(string name, IDictionary<string, object> parameters = null)
        {
            var sb = new StringBuilder();
            sb.Append("[Analytics] ").Append(name);
            if (parameters != null)
            {
                sb.Append(" { ");
                bool first = true;
                foreach (var kv in parameters)
                {
                    if (!first) sb.Append(", ");
                    sb.Append(kv.Key).Append(": ").Append(kv.Value);
                    first = false;
                }
                sb.Append(" }");
            }
            Debug.Log(sb.ToString());
        }

        public void SetUserProperty(string name, string value)
        {
            Debug.Log($"[Analytics] user_property {name}={value}");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallIfMissing()
        {
            if (AnalyticsLocator.Instance == null)
            {
                AnalyticsLocator.Set(new DebugLogAnalytics());
                Debug.Log("[Analytics] Installed DebugLogAnalytics as default backend.");
            }
        }
    }
}
