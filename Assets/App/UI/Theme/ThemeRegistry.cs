using UnityEngine;

[CreateAssetMenu(fileName="ThemeRegistry", menuName="AQ/ThemeRegistry", order=30)]
public class ThemeRegistry : ScriptableObject
{
    public ThemeSO[] Themes;

    public ThemeSO FindByName(string name){
        if(string.IsNullOrEmpty(name) || Themes == null) return null;
        foreach(var t in Themes) if(t && t.name == name) return t;
        return null;
    }
}
