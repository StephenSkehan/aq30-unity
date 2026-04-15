using System;
using UnityEngine;
using UnityEngine.UI;

#if TMP_PRESENT
using TMPro;
#endif

namespace AQ.App
{
    /// <summary>
    /// Extended DialogueController with support for:
    /// - Character portraits
    /// - Emotion-based animations
    /// - Back button for history navigation
    /// - Choice flag filtering
    /// </summary>
    public class DialogueController : MonoBehaviour
    {
        [Header("Text Components")]
#if TMP_PRESENT
        public TMP_Text Speaker;
        public TMP_Text Body;
        public TMP_Text[] ChoiceLabels;
#else
        public Text Speaker;
        public Text Body;
        public Text[] ChoiceLabels;
#endif

        [Header("Portrait")]
        [Tooltip("Image component for character portraits")]
        public Image portraitImage;
        
        [Tooltip("Optional animator for portrait emotions (neutral, happy, sad, etc.)")]
        public Animator portraitAnimator;

        [Header("Buttons")]
        public Button AdvanceArea;
        public Button[] ChoiceButtons;
        
        [Tooltip("Optional back button for dialogue history")]
        public Button BackButton;

        [Header("Visual Feedback")]
        [Tooltip("Optional panel fade animation")]
        public CanvasGroup panelCanvasGroup;

        // Events
        public event Action AdvanceClicked;
        public event Action<int> ChoiceClicked;
        public event Action BackClicked; // NEW

        void Awake()
        {
            // Wire advance button
            if (AdvanceArea != null)
                AdvanceArea.onClick.AddListener(() => AdvanceClicked?.Invoke());

            // Wire choice buttons
            if (ChoiceButtons != null)
            {
                for (int i = 0; i < ChoiceButtons.Length; i++)
                {
                    int idx = i; // Capture for closure
                    ChoiceButtons[i].onClick.AddListener(() => ChoiceClicked?.Invoke(idx));
                }
            }

            // Wire back button
            if (BackButton != null)
                BackButton.onClick.AddListener(() => BackClicked?.Invoke());

            // Initial state
            ShowChoices(false);
            UpdateBackButton(false);
        }

        /// <summary>
        /// Bind node data to UI elements.
        /// </summary>
        public void BindNode(CaseGraph.Node n)
        {
            if (n == null) return;

            // Update text
            if (Speaker) Speaker.text = n.speaker ?? "";
            if (Body) Body.text = n.line ?? "";

            // Update portrait
            UpdatePortrait(n.portrait, n.emotion);

            // Update choices or show advance button
            if (n.choices != null && n.choices.Length > 0)
            {
                BindChoices(n.choices);
                ShowChoices(true);
            }
            else
            {
                ShowChoices(false);
            }
        }

        void UpdatePortrait(Sprite portrait, CaseGraph.EmotionType emotion)
        {
            if (portraitImage == null) return;

            if (portrait != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.gameObject.SetActive(true);

                // Trigger emotion animation if animator present
                if (portraitAnimator != null)
                {
                    portraitAnimator.SetTrigger(emotion.ToString());
                }
            }
            else
            {
                portraitImage.gameObject.SetActive(false);
            }
        }

        void BindChoices(CaseGraph.Choice[] choices)
        {
            if (ChoiceButtons == null || ChoiceLabels == null) return;

            int validChoiceIndex = 0;

            for (int i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];

                // Check if choice has flag requirement
                bool isAvailable = true;
                if (!string.IsNullOrEmpty(choice.requiresFlag))
                {
                    isAvailable = DialogueFlags.Has(choice.requiresFlag);
                }

                // Skip unavailable choices
                if (!isAvailable) continue;

                // Show this choice if we have a button for it
                if (validChoiceIndex < ChoiceButtons.Length)
                {
                    ChoiceButtons[validChoiceIndex].gameObject.SetActive(true);

                    if (validChoiceIndex < ChoiceLabels.Length)
                    {
                        ChoiceLabels[validChoiceIndex].text = choice.text;
                    }

                    validChoiceIndex++;
                }
            }

            // Hide unused buttons
            for (int i = validChoiceIndex; i < ChoiceButtons.Length; i++)
            {
                ChoiceButtons[i].gameObject.SetActive(false);
            }
        }

        void ShowChoices(bool on)
        {
            if (ChoiceButtons == null) return;

            foreach (var b in ChoiceButtons)
            {
                if (b) b.gameObject.SetActive(on);
            }

            // Toggle advance button (hidden during choices)
            if (AdvanceArea)
                AdvanceArea.gameObject.SetActive(!on);
        }

        /// <summary>
        /// Update back button visibility based on history state.
        /// </summary>
        public void UpdateBackButton(bool canGoBack)
        {
            if (BackButton != null)
            {
                BackButton.gameObject.SetActive(canGoBack);
            }
        }

        /// <summary>
        /// Optional: Fade in panel.
        /// </summary>
        public void FadeIn(float duration = 0.3f)
        {
            if (panelCanvasGroup != null)
            {
                StopAllCoroutines();
                StartCoroutine(FadeCoroutine(0f, 1f, duration));
            }
        }

        /// <summary>
        /// Optional: Fade out panel.
        /// </summary>
        public void FadeOut(float duration = 0.3f)
        {
            if (panelCanvasGroup != null)
            {
                StopAllCoroutines();
                StartCoroutine(FadeCoroutine(1f, 0f, duration));
            }
        }

        System.Collections.IEnumerator FadeCoroutine(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                panelCanvasGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }
            panelCanvasGroup.alpha = to;
        }
    }
}