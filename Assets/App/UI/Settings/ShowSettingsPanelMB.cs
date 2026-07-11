using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Settings
{
    [RequireComponent(typeof(Button))]
    public class ShowSettingsPanelMB : MonoBehaviour
    {
        void Awake() => GetComponent<Button>().onClick.AddListener(GameControlPanelMB.Show);
    }
}
