using System.Collections.Generic;
using UnityEngine;

namespace AQ.App.Leads
{
    /// <summary>
    /// Holds a list of LeadData assets.
    /// Create via: Assets → Create → AQ → Leads → Database
    /// </summary>
    [CreateAssetMenu(fileName = "LeadsDatabase", menuName = "AQ/Leads/Database", order = 11)]
    public sealed class LeadsDatabase : ScriptableObject
    {
        [SerializeField] private List<LeadData> leads = new List<LeadData>();

        public IReadOnlyList<LeadData> Leads => leads;

        public void Add(LeadData lead)
        {
            if (lead == null || leads.Contains(lead)) return;
            leads.Add(lead);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public void Remove(LeadData lead)
        {
            if (lead == null) return;
            if (leads.Remove(lead))
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        public LeadData FindById(string leadId)
        {
            if (string.IsNullOrEmpty(leadId)) return null;
            for (int i = 0; i < leads.Count; i++)
                if (leads[i] != null && leads[i].leadId == leadId) return leads[i];
            return null;
        }
    }
}
