[CmdletBinding()]
param(
  [string]$RepoRoot = (Get-Location).Path,
  [string]$ToolsRel = "Tools",
  [string]$IncludeGlob = "*.ps1",

  # Interactive picker (default: on)
  [switch]$Interactive,

  # Auto quarantine by explicit names (comma-separated or array)
  [string[]]$AutoQuarantineByName,

  # Auto quarantine unused files older than N days (no references + no recent git commits)
  [int]$AutoQuarantineUnusedOlderThanDays = 0,

  # Dry run (show what would happen, don't move)
  [switch]$DryRun
)

$ErrorActionPreference = 'Stop'

# --- preflight ----------------------------------------------------------------
try { git --version | Out-Null } catch { throw "git not found on PATH" }

$repo = Resolve-Path -LiteralPath $RepoRoot
$tools = Join-Path $repo $ToolsRel
if (-not (Test-Path $tools)) { throw "Tools directory not found: $tools" }

# --- gather files --------------------------------------------------------------
$files = Get-ChildItem -LiteralPath $tools -Filter $IncludeGlob -File -ErrorAction SilentlyContinue

# repo-wide grep exclusions
$excludeDirs = @(
  "_audit/","Backups/","Library/","Temp/",".git/","Z._quarantine_","Build/","Builds/","Logs/","UserSettings/"
)

function Get-GitLastCommit([string]$pathRel) {
  $out = git -C $repo log -1 --format=%ci -- "$pathRel" 2>$null
  if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($out)) { return $null }
  try { return [datetime]::Parse($out.Trim()) } catch { return $null }
}

function Get-References([string]$name) {
  # search for literal filename mentions; ignore noisy dirs
  $args = @('grep','-n','-I','--', $name)
  $results = & git -C $repo @args 2>$null
  if ([string]::IsNullOrWhiteSpace($results)) { return @() }
  $lines = $results -split "`n"
  # filter out excluded dirs
  $keep = @()
  foreach ($line in $lines) {
    $colon = $line.IndexOf(':')
    if ($colon -gt 0) {
      $path = $line.Substring(0,$colon)
      $skip = $false
      foreach ($ex in $excludeDirs) { if ($path -like ("*{0}*" -f $ex)) { $skip = $true; break } }
      if (-not $skip) { $keep += $line }
    }
  }
  return $keep
}

# Build report rows
$rows = foreach ($f in $files) {
  $rel = $f.FullName.Substring($repo.Path.Length).TrimStart('\') -replace '\\','/'
  $relForGit = $rel
  $gitDate = Get-GitLastCommit $relForGit
  $refs = Get-References $f.Name
  [pscustomobject]@{
    Name            = $f.Name
    FullPath        = $f.FullName
    RepoRel         = $rel
    SizeKB          = [Math]::Round($f.Length/1KB, 1)
    LastWrite       = $f.LastWriteTime
    GitLastTouched  = $gitDate
    RefCount        = $refs.Count
  }
}

# Print table
$rows | Sort-Object Name | Format-Table Name,SizeKB,LastWrite,GitLastTouched,RefCount -AutoSize

# --- quarantine helpers --------------------------------------------------------
function New-QuarantineFolder {
  param([string]$root)
  $stamp = Get-Date -Format "yyyyMMdd_HHmmss"
  $q = Join-Path $root ("Z._quarantine_{0}" -f $stamp)
  if (-not (Test-Path $q)) { New-Item -ItemType Directory -Path $q | Out-Null }
  return $q
}

function Quarantine-Files {
  param([string[]]$paths, [switch]$DryRun)
  if (-not $paths -or $paths.Count -eq 0) { return $null }
  $q = New-QuarantineFolder -root $repo
  foreach ($p in $paths) {
    $dest = Join-Path $q (Split-Path $p -Leaf)
    if ($DryRun) {
      Write-Host "[dry-run] Move '$p' -> '$dest'"
    } else {
      try {
        Move-Item -LiteralPath $p -Destination $q -Force
        Write-Host "Moved: $p -> $q"
      } catch {
        Write-Warning "Failed to move $p : $($_.Exception.Message)"
      }
    }
  }
  return $q
}

# --- auto quarantine by explicit name -----------------------------------------
$autoList = @()
if ($AutoQuarantineByName -and $AutoQuarantineByName.Count -gt 0) {
  foreach ($n in $AutoQuarantineByName) {
    $match = $rows | Where-Object { $_.Name -ieq $n }
    if ($match) { $autoList += $match.FullPath }
    else { Write-Warning "Not found in Tools: $n" }
  }
}

# --- auto quarantine unused+old -----------------------------------------------
if ($AutoQuarantineUnusedOlderThanDays -gt 0) {
  $cutoff = (Get-Date).AddDays(-$AutoQuarantineUnusedOlderThanDays)
  $cands = $rows | Where-Object {
    ( $_.RefCount -eq 0 ) -and
    ( ($_.GitLastTouched -and $_.GitLastTouched -lt $cutoff) -or (-not $_.GitLastTouched -and $_.LastWrite -lt $cutoff) )
  }
  $autoList += ($cands | Select-Object -ExpandProperty FullPath)
}

$autoList = $autoList | Sort-Object -Unique
if ($autoList.Count -gt 0) {
  Write-Host "`nAuto-selected for quarantine:" -ForegroundColor Yellow
  $autoList | ForEach-Object { Write-Host "  - $_" }
  $qdir = Quarantine-Files -paths $autoList -DryRun:$DryRun
  if ($qdir -and -not $DryRun) { Write-Host "Quarantined to: $qdir" -ForegroundColor Green }
}

# --- interactive selection -----------------------------------------------------
if ($Interactive -and -not $DryRun) {
  Write-Host "`nInteractive mode: type the numbers to quarantine (comma-separated), or press Enter to skip." -ForegroundColor Cyan
  # show numbered list
  $indexed = $rows | Sort-Object Name | Select-Object @{n='Idx';e={[int]$script:__i; $script:__i+=1}}, Name, RefCount, GitLastTouched, LastWrite, SizeKB, FullPath
  $script:__i = 1
  $indexed = $rows | Sort-Object Name | ForEach-Object -Begin { $i=1 } -Process {
    [pscustomobject]@{ Idx=$i; Name=$_.Name; RefCount=$_.RefCount; GitLastTouched=$_.GitLastTouched; LastWrite=$_.LastWrite; SizeKB=$_.SizeKB; FullPath=$_.FullPath }
    $i++
  }
  $indexed | Format-Table Idx,Name,RefCount,GitLastTouched,LastWrite,SizeKB -AutoSize

  $sel = Read-Host "Select indices to quarantine (e.g. 2,5) or just Enter"
  if ($sel) {
    $nums = $sel -split '[^\d]+' | Where-Object { $_ -match '^\d+$' } | ForEach-Object { [int]$_ }
    $chosen = @()
    foreach ($n in $nums) {
      $hit = $indexed | Where-Object { $_.Idx -eq $n }
      if ($hit) { $chosen += $hit.FullPath } else { Write-Warning "No item at index $n" }
    }
    if ($chosen.Count -gt 0) {
      $qdir = Quarantine-Files -paths $chosen
      if ($qdir) { Write-Host "Quarantined to: $qdir" -ForegroundColor Green }
    } else {
      Write-Host "Nothing selected."
    }
  } else {
    Write-Host "Skipped interactive quarantine."
  }
}

Write-Host "`nDone." -ForegroundColor Green
