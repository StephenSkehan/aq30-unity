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

            // tick
            if (tickOverlay != null)
                tickOverlay.SetActive(data?.Met == true);

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
