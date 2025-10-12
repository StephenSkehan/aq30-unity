using System;
using System.Collections.Generic;
using UnityEngine;

namespace AQ.App.Leads
{
    /// <summary>
    /// Scene-side source of truth for leads at runtime.
    /// Subscribes to LeadsRuntimeBus and exposes events for UI.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public sealed class LeadsRepository : MonoBehaviour
    {
        public static LeadsRepository Instance { get; private set; }

        // Optional: default DB to push at start (can be null).
        public LeadsDatabase startupDatabase;

        public event Action<IReadOnlyList<LeadData>> LeadsRefreshed;
        public event Action<LeadData>              LeadStateChanged;

        readonly List<LeadData> _current = new List<LeadData>();
        public IReadOnlyList<LeadData> CurrentLeads => _current;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("LeadsRepository: duplicate instance destroyed.", this);
                DestroyImmediate(gameObject);
                return;
            }
            Instance = this;

            LeadsRuntimeBus.OnPushAll += ReceiveAll;
            LeadsRuntimeBus.OnPushOne += ReceiveOne;

            // Optional convenience: if a DB is assigned, push it once on play.
            if (startupDatabase != null && startupDatabase.Leads != null && startupDatabase.Leads.Count > 0)
                ReceiveAll(startupDatabase.Leads);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            LeadsRuntimeBus.OnPushAll -= ReceiveAll;
            LeadsRuntimeBus.OnPushOne -= ReceiveOne;
        }

        void ReceiveAll(IReadOnlyList<LeadData> leads)
        {
            _current.Clear();
            if (leads != null) _current.AddRange(leads);
            LeadsRefreshed?.Invoke(_current);
        }

        void ReceiveOne(LeadData lead)
        {
            // Don’t rebuild the whole list; tell UI a single item changed.
            LeadStateChanged?.Invoke(lead);
        }
    }
}
