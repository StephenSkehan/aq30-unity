using UnityEngine;

namespace AQ.App.Leads
{
    /// <summary>
    /// Binds the repository to the bar and keeps the bar rebuilt when the repo changes.
    /// </summary>
    [DefaultExecutionOrder(-5)]
    public sealed class LeadsRuntimeGlue : MonoBehaviour
    {
        [SerializeField] private LeadsRepository repo;
        [SerializeField] private LeadsBarView    bar;

        private bool _bound;

        private void Awake()
        {
            if (!repo) repo = FindAnyObjectByType<LeadsRepository>();
            if (!bar)  bar  = FindAnyObjectByType<LeadsBarView>();
        }

        private void Start()
        {
            if (!repo || !bar)
            {
                Debug.LogWarning($"[LeadsGlue] Missing pieces. repo={(repo ? "OK" : "<none>")} bar={(bar ? "OK" : "<none>")}", this);
                return;
            }

            BindOnce();
        }

        public void BindOnce()
        {
            if (_bound || !repo || !bar) return;

            repo.LeadsChanged += OnLeadsChanged;
            _bound = true;

            OnLeadsChanged(); // paint now
            Debug.Log("[LeadsGlue] Bound and live.", this);
        }

        private void OnDestroy()
        {
            if (_bound && repo != null)
                repo.LeadsChanged -= OnLeadsChanged;

            _bound = false;
        }

        private void OnLeadsChanged()
        {
            if (repo == null || bar == null) return;

            bar.Rebuild(repo.CurrentLeads);
        }
    }
}
