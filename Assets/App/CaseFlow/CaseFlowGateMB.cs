// Assembly: AQ.App
// Purpose: Enable/disable a target object based on CaseFlow progress.

using UnityEngine;
using AQ.SharedKernel.CaseFlow;

namespace AQ.App.CaseFlow
{
    public sealed class CaseFlowGateMB : MonoBehaviour
    {
        public enum GateMode { AtIndex, AtOrBeyondIndex, HasKeyActive }

        [Header("Gate Rule")]
        public GateMode mode = GateMode.AtIndex;
        [Tooltip("Used for AtIndex / AtOrBeyondIndex")]
        public int requiredIndex = 0;
        [Tooltip("Used for HasKeyActive (exact match to steps array)")]
        public string requiredKey = "FTUE_Entitlements";

        [Header("Target")]
        [Tooltip("Target to show/hide. Defaults to this GameObject.")]
        public GameObject target;

        [Header("Polling")]
        [Tooltip("Check every frame (safe, cheap). You can disable and call Apply() manually from UI/events.")]
        public bool pollEveryFrame = true;

        private ICaseFlowService _svc;

        void Awake()
        {
            if (target == null) target = gameObject;
            _svc = CaseFlowLocator.Instance;
        }

        void OnEnable() => Apply();

        void Update()
        {
            if (pollEveryFrame) Apply();
        }

        public void Apply()
        {
            if (_svc == null) _svc = CaseFlowLocator.Instance;
            if (_svc == null || target == null) return;

            var state = _svc.Current;
            bool show = false;

            switch (mode)
            {
                case GateMode.AtIndex:
                    show = state.StepIndex == requiredIndex;
                    break;
                case GateMode.AtOrBeyondIndex:
                    show = state.StepIndex >= requiredIndex;
                    break;
                case GateMode.HasKeyActive:
                    var activeKey = (state.StepIndex < state.Steps.Count) ? state.Steps[state.StepIndex] : null;
                    show = activeKey == requiredKey;
                    break;
            }

            if (target.activeSelf != show) target.SetActive(show);
        }
    }
}
