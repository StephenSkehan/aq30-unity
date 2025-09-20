param(
  [string]$RepoRoot = '.'
)

# ---------- Basics ----------
$ErrorActionPreference = 'Stop'
$RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
$stamp    = Get-Date -Format 'yyyyMMdd_HHmmss'
$reportDir = Join-Path $RepoRoot '_Audit'
$null = New-Item -ItemType Directory -Force -Path $reportDir
$report = Join-Path $reportDir ("wk3_stabilize_report_{0}.txt" -f $stamp)

$script:PASS = 0
$script:FAIL = 0
$lines = New-Object System.Collections.Generic.List[string]

function Line([string]$msg) {
  $lines.Add($msg)
  Write-Output $msg
}
function Pass([string]$msg) {
  $script:PASS++
  Line "[PASS] $msg"
}
function Fail([string]$msg) {
  $script:FAIL++
  Line "[FAIL] $msg"
}

# ---------- Helpers ----------
function Test-PathExists([string]$relPath, [string]$label) {
  $p = Join-Path $RepoRoot $relPath
  if (Test-Path -LiteralPath $p) { Pass "File exists: $label" }
  else                           { Fail "Missing file: $label" }
}

function Find-First([string]$rootRel, [string]$filter) {
  $root = Join-Path $RepoRoot $rootRel
  Get-ChildItem -Path $root -Recurse -File -Filter $filter -ErrorAction SilentlyContinue | Select-Object -First 1
}

function Get-ProjectCsFiles {
  param(
    [string[]]$RootsRel,
    [switch]$ExcludeEditor,
    [string[]]$ExtraExcludesRegex = @()
  )
  $roots = $RootsRel | ForEach-Object { Join-Path $RepoRoot $_ }
  $files = Get-ChildItem -Path $roots -Recurse -File -Filter *.cs -ErrorAction SilentlyContinue

  $files = $files | Where-Object {
    $_.FullName -notmatch '\\Tests\\'      -and
    $_.FullName -notmatch 'Samples~'       -and
    $_.FullName -notmatch '\\Docs\\'       -and
    $_.FullName -notmatch '\\Library\\'    -and
    $_.FullName -notmatch '\\PackageCache\\'
  }

  if ($ExcludeEditor) {
    $files = $files | Where-Object { $_.FullName -notmatch '\\Editor\\' }
  }
  if ($ExtraExcludesRegex.Count -gt 0) {
    foreach ($rx in $ExtraExcludesRegex) {
      $files = $files | Where-Object { $_.FullName -notmatch $rx }
    }
  }
  return $files
}

# ---------- Rules ----------
function Rule-Header {
  Line "=== WK3 Stabilize Audit ==="
  Line ("RepoRoot: {0}" -f $RepoRoot)
  Line ("Timestamp: {0}" -f (Get-Date))
  Line ""
}

function Rule-KeyFiles {
  Test-PathExists 'Packages/com.aq.presentation/Runtime/GlobalBus.cs'         'Packages/com.aq.presentation/Runtime/GlobalBus.cs'
  Test-PathExists 'Packages/com.aq.presentation/Runtime/MergeEventsBridge.cs' 'Packages/com.aq.presentation/Runtime/MergeEventsBridge.cs'
  Test-PathExists 'Assets/App/Presentation/EventBusInstaller.cs'              'Assets/App/Presentation/EventBusInstaller.cs'
  Test-PathExists 'Assets/App/Presentation/MergeEventsToUI.cs'                'Assets/App/Presentation/MergeEventsToUI.cs'
  Test-PathExists 'Assets/Editor/AQ/GeneratePlaceholderIcons.cs'              'Assets/Editor/AQ/GeneratePlaceholderIcons.cs'
  Test-PathExists 'Assets/Editor/AQ/MergeUIBridgeVerify.cs'                   'Assets/Editor/AQ/MergeUIBridgeVerify.cs'
}

function Rule-Asmdefs {
  $appAsm   = Join-Path $RepoRoot 'Assets/App/AQ.App.Presentation.asmdef'
  $editorAsm= Join-Path $RepoRoot 'Assets/Editor/AQ.Editor.asmdef'
  if (Test-Path $appAsm)   { Pass 'Assets/App/AQ.App.Presentation.asmdef present' } else { Fail 'Missing asmdef: Assets/App/AQ.App.Presentation.asmdef' }
  if (Test-Path $editorAsm){ Pass 'Assets/Editor/AQ.Editor.asmdef present' }       else { Fail 'Missing asmdef: Assets/Editor/AQ.Editor.asmdef' }
}

function Rule-RecipeSpawn {
  $rb = Find-First 'Assets' 'RecipeBook.asset'
  if ($rb) { Pass 'RecipeBook present' } else { Fail 'RecipeBook.asset not found under Assets/' }

  $sp = Find-First 'Assets' 'SpawnPolicy.asset'
  if ($sp) { Pass 'SpawnPolicy present' } else { Fail 'SpawnPolicy.asset not found under Assets/' }
}

function Rule-Icons {
  $pngs = Get-ChildItem -Path (Join-Path $RepoRoot 'Assets') -Recurse -File -Include *.png -ErrorAction SilentlyContinue |
          Where-Object { $_.FullName -notmatch '\\Library\\' -and $_.FullName -notmatch '\\PackageCache\\' }
  $count = ($pngs | Measure-Object).Count
  if ($count -ge 3) { Pass ("Icons present: {0} png(s)" -f $count) }
  else              { Fail ("Icons missing: found {0} png(s) under Assets/" -f $count) }
}

# Forbid UnityEditor.AddressableAssets in NON-Editor code only.
function Rule-NoAddressablesEditorInRuntime {
  $pattern = '\bUnityEditor\.AddressableAssets\b'
  $hits = Get-ProjectCsFiles -RootsRel @('Assets', 'Packages\com.aq.*') -ExcludeEditor |
          Select-String -Pattern $pattern -SimpleMatch:$false

  if ($hits) {
    $files = $hits | ForEach-Object { $_.Path } | Sort-Object -Unique
    Fail ("UnityEditor.AddressableAssets used outside Editor folders: {0}" -f ($files -join '; '))
  } else {
    Pass "No UnityEditor.AddressableAssets in runtime/non-Editor code"
  }
}

# Editor ASCII hygiene (skip addressables wiring; allowed to be UTF-8)
function Rule-EditorAscii {
  $extraExcludes = @('Addressables', 'EnsureAddressablesDefine\.cs')
  $files = Get-ProjectCsFiles -RootsRel @('Assets\Editor') -ExtraExcludesRegex $extraExcludes

  $bad = @()
  foreach ($f in $files) {
    $text = Get-Content -LiteralPath $f.FullName -Raw -ErrorAction SilentlyContinue
    if ($null -ne $text -and $text -match '[^\u0000-\u007F]') { $bad += $f.FullName }
  }

  if ($bad.Count -gt 0) {
    Fail ("Non-ASCII bytes in: {0}" -f ($bad -join '; '))
  } else {
    Pass "Editor scripts (excluding Addressables wiring) are ASCII-clean"
  }
}

function Rule-Summary {
  Line ""
  Line ("Summary: PASS={0}, FAIL={1}" -f $script:PASS, $script:FAIL)
  Line ("Report: {0}" -f $report)
  Set-Content -Path $report -Value ($lines -join [Environment]::NewLine) -Encoding UTF8
}

# ---------- Run ----------
Rule-Header
Rule-KeyFiles
Rule-Asmdefs
Rule-RecipeSpawn
Rule-Icons
Rule-EditorAscii
Rule-NoAddressablesEditorInRuntime
Rule-Summary

# Exit non-zero on FAIL to signal CI
if ($script:FAIL -gt 0) { exit 1 } else { exit 0 }
