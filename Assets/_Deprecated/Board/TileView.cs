using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Board
{
    /// <summary>
    /// Minimal per-slot view: exposes the icon Image and a helper to set the sprite.
    /// No coupling to controllers.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TileView : MonoBehaviour
    {
        [SerializeField] private Image icon;   // assign the foreground Image in the prefab (e.g., the child named "icon")

        public Image Icon
        {
            get
            {
                if (icon == null) icon = GetComponentInChildren<Image>(true);
                return icon;
            }
        }

        public void SetSprite(Sprite sprite)
        {
            if (Icon == null) return;
            Icon.sprite = sprite;
            Icon.enabled = sprite != null;
        }
    }
}
