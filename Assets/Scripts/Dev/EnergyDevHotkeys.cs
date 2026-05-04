using UnityEngine;
using UnityEngine.InputSystem; // new Input System
using AQ.App.Economy;
using AQ.SharedKernel.Economy;

namespace AQ.App.Dev
{
    /// <summary>
    /// Editor/dev hotkeys for adjusting energy:
    /// F6 = spend 10
    /// F7 = grant 5
    /// F8 = grant 20
    /// </summary>
    [DefaultExecutionOrder(10000)]
    public sealed class EnergyDevHotkeys : MonoBehaviour
    {
        private void Update()
        {
#if UNITY_EDITOR
            var kb = Keyboard.current;
            if (kb == null) return;

            var wallet = WalletLocator.Instance;
            if (wallet == null) return;

            if (kb.f6Key.wasPressedThisFrame)
                wallet.TrySpend(Currency.Energy, 10, "dev.hotkey");

            if (kb.f7Key.wasPressedThisFrame)
                wallet.Grant("dev.hotkey", Reward.Energy(5));

            if (kb.f8Key.wasPressedThisFrame)
                wallet.Grant("dev.hotkey", Reward.Energy(20));
#endif
        }
    }
}
