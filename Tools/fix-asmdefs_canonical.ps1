param(
  [string]$RepoRoot = "."
)

$ErrorActionPreference = "Stop"

function Info($m)  { Write-Host $m -ForegroundColor Cyan }
function Warn($m)  { Write-Host $m -ForegroundColor Yellow }
function Good($m)  { Write-Host $m -ForegroundColor Green }
function Bad($m)   { Write-Host $m -ForegroundColor Red }

# --- Paths (canonical layout from the tech seed) ---
$repo      = (Resolve-Path $RepoRoot).Path
$appDir    = Join-Path $repo "Assets\App"
$edtDir    = Join-Path $repo "Assets\Editor"
$pkRuntime = Join-Path $repo "Packages\com.aq.presentation\Runtime"

$canonAppAsm   = Join-Path $appDir "AQ.App.asmdef"                         # runtime composition
$rogueAppPresA = Join-Path $appDir "AQ.App.Presentation.asmdef"            # rogue
$rogueAppPresB = Join-Path $appDir "Presentation\AQ.App.Presentation.asmdef" # old rogue location

# Editor asmdef can be either path historically; keep whichever exists as canonical and quarantine the other if duplicates
$editorAsmA = Join-Path $edtDir "AQ.Editor.asmdef"
$editorAsmB = Join-Path $edtDir "AQ\AQ.Editor.asmdef"

$pkgPresentationAsm = Join-Path $pkRuntime "AQ30.Presentation.asmdef"

# --- Quarantine helper ---
$quarantineRoot = Join-Path $repo ("_quarantine_asmdefs_" + (Get-Date -Format yyyyMMdd_HHmmss))
function Quarantine-IfExists([string]$path) {
  if (Test-Path $path -PathType Leaf) {
    $rel = Resolve-Path $path | ForEach-Object { $_.Path.Replace($repo + [IO.Path]::DirectorySeparatorChar, "") }
    $dest = Join-Path $quarantineRoot $rel
    New-Item -ItemType Directory -Path (Split-Path $dest -Parent) -Force | Out-Null
    Move-Item -LiteralPath $path -Destination $dest -Force
    Warn "Quarantined: $rel -> $dest"
    return $true
  }
  return $false
}

# --- JSON helpers ---
function Read-Asmdef([string]$path) {
  if (-not (Test-Path $path -PathType Leaf)) { return $null }
  $json = Get-Content -Raw -LiteralPath $path
  try {
    return $json | ConvertFrom-Json -ErrorAction Stop
  } catch {
    Bad "Invalid asmdef JSON: $path"
    throw
  }
}
function Write-Asmdef([string]$path, $obj) {
  $dir = Split-Path $path -Parent
  New-Item -ItemType Directory -Force -Path $dir | Out-Null
  $json = $obj | ConvertTo-Json -Depth 10
  Set-Content -LiteralPath $path -Value ($json + "`r`n")
  Good "Wrote asmdef: $path"
}

# --- 1) Quarantine any rogue AQ.App.Presentation under Assets/App* ---
$didQ = $false
$didQ = (Quarantine-IfExists $rogueAppPresA) -or $didQ
$didQ = (Quarantine-IfExists $rogueAppPresB) -or $didQ
if ($didQ) { Info "Rogue 'AQ.App.Presentation.asmdef' removed to stop name collisions." } else { Info "No rogue 'AQ.App.Presentation.asmdef' found. ✓" }

# --- 2) Ensure canonical runtime asmdef: Assets/App/AQ.App.asmdef ---
$appAsm = Read-Asmdef $canonAppAsm
if ($null -eq $appAsm) {
  Info "Creating canonical runtime asmdef: $canonAppAsm"
  $appAsm = [ordered]@{
    name                 = "AQ.App"
    references           = @("AQ30.Presentation","AQ.Domain.Merge","AQ.SharedKernel")
    includePlatforms     = @()            # runtime (all platforms)
    excludePlatforms     = @()
    allowUnsafeCode      = $false
    overrideReferences   = $false
    precompiledReferences= @()
    autoReferenced       = $true
    defineConstraints    = @()
    versionDefines       = @()
    noEngineReferences   = $false
  }
  Write-Asmdef $canonAppAsm $appAsm
} else {
  $changed = $false
  if ($appAsm.name -ne "AQ.App") { $appAsm.name = "AQ.App"; $changed = $true }
  # normalize refs
  $targetRefs = @("AQ30.Presentation","AQ.Domain.Merge","AQ.SharedKernel")
  if (@($appAsm.references) -join "|" -ne ($targetRefs -join "|")) {
    $appAsm.references = $targetRefs; $changed = $true
  }
  if ($changed) {
    Info "Updating canonical runtime asmdef refs/name."
    Write-Asmdef $canonAppAsm $appAsm
  } else {
    Good "Runtime asmdef OK: $canonAppAsm"
  }
}

# --- 3) Normalize package presentation asmdef refs (must NOT reference AQ.App) ---
$pkgAsm = Read-Asmdef $pkgPresentationAsm
if ($null -eq $pkgAsm) {
  Bad "Missing package asmdef: $pkgPresentationAsm"
  throw "Package com.aq.presentation runtime asmdef not found."
}
$pkgChanged = $false
# name should be AQ30.Presentation (keep if already correct)
if ($pkgAsm.name -ne "AQ30.Presentation") { $pkgAsm.name = "AQ30.Presentation"; $pkgChanged = $true }
$pkgTargetRefs = @("AQ.Domain.Merge","AQ.SharedKernel")
if (@($pkgAsm.references) -join "|" -ne ($pkgTargetRefs -join "|")) {
  $pkgAsm.references = $pkgTargetRefs; $pkgChanged = $true
}
if ($pkgChanged) {
  Info "Updating package presentation refs/name."
  Write-Asmdef $pkgPresentationAsm $pkgAsm
} else {
  Good "Package presentation asmdef OK: $pkgPresentationAsm"
}

# --- 4) Editor asmdef: de-duplicate and align ---
$edPaths = @()
if (Test-Path $editorAsmA -PathType Leaf) { $edPaths += $editorAsmA }
if (Test-Path $editorAsmB -PathType Leaf) { $edPaths += $editorAsmB }

if ($edPaths.Count -eq 0) {
  Bad "No AQ.Editor asmdef found under Assets/Editor. Please ensure the editor asmdef exists."
} elseif ($edPaths.Count -eq 1) {
  $chosen = $edPaths[0]
  Info "Editor asmdef found: $chosen"
  $edAsm = Read-Asmdef $chosen
  $edChanged = $false
  if ($edAsm.name -ne "AQ.Editor") { $edAsm.name = "AQ.Editor"; $edChanged = $true }
  # Ensure editor-only include platforms
  $incl = @($edAsm.includePlatforms)
  if ($incl.Count -ne 1 -or $incl[0] -ne "Editor") { $edAsm.includePlatforms = @("Editor"); $edChanged = $true }
  # Ensure references
  $edTargetRefs = @("AQ30.Presentation","AQ.Domain.Merge","AQ.SharedKernel")
  if (@($edAsm.references) -join "|" -ne ($edTargetRefs -join "|")) { $edAsm.references = $edTargetRefs; $edChanged = $true }
  if ($edChanged) { Write-Asmdef $chosen $edAsm } else { Good "Editor asmdef OK: $chosen" }
} else {
  # two exist; pick the “flat” path as canonical, quarantine the nested one
  Info "Duplicate AQ.Editor asmdefs found; keeping flat path and quarantining nested:"
  $keep = $editorAsmA
  $quar = $editorAsmB
  if (-not (Test-Path $keep -PathType Leaf)) { $keep = $editorAsmB; $quar = $editorAsmA }
  Info " Keeping : $keep"
  Quarantine-IfExists $quar | Out-Null
  # normalize the one we keep
  $edAsm = Read-Asmdef $keep
  $edChanged = $false
  if ($edAsm.name -ne "AQ.Editor") { $edAsm.name = "AQ.Editor"; $edChanged = $true }
  if (@($edAsm.includePlatforms).Count -ne 1 -or $edAsm.includePlatforms[0] -ne "Editor") { $edAsm.includePlatforms = @("Editor"); $edChanged = $true }
  $edTargetRefs = @("AQ30.Presentation","AQ.Domain.Merge","AQ.SharedKernel")
  if (@($edAsm.references) -join "|" -ne ($edTargetRefs -join "|")) { $edAsm.references = $edTargetRefs; $edChanged = $true }
  if ($edChanged) { Write-Asmdef $keep $edAsm } else { Good "Editor asmdef OK: $keep" }
}

Good "Canonical asmdefs enforced."

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Magenta
Write-Host "  1) git status"
Write-Host "  2) git add -A"
Write-Host "  3) git commit -m 'Canonicalize asmdefs: AQ.App, AQ30.Presentation refs, AQ.Editor; quarantine rogues'"
Write-Host "  4) Return to Unity and let it recompile (or re-open the project)."
Write-Host "  5) Run the audits again:"
Write-Host "       pwsh -NoProfile -ExecutionPolicy Bypass -File .\Tools\project-audit.ps1 -RepoRoot ."
Write-Host "       pwsh -NoProfile -ExecutionPolicy Bypass -File .\Tools\wk3-stabilize-audit.ps1 -RepoRoot ."
