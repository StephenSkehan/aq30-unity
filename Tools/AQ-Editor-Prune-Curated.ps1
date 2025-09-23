<# 
AQ-Editor-Prune-Curated.ps1

Curated, deterministic cleanup for "Editor" scripts that are very likely one-offs / obsolete.
- Safe default: DRY RUN unless -Apply is passed.
- Action: MOVE candidates to Backups\Pruned\curated_<timestamp>\... (no hard delete).
- Two scopes:
    Minimal   = only clearly legacy/prototype/sample/one-off items
    Aggressive= Minimal + debug helpers, probes, extra variant/overlay tools

Usage:
  pwsh -NoProfile -ExecutionPolicy Bypass .\Tools\AQ-Editor-Prune-Curated.ps1 -Scope Minimal
  pwsh -NoProfile -ExecutionPolicy Bypass .\Tools\AQ-Editor-Prune-Curated.ps1 -Scope Minimal -Apply
  pwsh -NoProfile -ExecutionPolicy Bypass .\Tools\AQ-Editor-Prune-Curated.ps1 -Scope Aggressive -WhatIf
  (Note: -WhatIf is the built-in common parameter provided by SupportsShouldProcess.)
#>

[CmdletBinding(SupportsShouldProcess=$true)]
param(
  [ValidateSet('Minimal','Aggressive')]
  [string]$Scope = 'Minimal',
  [switch]$Apply
)

$ErrorActionPreference = 'Stop'

# --- Resolve project root and setup ---
$ProjectRoot = (Resolve-Path ".").Path
$Stamp       = Get-Date -Format "yyyyMMdd_HHmmss"
$BackupRoot  = Join-Path $ProjectRoot ("Backups\Pruned\curated_{0}" -f $Stamp)

function Add-Candidates {
  param([string[]]$Paths,[ref]$Bag)
  foreach($p in $Paths){
    if([string]::IsNullOrWhiteSpace($p)){ continue }
    $full = Join-Path $ProjectRoot $p
    if(Test-Path $full){ $Bag.Value += ,$full }
  }
}

# --- Curated candidate sets (relative to repo root) ---

# 1) Minimal: week-prototype scaffolding, sample menus, duplicate "Min" maker files,
#    integration/runtime probes, and a redundant overlay tidy.
$candidatesMinimal = @(
  # Week 3 audits (prototype scaffolding)
  'Assets\Editor\AQ\Audit\WK3_1_TmpExplainer.cs',
  'Assets\Editor\AQ\Audit\WK3_1_Validator.cs',
  'Assets\Editor\AQ\Audit\WK3_4_Discover.cs',
  'Assets\Editor\AQ\Audit\WK3_4_IntegrationProbe.cs',

  # Variants (week 3 specific, A/B/C binders & verifiers)
  'Assets\Editor\AQ\Variants\WK3_2_BindA_ToOverlay.cs',
  'Assets\Editor\AQ\Variants\WK3_2_BindB_ToOverlay.cs',
  'Assets\Editor\AQ\Variants\WK3_2_BindC_ToOverlay.cs',
  'Assets\Editor\AQ\Variants\WK3_2_RuntimeProbe.cs',
  'Assets\Editor\AQ\Variants\WK3_2_VariantA_Verifier.cs',
  'Assets\Editor\AQ\Variants\WK3_2_VariantMenu.cs',

  # Sample content / demo menus
  'Assets\Editor\Dialogue\DialogueSampleMenus.cs',

  # Prototype/minimal maker duplicates
  'Assets\Editor\HUDPrefabMaker.Min.cs',
  'Assets\Editor\PrefabMakers.Min.cs',

  # Overlay tidy duplicate (prefer the layout-aware one)
  'Assets\Editor\AQ\Content\OverlayTidy_NoLayout.cs'
)

# 2) Aggressive adds debug & extra probes used during bring-up.
$candidatesAggressive = $candidatesMinimal + @(
  # Debug helpers used during bring-up
  'Assets\Editor\AQ\Debug\ListAdvanceWiring.cs',
  'Assets\Editor\AQ\Debug\OverlayForceVisible.cs',
  'Assets\Editor\AQ\Debug\ResolutionOverlayByPS.cs',

  # Extra diagnostics that were useful once
  'Assets\Editor\AQ\CaseFlowProbe.cs',
  'Assets\Editor\AQ\Diag\DialogueContractsProbe.cs',
  'Assets\Editor\AQ\Diag\DialogueGuidProbe.cs',
  'Assets\Editor\AQ\Diag\DialoguePrefabSanity.cs'
)

# --- Build candidate list for requested scope ---
$candidates = @()
switch($Scope){
  'Minimal'    { Add-Candidates -Paths $candidatesMinimal    -Bag ([ref]$candidates) }
  'Aggressive' { Add-Candidates -Paths $candidatesAggressive -Bag ([ref]$candidates) }
}

# De-dup and sort
$candidates = $candidates | Sort-Object -Unique
if($candidates.Count -eq 0){
  Write-Host "Nothing to prune for scope '$Scope' (all curated paths missing or already moved)." -ForegroundColor Yellow
  return
}

# --- Dry-run summary ---
Write-Host ""
Write-Host "🔎 Curated prune plan ($Scope):" -ForegroundColor Cyan
$candidates | ForEach-Object {
  $rel = $_.Substring($ProjectRoot.Length).TrimStart('\','/')
  Write-Host " - $rel"
}
Write-Host ""
Write-Host ("Total candidates: {0}" -f $candidates.Count)

# --- Execute (move) if -Apply, else DRY RUN ---
if(-not $Apply){
  Write-Host ""
  Write-Host "DRY RUN: pass -Apply to move these files. Use -WhatIf with -Apply to preview moves." -ForegroundColor Yellow
  return
}

# Ensure backup root exists
if(-not (Test-Path $BackupRoot)){ New-Item -ItemType Directory -Path $BackupRoot | Out-Null }

$moveCount = 0
foreach($src in $candidates){
  $rel   = $src.Substring($ProjectRoot.Length).TrimStart('\','/')
  $dest  = Join-Path $BackupRoot $rel
  $dDir  = Split-Path $dest -Parent
  if(-not (Test-Path $dDir)){ New-Item -ItemType Directory -Path $dDir | Out-Null }

  if($PSCmdlet.ShouldProcess($src,"Move -> $dest")){
    Move-Item -LiteralPath $src -Destination $dest -Force
    $moveCount++
    Write-Host ("Moved: {0}" -f $rel) -ForegroundColor Green
  }
}

Write-Host ""
Write-Host ("✅ Completed. Files moved: {0}" -f $moveCount) -ForegroundColor Green
Write-Host ("Backup folder: {0}" -f $BackupRoot)
