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
        /// visibleChoices is the pre-filtered set from ChoiceFilter.GetAvailable —
        /// the controller just renders what it receives.
        /// </summary>
        public void BindNode(CaseGraph.Node n, CaseGraph.Choice[] visibleChoices)
        {
            if (n == null) return;

            if (Speaker) Speaker.text = n.speaker ?? "";
            if (Body) Body.text = n.line ?? "";

            UpdatePortrait(n.portrait, n.emotion);

            if (visibleChoices != null && visibleChoices.Length > 0)
            {
                BindChoices(visibleChoices);
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

            for (int i = 0; i < choices.Length && i < ChoiceButtons.Length; i++)
            {
                ChoiceButtons[i].gameObject.SetActive(true);
                if (i < ChoiceLabels.Length)
                    ChoiceLabels[i].text = choices[i].text;
            }

            // Hide unused buttons
            for (int i = choices.Length; i < ChoiceButtons.Length; i++)
                ChoiceButtons[i].gameObject.SetActive(false);
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
        /// Normalizes the stage layout each boot: the portrait renders as a large
        /// bust standing on the text strip (not the legacy full-height left
        /// column), and speaker/body reclaim the freed left indent. The bust is
        /// RIGHT-anchored: the cast art faces screen-left, so the character looks
        /// into the scene and their straight-cropped back sits at the screen edge.
        /// </summary>
        public void ApplyStageLayout()
        {
            const float stripTop = 300f / 1920f;

            if (portraitImage != null)
            {
                portraitImage.preserveAspect = true;
                var imageRt = (RectTransform)portraitImage.transform;
                var holder = imageRt.parent != null && imageRt.parent != transform
                    ? (RectTransform)imageRt.parent
                    : imageRt;

                holder.anchorMin = holder.anchorMax = new Vector2(1f, stripTop);
                holder.pivot = new Vector2(1f, 0f);
                holder.sizeDelta = new Vector2(460f, 460f);
                holder.anchoredPosition = new Vector2(-8f, -12f);

                if (holder != imageRt)
                {
                    imageRt.anchorMin = Vector2.zero;
                    imageRt.anchorMax = Vector2.one;
                    imageRt.offsetMin = imageRt.offsetMax = Vector2.zero;
                }
            }

            WidenToStrip(Speaker);
            WidenToStrip(Body);
        }

        static void WidenToStrip(Component text)
        {
            var rt = text != null ? text.transform as RectTransform : null;
            if (rt == null) return;
            var min = rt.anchorMin; min.x = 0.06f; rt.anchorMin = min;
            var max = rt.anchorMax; max.x = 0.94f; rt.anchorMax = max;
            rt.offsetMin = new Vector2(0f, rt.offsetMin.y);
            rt.offsetMax = new Vector2(0f, rt.offsetMax.y);
        }

        /// <summary>
        /// Creates two runtime choice buttons when the prefab has none assigned.
        /// Buttons carry no onClick listeners on purpose: this panel's input is
        /// raw (DialogueRunner.Update hit-tests) because EventSystem routing has
        /// historically failed here — Button components exist for visuals only.
        /// </summary>
        public void EnsureRuntimeChoiceUI()
        {
            if (ChoiceButtons != null && ChoiceButtons.Length > 0) return;

            const int count = 2;
            ChoiceButtons = new Button[count];
            ChoiceLabels  = new Text[count];

            for (int i = 0; i < count; i++)
            {
                var go = new GameObject($"ChoiceBtn_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(transform, false);
                var rt = (RectTransform)go.transform;
                // Stacked above the bottom dialogue strip (which spans 0..300/1920).
                float bottom = (320f + i * 130f) / 1920f;
                rt.anchorMin = new Vector2(0.06f, bottom);
                rt.anchorMax = new Vector2(0.94f, bottom + 110f / 1920f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;

                var img = go.GetComponent<Image>();
                img.color = new Color(0.10f, 0.14f, 0.22f, 0.97f);

                var lblGo = new GameObject("Label", typeof(RectTransform));
                lblGo.transform.SetParent(rt, false);
                var lblRt = (RectTransform)lblGo.transform;
                lblRt.anchorMin = Vector2.zero;
                lblRt.anchorMax = Vector2.one;
                lblRt.offsetMin = new Vector2(24f, 6f);
                lblRt.offsetMax = new Vector2(-24f, -6f);
                var txt = lblGo.AddComponent<Text>();
                txt.fontSize  = 34;
                txt.color     = Color.white;
                txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txt.alignment = TextAnchor.MiddleCenter;
                txt.raycastTarget = false;

                ChoiceButtons[i] = go.GetComponent<Button>();
                ChoiceLabels[i]  = txt;
                go.SetActive(false);
            }
        }

        /// <summary>
        /// Raw-input helper: returns the index of the visible choice button under
        /// the screen point, or -1.
        /// </summary>
        public int ChoiceIndexAtScreenPoint(Vector2 screenPoint)
        {
            if (ChoiceButtons == null) return -1;
            for (int i = 0; i < ChoiceButtons.Length; i++)
            {
                var b = ChoiceButtons[i];
                if (b == null || !b.gameObject.activeInHierarchy) continue;
                if (RectTransformUtility.RectangleContainsScreenPoint(
                        (RectTransform)b.transform, screenPoint, null))
                    return i;
            }
            return -1;
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