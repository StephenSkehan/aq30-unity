using UnityEngine;
public static class ThemeBootstrap {
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  static void EnsureThemeController(){
    if(ThemeController.Instance==null){
      var go=new GameObject("ThemeController", typeof(ThemeController));
      Object.DontDestroyOnLoad(go);
    }
  }
}
