using System; using System.Collections.Generic; using UnityEngine;
public class ThemeController : MonoBehaviour {
  public ThemeSO ActiveTheme;
  private static ThemeController _instance; public static ThemeController Instance => _instance;
  private readonly List<IThemeBindable> _bindables = new(); public static event Action<ThemeSO> ThemeChanged;
  void Awake(){ if(_instance!=null && _instance!=this){ Destroy(gameObject); return; } _instance=this; DontDestroyOnLoad(gameObject); }
  public void Register(IThemeBindable b){ if(!_bindables.Contains(b)) _bindables.Add(b); }
  public void Unregister(IThemeBindable b){ _bindables.Remove(b); }
  [ContextMenu("Apply Theme")] public void ApplyTheme(){ var t=ActiveTheme; foreach(var b in _bindables){ b.ApplyTheme(t); } ThemeChanged?.Invoke(t); Debug.Log($"[Theme] Applied { (t? t.name : "null") } to {_bindables.Count} bindables."); }
  public void SetActiveTheme(ThemeSO t){ ActiveTheme=t; ApplyTheme(); }
}
public interface IThemeBindable { void ApplyTheme(ThemeSO theme); }
