using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AQ.App.Home
{
    /// <summary>Big "Resume Case" card: routes to Board→LeadsAvailable.</summary>
    public class HomeResumeCard : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;     // e.g., "Resume Case"
        [SerializeField] private TMP_Text sublineText;   // e.g., "Last objective: Lift a usable print"
        [SerializeField] private Button resumeButton;

        public UnityEvent OnResumeClicked;

        private void Awake()
        {
            if (titleText) titleText.text = "Resume Case";
            if (sublineText) sublineText.text = "Continue from Leads";
            resumeButton.onClick.AddListener(() => OnResumeClicked?.Invoke());
        }

        // Call this from a binder after reading save to show last objective
        public void SetSubline(string copy)
        {
            if (sublineText) sublineText.text = copy;
        }
    }
}
