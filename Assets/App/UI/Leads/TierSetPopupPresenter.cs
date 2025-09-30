using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Leads
{
    public sealed class TierSetPopupPresenter : MonoBehaviour
    {
        [SerializeField] private CanvasGroup cg;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Transform iconGrid;     // 6 children with Image
        [SerializeField] private Image highlight;        // moves over selected
        [SerializeField] private Button closeButton;

        void Awake()
        {
            if (closeButton) closeButton.onClick.AddListener(Hide);
            Hide();
        }

        public void Show(string groupTitle, List<Sprite> tierSprites, int selectedIndex)
        {
            if (titleText) titleText.text = groupTitle ?? "";
            // populate icons
            int i = 0;
            foreach (Transform child in iconGrid)
            {
                if (child.name.StartsWith("Icon"))
                {
                    var img = child.GetComponent<Image>();
                    if (img)
                    {
                        img.sprite = (tierSprites != null && i < tierSprites.Count) ? tierSprites[i] : null;
                        img.enabled = img.sprite != null;
                        i++;
                    }
                }
            }
            // position highlights
            if (highlight && selectedIndex >= 0 && selectedIndex < iconGrid.childCount)
            {
                var target = iconGrid.GetChild(selectedIndex) as RectTransform;
                var hrt = highlight.rectTransform;
                hrt.SetParent(target, false);
                hrt.anchorMin = hrt.anchorMax = new Vector2(0.5f, 0.5f);
                hrt.anchoredPosition = Vector2.zero;
                highlight.enabled = true;
            }

            if (cg) { cg.alpha = 1; cg.blocksRaycasts = true; cg.interactable = true; }
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (cg) { cg.alpha = 0; cg.blocksRaycasts = false; cg.interactable = false; }
            if (highlight) highlight.enabled = false;
            gameObject.SetActive(true); // keep alive; hidden via CG
        }
    }
}
