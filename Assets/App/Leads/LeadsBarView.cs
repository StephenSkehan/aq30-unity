using System;
using System.Collections.Generic;
using AQ.App.UI.Leads;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.Leads
{
    public sealed class LeadsBarView : MonoBehaviour
    {
        [Header("Wiring (assign in Inspector)")]
        public ScrollRect scrollRect;
        public RectTransform contentRoot;
        public GameObject cardPrefab;   // LeadCard.prefab (has LeadCardPresenter)

        public event Action<LeadData> ProceedRequested;

        readonly List<GameObject> _spawned = new List<GameObject>();
        readonly Dictionary<LeadData, Button> _proceedByLead = new Dictionary<LeadData, Button>();

        UnityEngine.Object _boundRepo;

        void Awake()
        {
            if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();
            if (contentRoot == null && scrollRect != null) contentRoot = scrollRect.content;
        }

        public void Bind(UnityEngine.Object repo) { _boundRepo = repo; }

        public void Rebuild() { }

        public void Rebuild(IReadOnlyList<LeadData> leads)
        {
            if (contentRoot == null || cardPrefab == null) return;

            for (int i = contentRoot.childCount - 1; i >= 0; i--)
            {
                var child = contentRoot.GetChild(i);
                if (child != null) DestroyImmediate(child.gameObject);
            }
            _spawned.Clear();
            _proceedByLead.Clear();

            if (leads == null) return;

            for (int i = 0; i < leads.Count; i++)
            {
                var so = leads[i];
                var go = Instantiate(cardPrefab, contentRoot);
                go.name = $"LeadCard_{i}_{(so != null ? so.name : "Null")}";

                var presenter = go.GetComponent<LeadCardPresenter>();
                if (presenter != null)
                {
                    presenter.Bind(ToCardData(so));
                    bool hasReqs = so != null && so.requirements != null && so.requirements.Length > 0;
                    if (presenter.requirementsRow != null)
                        presenter.requirementsRow.gameObject.SetActive(hasReqs);
                    if (presenter.rewardsRow != null)
                        presenter.rewardsRow.gameObject.SetActive(false);
                }

                // Show a "tap to proceed" hint on ready lead cards
                if (so != null && so.state == LeadState.Ready)
                    AddProceedHint(go.transform);

                var btn = FindProceedButton(go.transform);
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        if (so != null) ProceedRequested?.Invoke(so);
                    });
                    if (so != null) _proceedByLead[so] = btn;
                }

                _spawned.Add(go);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
        }

        public void ApplyOutcome(LeadData lead)
        {
            if (lead == null) return;
            if (_proceedByLead.TryGetValue(lead, out var btn) && btn != null)
                btn.interactable = false;
        }

        // ----- Helpers -----

        static AQ.App.UI.Leads.LeadCardData ToCardData(LeadData lead)
        {
            if (lead == null) return new AQ.App.UI.Leads.LeadCardData();

            var reqs = new List<AQ.App.UI.Leads.RequirementData>(lead.requirements?.Length ?? 0);
            if (lead.requirements != null)
            {
                foreach (var r in lead.requirements)
                {
                    var tiers = r.Icon != null ? new List<Sprite> { r.Icon } : new List<Sprite>();
                    reqs.Add(new AQ.App.UI.Leads.RequirementData(r.Label, tiers, 0, r.IsSatisfied));
                }
            }

            return new AQ.App.UI.Leads.LeadCardData
            {
                Title        = lead.title,
                Objective    = lead.subtitle,
                LeadId       = lead.leadId,
                ActorBadge   = lead.actorPortrait,
                Requirements = reqs,
                VisualState  = lead.state == LeadState.Ready      ? AQ.App.UI.Leads.CardState.Complete
                             : lead.state == LeadState.InProgress ? AQ.App.UI.Leads.CardState.InProgress
                             : AQ.App.UI.Leads.CardState.New
            };
        }

        static void AddProceedHint(Transform cardRoot)
        {
            var go = new GameObject("Txt_ProceedHint");
            go.transform.SetParent(cardRoot, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0f, 0f);
            rt.anchorMax        = new Vector2(1f, 0f);
            rt.pivot            = new Vector2(0.5f, 0f);
            rt.sizeDelta        = new Vector2(0f, 28f);
            rt.anchoredPosition = new Vector2(0f, 8f);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text        = "▶  Tap card to proceed";
            tmp.fontSize    = 13f;
            tmp.color       = new Color(0.12f, 0.50f, 0.12f, 1f);
            tmp.alignment   = TextAlignmentOptions.Center;
            tmp.fontStyle   = FontStyles.Bold;
        }

        static Button FindProceedButton(Transform root)
        {
            var named = TryFind<Button>(root, "Button_Proceed") ?? TryFind<Button>(root, "Proceed");
            if (named != null) return named;
            return root.GetComponentInChildren<Button>(true);
        }

        static T TryFind<T>(Transform root, string childName) where T : Component
        {
            var tr = FindDeep(root, childName);
            return tr ? tr.GetComponent<T>() : null;
        }

        static Transform FindDeep(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var hit = FindDeep(root.GetChild(i), name);
                if (hit) return hit;
            }
            return null;
        }
    }
}
