using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.App.UI.Leads
{
    /// <summary>
    /// Presenter that binds a LeadCardData into the Lead Card prefab.
    /// Field names are public on purpose because Editor fixers wire them directly.
    /// </summary>
    public class LeadCardPresenter : MonoBehaviour
    {
        // ----- Public wires expected by Editor utilities (names matter) -----
        [Header("Card Visual")]
        public Image background;
        public Button wholeCardButton;

        [Header("Texts")]
        public TMP_Text titleText;
        public TMP_Text objectiveText;
        public TMP_Text leadIdText;

        [Header("Actor")]
        public Image actorAnchor;

        [Header("Layout Rows")]
        public RectTransform requirementsRow;
        public RectTransform rewardsRow;

        [Header("Requirement Slots (0..2)")]
        public RequirementSlotView[] slots = new RequirementSlotView[3];

        [Header("Popup (optional)")]
        public TierSetPopup tierSetPopup;

        // ----- Runtime state -----
        private LeadCardData _data;

        /// <summary>Bind full card data and optional popup (used by requirement clicks).</summary>
        public void Bind(LeadCardData data, TierSetPopup popup = null)
        {
            _data = data;
            if (popup != null) tierSetPopup = popup;

            // Title / Objective / LeadId
            if (titleText     != null) titleText.text     = Safe(data?.Title);
            if (objectiveText != null) objectiveText.text = Safe(data?.Objective);
            if (leadIdText    != null) leadIdText.text    = string.IsNullOrEmpty(data?.LeadId) ? string.Empty : $"#{data.LeadId}";

            // Actor image
            if (actorAnchor != null)
            {
                var sprite = data?.ActorBadge;
                actorAnchor.sprite = sprite;
                actorAnchor.preserveAspect = true;
                actorAnchor.color = sprite != null ? Color.white : new Color(1, 1, 1, 0);
            }

            // Requirements
            SetupSlotsFromData();

            // Rewards row is optional (leave as-is for now)

            // Whole-card button for complete state (optional)
            if (wholeCardButton != null)
            {
                wholeCardButton.onClick.RemoveAllListeners();
                wholeCardButton.onClick.AddListener(OnWholeCardClicked);
            }

            RefreshVisual();
        }

        // ----- Helpers -----

        private void SetupSlotsFromData()
        {
            // Hide all by default
            for (int i = 0; i < slots.Length; i++)
                if (slots[i] != null) slots[i].gameObject.SetActive(false);

            var reqs = _data?.Requirements;
            if (reqs == null || reqs.Count == 0) return;

            for (int i = 0; i < slots.Length && i < reqs.Count; i++)
            {
                var slot = slots[i];
                if (slot == null) continue;

                slot.gameObject.SetActive(true);
                slot.Bind(reqs[i]);

                // rewire click → open tier popup
                slot.onClick.RemoveListener(OnRequirementClicked);
                slot.onClick.AddListener(OnRequirementClicked);
            }
        }

        private void OnRequirementClicked(RequirementData req)
        {
            if (req == null || tierSetPopup == null) return;

            var tiers = (IList<Sprite>)(req.Tiers ?? new List<Sprite>());
            int hi = Mathf.Clamp(req.TierIndex, 0, Mathf.Max(0, tiers.Count - 1));
            tierSetPopup.Show(Safe(req.GroupTitle), tiers, hi);
        }

        private void OnWholeCardClicked()
        {
            // In real game this is "complete" action. For now, noop.
            // You can add a debug log if helpful.
            // Debug.Log($"Lead '{_data?.Title}' clicked (state={_data?.VisualState})", this);
        }

        private void RefreshVisual()
        {
            if (background == null) return;

            // Card colors by state (white, eggshell blue, pale green)
            var c = Color.white;
            switch (_data?.VisualState ?? CardState.New)
            {
                case CardState.InProgress: c = Hex(0xEAF2FF); break;
                case CardState.Complete:   c = Hex(0xE9F8EE); break;
                default:                   c = Color.white;   break;
            }
            background.color = c;
        }

        private static string Safe(string s) => string.IsNullOrEmpty(s) ? string.Empty : s;

        private static Color Hex(uint rgb)
        {
            float r = ((rgb >> 16) & 0xff) / 255f;
            float g = ((rgb >>  8) & 0xff) / 255f;
            float b = ( rgb        & 0xff) / 255f;
            return new Color(r,g,b,1f);
        }
    }
}
