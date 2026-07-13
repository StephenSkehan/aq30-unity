using UnityEngine;
using UnityEngine.UI;
using AQ.App.UI.Common;

namespace AQ.App.UI.HUD
{
    /// <summary>
    /// Self-wiring HUD "+" button: opens the energy-out / ingot-store popup.
    /// Mirrors ShowSettingsPanelMB — attach next to a Button and it binds itself
    /// on Awake. EnergyOutPopup doubles as the ingot store, so energy+ and
    /// ingot+ both route here.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class ShowEnergyStoreMB : MonoBehaviour
    {
        private void Awake() => GetComponent<Button>().onClick.AddListener(EnergyOutPopup.Show);
    }
}
