using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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

        private void Awake()
        {
            var bg = GetComponent<UnityEngine.UI.Image>();
            if (bg != null) bg.color = new Color(1, 1, 1, 0);

            if (tickOverlay == null)
                tickOverlay = CreateTickOverlay();
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
                var tint = AQTheme.Success; tint.a = 0.25f;
                slotBg.color = met ? tint : new Color(1, 1, 1, 0);
            }

            if (tickOverlay != null)
            {
                tickOverlay.SetActive(met);
                if (met) tickOverlay.transform.SetAsLastSibling();
            }

            // click
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(HandleClick);
            }
        }

        private void HandleClick()
        {
            if (_data != null) onClick?.Invoke(_data);
        }
    }
}
