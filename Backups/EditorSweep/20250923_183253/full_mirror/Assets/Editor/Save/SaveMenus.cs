using UnityEditor; using UnityEngine;

public static class SaveMenus
{
    [MenuItem("AQ/Save/Create ThemeRegistry (Resources)")]
    public static void CreateThemeRegistry(){
        if(!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets","Resources");
        if(!AssetDatabase.IsValidFolder("Assets/Resources/Theme")) AssetDatabase.CreateFolder("Assets/Resources","Theme");
        var path = "Assets/Resources/Theme/ThemeRegistry.asset";
        var reg = ScriptableObject.CreateInstance<ThemeRegistry>();
        AssetDatabase.CreateAsset(reg, path);
        AssetDatabase.SaveAssets(); Selection.activeObject = reg;
        Debug.Log("[Theme] Created ThemeRegistry at " + path + ". Assign your ThemeSO assets here.");
    }

    [MenuItem("AQ/Save/Drop SaveLoad Driver")]
    public static void DropDriver(){
        var go = new GameObject("SaveLoadDriver", typeof(SaveLoadDriver));
        var drv = go.GetComponent<SaveLoadDriver>();
        drv.Theme = Object.FindFirstObjectByType<ThemeController>();
        drv.Dialogue = Object.FindFirstObjectByType<DialogueRunner>();
        drv.Board = Object.FindFirstObjectByType<BoardPresenter>();
        drv.ThemeRegistry = Resources.Load<ThemeRegistry>("Theme/ThemeRegistry");
        Selection.activeGameObject = go;
        Debug.Log("[Save] Dropped driver and auto-wired refs where possible.");
    }

    // --- Runtime actions (work while playing) ---
    [MenuItem("AQ/Save/Runtime/Save", priority = 1000)]
    public static void RuntimeSave(){
        var drv = Object.FindFirstObjectByType<SaveLoadDriver>();
        if(drv) drv.Save(); else Debug.LogWarning("[Save] No SaveLoadDriver in scene.");
    }
    [MenuItem("AQ/Save/Runtime/Load", priority = 1001)]
    public static void RuntimeLoad(){
        var drv = Object.FindFirstObjectByType<SaveLoadDriver>();
        if(drv) drv.Load(); else Debug.LogWarning("[Save] No SaveLoadDriver in scene.");
    }
    [MenuItem("AQ/Save/Runtime/Clear", priority = 1002)]
    public static void RuntimeClear(){
        var drv = Object.FindFirstObjectByType<SaveLoadDriver>();
        if(drv) drv.Clear(); else Debug.LogWarning("[Save] No SaveLoadDriver in scene.");
    }
}
