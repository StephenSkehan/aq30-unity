using System.Collections.Generic;
using UnityEngine;

namespace AQ.App.Leads
{
    /// <summary>
    /// Subscribes a LeadsBarView to LeadsRuntimeBus.OnLeadsRefreshed.
    ///
    /// Use this when the bar doesn't have a direct repository reference — it relies
    /// on LeadsRepository broadcasting through the static bus on every change.
    ///
    /// For scene-level wiring with explicit inspector references, use LeadsRuntimeGlue instead.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LeadsBarRuntimeBinder : MonoBehaviour
    {
        [Tooltip("If null, taken from the same GameObject.")]
        public LeadsBarView view;

        void Awake()
        {
            if (view == null) view = GetComponent<LeadsBarView>();
        }

        void OnEnable()
        {
            LeadsRuntimeBus.OnLeadsRefreshed += OnLeadsRefreshed;
        }

        void OnDisable()
        {
            LeadsRuntimeBus.OnLeadsRefreshed -= OnLeadsRefreshed;
        }

        void OnLeadsRefreshed(IReadOnlyList<LeadData> leads)
        {
            if (view != null) view.Rebuild(leads);
        }
    }
}
