using UnityEngine; using UnityEngine.UI;
[RequireComponent(typeof(Image))]
public class ThemeImageBinder : MonoBehaviour, IThemeBindable {
  public enum Which { Panel, Primary, Secondary, Accent } public Which Use=Which.Panel;
  Image _img; void Awake(){ _img=GetComponent<Image>(); }
  void OnEnable(){ if(ThemeController.Instance!=null) ThemeController.Instance.Register(this); ThemeController.ThemeChanged+=ApplyTheme; }
  void Start(){ if(ThemeController.Instance!=null) ThemeController.Instance.Register(this); ApplyTheme(ThemeController.Instance? ThemeController.Instance.ActiveTheme : null); }
  void OnDisable(){ if(ThemeController.Instance!=null) ThemeController.Instance.Unregister(this); ThemeController.ThemeChanged-=ApplyTheme; }
  public void ApplyTheme(ThemeSO t){ if(!_img||t==null) return; _img.color = Use switch { Which.Panel=>t.Panel, Which.Primary=>t.Primary, Which.Secondary=>t.Secondary, Which.Accent=>t.Accent, _=>_img.color }; }
}

