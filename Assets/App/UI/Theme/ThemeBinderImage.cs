using UnityEngine;
using UnityEngine.UI;

namespace AQ.App
{
    [RequireComponent(typeof(Image))]
    public class ThemeBinderImage : MonoBehaviour
    {
        private Image _img;
        
        private void Awake()
        {
            _img = GetComponent<Image>();
        }
        
        public void Apply(ThemeSO theme)
        {
            if(theme != null)
                _img.color = theme.Primary;  // Fixed: was theme.primaryColor
        }
    }
}
