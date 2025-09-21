# Tools\AQ-RepairGuard.ps1
# Fixes the PreCommit-LFSGuard.ps1 quoting issue and ensures a proper 'pre-commit' hook is installed.

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Ensure-Dir($p){
  if(!(Test-Path $p)){ New-Item -ItemType Directory -Force -Path $p | Out-Null }
}

# Locate repo root
$root = (& git.exe rev-parse --show-toplevel) 2>&1
if($LASTEXITCODE -ne 0){ throw "Not inside a git repository: $root" }
$root = $root.Trim()

# Make sure Git uses .git/hooks (unset custom hooksPath if present)
try {
  $hooksPath = (& git.exe config --local --get core.hooksPath) 2>$null
  if($hooksPath){ & git.exe config --local --unset core.hooksPath | Out-Null }
} catch {}

# Paths
$guardRel  = "Tools\Hooks\PreCommit-LFSGuard.ps1"
$guardAbs  = Join-Path $root $guardRel
$hooksDir  = Join-Path $root ".git\hooks"
$hookSh    = Join-Path $hooksDir "pre-commit"
$hookCmd   = Join-Path $hooksDir "pre-commit.cmd"

Ensure-Dir (Split-Path $guardAbs -Parent)
Ensure-Dir $hooksDir

# Correct, self-contained guard content with safe single-quoted strings
$guardContent = @'
# Tools\Hooks\PreCommit-LFSGuard.ps1
# Blocks commits that stage files > 10 MB unless covered by LFS rules.

param([int]$MaxSizeMB = 10)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function S($x){ return (($x | Out-String) -replace "`r","").Trim() }

# Gather staged files (Added/Modified only)
$staged = (& git.exe diff --cached --name-only --diff-filter=AM) 2>&1
$files  = S $staged -split "`n" | Where-Object { $_ -and (Test-Path $_) }
if(-not $files){ exit 0 }

# Build an LFS glob list from .gitattributes and 'git lfs track'
$attrs   = Test-Path .gitattributes ? (Get-Content .gitattributes -Raw) : ""
$lfsList = (& git.exe lfs track) 2>&1

$globs = New-Object System.Collections.Generic.List[string]

# From .gitattributes
foreach($line in ($attrs -split "`n")){
  if($line -match '^\s*(\S+)\s+filter=lfs'){ $globs.Add($Matches[1]) }
}

# From 'git lfs track' output (lines look like: "    *.png (.gitattributes)")
foreach($line in (S $lfsList -split "`n")){
  if($line -match '^\s*([^\s]+)\s+\(.*\)'){ $globs.Add($Matches[1]) }
}

$globs = $globs | Select-Object -Unique

$maxBytes = $MaxSizeMB * 1MB
$violations = @()

foreach($f in $files){
  try{
    $len = (Get-Item $f).Length
    if($len -gt $maxBytes){
      # Check if path matches any LFS glob
      $isLfs = $false
      foreach($g in $globs){
        if($f -like $g){ $isLfs = $true; break }
      }
      if(-not $isLfs){
        $violations += [pscustomobject]@{Path=$f; SizeMB=[math]::Round($len/1MB,2)}
      }
    }
  } catch {}
}

if($violations.Count -gt 0){
  Write-Host ''
  Write-Host ('❌ Commit blocked: large files not tracked by LFS (> {0} MB):' -f $MaxSizeMB)
  $violations | ForEach-Object { Write-Host ('  - {0}  ({1} MB)' -f $_.Path,$_.SizeMB) }
  Write-Host ''
  Write-Host 'Fix:'
  Write-Host '  1) git lfs track "<pattern>"   (e.g., *.unity, *.prefab, *.png, *.wav, *.psd, *.ttf)'
  Write-Host '  2) git add .gitattributes'
  Write-Host '  3) git add <files> && git commit'
  exit 1
}

exit 0
'@

$guardContent | Out-File -FilePath $guardAbs -Encoding UTF8 -Force

# Write a real POSIX-style pre-commit shim (no extension) that calls PowerShell
$sh = @'
#!/bin/sh
# Git pre-commit hook → PowerShell guard
ROOT="$(cd "$(dirname "$0")/../.." && pwd -P)"
if command -v pwsh >/dev/null 2>&1; then
  pwsh -NoProfile -ExecutionPolicy Bypass -File "$ROOT/Tools/Hooks/PreCommit-LFSGuard.ps1"
else
  powershell -NoProfile -ExecutionPolicy Bypass -File "$ROOT/Tools/Hooks/PreCommit-LFSGuard.ps1"
fi
exit $?
'@
# Force LF endings so /bin/sh is happy
$bytes = [Text.Encoding]::UTF8.GetBytes(($sh -replace "`r`n","`n" -replace "`r","`n"))
[IO.File]::WriteAllBytes($hookSh, $bytes)

# Windows fallback .cmd (nice-to-have)
$cmdShim = @'
@echo off
set ROOT=%~dp0..\..
pwsh -NoProfile -ExecutionPolicy Bypass -File "%ROOT%\Tools\Hooks\PreCommit-LFSGuard.ps1"
set ERR=%ERRORLEVEL%
exit /B %ERR%
'@
$cmdShim | Out-File -FilePath $hookCmd -Encoding ASCII -Force

Write-Host "Repaired guard and installed hooks:"
Write-Host "  $guardRel"
Write-Host "  $hookSh"
Write-Host "  $hookCmd"
