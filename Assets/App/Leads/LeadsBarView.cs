using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.Leads
{
    /// <summary>
    /// Minimal, robust Leads bar:
    /// - Spawns card prefabs under a horizontal content root
    /// - Raises ProceedRequested(LeadData) when a card's proceed button is clicked
    /// - ApplyOutcome disables that card's proceed (visual safety) without repo coupling
    /// - Exposes Rebuild() and Rebuild(list) so any glue/repo can drive it
    /// </summary>
    public sealed class LeadsBarView : MonoBehaviour
    {
        [Header("Wiring (assign in Inspector)")]
        public ScrollRect scrollRect;           // LeadsBar (has ScrollRect; horizontal true)
        public RectTransform contentRoot;       // Content_Leads
        public LeadCardView cardPrefab;         // Lead card prefab with LeadCardView (or at least the expected children)

        // Event consumed by BoardPresenter
        public event Action<LeadData> ProceedRequested;

        // Internal runtime state
        readonly List<LeadCardView> _spawned = new List<LeadCardView>();
        readonly Dictionary<LeadData, Button> _proceedByLead = new Dictionary<LeadData, Button>();

        // Optional: a repo can be bound by external glue; we don't assume any API on it.
        UnityEngine.Object _boundRepo;

        void Awake()
        {
            // Be lenient about wiring
            if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();
            if (contentRoot == null && scrollRect != null) contentRoot = scrollRect.content;
        }

        /// <summary>
        /// Kept for glue that expects a Bind(repo). We store it but do not depend on any specific members.
        /// </summary>
        public void Bind(UnityEngine.Object repo)
        {
            _boundRepo = repo; // informational only
        }

        /// <summary>
        /// Fallback rebuild used by older binders that call bar.Rebuild().
        /// If you have glue that knows the current list, call Rebuild(list) instead.
        /// </summary>
        public void Rebuild()
        {
            // No source-of-truth here by design; this is a no-op unless external code calls Rebuild(list).
            // Keeping the method to satisfy existing binders and avoid compile errors.
        }

        /// <summary>
        /// Replace all cards with the provided list.
        /// </summary>
        public void Rebuild(IReadOnlyList<LeadData> leads)
        {
            if (contentRoot == null || cardPrefab == null) return;

            // Clear previous
            for (int i = _spawned.Count - 1; i >= 0; i--)
            {
                var v = _spawned[i];
                if (v != null) DestroyImmediate(v.gameObject);
            }
            _spawned.Clear();
            _proceedByLead.Clear();

            if (leads == null) return;

            // Spawn
            for (int i = 0; i < leads.Count; i++)
            {
                var so = leads[i];
                var inst = Instantiate(cardPrefab, contentRoot);
                inst.gameObject.name = $"LeadCard_{i}_{(so != null ? so.name : "Null")}";

                inst.Bind(so);

                // Find a proceed button by common names; fall back to first Button in children
                var btn = FindProceedButton(inst.transform);
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        if (so != null) ProceedRequested?.Invoke(so);
                    });
                    _proceedByLead[so] = btn;
                }

                _spawned.Add(inst);
            }

            // Ensure layout updates for horizontal content
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
        }

        /// <summary>
        /// Called by BoardPresenter after an action resolves (visual safety).
        /// Disables the proceed button for the supplied lead if we spawned one.
        /// </summary>
        public void ApplyOutcome(LeadData lead)
        {
            if (lead == null) return;
            if (_proceedByLead.TryGetValue(lead, out var btn) && btn != null)
            {
                btn.interactable = false;
            }
        }

        // --- helpers ---------------------------------------------------------

        static Button FindProceedButton(Transform root)
        {
            // Preferred names first
            var named = TryFind<Button>(root, "Button_Proceed") ?? TryFind<Button>(root, "Proceed");
            if (named != null) return named;

            // Fallback: first button in subtree
            return root.GetComponentInChildren<Button>(true);
        }

        static T TryFind<T>(Transform root, string childName) where T : Component
        {
            var tr = FindTransformByNameRecursive(root, childName);
            return tr ? tr.GetComponent<T>() : null;
        }

        static Transform FindTransformByNameRecursive(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var hit = FindTransformByNameRecursive(root.GetChild(i), name);
                if (hit) return hit;
            }
            return null;
        }
    }
}
