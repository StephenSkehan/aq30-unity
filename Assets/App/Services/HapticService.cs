// Assembly: AQ.App
// Purpose: Impact haptics (economy/game-feel spec: light on merge, medium on
// lead fulfilled, heavy on case resolve). No-op everywhere except iOS device.

using UnityEngine;
using AQ.App.Events;
using AQ.App.Presentation;

#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace AQ.App.Services
{
    public static class HapticService
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void _aqHapticImpact(int style);
#endif

        public static void Light()  => Impact(0);
        public static void Medium() => Impact(1);
        public static void Heavy()  => Impact(2);

        private static void Impact(int style)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _aqHapticImpact(style);
#endif
        }

        // Heavy on case resolution comes via the bus so nothing else needs wiring.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void SubscribeCaseResolved()
        {
            GlobalBus.Bus.Subscribe<CaseResolvedEvent>(_ => Heavy());
        }
    }
}
