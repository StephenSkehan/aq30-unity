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

        /// <summary>Resolved-lead count — surfaced in the combined debug overlay line.</summary>
        public int ActivatedCount => _activatedCount;

        int _caseArcTotal = -1;

        /// <summary>
        /// Total leads in this episode's case arc (boardPhase &gt; 0), derived from the bound
        /// repository's database. Never hardcode this: the arc was twelve for The Listener and
        /// is sixteen-plus for later episodes, and a literal denominator ships "14 / 12".
        /// </summary>
        public int CaseArcTotal
        {
            get
            {
                if (_caseArcTotal < 0) _caseArcTotal = ComputeCaseArcTotal();
                // A lead spawned at runtime that is not in the authored database can push the
                // numerator past the total. Clamp so the counter can never read past itself.
                return Mathf.Max(_caseArcTotal, _activatedCount);
            }
        }

        int ComputeCaseArcTotal()
        {
            var repo = _boundRepo as LeadsRepository;
            var db   = repo != null ? repo.database : null;
            if (db == null) return 0;

            var all = db.Leads;
            int n = 0;
            for (int i = 0; i < all.Count; i++)
                if (all[i] != null && all[i].boardPhase > 0) n++;
            return n;
        }

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
            if (scrollRect != null && scrollRect.GetComponent<LeadCardSnapMB>() == null)
                scrollRect.gameObject.AddComponent<LeadCardSnapMB>();
            // Standalone progress pill retired 2026-07-18 — progress now rides the
            // combined debug overlay line (CaseFlowDebugOverlayMB, toggle-gated).
            // CreateProgressLabel();
        }

        public void Bind(UnityEngine.Object repo) { _boundRepo = repo; _caseArcTotal = -1; }

        void OnEnable()  { LeadsRuntimeBus.OnLeadActivated += HandleLeadActivated; }
        void OnDisable() { LeadsRuntimeBus.OnLeadActivated -= HandleLeadActivated; }

        void HandleLeadActivated(LeadData lead)
        {
            if (lead == null) return;
            _lastFulfillId = lead.leadId;
            // boardPhase 0 = repeatables/teasers, outside the counted case arc.
            if (lead.boardPhase > 0)
            {
                _activatedCount++;
                UpdateProgressLabel();
            }
        }

        public void Rebuild() { }

        /// <summary>
        /// The card root of the first Ready lead in the bar, or null. FTUE
        /// guidance parks its banner against this; the bar's cards carry no
        /// LeadCardView component, so searching for one found nothing
        /// (guided-loop banner never reached the green card, 2026-09-03).
        /// </summary>
        public RectTransform ReadyCardRoot()
        {
            if (contentRoot == null) return null;
            foreach (var kv in _proceedByLead)
            {
                var lead = kv.Key;
                var btn = kv.Value;
                if (lead == null || btn == null || lead.RuntimeState != LeadState.Ready) continue;
                Transform t = btn.transform;
                while (t != null && t.parent != contentRoot) t = t.parent;
                if (t != null) return t as RectTransform;
            }
            return null;
        }

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
                        BuildRewardPreview(presenter.rewardsRow, (RectTransform)presenter.transform, so);
                }

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
            _progressLabel.text = $"{_activatedCount} / {CaseArcTotal}";
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
        /// Reward-preview chips (icon + amount) floating beside the bust's
        /// shoulder, above the card's top edge — the reward tags the character,
        /// GH-style, instead of burying itself in the copy.
        /// </summary>
        static void BuildRewardPreview(RectTransform row, RectTransform cardRoot, LeadData lead)
        {
            for (int i = row.childCount - 1; i >= 0; i--)
                Destroy(row.GetChild(i).gameObject);

            // The prefab parents the row under an inner container narrower than
            // the card, so parent-relative offsets could never reach the card's
            // right edge. Reparent to the card root: offsets below are then
            // card-relative by construction (2026-08-05).
            if (cardRoot != null && row.parent != cardRoot)
                row.SetParent(cardRoot, false);

            bool any = lead != null && (lead.SoftCurrency > 0 || lead.EnergyGrant > 0 || lead.PremiumGrant > 0 ||
                                        !string.IsNullOrEmpty(lead.generatorRewardTypeId));
            row.gameObject.SetActive(any);
            if (!any) return;

            // 2x2 pill grid above the card, next to the bust (Stephen-ruled
            // 2026-07-17): exactly four slots, bigger pills, white text.
            row.anchorMin = new Vector2(0f, 1f);
            row.anchorMax = new Vector2(1f, 1f);
            row.pivot     = new Vector2(0f, 0f);
            row.offsetMin = new Vector2(138f, 24f); // clear of the bust (2026-07-18)
            row.offsetMax = new Vector2(-12f, 106f); // block right-aligns flush with the card edge (2026-08-05)

            // Immediate, not deferred: Unity refuses AddComponent<GridLayoutGroup>
            // while a conflicting LayoutGroup is still alive on the object.
            var hLayout = row.GetComponent<HorizontalLayoutGroup>();
            if (hLayout != null) DestroyImmediate(hLayout); // pre-2026-07-17 single-row layout

            var layout = row.GetComponent<GridLayoutGroup>();
            if (layout == null)
                layout = row.gameObject.AddComponent<GridLayoutGroup>();
            layout.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 2;
            layout.cellSize        = new Vector2(96f, 34f); // slightly reduced, in-card (2026-08-05)
            layout.spacing         = new Vector2(8f, 8f);
            layout.startCorner     = GridLayoutGroup.Corner.LowerLeft;
            layout.childAlignment  = TextAnchor.LowerRight; // flush to card right edge (2026-08-05)

            // Cash chip mirrors the HUD's soft-currency icon at the SAME ON-SCREEN
            // size: the card hierarchy renders ~1.7x larger than the HUD canvas,
            // so 90x66 local was oversized — 52x38 measures identical (2026-07-18).
            if (lead.SoftCurrency > 0) AddRewardChip(row, "App/UI/Icons/ui_top_soft",      lead.SoftCurrency, 62f, 45f);
            if (lead.EnergyGrant  > 0) AddRewardChip(row, "App/UI/MergeBoard/energy_badge", lead.EnergyGrant);
            if (lead.PremiumGrant > 0) AddRewardChip(row, "App/UI/Icons/flight_ingot",     lead.PremiumGrant);

            // A granted generator is a headline reward — fourth chip slot.
            // Icon comes from the loaded GeneratorTypeSO (the board scene keeps
            // them all in memory); no SO found → no chip rather than a bare pill.
            if (!string.IsNullOrEmpty(lead.generatorRewardTypeId))
            {
                var genSprite = FindGeneratorSprite(lead.generatorRewardTypeId, lead.generatorRewardTier);
                // Full-bleed tile art: cash-icon width (62px), overflowing the
                // pill vertically like the currency chips, left edge pinned to
                // the pill edge so it cannot bleed into the neighbouring chip.
                if (genSprite != null) AddRewardChip(row, genSprite, 1, 62f, 62f, iconX: 29f);
            }
        }

        static Sprite FindGeneratorSprite(string typeId, int tier)
        {
            foreach (var so in Resources.FindObjectsOfTypeAll<AQ.App.Generators.GeneratorTypeSO>())
                if (so != null && so.generatorTypeId == typeId)
                    return so.SpriteForTier(tier);
            return null;
        }

        static void AddRewardChip(RectTransform row, string spritePath, int amount,
                                  float iconW = 64f, float iconH = 46f)
            => AddRewardChip(row, Resources.Load<Sprite>(spritePath), amount, iconW, iconH);

        static void AddRewardChip(RectTransform row, Sprite sprite, int amount,
                                  float iconW = 64f, float iconH = 46f, float iconX = 4f)
        {
            var chip = new GameObject("Reward");
            chip.transform.SetParent(row, false);
            var rt = chip.AddComponent<RectTransform>(); // sized by the 2x2 grid

            // HUD pill look (teal outline + cream body, RebuildHudComponents):
            // dark icons read against cream, cream reads against the dark card
            // (2026-08-05 — the old BoardFrame pill vanished into both).
            var pill = chip.AddComponent<Image>();
            pill.sprite = AQ.App.UI.AQTheme.Rounded;
            pill.type   = Image.Type.Sliced;
            pill.pixelsPerUnitMultiplier = 2.5f;
            pill.color  = AQ.App.UI.AQTheme.Teal;
            pill.raycastTarget = false;

            var bodyGo = new GameObject("PillBody");
            bodyGo.transform.SetParent(rt, false);
            var bodyRt = bodyGo.AddComponent<RectTransform>();
            bodyRt.anchorMin = Vector2.zero;
            bodyRt.anchorMax = Vector2.one;
            bodyRt.offsetMin = new Vector2(3f, 3f);
            bodyRt.offsetMax = new Vector2(-3f, -3f);
            var body = bodyGo.AddComponent<Image>();
            body.sprite = AQ.App.UI.AQTheme.Rounded;
            body.type   = Image.Type.Sliced;
            body.pixelsPerUnitMultiplier = 2.5f;
            body.color  = AQ.App.UI.AQTheme.Paper;
            body.raycastTarget = false;

            // A count of 1 is redundant (2026-08-05, Stephen-ruled): drop the
            // digit and centre the icon in the pill instead.
            bool showAmount = amount > 1;

            // Icon straddles the pill's left edge, slightly taller than the pill
            // (2026-07-18) — full art, never boxed down into a square crop.
            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(rt, false);
            var irt = iconGo.AddComponent<RectTransform>();
            var iconAnchor = showAmount ? new Vector2(0f, 0.5f) : new Vector2(0.5f, 0.5f);
            irt.anchorMin = iconAnchor;
            irt.anchorMax = iconAnchor;
            irt.pivot     = new Vector2(0.5f, 0.5f);
            irt.anchoredPosition = showAmount ? new Vector2(iconX, 0f) : Vector2.zero;
            irt.sizeDelta = new Vector2(iconW, iconH);
            var img = iconGo.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget  = false;
            img.enabled = img.sprite != null;

            if (!showAmount) return;

            var txtGo = new GameObject("Amount");
            txtGo.transform.SetParent(rt, false);
            var trt = txtGo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(iconX + iconW * 0.5f + 4f, 0f);
            trt.offsetMax = Vector2.zero;
            var tmp = txtGo.AddComponent<TextMeshProUGUI>();
            tmp.text      = amount.ToString(); // bare number (2026-07-18: no "+")
            tmp.fontSize  = 26f;
            tmp.color     = AQ.App.UI.AQTheme.Navy; // dark on cream, like the HUD
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.raycastTarget = false;
            // Same rule as the HUD counters (display font, Staatliches): reward
            // numbers and wallet numbers speak with one voice (2026-08-05).
            AQ.App.UI.AQTheme.StyleText(tmp, display: true);
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
