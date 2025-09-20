using System.Collections;
using UnityEngine;

namespace AQ.App.CaseFlow
{
    /// Routes a button click to CaseFlowAdvanceOnEventMB.Advance(),
    /// but only once per press (with a short cooldown).
    public class AdvanceOnceMB : MonoBehaviour
    {
        public CaseFlowAdvanceOnEventMB target;
        public float cooldown = 0.20f;

        bool _busy;

        public void OnClick()
        {
            if (_busy || target == null) return;

            _busy = true;
            Debug.Log("[AdvanceOnceMB] Advance()");
            target.Advance();

            if (isActiveAndEnabled) StartCoroutine(ResetBusy());
            else _busy = false;
        }

        IEnumerator ResetBusy()
        {
            yield return new WaitForSecondsRealtime(cooldown);
            _busy = false;
        }
    }
}
