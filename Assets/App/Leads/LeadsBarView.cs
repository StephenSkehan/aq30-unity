using System;
using System.Collections;
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

        string _lastFulfillId;
        int _activatedCount;
        TextMeshProUGUI _progressLabel;

        UnityEngine.Object _boundRepo;

        void Awake()
        {
            // The lead bar sits inside a VerticalLayoutGroup whose later siblings (the board grid)
            // would otherwise render on top. Override sorting ensures leads always draw above the grid.
            var c = GetComponent<Canvas>();
            if (c == null)
            {
                c = gameObject.AddComponent<Canvas>();
                c.overrideSorting = true;
                c.sortingOrder = 1;
                // A nested Canvas with overrideSorting requires its own GraphicRaycaster;
                // without it the EventSystem cannot detect clicks on child buttons.
                gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();
            if (contentRoot == null && scrollRect != null) contentRoot = scrollRect.content;
            CreateProgressLabel();
        }

        public void Bind(UnityEngine.Object repo) { _boundRepo = repo; }

        void OnEnable()  { LeadsRuntimeBus.OnLeadActivated += HandleLeadActivated; }
        void OnDisable() { LeadsRuntimeBus.OnLeadActivated -= HandleLeadActivated; }

        void HandleLeadActivated(LeadData lead)
        {
            if (lead == null) return;
            _lastFulfillId = lead.leadId;
            // boardPhase 0 = repeatables/teasers, outside the "X / 12" case arc.
            if (lead.boardPhase > 0)
            {
                _activatedCount++;
                UpdateProgressLabel();
            }
        }

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
                if (so != null && so.RuntimeState == LeadState.Blocked) continue;
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
                        BuildRewardPreview(presenter.rewardsRow, so);
                }

                // Show a "tap to proceed" hint on ready lead cards
                if (so != null && so.RuntimeState == LeadState.Ready)
                    AddProceedHint(go.transform);

                var btn = FindProceedButton(go.transform);
                if (btn != null)
                {
                    var capturedSo = so;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        if (capturedSo != null) ProceedRequested?.Invoke(capturedSo);
                    });
                    if (so != null) _proceedByLead[so] = btn;
                }
                else
                {
                    Debug.LogWarning($"[LeadsBarView] No button found on card '{so?.leadId}' — tap-to-proceed will not work.");
                }

                _spawned.Add(go);

                if (_lastFulfillId != null && so != null && so.leadId == _lastFulfillId)
                {
                    StartCoroutine(PlayFulfillBounce(go.GetComponent<RectTransform>()));
                    _lastFulfillId = null;
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
        }

        public void ApplyOutcome(LeadData lead)
        {
            if (lead == null) return;
            if (_proceedByLead.TryGetValue(lead, out var btn) && btn != null)
                btn.interactable = false;
        }

        // ----- Progress HUD + Fulfill Animation -----

        void CreateProgressLabel()
        {
            var go = new GameObject("Txt_CaseProgress");
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin        = new Vector2(1f, 1f);
            rt.anchorMax        = new Vector2(1f, 1f);
            rt.pivot            = new Vector2(1f, 1f);
            rt.sizeDelta        = new Vector2(64f, 26f);
            rt.anchoredPosition = new Vector2(-8f, -8f);

            var pill = go.AddComponent<Image>();
            pill.sprite = AQ.App.UI.AQTheme.Rounded;
            pill.type   = Image.Type.Sliced;
            pill.pixelsPerUnitMultiplier = 2.5f;
            pill.color  = AQ.App.UI.AQTheme.BoardFrame;
            pill.raycastTarget = false;

            var lblGo = new GameObject("Label");
            lblGo.transform.SetParent(rt, false);
            var lrt = lblGo.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = lrt.offsetMax = Vector2.zero;
            _progressLabel = lblGo.AddComponent<TextMeshProUGUI>();
            _progressLabel.fontSize  = 14f;
            _progressLabel.color     = AQ.App.UI.AQTheme.PaperDim;
            _progressLabel.alignment = TextAlignmentOptions.Center;
            _progressLabel.raycastTarget = false;
            AQ.App.UI.AQTheme.StyleText(_progressLabel);
            UpdateProgressLabel();
        }

        void UpdateProgressLabel()
        {
            if (_progressLabel == null) return;
            _progressLabel.text = $"{_activatedCount} / 12";
        }

        static IEnumerator PlayFulfillBounce(RectTransform rt)
        {
            if (rt == null) yield break;
            float elapsed = 0f;
            const float duration = 0.2f;
            const float peak = 1.08f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale = t < 0.5f
                    ? Mathf.Lerp(1f, peak, t * 2f)
                    : Mathf.Lerp(peak, 1f, (t - 0.5f) * 2f);
                rt.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            rt.localScale = Vector3.one;
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
                    // One chip per requirement; quantity shows as a live owned/needed
                    // count on the chip instead of duplicate slots.
                    var tiers = r.Icon != null ? new List<Sprite> { r.Icon } : new List<Sprite>();
                    reqs.Add(new AQ.App.UI.Leads.RequirementData(r.Label, tiers, 0, r.IsSatisfied)
                    {
                        ItemId      = r.itemDefinition != null ? r.itemDefinition.itemId : null,
                        NeededCount = r.quantity < 1 ? 1 : r.quantity
                    });
                }
            }

            return new AQ.App.UI.Leads.LeadCardData
            {
                Title        = lead.title,
                Objective    = lead.subtitle,
                LeadId       = lead.leadId,
                ActorBadge   = lead.actorPortrait,
                Requirements = reqs,
                VisualState  = lead.RuntimeState == LeadState.Ready      ? AQ.App.UI.Leads.CardState.Complete
                             : lead.RuntimeState == LeadState.InProgress ? AQ.App.UI.Leads.CardState.InProgress
                             : AQ.App.UI.Leads.CardState.New
            };
        }

        /// <summary>
        /// Reward-preview chips (icon + amount) on the card. The prefab's
        /// rewardsRow rect predates the card restyle and overlaps the
        /// requirement row, so it is re-anchored here to a slim band above it.
        /// </summary>
        static void BuildRewardPreview(RectTransform row, LeadData lead)
        {
            for (int i = row.childCount - 1; i >= 0; i--)
                Destroy(row.GetChild(i).gameObject);

            bool any = lead != null && (lead.SoftCurrency > 0 || lead.EnergyGrant > 0 || lead.PremiumGrant > 0);
            row.gameObject.SetActive(any);
            if (!any) return;

            // Indent past the actor bust (which spans the card's left ~110px).
            row.anchorMin = new Vector2(0f, 0f);
            row.anchorMax = new Vector2(1f, 0f);
            row.pivot     = new Vector2(0.5f, 0f);
            row.offsetMin = new Vector2(118f, 104f);
            row.offsetMax = new Vector2(-12f, 132f);

            var layout = row.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 12f;
                layout.childAlignment = TextAnchor.MiddleLeft;
                layout.childForceExpandWidth  = false;
                layout.childForceExpandHeight = false;
                layout.childControlWidth  = false;
                layout.childControlHeight = false;
            }

            if (lead.SoftCurrency > 0) AddRewardChip(row, "App/UI/Icons/flight_cash",      lead.SoftCurrency);
            if (lead.EnergyGrant  > 0) AddRewardChip(row, "App/UI/MergeBoard/energy_badge", lead.EnergyGrant);
            if (lead.PremiumGrant > 0) AddRewardChip(row, "App/UI/Icons/flight_ingot",     lead.PremiumGrant);
        }

        static void AddRewardChip(RectTransform row, string spritePath, int amount)
        {
            var chip = new GameObject("Reward");
            chip.transform.SetParent(row, false);
            var rt = chip.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(88f, 26f);

            var pill = chip.AddComponent<Image>();
            pill.sprite = AQ.App.UI.AQTheme.Rounded;
            pill.type   = Image.Type.Sliced;
            pill.pixelsPerUnitMultiplier = 2.5f;
            pill.color  = AQ.App.UI.AQTheme.BoardFrame;
            pill.raycastTarget = false;

            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(rt, false);
            var irt = iconGo.AddComponent<RectTransform>();
            irt.anchorMin = new Vector2(0f, 0.5f);
            irt.anchorMax = new Vector2(0f, 0.5f);
            irt.pivot     = new Vector2(0f, 0.5f);
            irt.anchoredPosition = Vector2.zero;
            irt.sizeDelta = new Vector2(24f, 24f);
            var img = iconGo.AddComponent<Image>();
            img.sprite = Resources.Load<Sprite>(spritePath);
            img.preserveAspect = true;
            img.raycastTarget  = false;
            img.enabled = img.sprite != null;

            var txtGo = new GameObject("Amount");
            txtGo.transform.SetParent(rt, false);
            var trt = txtGo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(28f, 0f);
            trt.offsetMax = Vector2.zero;
            var tmp = txtGo.AddComponent<TextMeshProUGUI>();
            tmp.text      = $"+{amount}";
            tmp.fontSize  = 16f;
            tmp.color     = AQ.App.UI.AQTheme.Amber;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.raycastTarget = false;
            AQ.App.UI.AQTheme.StyleText(tmp);
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
            tmp.text        = ">>  Tap card to proceed";
            tmp.fontSize    = 13f;
            tmp.color       = AQ.App.UI.AQTheme.Amber;
            tmp.alignment   = TextAlignmentOptions.Center;
            tmp.fontStyle   = FontStyles.Bold;
            AQ.App.UI.AQTheme.StyleText(tmp);
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
