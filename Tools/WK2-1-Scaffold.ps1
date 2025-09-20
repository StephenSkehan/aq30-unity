[CmdletBinding()]
param(
  [Parameter(Mandatory=$true)][string]$RepoRoot,
  [switch]$CreateHUD,
  [switch]$CreateDialogue,
  [switch]$CreateTheme
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath $RepoRoot).Path

function W([string]$path,[string]$text){
  $full = Join-Path $root $path
  $dir = Split-Path -Parent $full
  if(!(Test-Path $dir)){ New-Item -ItemType Directory -Path $dir -Force | Out-Null }
  Set-Content -LiteralPath $full -Value $text -Encoding UTF8
  Write-Host "Wrote $path"
}

# ----------------------------- C#: Theme -----------------------------
$themeSO = @"
using UnityEngine;

[CreateAssetMenu(fileName = "Theme", menuName = "AQ/ThemeSO", order = 10)]
public class ThemeSO : ScriptableObject
{
    [Header("Palette")]
    public Color Primary = new Color(0.85f, 0.2f, 0.2f);
    public Color Secondary = new Color(0.10f, 0.10f, 0.12f);
    public Color Accent = new Color(1f, 0.85f, 0.4f);
    public Color TextPrimary = Color.white;
    public Color TextSecondary = new Color(0.85f,0.85f,0.85f);
    public Color Panel = new Color(0.08f,0.08f,0.10f, 0.92f);

    [Header("Spacing")]
    public float CornerRadius = 16f;
    public float Padding = 8f;

    [Header("Typography")]
    public int TitleSize = 28;
    public int BodySize = 16;
}
"@

$themeController = @"
using System;
using System.Collections.Generic;
using UnityEngine;

public class ThemeController : MonoBehaviour
{
    public ThemeSO ActiveTheme;

    private static ThemeController _instance;
    public static ThemeController Instance => _instance;

    private readonly List<IThemeBindable> _bindables = new List<IThemeBindable>();

    public static event Action<ThemeSO> ThemeChanged;

    void Awake(){
        if(_instance != null && _instance != this){ Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Register(IThemeBindable b){ if(!_bindables.Contains(b)) _bindables.Add(b); }
    public void Unregister(IThemeBindable b){ _bindables.Remove(b); }

    [ContextMenu("Apply Theme")]
    public void ApplyTheme(){
        var t = ActiveTheme;
        foreach(var b in _bindables){ b.ApplyTheme(t); }
        ThemeChanged?.Invoke(t);
        Debug.Log($"[Theme] Applied { (t ? t.name : "null") } to {_bindables.Count} bindables.");
    }

    public void SetActiveTheme(ThemeSO t){
        ActiveTheme = t;
        ApplyTheme();
    }
}

public interface IThemeBindable { void ApplyTheme(ThemeSO theme); }
"@

$themeImageBinder = @"
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ThemeImageBinder : MonoBehaviour, IThemeBindable
{
    public enum Which { Panel, Primary, Secondary, Accent }
    public Which Use = Which.Panel;

    Image _img;

    void Awake(){
        _img = GetComponent<Image>();
        ThemeController.Instance?.Register(this);
    }
    void OnDestroy(){
        ThemeController.Instance?.Unregister(this);
    }

    public void ApplyTheme(ThemeSO t){
        if(!_img || t == null) return;
        switch(Use){
            case Which.Panel: _img.color = t.Panel; break;
            case Which.Primary: _img.color = t.Primary; break;
            case Which.Secondary: _img.color = t.Secondary; break;
            case Which.Accent: _img.color = t.Accent; break;
        }
    }

    [ContextMenu("Apply Now")]
    void ApplyNow(){ ApplyTheme(ThemeController.Instance ? ThemeController.Instance.ActiveTheme : null); }
}
"@

$themeTMPBinder = @"
#if TMP_PRESENT
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class ThemeTMPBinder : MonoBehaviour, IThemeBindable
{
    public bool UseSecondary = false;
    TMP_Text _txt;

    void Awake(){
        _txt = GetComponent<TMP_Text>();
        ThemeController.Instance?.Register(this);
    }
    void OnDestroy(){
        ThemeController.Instance?.Unregister(this);
    }

    public void ApplyTheme(ThemeSO t){
        if(!_txt || t == null) return;
        _txt.color = UseSecondary ? t.TextSecondary : t.TextPrimary;
        _txt.fontSize = UseSecondary ? t.BodySize : t.TitleSize;
    }

    [ContextMenu("Apply Now")]
    void ApplyNow(){ ApplyTheme(ThemeController.Instance ? ThemeController.Instance.ActiveTheme : null); }
}
#endif
"@

$themeEditor = @"
using UnityEditor;
using UnityEngine;

public static class ThemeEditorMenu
{
    const string ThemeDir = ""Assets/ScriptableObjects/Theme"";

    [MenuItem(""AQ/Theme/Create Noir & Light"")]
    public static void CreateThemes(){
        if(!AssetDatabase.IsValidFolder(""Assets/ScriptableObjects"")) AssetDatabase.CreateFolder(""Assets"", ""ScriptableObjects"");
        if(!AssetDatabase.IsValidFolder(ThemeDir)) AssetDatabase.CreateFolder(""Assets/ScriptableObjects"", ""Theme"");

        var noir = ScriptableObject.CreateInstance<ThemeSO>();
        noir.name = ""Theme_Noir"";
        noir.Primary = new Color(0.94f,0.27f,0.27f);
        noir.Secondary = new Color(0.10f,0.10f,0.12f);
        noir.Accent = new Color(1f,0.85f,0.40f);
        noir.TextPrimary = Color.white;
        noir.Panel = new Color(0.07f,0.07f,0.09f, 0.95f);
        AssetDatabase.CreateAsset(noir, ThemeDir+""/Theme_Noir.asset"");

        var light = ScriptableObject.CreateInstance<ThemeSO>();
        light.name = ""Theme_Light"";
        light.Primary = new Color(0.20f,0.35f,0.80f);
        light.Secondary = new Color(0.92f,0.93f,0.96f);
        light.Accent = new Color(0.10f,0.10f,0.12f);
        light.TextPrimary = Color.black;
        light.Panel = new Color(1f,1f,1f, 0.95f);
        AssetDatabase.CreateAsset(light, ThemeDir+""/Theme_Light.asset"");

        AssetDatabase.SaveAssets();
        Selection.activeObject = noir;
        Debug.Log(""[Theme] Created Theme_Noir & Theme_Light in ""+ThemeDir);
    }

    [MenuItem(""AQ/Theme/Apply Active Theme"")]
    public static void ApplyActive(){
        var ctrl = Object.FindObjectOfType<ThemeController>();
        if(!ctrl){ Debug.LogWarning(""[Theme] No ThemeController in scene.""); return; }
        ctrl.ApplyTheme();
    }
}
"@

# ---------------------- C#: Prefab Makers (Editor) ----------------------
$prefabHUD = @"
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public static class HUDPrefabMaker
{
    const string Path = ""Assets/UI/HUD/HUD.prefab"";
    [MenuItem(""AQ/Prefabs/Make HUD"")]
    public static void Make(){
        var go = new GameObject(""HUD"", typeof(RectTransform));
        var canvas = new GameObject(""Canvas"", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var ctrlObj = new GameObject(""ThemeController"", typeof(ThemeController));

        var c = canvas.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080,1920);

        // Panel
        var panel = new GameObject(""HUDPanel"", typeof(RectTransform), typeof(Image), typeof(ThemeImageBinder));
        panel.transform.SetParent(canvas.transform, false);
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        var img = panel.GetComponent<Image>(); img.raycastTarget = false;

        // Save
        if(!AssetDatabase.IsValidFolder(""Assets/UI"")) AssetDatabase.CreateFolder(""Assets"", ""UI"");
        if(!AssetDatabase.IsValidFolder(""Assets/UI/HUD"")) AssetDatabase.CreateFolder(""Assets/UI"", ""HUD"");
        var prefab = PrefabUtility.SaveAsPrefabAsset(canvas, Path);
        Object.DestroyImmediate(go); Object.DestroyImmediate(ctrlObj);
        Debug.Log(""[HUD] Wrote ""+Path);
        Selection.activeObject = prefab;
    }
}
"@

$prefabDialogue = @"
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
#if TMP_PRESENT
using TMPro;
#endif

public static class DialoguePanelPrefabMaker
{
    const string Path = ""Assets/UI/Dialogue/DialoguePanel.prefab"";
    [MenuItem(""AQ/Prefabs/Make DialoguePanel"")]
    public static void Make(){
        var root = new GameObject(""DialoguePanel"", typeof(RectTransform));
        var bg = new GameObject(""BG"", typeof(RectTransform), typeof(Image), typeof(ThemeImageBinder));
        bg.transform.SetParent(root.transform, false);
        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0.05f,0.05f);
        bgRt.anchorMax = new Vector2(0.95f,0.35f);
        bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;

#if TMP_PRESENT
        var textGO = new GameObject(""Text"", typeof(RectTransform), typeof(TMP_Text));
        textGO.transform.SetParent(root.transform, false);
        var txt = textGO.GetComponent<TMP_Text>();
        txt.text = ""Lena: I think I'm being followed…"";
        var tbind = textGO.AddComponent<ThemeTMPBinder>();
        tbind.UseSecondary = false;
#endif

        if(!AssetDatabase.IsValidFolder(""Assets/UI"")) AssetDatabase.CreateFolder(""Assets"", ""UI"");
        if(!AssetDatabase.IsValidFolder(""Assets/UI/Dialogue"")) AssetDatabase.CreateFolder(""Assets/UI"", ""Dialogue"");
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, Path);
        Object.DestroyImmediate(root);
        Debug.Log(""[DialoguePanel] Wrote ""+Path);
        Selection.activeObject = prefab;
    }
}
"@

# ----------------------------- Write files -----------------------------
W 'Assets/Scripts/UI/Theme/ThemeSO.cs' $themeSO
W 'Assets/Scripts/UI/Theme/ThemeController.cs' $themeController
W 'Assets/Scripts/UI/Theme/Binders/ThemeImageBinder.cs' $themeImageBinder
W 'Assets/Scripts/UI/Theme/Binders/ThemeTMPBinder.cs' $themeTMPBinder
W 'Assets/Editor/Theme/ThemeEditorMenu.cs' $themeEditor
W 'Assets/Editor/PrefabMakers/HUDPrefabMaker.cs' $prefabHUD
W 'Assets/Editor/PrefabMakers/DialoguePanelPrefabMaker.cs' $prefabDialogue

Write-Host "`nWK2-1 scaffold written. Next: open Unity and run menu items under AQ/*" -ForegroundColor Cyan
