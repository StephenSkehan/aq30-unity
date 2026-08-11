using UnityEngine;

namespace AQ.App.Boot
{
    /// <summary>
    /// Unity's Rendering Debugger runtime overlay ("Display Stats") opens on
    /// Ctrl+Backquote in the editor and a three-finger double-tap on device —
    /// it kept ambushing QA sessions (2026-08-11). Not our tool; off for good.
    /// </summary>
    public static class DisableRenderingDebugger
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Disable()
        {
            UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
        }
    }
}
