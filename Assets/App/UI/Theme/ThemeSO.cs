using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "Theme", menuName = "AQ/Theme")]
public class ThemeSO : ScriptableObject
{
    public Color Panel = Color.gray;         // ADD THIS
    public Color Primary = Color.white;      // was primaryColor
    public Color Secondary = Color.gray;     // ADD THIS
    public Color Accent = Color.cyan;        // was accentColor
    public TMP_FontAsset font;
    public int baseFontSize = 36;
}
