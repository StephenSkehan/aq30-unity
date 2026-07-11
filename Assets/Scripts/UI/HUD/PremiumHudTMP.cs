using TMPro;
using UnityEngine;
using AQ.App.Economy;
using AQ.SharedKernel.Economy;

namespace AQ.App.UI.HUD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class PremiumHudTMP : MonoBehaviour
    {
        public TextMeshProUGUI label;

        [Min(0.03f)]
        public float refreshSeconds = 0.1f;

        private float _nextAt;

        private void Awake()
        {
            if (!label) label = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextAt) return;
            _nextAt = Time.unscaledTime + refreshSeconds;

            var wallet = WalletLocator.Instance;
            if (label) label.text = $"{wallet?.Get(Currency.Premium) ?? 0}";
        }
    }
}
