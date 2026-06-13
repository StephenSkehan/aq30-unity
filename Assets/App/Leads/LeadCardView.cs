// Assets/App/Leads/LeadCardView.cs
// Prefab-first lead card view. Binds an ILeadCardModel to UI elements;
// never rebuilds layout at runtime.

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.Leads
{
    [DisallowMultipleComponent]
    public sealed class LeadCardView : MonoBehaviour
    {
        /// <summary>Fired the first time a card transitions to CanProceed = true. Used by FTUE hint.</summary>
        public static event Action<LeadCardView> CardBecameReady;
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
        bool          _wasCanProceed;

        /// <summary>RectTransform of the Proceed button — used by FTUE hint to position itself.</summary>
        public RectTransform ProceedButtonRect =>
            proceedButton ? proceedButton.GetComponent<RectTransform>() : null;

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

            if (model.CanProceed && !_wasCanProceed)
                CardBecameReady?.Invoke(this);
            _wasCanProceed = model.CanProceed;
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
            // Handles both LeadCard.prefab names and LeadCardView.prefab names
            if (!title)    title    = FindDeep<TMP_Text>("Text_Title")    ?? FindDeep<TMP_Text>("Title");
            if (!subtitle) subtitle = FindDeep<TMP_Text>("Text_OneLiner") ?? FindDeep<TMP_Text>("Text_Objective") ?? FindDeep<TMP_Text>("OneLiner");
            if (!action)   action   = FindDeep<TMP_Text>("Text_ActionTag") ?? FindDeep<TMP_Text>("Text_Cost") ?? FindDeep<TMP_Text>("Action");
            if (!reqRow)   reqRow   = FindTransformDeep(transform, "RewardsRow")
                                      ?? FindTransformDeep(transform, "Badges")
                                      ?? FindTransformDeep(transform, "Requirements")
                                      ?? FindTransformDeep(transform, "ReqRow");
            if (!actorAnchor) actorAnchor = FindTransformDeep(transform, "ActorAnchor") as RectTransform;
            if (!actorBadge)
            {
                var anchor = actorAnchor ? actorAnchor : FindTransformDeep(transform, "ActorAnchor");
                if (anchor) actorBadge = FindDeep<Image>(anchor, "ActorBadge");
                if (!actorBadge) actorBadge = FindDeep<Image>("ActorBadge") ?? FindDeep<Image>("Portrait");
            }
            if (!proceedButton)
            {
                var p = FindTransformDeep(transform, "Button_Proceed")
                        ?? FindTransformDeep(transform, "Proceed");
                if (p) proceedButton = p.GetComponentInChildren<Button>(true);
            }
        }

        T FindDeep<T>(string n) where T : Component
        {
            foreach (var c in GetComponentsInChildren<T>(true))
                if (c.gameObject.name == n) return c;
            return null;
        }

        static T FindDeep<T>(Transform root, string n) where T : Component
        {
            foreach (var c in root.GetComponentsInChildren<T>(true))
                if (c.gameObject.name == n) return c;
            return null;
        }

        static Transform FindTransformDeep(Transform root, string n)
        {
            if (root.name == n) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var hit = FindTransformDeep(root.GetChild(i), n);
                if (hit) return hit;
            }
            return null;
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
