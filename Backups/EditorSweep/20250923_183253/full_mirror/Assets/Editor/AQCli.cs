using UnityEditor;
public static class AQCli {
  public static void MakeHUD(){ HUDPrefabMaker.Make(); }
  public static void MakeDialogue(){ DialoguePanelPrefabMaker.Make(); }
  public static void CreateThemes(){ ThemeEditorMenu.CreateThemes(); }
  public static void RepairHUD(){ HUDPrefabRepair.Repair(); }
  public static void RepairScene(){ ThemeSceneRepair.Repair(); }
}
