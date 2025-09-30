using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.App.UI.Leads
{
    /// <summary>
    /// Popup that shows a group title and a row/grid of tier icons, highlighting one.
    /// Public field names match Editor fixer expectations.
    /// </summary>
    public class TierSetPopup : MonoBehaviour
    {
        [Header("Wires (must be assigned)")]
        public CanvasGroup canvasGroup;    // was cg
        public Image       blocker;        // full-screen dimmer (optional)
        public RectTransform panelRoot;    // the panel body
        public TMP_Text    titleText;
        public Transform   gridRoot;       // parent for icons (was tiersRoot)
        public Image       tierIconPrefab; // template Image
        public Button      closeButton;

        private readonly List<Image> _pool = new List<Image>(16);

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Hide);
            }
            HideImmediate(); // never block at start
        }

        public void Show(string groupTitle, IList<Sprite> tiers, int highlightIndex)
        {
            // Visible + interactive
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }
            if (blocker != null) blocker.raycastTarget = true;

            if (titleText != null)
                titleText.text = string.IsNullOrEmpty(groupTitle) ? string.Empty : groupTitle;

            int need = tiers != null ? tiers.Count : 0;

            // grow pool
            for (int i = _pool.Count; i < need; i++)
            {
                var img = Instantiate(tierIconPrefab, gridRoot);
                _pool.Add(img);
            }

            // populate
            for (int i = 0; i < _pool.Count; i++)
            {
                var img = _pool[i];
                bool active = i < need;

                if (img != null) img.gameObject.SetActive(active);
                if (!active) continue;

                var sprite = tiers[i];
                img.sprite = sprite;
                img.preserveAspect = true;
                img.color = Color.white;

                // visual highlight via Outline
                var outline = img.GetComponent<Outline>();
                if (outline == null) outline = img.gameObject.AddComponent<Outline>();
                bool isHi = (i == highlightIndex);
                outline.effectColor = isHi ? new Color(0.15f,0.3f,0.5f,0.9f) : new Color(0,0,0,0.2f);
                outline.effectDistance = isHi ? new Vector2(2,2) : new Vector2(1,1);
            }
        }

        public void Hide()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
            if (blocker != null) blocker.raycastTarget = false;
        }

        /// <summary>Instantly hide without tween; used by Editor fixer.</summary>
        public void HideImmediate() => Hide();
    }
}
