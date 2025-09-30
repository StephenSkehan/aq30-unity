<#
Moves Unity C# files into/out of "Editor" folders based on simple heuristics
(the same ones used in the audit script). Default is DRY-RUN; add -Apply to move.
It moves the .cs **and** its .meta together to preserve GUIDs.

Examples:
  powershell -ExecutionPolicy Bypass -File tools\Move-CsByAudit.ps1
  powershell -ExecutionPolicy Bypass -File tools\Move-CsByAudit.ps1 -Apply
  powershell -ExecutionPolicy Bypass -File tools\Move-CsByAudit.ps1 -Apply -Skip '*TextMesh Pro*','*Examples & Extras*'
#>

param(
  [string]$Root = "Assets",
  [switch]$Apply,
  [string[]]$Skip = @()
)

function Read-FileText([string]$path) {
  try   { return [System.IO.File]::ReadAllText($path) }
  catch { return "" }
}

if (!(Test-Path $Root)) { Write-Error "Root '$Root' not found."; exit 1 }

# Collect .cs files (ignore Library/Packages)
$files = Get-ChildItem -Path $Root -Filter *.cs -Recurse -ErrorAction SilentlyContinue |
  Where-Object { $_.FullName -notmatch '\\Library\\' -and $_.FullName -notmatch '\\Packages\\' }

# Optional skip patterns
if ($Skip.Count -gt 0) {
  $files = $files | Where-Object {
    $keep = $true
    foreach ($pat in $Skip) { if ($_.FullName -like $pat) { $keep = $false; break } }
    $keep
  }
}

$plan = @()

foreach ($f in $files) {
  $txt = Read-FileText $f.FullName
  $inEditorFolder = ($f.FullName -match "(^|[\\/])Editor([\\/]|$)")

  # Heuristics to detect Editor scripts
  $usesUnityEditor = ($txt -match '\busing\s+UnityEditor\b' -or $txt -match '\bUnityEditor\.')
  $hasMenuItem     = ($txt -match '\[MenuItem\b')
  $hasInitOnLoad   = ($txt -match '\[InitializeOnLoad\b')
  $isEditorWindow  = ($txt -match ':\s*EditorWindow\b')
  $isCustomEditor  = ($txt -match '\bCustomEditor\b')
  $isPropDrawer    = ($txt -match '\bPropertyDrawer\b')
  $isAssetPost     = ($txt -match '\bAssetPostprocessor\b')
  $isSettingsProv  = ($txt -match '\bSettingsProvider\b')

  # Namespace (best-effort)
  $nsMatch = [regex]::Match($txt, 'namespace\s+([A-Za-z0-9_.]+)')
  $ns = ""
  if ($nsMatch.Success) { $ns = $nsMatch.Groups[1].Value }

  # Heuristics to detect runtime scripts
  $isMono          = ($txt -match ':\s*MonoBehaviour\b')
  $isSO            = ($txt -match ':\s*ScriptableObject\b')
  $usesUnityEngine = ($txt -match '\busing\s+UnityEngine\b' -or $txt -match '\bUnityEngine\.')

  $looksEditor  = ($usesUnityEditor -or $hasMenuItem -or $hasInitOnLoad -or
                   $isEditorWindow -or $isCustomEditor -or $isPropDrawer -or
                   $isAssetPost -or $isSettingsProv -or ($ns -match '\bAQ\.Editor\b'))

  $looksRuntime = ($isMono -or $isSO -or $usesUnityEngine -or
                   ($ns -match '\bAQ\.App\b') -or ($ns -match '\bAQ\.UI\b'))

  $suggest = "Unknown"
  if ($looksEditor) { $suggest = "Editor" }
  elseif ($looksRuntime) { $suggest = "Runtime" }

  $status = "OK"
  if ($suggest -eq "Editor" -and -not $inEditorFolder) { $status = "MOVE_TO_EDITOR" }
  elseif ($suggest -eq "Runtime" -and $inEditorFolder) { $status = "MOVE_OUT_OF_EDITOR" }
  elseif ($suggest -eq "Unknown") { $status = "REVIEW" }

  if ($status -in @('MOVE_TO_EDITOR','MOVE_OUT_OF_EDITOR')) {
    $srcDir = Split-Path $f.FullName -Parent
    $name   = Split-Path $f.FullName -Leaf

    if ($status -eq 'MOVE_TO_EDITOR') {
      $dstDir = Join-Path $srcDir "Editor"
    } else {
      # move out of Editor -> go to parent of the Editor folder
      $dstDir = Split-Path $srcDir -Parent
    }

    $dstPath = Join-Path $dstDir $name

    # Avoid collision by appending .moved
    if (Test-Path $dstPath) {
      $base = [System.IO.Path]::GetFileNameWithoutExtension($name)
      $ext  = [System.IO.Path]::GetExtension($name)
      $dstPath = Join-Path $dstDir ($base + ".moved" + $ext)
    }

    $metaSrc = $f.FullName + ".meta"
    $metaDst = $dstPath + ".meta"
    $srcMetaOut = $null
    if (Test-Path $metaSrc) { $srcMetaOut = $metaSrc }

    $plan += New-Object psobject -Property @{
      Status     = $status
      Source     = $f.FullName
      SourceMeta = $srcMetaOut
      Dest       = $dstPath
      DestMeta   = $metaDst
    }
  }
}

if ($plan.Count -eq 0) {
  Write-Host "No moves required. (Nothing classified as MOVE_TO_EDITOR or MOVE_OUT_OF_EDITOR.)" -ForegroundColor Green
  exit 0
}

Write-Host "===== MOVE PLAN (DRY-RUN=$([string]::Copy(((-not $Apply)).ToString()))) =====" -ForegroundColor Yellow
$plan | Format-Table -Auto Status, Source, Dest
Write-Host "==========================================================" -ForegroundColor Yellow

if (-not $Apply) {
  Write-Host "`nNothing moved. Re-run with -Apply to perform moves." -ForegroundColor Cyan
  exit 0
}

# Execute moves
$errors = 0
foreach ($m in $plan) {
  try {
    $dstDir = Split-Path $m.Dest -Parent
    if (!(Test-Path $dstDir)) { New-Item -ItemType Directory -Path $dstDir | Out-Null }

    Move-Item -LiteralPath $m.Source -Destination $m.Dest -Force
    if ($m.SourceMeta -and (Test-Path $m.SourceMeta)) {
      Move-Item -LiteralPath $m.SourceMeta -Destination $m.DestMeta -Force
    }
    Write-Host "Moved: $($m.Source) -> $($m.Dest)" -ForegroundColor Green
  }
  catch {
    Write-Warning "Failed to move: $($m.Source) -> $($m.Dest) : $($_.Exception.Message)"
    $errors++
  }
}

if ($errors -gt 0) { exit 2 } else { exit 0 }
