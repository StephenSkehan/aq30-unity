using UnityEngine;

namespace AQ.App.CaseFlow
{
    /// <summary>Simple logger you can call from a Button.</summary>
    public class DebugLogOnResolveMB : MonoBehaviour
    {
        // Hook this from a Button listener to confirm the overlay fired.
        public void LogNow()
        {
            Debug.Log("[Resolution] OnResolve start");
        }
    }
}
