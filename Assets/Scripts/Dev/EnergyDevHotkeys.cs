using UnityEngine;
using AQ.App.Config;
using AQ.App.Services;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem; // Keyboard
#endif

namespace AQ.App.Dev
{
    /// <summary>
    /// Editor/Dev-only hotkeys for energy testing (no effect in Release):
    ///   F6 -> +1
    ///   F7 -> +10
    ///   F8 -> Fill to Cap
    /// Works with both the new Input System and the legacy Input Manager.
    /// Requires FeatureFlags.EnergySystem to be ON.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnergyDevHotkeys : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        void Update()
        {
            var flags = FeatureFlagsRuntime.Current;
            if (flags == null || !flags.EnergySystem) return;

            var cfg = EnergyRuntime.Config;
            var mgr = EnergyRuntime.Manager;
            if (cfg == null || mgr == null) return;

            // --- New Input System path ---
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.f6Key.wasPressedThisFrame)
                {
                    mgr.AddOverflow(1);
                    Debug.Log("[EnergyDev] +1");
                }
                else if (kb.f7Key.wasPressedThisFrame)
                {
                    mgr.AddOverflow(10);
                    Debug.Log("[EnergyDev] +10");
                }
                else if (kb.f8Key.wasPressedThisFrame)
                {
                    var add = Mathf.Max(0, mgr.Cap - mgr.Current);
                    mgr.AddOverflow(add);
                    Debug.Log("[EnergyDev] Fill to Cap");
                }
                return; // handled
            }
#endif

            // --- Legacy Input Manager path ---
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.F6))
            {
                mgr.AddOverflow(1);
                Debug.Log("[EnergyDev] +1");
            }
            else if (Input.GetKeyDown(KeyCode.F7))
            {
                mgr.AddOverflow(10);
                Debug.Log("[EnergyDev] +10");
            }
            else if (Input.GetKeyDown(KeyCode.F8))
            {
                var add = Mathf.Max(0, mgr.Cap - mgr.Current);
                mgr.AddOverflow(add);
                Debug.Log("[EnergyDev] Fill to Cap");
            }
#endif
        }
#else
        // In release builds this does nothing.
        void Update() { }
#endif
    }
}
