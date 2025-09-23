#Requires -Version 7.0
<#
AQ-Editor-Whitelist-Sweep.ps1 (final)

Strategy (robust):
- Find ALL C# files under Assets whose path contains "\Editor\" (segment match).
- Keep ONLY files with leaf names in a hard whitelist (directory-agnostic).
- Default: list plan, no file changes. With -Apply: backup mirror, then move non-whitelisted to Backups\EditorSweep\<stamp>\removed.
- Use -HardDelete to permanently delete instead of moving. Use -Force to bypass sanity check.

Usage:
  pwsh -NoProfile -ExecutionPolicy Bypass .\Tools\AQ-Editor-Whitelist-Sweep.ps1 -ListOnly
  pwsh -NoProfile -ExecutionPolicy Bypass .\Tools\AQ-Editor-Whitelist-Sweep.ps1          # dry run + backup mirror
  pwsh -NoProfile -ExecutionPolicy Bypass .\Tools\AQ-Editor-Whitelist-Sweep.ps1 -Apply   # move non-whitelist to removed
  pwsh -NoProfile -ExecutionPolicy Bypass .\Tools\AQ-Editor-Whitelist-Sweep.ps1 -Apply -HardDelete
  pwsh -NoProfile -ExecutionPolicy Bypass .\Tools\AQ-Editor-Whitelist-Sweep.ps1 -Apply -Force
#>

[CmdletBinding(SupportsShouldProcess = $true)]
param(
  [switch]$Apply,
  [switch]$HardDelete,
  [switch]$Force,
  [switch]$ListOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function To-Array([object]$x){
  if ($null -eq $x) { return @() }
  if ($x -is [string]) { return @($x) }
  if ($x -is [System.Collections.IEnumerable]) { return @($x) }
  return @($x)
}
function Get-Count($items){ return (To-Array $items | Measure-Object).Count }

# Hard whitelist (leaf filenames, case-insensitive)
$KeepNames = @(
  'TopBarNukeRebuild.cs',
  'TopBarFinalize.cs',
  'AuditCanvasBoard.cs',
  'UIImportGuard.cs',
  'LeadsBarWire.cs',
  'LeadsBarCleanup.cs',
  'LeadCard_FloatActors.cs',
  'RequirementsHUDWire.cs',
  'FindDotItem.cs'
)

$ProjectRoot = (Resolve-Path ".").Path
$AssetsRoot  = Join-Path $ProjectRoot "Assets"
if (-not (Test-Path $AssetsRoot)) { throw "Assets folder not found at: $AssetsRoot" }

# Robust file discovery: ANY .cs under Assets whose path contains "\Editor\" (segment)
$EditorFiles = Get-ChildItem -Path $AssetsRoot -Recurse -File -Include *.cs -ErrorAction SilentlyContinue |
               Where-Object { $_.FullName -match '\\Editor\\' } |
               Sort-Object FullName
$Total = Get-Count $EditorFiles
if ($Total -eq 0) { Write-Warning "No .cs files found under Assets/** with an '\Editor\' segment. Nothing to do."; return }

# Classify by leaf filename
$keepSet     = New-Object System.Collections.Generic.List[System.IO.FileInfo]
$removeSet   = New-Object System.Collections.Generic.List[System.IO.FileInfo]
$keepHash    = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
$KeepNames | ForEach-Object { [void]$keepHash.Add($_) }

foreach($f in $EditorFiles){
  $leaf = [System.IO.Path]::GetFileName($f.FullName)
  if($keepHash.Contains($leaf)){ [void]$keepSet.Add($f) } else { [void]$removeSet.Add($f) }
}

$KeepCount   = Get-Count $keepSet
$RemoveCount = Get-Count $removeSet

Write-Host "Editor whitelist sweep (by filename; path contains '\Editor\')" -ForegroundColor Cyan
Write-Host (" - Editor .cs total : {0}" -f $Total)
Write-Host (" - KEEP (whitelist) : {0}" -f $KeepCount)
Write-Host (" - REMOVE (others)  : {0}" -f $RemoveCount)

if(-not $Force -and $KeepCount -lt 5){
  Write-Warning "Sanity check: only $KeepCount files would be KEPT. Use -Force if this is expected."
  return
}

Write-Host "`nKEEP (whitelist):" -ForegroundColor Green
foreach($k in (To-Array $keepSet | Sort-Object FullName)){
  $rel = $k.FullName.Substring($ProjectRoot.Length).TrimStart('\','/')
  Write-Host ("  + {0}" -f $rel)
}

Write-Host "`nREMOVE (others):" -ForegroundColor Yellow
foreach($r in (To-Array $removeSet | Sort-Object FullName)){
  $rel = $r.FullName.Substring($ProjectRoot.Length).TrimStart('\','/')
  Write-Host ("  - {0}" -f $rel)
}

if($ListOnly){
  Write-Host "`nList-only mode: no backup or file changes performed." -ForegroundColor Yellow
  return
}

# Backup (mirror) before any changes
$Stamp       = Get-Date -Format "yyyyMMdd_HHmmss"
$BackupRoot  = Join-Path $ProjectRoot ("Backups\EditorSweep\{0}" -f $Stamp)
$MirrorRoot  = Join-Path $BackupRoot "full_mirror"
$RemovedRoot = Join-Path $BackupRoot "removed"
New-Item -ItemType Directory -Path $BackupRoot  -Force | Out-Null
New-Item -ItemType Directory -Path $MirrorRoot  -Force | Out-Null
New-Item -ItemType Directory -Path $RemovedRoot -Force | Out-Null

Write-Host "`nBacking up ALL matched Editor .cs files to: $MirrorRoot" -ForegroundColor DarkCyan
foreach($f in $EditorFiles){
  $rel = $f.FullName.Substring($ProjectRoot.Length).TrimStart('\','/')
  $dst = Join-Path $MirrorRoot $rel
  $dstDir = Split-Path $dst -Parent
  if(-not (Test-Path $dstDir)){ New-Item -ItemType Directory -Path $dstDir -Force | Out-Null }
  Copy-Item -LiteralPath $f.FullName -Destination $dst -Force
}
Write-Host (" - Backed up {0} files." -f $Total)

if(-not $Apply){
  Write-Host "`nDRY RUN complete. Pass -Apply to move non-whitelisted files to '$RemovedRoot' (or -HardDelete to delete)." -ForegroundColor Yellow
  Write-Host "Backup mirror is ready at: $MirrorRoot"
  return
}

# Apply changes: move (default) or delete (if -HardDelete)
Write-Host ""
if($HardDelete){ Write-Host "APPLY: permanently DELETING non-whitelisted files..." -ForegroundColor Red }
else { Write-Host "APPLY: MOVING non-whitelisted files to: $RemovedRoot" -ForegroundColor Yellow }

$changed = 0
foreach($r in $removeSet){
  $src = $r.FullName
  if($HardDelete){
    if($PSCmdlet.ShouldProcess($src,"Delete")){
      Remove-Item -LiteralPath $src -Force
      $changed++
    }
  } else {
    $rel   = $src.Substring($ProjectRoot.Length).TrimStart('\','/')
    $dest  = Join-Path $RemovedRoot $rel
    $dDir  = Split-Path $dest -Parent
    if(-not (Test-Path $dDir)){ New-Item -ItemType Directory -Path $dDir -Force | Out-Null }
    if($PSCmdlet.ShouldProcess($src,"Move -> $dest")){
      Move-Item -LiteralPath $src -Destination $dest -Force
      $changed++
    }
  }
}

Write-Host ""
Write-Host ("Done. Files affected: {0}" -f $changed) -ForegroundColor Green
Write-Host ("Backup mirror:  {0}" -f $MirrorRoot)
if(-not $HardDelete){ Write-Host ("Moved (removed): {0}" -f $RemovedRoot) } else { Write-Host "Note: -HardDelete was used; nothing in 'removed'." }
