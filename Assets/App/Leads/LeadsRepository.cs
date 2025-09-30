using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AQ.App.Leads
{
    /// <summary>
    /// Runtime store for the active leads list in the scene.
    /// Keeps items as LeadCardSO (what LeadsBarView expects).
    /// </summary>
    [DefaultExecutionOrder(-10)]
    public sealed class LeadsRepository : MonoBehaviour
    {
        // NOTE: public and named exactly "database" so existing editor utilities
        // that reference repo.database keep compiling.
        [SerializeField] public LeadsDatabase database;

        private readonly List<LeadCardSO> _current = new List<LeadCardSO>();
        public IReadOnlyList<LeadCardSO> CurrentLeads => _current;

        /// <summary>Raised whenever the in-memory list changes.</summary>
        public event Action LeadsChanged;

        private void Start()
        {
            if (database != null)
            {
                ReplaceFromDatabase(database);
            }
            else
            {
                Debug.LogWarning("[LeadsRepository] No database assigned.", this);
            }
        }

        public void SetDatabase(LeadsDatabase db) => database = db;

        /// <summary>
        /// Replace the in-memory list with the content of the given database.
        /// Uses reflection to read the serialized list without changing your DB class.
        /// </summary>
        public void ReplaceFromDatabase(LeadsDatabase db)
        {
            database = db;
            _current.Clear();

            if (db != null)
            {
                foreach (var item in ReflectLeadList(db))
                {
                    if (item is LeadCardSO so && so != null)
                        _current.Add(so);
                }
            }

            LeadsChanged?.Invoke();
        }

        /// <summary>Replace with a supplied set of items (already LeadCardSO).</summary>
        public void ReplaceWith(IEnumerable<LeadCardSO> leads)
        {
            _current.Clear();
            if (leads != null) _current.AddRange(leads);
            LeadsChanged?.Invoke();
        }

        /// <summary>
        /// Returns IEnumerable over the DB's private list (e.g., "leads", "_leads", "items", "_items").
        /// </summary>
        private static IEnumerable ReflectLeadList(LeadsDatabase db)
        {
            if (db == null) yield break;

            const BindingFlags BF = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var t = typeof(LeadsDatabase);

            FieldInfo field =
                t.GetField("leads", BF) ??
                t.GetField("_leads", BF) ??
                t.GetField("items", BF) ??
                t.GetField("_items", BF);

            if (field == null)
            {
                Debug.LogWarning("[LeadsRepository] Could not find a serialized list on LeadsDatabase via reflection.");
                yield break;
            }

            if (field.GetValue(db) is not IEnumerable value) yield break;

            foreach (var it in value)
                yield return it;
        }
    }
}
