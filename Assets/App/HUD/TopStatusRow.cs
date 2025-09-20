using TMPro;
using UnityEngine;

namespace AQ.App.HUD
{
    /// <summary>%Solved • Evidence • Active Leads • Last Breakthrough</summary>
    public class TopStatusRow : MonoBehaviour
    {
        [SerializeField] private TMP_Text solvedText;
        [SerializeField] private TMP_Text evidenceText;
        [SerializeField] private TMP_Text leadsText;
        [SerializeField] private TMP_Text lastBreakthroughText;

        private int _evidence;
        private int _leads;
        private float _solved; // 0..1
        private System.DateTime _last;

        private void OnEnable() => Refresh();

        public void SetSolved(float fraction)
        {
            _solved = Mathf.Clamp01(fraction);
            Refresh();
        }

        public void SetEvidenceCount(int count)
        {
            _evidence = Mathf.Max(0, count);
            Refresh();
        }

        public void SetActiveLeads(int count)
        {
            _leads = Mathf.Max(0, count);
            Refresh();
        }

        public void TickBreakthroughNow()
        {
            _last = System.DateTime.UtcNow;
            Refresh();
        }

        private void Refresh()
        {
            if (solvedText) solvedText.text = $"Solved {(int)(_solved * 100)}%";
            if (evidenceText) evidenceText.text = $"Evidence {_evidence}";
            if (leadsText) leadsText.text = $"Leads {_leads}";
            if (lastBreakthroughText)
            {
                if (_last == default) lastBreakthroughText.text = "Last ✓ — —";
                else
                {
                    var mins = (int)System.DateTime.UtcNow.Subtract(_last).TotalMinutes;
                    lastBreakthroughText.text = $"Last ✓ {mins}m ago";
                }
            }
        }
    }
}
