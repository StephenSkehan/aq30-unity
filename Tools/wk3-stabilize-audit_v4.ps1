[CmdletBinding()]
param([string]$RepoRoot = ".")
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Pass([string]$m){ Write-Host "[PASS] $m" -ForegroundColor Green }
function Fail([string]$m){ Write-Host "[FAIL] $m" -ForegroundColor Red }
function Info([string]$m){ Write-Host "$m"       -ForegroundColor Cyan }

$root = (Resolve-Path $RepoRoot).Path
$ts   = Get-Date -Format "dd/MM/yyyy h:mm tt"

Write-Host "=== WK3 Stabilize Audit (v4) ==="
Write-Host ("RepoRoot: {0}" -f $root)
Write-Host ("Timestamp: {0}" -f $ts)
Write-Host ""

# v4 expectations:
# - Canonical runtime asmdefs: AQ.SharedKernel (pkg), AQ.Domain.Merge (pkg), AQ.App (Assets/App)
# - Presentation bridge files under Assets/App/Presentation/
# - AQ.App.Presentation.asmdef removed in v4

# 1) Required App/Presentation files
$expect = @(
  "Assets/App/Presentation/GlobalBus.cs",
  "Assets/App/Presentation/MergeEventsBridge.cs",
  "Assets/App/Presentation/EventBusInstaller.cs",
  "Assets/App/Presentation/MergeEventsToUI.cs"
)
foreach($rel in $expect){
  $p = Join-Path $root $rel
  if(Test-Path -LiteralPath $p){ Pass "File exists: $rel" } else { Fail "Missing file: $rel" }
}

# 2) Editor asmdef present
$editorAsm = Join-Path $root "Assets/Editor/AQ.Editor.asmdef"
if(Test-Path -LiteralPath $editorAsm){ Pass "Assets/Editor/AQ.Editor.asmdef present" } else { Fail "Missing asmdef: Assets/Editor/AQ.Editor.asmdef" }

# 3) Icons check
$iconsDir = Join-Path $root "Assets/Art/Icons"
if(Test-Path -LiteralPath $iconsDir){
  $icons = @(Get-ChildItem -Path $iconsDir -Filter *.png -Recurse -ErrorAction SilentlyContinue)
  if($icons.Count -ge 6){ Pass ("Icons present: {0} png(s)" -f $icons.Count) } else { Fail "Icons missing or too few (<6) in Assets/Art/Icons" }
}else{
  Fail "Icons folder missing: Assets/Art/Icons"
}

# 4) Canonical runtime asmdefs (ignore Editor/Tests/Library/PackageCache and any quarantine folders)
$asmdefFiles = Get-ChildItem -Path $root -Recurse -Filter *.asmdef -File |
  Where-Object {
    $_.FullName -notmatch '\\Editor\\' -and
    $_.FullName -notmatch '\\Tests\\' -and
    $_.FullName -notmatch '\\Library\\PackageCache\\' -and
    $_.FullName -notmatch '(\\|/)(?:_quarantine|Z\.?_quarantine)[^\\/]*(\\|/)'
  }

$names = @()
foreach($f in $asmdefFiles){
  try {
    $j = Get-Content -Raw -LiteralPath $f.FullName | ConvertFrom-Json
    if($j.name){ $names += [string]$j.name }
  } catch {}
}
$names = @($names | Sort-Object -Unique)

$allowed = @('AQ.SharedKernel','AQ.Domain.Merge','AQ.App')
$missing = @($allowed | Where-Object { $names -notcontains $_ })
$extras  = @($names   | Where-Object { $allowed -notcontains $_ })

if($missing.Count -eq 0 -and $extras.Count -eq 0){
  Pass ("Canonical runtime asmdefs detected: {0}" -f ($names -join ', '))
}else{
  if($missing.Count -gt 0){ Fail ("Missing canonical asmdefs: {0}" -f ($missing -join ', ')) }
  if($extras.Count  -gt 0){ Fail ("Non-canonical runtime asmdefs present: {0}" -f ($extras -join ', ')) }
}

# 5) Legacy v3 check explicitly skipped
Write-Host "[SKIP] AQ.App.Presentation.asmdef removed in v4 (expected)" -ForegroundColor Yellow

# 6) No UnityEditor.AddressableAssets in runtime code (exclude Editor)
$allCs = @(Get-ChildItem -Path (Join-Path $root 'Assets') -Recurse -Filter *.cs -File -ErrorAction SilentlyContinue |
           Where-Object { $_.FullName -notmatch '\\Editor\\' })
$offenders = @()
foreach($cs in $allCs){
  try {
    $text = Get-Content -Raw -LiteralPath $cs.FullName
    if($text -match 'UnityEditor\.AddressableAssets'){
      $offenders += $cs.FullName.Substring($root.Length+1)
    }
  } catch {}
}
if($offenders.Count -eq 0){
  Pass 'No UnityEditor.AddressableAssets in runtime/non-Editor code'
}else{
  Fail ('UnityEditor.AddressableAssets reference found in runtime code: ' + ($offenders -join ', '))
}

Write-Host ""
