param([switch]$CreateHUD=$true,[switch]$CreateDialoguePanel=$true,[switch]$RunUnityAfterWrite=$true)
$ErrorActionPreference='Stop'
$repo=(Get-Location).Path
$editorDir=Join-Path $repo 'Assets\Editor'
if(-not(Test-Path $editorDir)){New-Item -ItemType Directory -Path $editorDir|Out-Null}
$codePath=Join-Path $editorDir 'PrefabMakers.Min.cs'
$code=@"
using UnityEditor; using UnityEngine; using UnityEngine.UI; using System.IO;
namespace AQ.BuildTools {
  public static class SimplePrefabMakers {
    public static void CreateHUDPrefab() {
      const string assetPath = "Assets/Resources/App/UI/Prefabs/HUD.prefab";
      var dir = Path.GetDirectoryName(assetPath); if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
      var root = new GameObject("HUD");
      try {
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(root.transform, false);
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var boardGO = new GameObject("Board", typeof(RectTransform)); boardGO.transform.SetParent(canvasGO.transform, false);
        var boardView = boardGO.AddComponent<MergeBoardView>();
        var adapterGO = new GameObject("MergeInputAdapter", typeof(RectTransform)); adapterGO.transform.SetParent(canvasGO.transform, false);
        var adapter = adapterGO.AddComponent<MergeInputAdapter>(); adapter.board = boardView;
        var themeGO = new GameObject("ThemeRoot", typeof(RectTransform)); themeGO.transform.SetParent(canvasGO.transform, false);
        themeGO.AddComponent<ThemeController>();
        var imgGO = new GameObject("ThemedImage", typeof(RectTransform), typeof(Image)); imgGO.transform.SetParent(themeGO.transform, false);
        imgGO.AddComponent<ThemeBinderImage>();
        PrefabUtility.SaveAsPrefabAsset(root, assetPath, out bool ok); if (!ok) throw new System.Exception("SaveAsPrefabAsset failed: " + assetPath);
        AssetDatabase.SaveAssets(); AssetDatabase.ImportAsset(assetPath); AssetDatabase.Refresh();
        Debug.Log("[SimplePrefabMakers] Created " + assetPath);
      } finally { Object.DestroyImmediate(root); }
    }
    public static void CreateDialoguePanelPrefab() {
      const string assetPath = "Assets/App/UI/Narrative/Resources/App/UI/Narrative/Prefabs/DialoguePanel.prefab";
      var dir = Path.GetDirectoryName(assetPath); if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
      var go = new GameObject("DialoguePanel");
      try {
        PrefabUtility.SaveAsPrefabAsset(go, assetPath, out bool ok); if (!ok) throw new System.Exception("SaveAsPrefabAsset failed: " + assetPath);
        AssetDatabase.SaveAssets(); AssetDatabase.ImportAsset(assetPath); AssetDatabase.Refresh();
        Debug.Log("[SimplePrefabMakers] Created " + assetPath);
      } finally { Object.DestroyImmediate(go); }
    }
  }
}
"@
$code | Set-Content -Encoding UTF8 $codePath
function Get-UnityExe {
  $preferred = 'C:\Program Files\Unity\Hub\Editor\6000.2.2f1\Editor\Unity.exe'
  if (Test-Path $preferred) { return $preferred }
  $hub = 'C:\Program Files\Unity\Hub\Editor'
  if (-not (Test-Path $hub)) { throw "Unity Hub Editor path not found: $hub" }
  foreach($v in (Get-ChildItem $hub -Directory | Sort-Object Name -Descending)){
    $cand = Join-Path $v.FullName 'Editor\Unity.exe'
    if (Test-Path $cand) { return $cand }
  }
  throw "Unity.exe not found"
}
if ($RunUnityAfterWrite) {
  $unity = Get-UnityExe
  if ($CreateHUD)         { & $unity -batchmode -nographics -projectPath $repo -executeMethod AQ.BuildTools.SimplePrefabMakers.CreateHUDPrefab -quit -logFile ("_unity_make_HUD_" + (Get-Date -Format 'yyyyMMdd_HHmmss') + ".log") }
  if ($CreateDialoguePanel) { & $unity -batchmode -nographics -projectPath $repo -executeMethod AQ.BuildTools.SimplePrefabMakers.CreateDialoguePanelPrefab -quit -logFile ("_unity_make_Dialogue_" + (Get-Date -Format 'yyyyMMdd_HHmmss') + ".log") }
}
