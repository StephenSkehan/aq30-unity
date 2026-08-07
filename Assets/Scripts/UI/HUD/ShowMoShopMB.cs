using AQ.App.UI.Shop;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Shop
{
    /// <summary>
    /// Lives on the cash pill's + button: hidden until Mo's Back Room unlocks
    /// (L5 flag + feature flag), then opens the shop popup. Added by the HUD
    /// rebuild tool; the CanvasGroup it manages is created at runtime.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ShowMoShopMB : MonoBehaviour
    {
        private CanvasGroup _cg;
        private float _nextPoll;

        private void Awake()
        {
            if (!Application.isPlaying) return;
            _cg = GetComponent<CanvasGroup>();
            if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
            GetComponent<Button>().onClick.AddListener(MoShopPopup.Show);
            Apply();
        }

        private void Update()
        {
            if (_cg == null || Time.unscaledTime < _nextPoll) return;
            _nextPoll = Time.unscaledTime + 0.5f;
            Apply();
        }

        private void Apply()
        {
            bool on = MoShopService.Unlocked;
            _cg.alpha          = on ? 1f : 0f;
            _cg.interactable   = on;
            _cg.blocksRaycasts = on;
        }
    }
}
