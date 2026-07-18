#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools
{
    /// <summary>
    /// Closes any Device Simulator window so the classic Game view drives the
    /// render (the form-factor sweep can't switch resolutions while the
    /// simulator is the active play view). Reversible: Window > General >
    /// Device Simulator reopens it.
    /// </summary>
    public static class CloseDeviceSimulator
    {
        [MenuItem("AQ/Dev/Close Device Simulator")]
        public static void Close()
        {
            int closed = 0;
            foreach (var w in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                var tn = w.GetType().FullName ?? "";
                if (tn.Contains("DeviceSimulation") || tn.Contains("SimulatorWindow"))
                {
                    w.Close();
                    closed++;
                }
            }
            Debug.Log($"[FFPrep] Closed {closed} simulator window(s).");
        }
    }
}
#endif
