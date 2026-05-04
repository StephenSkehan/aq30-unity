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
            rt.anchoredPosition = new Vector2(-4f, 4f);
            rt.sizeDelta = new Vector2(28f, 28f);
            go.AddComponent<CanvasRenderer>();
            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.85f, 0.4f, 1f);
            go.SetActive(false);
            return go;
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
                slotBg.color = met ? new Color(0.2f, 0.85f, 0.4f, 0.4f) : new Color(1,1,1,0);

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
