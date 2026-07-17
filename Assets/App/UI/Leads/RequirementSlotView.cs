using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using AQ.App.Leads;

namespace AQ.App.UI.Leads
{
    /// <summary>
    /// Visual for one "required item" slot (icon + tick). Raises click with its RequirementData.
    /// Public names match Editor utilities.
    /// </summary>
    public class RequirementSlotView : MonoBehaviour
    {
        [System.Serializable] public sealed class RequirementClickedEvent : UnityEvent<RequirementData> {}

        [Header("Wires")]
        public Button button;
        public Image  icon;
        public GameObject tickOverlay;

        public RequirementClickedEvent onClick = new RequirementClickedEvent();

        private RequirementData _data;
        private TextMeshProUGUI _countLabel;

        private void Awake()
        {
            // Light chip plate so dark item icons stay visible on the dark card
            // (GH puts every item on a white shelf for the same reason).
            var bg = GetComponent<UnityEngine.UI.Image>();
            if (bg != null)
            {
                bg.sprite = AQTheme.Rounded;
                bg.type   = Image.Type.Sliced;
                bg.pixelsPerUnitMultiplier = 2f;
                bg.color  = PlateTint;
            }

            if (tickOverlay == null)
                tickOverlay = CreateTickOverlay();
        }

        // Unmet plate is fully transparent (Stephen-ruled 2026-07-17 — the grey
        // square read as clutter); the plate only appears as the green met state.
        private static Color PlateTint => Color.clear;

        private void OnEnable()
        {
            if (LeadRequirementChecker.Instance != null)
                LeadRequirementChecker.Instance.LiveCountsChanged += RefreshCount;
        }

        private void OnDisable()
        {
            if (LeadRequirementChecker.Instance != null)
                LeadRequirementChecker.Instance.LiveCountsChanged -= RefreshCount;
        }

        private GameObject CreateTickOverlay()
        {
            var go = new GameObject("Tick");
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot     = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-2f, 2f);
            rt.sizeDelta = new Vector2(28f, 28f);
            var img = go.AddComponent<Image>();
            img.sprite = AQTheme.Rounded;
            img.type   = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 0.35f; // corners overrun -> circular badge
            img.color  = AQTheme.Success;
            img.raycastTarget = false;

            // Checkmark drawn from two bars — no reliance on a ✓ glyph existing.
            AddBar(rt, new Vector2(4f, 10f),  45f, new Vector2(-8f, -2f));
            AddBar(rt, new Vector2(4f, 17f), -45f, new Vector2(1f, 0.5f));

            go.SetActive(false);
            return go;
        }

        private static void AddBar(RectTransform parent, Vector2 size, float zRot, Vector2 pos)
        {
            var go = new GameObject("Stroke");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta        = size;
            rt.anchoredPosition = pos;
            rt.localRotation    = Quaternion.Euler(0f, 0f, zRot);
            var img = go.AddComponent<Image>();
            img.color = Color.white;
            img.raycastTarget = false;
        }

        public void Bind(RequirementData data)
        {
            _data = data;

            // icon
            if (icon != null)
            {
                Sprite s = null;
                if (data?.Tiers != null && data.Tiers.Count > 0)
                {
                    int i = Mathf.Clamp(data.TierIndex, 0, data.Tiers.Count - 1);
                    s = data.Tiers[i];
                }
                icon.sprite = s;
                icon.preserveAspect = true;
                icon.color = s != null ? Color.white : new Color(1,1,1,0);
            }

            // tick — ensure it renders on top regardless of prefab sibling order
            bool met = data?.Met == true;
            var slotBg = GetComponent<UnityEngine.UI.Image>();
            if (slotBg != null)
            {
                var tint = AQTheme.Success; tint.a = 0.3f;
                slotBg.color = met ? tint : PlateTint;
            }

            if (tickOverlay != null)
            {
                tickOverlay.SetActive(met);
                if (met) tickOverlay.transform.SetAsLastSibling();
            }

            RefreshCount();

            // click
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(HandleClick);
            }
        }

        /// <summary>
        /// Owned/needed count badge ("1/2") for multi-quantity requirements —
        /// live board counts from LeadRequirementChecker, refreshed on every
        /// item create/remove. Single-quantity slots rely on the tick alone.
        /// </summary>
        private void RefreshCount()
        {
            bool show = _data != null && _data.NeededCount > 1 && !string.IsNullOrEmpty(_data.ItemId);
            if (!show)
            {
                if (_countLabel != null) _countLabel.transform.parent.gameObject.SetActive(false);
                return;
            }

            if (_countLabel == null) _countLabel = CreateCountLabel();

            int owned = 0;
            if (LeadRequirementChecker.Instance != null)
                owned = Mathf.Clamp(LeadRequirementChecker.Instance.GetLiveCount(_data.ItemId), 0, _data.NeededCount);

            _countLabel.transform.parent.gameObject.SetActive(true);
            _countLabel.text = $"{owned}/{_data.NeededCount}";
            _countLabel.color = owned >= _data.NeededCount ? AQTheme.Success : AQTheme.Paper;
        }

        private TextMeshProUGUI CreateCountLabel()
        {
            var go = new GameObject("Count");
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot     = new Vector2(0f, 0f);
            rt.anchoredPosition = new Vector2(2f, 2f);
            rt.sizeDelta = new Vector2(36f, 20f);
            var bg = go.AddComponent<Image>();
            bg.sprite = AQTheme.Rounded;
            bg.type   = Image.Type.Sliced;
            bg.pixelsPerUnitMultiplier = 0.35f;
            bg.color  = AQTheme.BoardFrame;
            bg.raycastTarget = false;

            var lblGo = new GameObject("Label");
            lblGo.transform.SetParent(rt, false);
            var lrt = lblGo.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = lrt.offsetMax = Vector2.zero;
            var tmp = lblGo.AddComponent<TextMeshProUGUI>();
            tmp.fontSize  = 14f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            AQTheme.StyleText(tmp);
            return tmp;
        }

        private void HandleClick()
        {
            if (_data != null) onClick?.Invoke(_data);
        }
    }
}
