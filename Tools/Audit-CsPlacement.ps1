<# 
Audits Unity C# files to ensure they're placed in the correct folders.

Editor code (should live under a folder named "Editor"):
 - uses UnityEditor, or Editor attributes/types (MenuItem, InitializeOnLoad, EditorWindow, CustomEditor,
   PropertyDrawer, AssetPostprocessor, SettingsProvider), or namespace like "AQ.Editor"

Runtime code (should live outside any "Editor" folder):
 - derives from MonoBehaviour/ScriptableObject, or uses UnityEngine, or namespace like "AQ.App"/"AQ.UI"

Outputs a table and CSV with Path, InEditorFolder, Namespace, Suggest, Status, etc.
Exit code: 0 = all good, 2 = files to move/review.

Usage:
  powershell -ExecutionPolicy Bypass -File tools\Audit-CsPlacement.ps1
#>

param(
  [string]$Root = "Assets",
  [string]$CsvOut = ("cs_audit_{0:yyyyMMdd_HHmmss}.csv" -f (Get-Date))
)

Write-Host "Scanning $Root for .cs files..." -ForegroundColor Cyan
if (!(Test-Path $Root)) {
  Write-Error "Root path '$Root' not found. Run from your repo root or pass -Root <path>."
  exit 1
}

# Collect .cs files (skip Library / Packages cache)
$files = Get-ChildItem -Path $Root -Filter *.cs -Recurse -ErrorAction SilentlyContinue |
  Where-Object { $_.FullName -notmatch '\\Library\\' -and $_.FullName -notmatch '\\Packages\\' }

if ($files.Count -eq 0) {
  Write-Warning "No .cs files found under '$Root'."
  exit 0
}

function Read-FileText([string]$path) {
  try   { return [System.IO.File]::ReadAllText($path) }
  catch { return "" }
}

$data = @()

foreach ($file in $files) {
  $text = Read-FileText $file.FullName

  $inEditorFolder = ($file.FullName -match "(^|[\\/])Editor([\\/]|$)")

  # ----- Editor markers
  $usesUnityEditor = ($text -match '\busing\s+UnityEditor\b' -or $text -match '\bUnityEditor\.')
  $hasMenuItem     = ($text -match '\[MenuItem\b')
  $hasInitOnLoad   = ($text -match '\[InitializeOnLoad\b')
  $isEditorWindow  = ($text -match ':\s*EditorWindow\b')
  $isCustomEditor  = ($text -match '\bCustomEditor\b')
  $isPropDrawer    = ($text -match '\bPropertyDrawer\b')
  $isAssetPost     = ($text -match '\bAssetPostprocessor\b')
  $isSettingsProv  = ($text -match '\bSettingsProvider\b')

  $nsMatch = [regex]::Match($text, 'namespace\s+([A-Za-z0-9_.]+)')
  $namespace = ""
  if ($nsMatch.Success) { $namespace = $nsMatch.Groups[1].Value }

  # ----- Runtime markers
  $isMono          = ($text -match ':\s*MonoBehaviour\b')
  $isSO            = ($text -match ':\s*ScriptableObject\b')
  $usesUnityEngine = ($text -match '\busing\s+UnityEngine\b' -or $text -match '\bUnityEngine\.')

  $nsSuggestEditor  = ($namespace -match '\bAQ\.Editor\b')
  $nsSuggestRuntime = (($namespace -match '\bAQ\.App\b') -or ($namespace -match '\bAQ\.UI\b'))

  $looksEditor  = ($usesUnityEditor -or $hasMenuItem -or $hasInitOnLoad -or $isEditorWindow -or
                   $isCustomEditor -or $isPropDrawer -or $isAssetPost -or $isSettingsProv -or
                   $nsSuggestEditor)

  $looksRuntime = ($isMono -or $isSO -or $usesUnityEngine -or $nsSuggestRuntime)

  $suggest = "Unknown"
  if ($looksEditor) { $suggest = "Editor" }
  elseif ($looksRuntime) { $suggest = "Runtime" }

  $status = "OK"
  if ($suggest -eq "Editor" -and -not $inEditorFolder) { $status = "MOVE_TO_EDITOR" }
  elseif ($suggest -eq "Runtime" -and $inEditorFolder) { $status = "MOVE_OUT_OF_EDITOR" }
  elseif ($suggest -eq "Unknown") { $status = "REVIEW" }

  $data += [pscustomobject]@{
    Path            = $file.FullName
    InEditorFolder  = $inEditorFolder
    Namespace       = $namespace
    Suggest         = $suggest
    Status          = $status
    UsesUnityEditor = $usesUnityEditor
    MenuItemAttr    = $hasMenuItem
    EditorWindow    = $isEditorWindow
    MonoBehaviour   = $isMono
    ScriptableObj   = $isSO
  }
}

$sorted = $data | Sort-Object @{Expression='Status';Descending=$false}, Path

# Summary
$summary = $data | Group-Object Status | Sort-Object Name
Write-Host ""
Write-Host "==== Audit Summary ====" -ForegroundColor Yellow
foreach ($g in $summary) { "{0,-16} {1,5}" -f $g.Name, $g.Count }
Write-Host "=======================" -ForegroundColor Yellow
Write-Host ""

# Details
$sorted | Format-Table -Auto Path, InEditorFolder, Namespace, Suggest, Status

# CSV
$sorted | Export-Csv -Path $CsvOut -NoTypeInformation -Encoding UTF8
Write-Host ""
Write-Host "Report written to $CsvOut" -ForegroundColor Green

# Exit code for CI/local checks
$hasIssues = $data | Where-Object { $_.Status -in @('MOVE_TO_EDITOR','MOVE_OUT_OF_EDITOR','REVIEW') }
if ($hasIssues) {
  Write-Warning "Audit found files that should be moved or reviewed."
  exit 2
} else {
  Write-Host "All files look correctly placed." -ForegroundColor Green
  exit 0
}
