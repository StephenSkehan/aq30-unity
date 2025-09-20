using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.App.Leads
{
    /// <summary>
    /// Simple UI row for one lead requirement (Icon  Label  Check).
    /// </summary>
    public class LeadRequirementItem : MonoBehaviour
    {
        [Header("Wiring")]
        public Image icon;
        public TextMeshProUGUI label;
        public Image check;

        [Header("Style")]
        [Range(0.3f, 1f)] public float unmetAlpha = 0.75f;

        public void Bind(string displayName, Sprite sprite, bool met)
        {
            if (label) label.text = displayName ?? "";
            if (icon)  icon.sprite = sprite;
            SetMet(met);
        }

        public void SetMet(bool met)
        {
            if (check) check.enabled = met;

            if (label)
            {
                var c = label.color;
                c.a = met ? 1f : unmetAlpha;
                label.color = c;
            }

            if (icon)
            {
                var c = icon.color;
                c.a = met ? 1f : unmetAlpha;
                icon.color = c;
            }
        }
    }
}
