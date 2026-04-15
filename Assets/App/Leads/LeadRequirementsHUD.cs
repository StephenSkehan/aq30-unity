using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.App.Leads
{
    /// <summary>Pins beneath the Leads Bar to show the selected lead's ≤3 requirements, large and readable.</summary>
    public class LeadRequirementsHUD : MonoBehaviour
    {
        [SerializeField] private Image[] icons;       // length 3
        [SerializeField] private TMP_Text[] labels;   // length 3
        [SerializeField] private Image[] ticks;       // length 3

        public void Bind(LeadCardSO data, ActiveLeadState state)
        {
            for (int i = 0; i < icons.Length; i++)
            {
                bool has = i < data.Requirements.Length;
                icons[i].gameObject.SetActive(has);
                labels[i].gameObject.SetActive(has);
                ticks[i].gameObject.SetActive(has);

                if (!has) continue;
                var req = data.Requirements[i];
                icons[i].sprite = req.Icon;
// CLEANUP:                 labels[i].text = string.IsNullOrEmpty(req.Label) ? req.ItemId : req.Label;
                ticks[i].enabled = state.IsRequirementMet(i);
            }
        }
    }
}
