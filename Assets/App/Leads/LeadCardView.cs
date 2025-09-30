// Assets/App/Leads/LeadCardView.cs
// Prefab-first Lead card view. At runtime we DO NOT rebuild layout;
// we only bind text / portrait (and optionally requirements and proceed button).

using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.Leads
{
    [DisallowMultipleComponent]
    public sealed class LeadCardView : MonoBehaviour
    {
        [Header("Prefab Mode")]
        [Tooltip("When true (default), the view uses the prefab's layout and never rebuilds UI at runtime.")]
        public bool usePrefabLayout = true;

        [Header("Prefab References")]
        [SerializeField] private RectTransform actorAnchor;   // parent for portrait image
        [SerializeField] private Image        actorBadge;     // portrait image (optional)
        [SerializeField] private TMP_Text     title;          // main title text
        [SerializeField] private TMP_Text     subtitle;       // one-liner / subtitle
        [SerializeField] private TMP_Text     action;         // small action tag (optional)
        [SerializeField] private Transform    reqRow;         // container for requirement chips (optional)
        [SerializeField] private Button       proceedButton;  // optional proceed button

        [Header("Optional Requirement Chip Prefab")]
        [SerializeField] private GameObject   reqChipPrefab;  // simple chip with Image (icon) + TMP_Text

        System.Action _proceedHandler;

        // ------------------------ Public API ------------------------

        public void Bind(object leadData)
        {
            if (leadData == null) return;
            CachePrefabRefsIfNeeded();

            // Strings
            SetText(title,    GetString(leadData, "title", "Title", "displayTitle", "name"));
            SetText(subtitle, GetString(leadData, "subtitle", "SubTitle", "oneLiner", "OneLiner", "summary"));
            SetText(action,   GetString(leadData, "actionTag", "ActionTag", "action", "Action"));

            // Portrait
            var sprite = GetSprite(leadData, "actorPortrait", "ActorPortrait", "portrait", "Portrait");
            if (actorBadge != null)
            {
                actorBadge.sprite         = sprite;
                actorBadge.enabled        = (sprite != null);
                actorBadge.preserveAspect = true;
            }

            // Requirements (optional)
            if (reqRow != null && reqChipPrefab != null)
            {
                ClearChildren(reqRow);
                var reqs = GetRequirements(leadData, "requirements", "Requirements", "reqs", "Reqs");
                if (reqs != null)
                {
                    foreach (var r in reqs)
                        AddReqChip(r);
                }
            }

            // Proceed button enable (if the model exposes a boolean)
            if (proceedButton)
            {
                var canProceed = GetBool(leadData, "canProceed", "CanProceed", "enabled", "Enabled", "interactable", "Interactable");
                proceedButton.interactable = canProceed ?? true;
                proceedButton.onClick.RemoveAllListeners();
                if (_proceedHandler != null) proceedButton.onClick.AddListener(() => _proceedHandler());
            }
        }

        // Back-compat with older callsites
        public void Rebuild() { /* No-op in prefab mode */ }

        /// <summary>Provide a callback that fires when the card's proceed button is clicked.</summary>
        public void SetProceedCallback(System.Action onProceed)
        {
            _proceedHandler = onProceed;
            if (proceedButton)
            {
                proceedButton.onClick.RemoveAllListeners();
                if (_proceedHandler != null) proceedButton.onClick.AddListener(() => _proceedHandler());
            }
        }

        // ------------------------ Unity ------------------------

        void Awake()
        {
            if (usePrefabLayout) CachePrefabRefsIfNeeded();
        }

        // ------------------------ Helpers ------------------------

        void CachePrefabRefsIfNeeded()
        {
            var t = transform;
            title    = title    ? title    : t.Find("Title")     ?.GetComponent<TMP_Text>();
            action   = action   ? action   : t.Find("Action")    ?.GetComponent<TMP_Text>();
            subtitle = subtitle ? subtitle : t.Find("OneLiner")  ?.GetComponent<TMP_Text>();
            reqRow   = reqRow   ? reqRow   : (t.Find("ReqRow")   ?? t.Find("Badges"));

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

        void AddReqChip(object leadRequirement)
        {
            if (reqRow == null || reqChipPrefab == null || leadRequirement == null) return;

            var go   = Instantiate(reqChipPrefab, reqRow, false);
            var img  = go.GetComponentInChildren<Image>(true);
            var txt  = go.GetComponentInChildren<TMP_Text>(true);

            var icon  = GetSprite(leadRequirement, "icon", "Icon");
            var label = GetString(leadRequirement, "label", "Label", "name", "Name");

            if (img) img.sprite = icon;
            if (txt) txt.text   = string.IsNullOrEmpty(label) ? "Req" : label;
        }

        static void ClearChildren(Transform parent)
        {
            if (!parent) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                var c = parent.GetChild(i);
                if (Application.isPlaying) Object.Destroy(c.gameObject);
                else Object.DestroyImmediate(c.gameObject);
            }
        }

        // ------------------------ Reflection helpers ------------------------

        static string GetString(object o, params string[] names)
        {
            foreach (var n in names)
            {
                var v = GetMemberValue(o, n);
                if (v is string s && !string.IsNullOrEmpty(s)) return s;
            }
            return null;
        }

        static Sprite GetSprite(object o, params string[] names)
        {
            foreach (var n in names)
            {
                var v = GetMemberValue(o, n);
                if (v is Sprite sp) return sp;
            }
            return null;
        }

        static bool? GetBool(object o, params string[] names)
        {
            foreach (var n in names)
            {
                var v = GetMemberValue(o, n);
                if (v is bool b) return b;
            }
            return null;
        }

        static IReadOnlyList<object> GetRequirements(object o, params string[] names)
        {
            foreach (var n in names)
            {
                var v = GetMemberValue(o, n);
                if (v is IEnumerable<object> e) return new List<object>(e);
                if (v is System.Collections.IEnumerable ie)
                {
                    var list = new List<object>();
                    foreach (var item in ie) list.Add(item);
                    return list;
                }
            }
            return null;
        }

        static object GetMemberValue(object o, string name)
        {
            if (o == null || string.IsNullOrEmpty(name)) return null;
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            var t = o.GetType();
            var p = t.GetProperty(name, flags | BindingFlags.IgnoreCase);
            if (p != null) return p.GetValue(o, null);

            var f = t.GetField(name, flags);
            if (f != null) return f.GetValue(o);

            return null;
        }
    }
}
