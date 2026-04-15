using UnityEngine;
using UnityEngine.InputSystem; // new Input System
using AQ.App.Services;

namespace AQ.App.Dev
{
    /// <summary>
    /// Editor/dev hotkeys for adjusting energy:
    /// F6 = consume 10 (min 0)
    /// F7 = +5 (no max)
    /// F8 = +20 (no max)
    /// HUD is self-updating; no direct HUD calls here.
    /// </summary>
    [DefaultExecutionOrder(10000)]
    public sealed class EnergyDevHotkeys : MonoBehaviour
    {
        private EnergyManager _mgr;

        private void Awake()
        {
#if UNITY_EDITOR
            _mgr = EnergyRuntime.Manager;
#endif
        }

        private void Update()
        {
#if UNITY_EDITOR
            // Only run in Editor; safe-guard if Input System isn't ready.
            var kb = Keyboard.current;
            if (kb == null) return;

            if (_mgr == null) _mgr = EnergyRuntime.Manager;
            if (_mgr == null) return;

            // F6: consume 10 (min 0)
            if (kb.f6Key.wasPressedThisFrame)
            {
                _mgr.TryConsume(10);
            }

            // F7: add 5 (no max)
            if (kb.f7Key.wasPressedThisFrame)
            {
                _mgr.Add(5);
            }

            // F8: add 20 (no max)
            if (kb.f8Key.wasPressedThisFrame)
            {
                _mgr.Add(20);
            }
#endif
        }
    }
}
