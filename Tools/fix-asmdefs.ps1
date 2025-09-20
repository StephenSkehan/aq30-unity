param(
  [string]$RepoRoot = '.'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Ensure-SingleAsmdef {
  param(
    [Parameter(Mandatory)][string]$PrimaryPath,
    [Parameter(Mandatory)][string]$DupePath,
    [Parameter(Mandatory)][string]$AssemblyName
  )

  $primaryExists = Test-Path -Path $PrimaryPath -PathType Leaf
  $dupeExists    = Test-Path -Path $DupePath    -PathType Leaf

  if ($primaryExists -and $dupeExists) {
    Write-Host "Removing duplicate asmdef: $DupePath"
    try { git rm -f -- "$DupePath" | Out-Null } catch { }
    if (Test-Path -Path $DupePath) { Remove-Item -Force -- "$DupePath" }
  }
  elseif (-not $primaryExists -and $dupeExists) {
    Write-Host "Moving asmdef from duplicate location to primary:"
    Write-Host "  $DupePath  ->  $PrimaryPath"
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $PrimaryPath) | Out-Null
    try { git mv -- "$DupePath" "$PrimaryPath" | Out-Null }
    catch {
      Move-Item -Force -- "$DupePath" "$PrimaryPath"
      git add -- "$PrimaryPath" | Out-Null
    }
  }
  elseif (-not $primaryExists -and -not $dupeExists) {
    Write-Host "Creating missing asmdef at $PrimaryPath"
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $PrimaryPath) | Out-Null
    $asm = @{
      name                 = $AssemblyName
      references           = @()
      includePlatforms     = @()
      excludePlatforms     = @()
      allowUnsafeCode      = $false
      overrideReferences   = $false
      precompiledReferences= @()
      autoReferenced       = $true
      defineConstraints    = @()
      versionDefines       = @()
      noEngineReferences   = $false
    } | ConvertTo-Json -Depth 6
    Set-Content -Path $PrimaryPath -Value $asm -Encoding UTF8
    git add -- "$PrimaryPath" | Out-Null
  }
}

function Ensure-Asmdef-References {
  param(
    [Parameter(Mandatory)][string]$AsmdefPath,
    [Parameter(Mandatory)][string[]]$RefsToAdd
  )
  if (-not (Test-Path -Path $AsmdefPath -PathType Leaf)) { return }

  $json = Get-Content -Raw -- $AsmdefPath | ConvertFrom-Json
  if (-not $json.PSObject.Properties.Name.Contains('references') -or -not $json.references) {
    $json | Add-Member -NotePropertyName references -NotePropertyValue @()
  }
  foreach ($r in $RefsToAdd) {
    if ($json.references -notcontains $r) { $json.references += $r }
  }
  ($json | ConvertTo-Json -Depth 10) | Set-Content -Path $AsmdefPath -Encoding UTF8
  Write-Host "Updated references in $AsmdefPath -> $($RefsToAdd -join ', ')"
}

# ---- Paths (normalized under the provided RepoRoot) ----
$presentPrimary = Join-Path $RepoRoot 'Assets/App/Presentation/AQ.App.Presentation.asmdef'
$presentDupe    = Join-Path $RepoRoot 'Assets/App/AQ.App.Presentation.asmdef'

$editorPrimary  = Join-Path $RepoRoot 'Assets/Editor/AQ.Editor.asmdef'
$editorDupe     = Join-Path $RepoRoot 'Assets/Editor/AQ/AQ.Editor.asmdef'

$pkgPresentationAsmdef = Join-Path $RepoRoot 'Packages/com.aq.presentation/Runtime/AQ30.Presentation.asmdef'

# ---- Enforce one asmdef per location ----
Ensure-SingleAsmdef -PrimaryPath $presentPrimary -DupePath $presentDupe -AssemblyName 'AQ.App.Presentation'
Ensure-SingleAsmdef -PrimaryPath $editorPrimary  -DupePath $editorDupe  -AssemblyName 'AQ.Editor'

# ---- Make sure references are present where needed ----
# Presentation assembly should be able to see AQ30.Presentation + Merge/SharedKernel domain code.
Ensure-Asmdef-References -AsmdefPath $presentPrimary -RefsToAdd @('AQ30.Presentation','AQ.Domain.Merge','AQ.SharedKernel')

# Package runtime asmdef should also reference Domain types used by MergeEventsBridge, etc.
Ensure-Asmdef-References -AsmdefPath $pkgPresentationAsmdef -RefsToAdd @('AQ.Domain.Merge','AQ.SharedKernel')

Write-Host ""
Write-Host "Next:"
Write-Host "  1) git status"
Write-Host "  2) git commit -m 'Fix: dedupe asmdefs; add domain refs to presentation assemblies'"
Write-Host "  3) Return to Unity and let it recompile."
