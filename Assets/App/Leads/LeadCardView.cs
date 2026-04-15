// Assets/App/Leads/LeadCardView.cs
// Prefab-first lead card view. Binds an ILeadCardModel to UI elements;
// never rebuilds layout at runtime.

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.Leads
{
    [DisallowMultipleComponent]
    public sealed class LeadCardView : MonoBehaviour
    {
        [Header("Prefab References")]
        [SerializeField] private RectTransform actorAnchor;
        [SerializeField] private Image        actorBadge;
        [SerializeField] private TMP_Text     title;
        [SerializeField] private TMP_Text     subtitle;
        [SerializeField] private TMP_Text     action;
        [SerializeField] private Transform    reqRow;
        [SerializeField] private Button       proceedButton;

        [Header("Optional Requirement Chip Prefab")]
        [SerializeField] private GameObject   reqChipPrefab;

        System.Action _proceedHandler;

        // ------------------------ Public API ------------------------

        public void Bind(ILeadCardModel model)
        {
            if (model == null) return;
            CachePrefabRefsIfNeeded();

            SetText(title,    model.Title);
            SetText(subtitle, model.Subtitle);
            SetText(action,   model.ActionTag);

            if (actorBadge != null)
            {
                actorBadge.sprite         = model.ActorPortrait;
                actorBadge.enabled        = model.ActorPortrait != null;
                actorBadge.preserveAspect = true;
            }

            if (reqRow != null && reqChipPrefab != null)
            {
                ClearChildren(reqRow);
                foreach (var r in model.Requirements)
                    AddReqChip(r);
            }

            if (proceedButton)
            {
                proceedButton.interactable = model.CanProceed;
                proceedButton.onClick.RemoveAllListeners();
                if (_proceedHandler != null)
                    proceedButton.onClick.AddListener(() => _proceedHandler());
            }
        }

        public void Rebuild() { /* No-op in prefab mode */ }

        public void SetProceedCallback(System.Action onProceed)
        {
            _proceedHandler = onProceed;
            if (proceedButton)
            {
                proceedButton.onClick.RemoveAllListeners();
                if (_proceedHandler != null)
                    proceedButton.onClick.AddListener(() => _proceedHandler());
            }
        }

        // ------------------------ Unity ------------------------

        void Awake() => CachePrefabRefsIfNeeded();

        // ------------------------ Helpers ------------------------

        void CachePrefabRefsIfNeeded()
        {
            var t = transform;
            title    = title    ? title    : t.Find("Title")   ?.GetComponent<TMP_Text>();
            action   = action   ? action   : t.Find("Action")  ?.GetComponent<TMP_Text>();
            subtitle = subtitle ? subtitle : t.Find("OneLiner")?.GetComponent<TMP_Text>();
            reqRow   = reqRow   ? reqRow   : (t.Find("ReqRow") ?? t.Find("Badges"));

            if (actorAnchor == null) actorAnchor = t.Find("ActorAnchor") as RectTransform;
            if (actorBadge  == null && actorAnchor != null)
                actorBadge = actorAnchor.Find("ActorBadge")?.GetComponent<Image>();

            if (proceedButton == null)
            {
                var p = t.Find("Proceed") ?? t.Find("Button_Proceed");
                if (p) proceedButton = p.GetComponentInChildren<Button>(true);
            }
        }

        static void SetText(TMP_Text label, string value)
        {
            if (!label) return;
            if (!string.IsNullOrEmpty(value)) label.text = value;
        }

        void AddReqChip(LeadRequirement req)
        {
            var go  = Instantiate(reqChipPrefab, reqRow, false);
            var img = go.GetComponentInChildren<Image>(true);
            var txt = go.GetComponentInChildren<TMP_Text>(true);
            if (img) img.sprite = req.Icon;
            if (txt) txt.text   = string.IsNullOrEmpty(req.Label) ? "Req" : req.Label;
        }

        static void ClearChildren(Transform parent)
        {
            if (!parent) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                var c = parent.GetChild(i);
                if (Application.isPlaying) Destroy(c.gameObject);
                else DestroyImmediate(c.gameObject);
            }
        }
    }
}
