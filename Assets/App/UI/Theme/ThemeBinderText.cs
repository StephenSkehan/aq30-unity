using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ThemeBinderText : MonoBehaviour, IThemeBindable
{
    public enum Which { Primary, Secondary, Accent }
    public Which Use = Which.Primary;
    
    TextMeshProUGUI _text;
    
    void Awake() { _text = GetComponent<TextMeshProUGUI>(); }
    
    void OnEnable()
    {
        if (ThemeController.Instance != null) ThemeController.Instance.Register(this);
        ThemeController.ThemeChanged += ApplyTheme;
    }
    
    void Start()
    {
        if (ThemeController.Instance != null) ThemeController.Instance.Register(this);
        ApplyTheme(ThemeController.Instance ? ThemeController.Instance.ActiveTheme : null);
    }
    
    void OnDisable()
    {
        if (ThemeController.Instance != null) ThemeController.Instance.Unregister(this);
        ThemeController.ThemeChanged -= ApplyTheme;
    }
    
    public void ApplyTheme(ThemeSO t)
    {
        if (!_text || t == null) return;
        
        _text.color = Use switch
        {
            Which.Primary => t.Primary,
            Which.Secondary => t.Secondary, 
            Which.Accent => t.Accent,
            _ => _text.color
        };
        
        if (t.font != null) _text.font = t.font;
        _text.fontSize = t.baseFontSize;
    }
}
