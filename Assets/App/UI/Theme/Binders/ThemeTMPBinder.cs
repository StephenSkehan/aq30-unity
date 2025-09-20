#if TMP_PRESENT
using UnityEngine; using TMPro;
[RequireComponent(typeof(TMP_Text))]
public class ThemeTMPBinder : MonoBehaviour, IThemeBindable {
  public bool UseSecondary=false; TMP_Text _txt; void Awake(){ _txt=GetComponent<TMP_Text>(); }
  void OnEnable(){ if(ThemeController.Instance!=null) ThemeController.Instance.Register(this); ThemeController.ThemeChanged+=ApplyTheme; }
  void Start(){ if(ThemeController.Instance!=null) ThemeController.Instance.Register(this); ApplyTheme(ThemeController.Instance? ThemeController.Instance.ActiveTheme : null); }
  void OnDisable(){ if(ThemeController.Instance!=null) ThemeController.Instance.Unregister(this); ThemeController.ThemeChanged-=ApplyTheme; }
  public void ApplyTheme(ThemeSO t){ if(!_txt||t==null) return; _txt.color = UseSecondary? t.TextSecondary : t.TextPrimary; _txt.fontSize = UseSecondary? t.BodySize : t.TitleSize; }
}
#endif
