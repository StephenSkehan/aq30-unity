# Tools\AQ-FixGuardOnce.ps1
# Repairs the LFS pre-commit guard and ensures Git actually runs it.

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Ensure-Dir($p){
  if(!(Test-Path $p)){ New-Item -ItemType Directory -Force -Path $p | Out-Null }
}

# 0) Find repo root
$root = (& git.exe rev-parse --show-toplevel) 2>&1
if($LASTEXITCODE -ne 0){ throw "Not inside a git repository: $root" }
$root = $root.Trim()

# 1) Make sure .git/hooks is the active hooks path
try {
  $hooksPath = (& git.exe config --local --get core.hooksPath) 2>$null
  if($hooksPath){ & git.exe config --local --unset core.hooksPath | Out-Null }
} catch {}

# 2) Write a clean guard that uses 'git check-attr' (no ternary, no trickery)
$guardRel = "Tools\Hooks\PreCommit-LFSGuard.ps1"
$guardAbs = Join-Path $root $guardRel
Ensure-Dir (Split-Path $guardAbs -Parent)

$guard = @'
# Tools\Hooks\PreCommit-LFSGuard.ps1
# Block commits that stage files > 10 MB unless those files are LFS-tracked.
# Exit 1 to block the commit; exit 0 to allow.

param([int]$MaxSizeMB = 10)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function S($x){ return (($x | Out-String) -replace "`r","").Trim() }

# Get staged Added/Modified files
$stagedText = (& git.exe diff --cached --name-only --diff-filter=AM) 2>&1
if ($LASTEXITCODE -ne 0) {
  Write-Host "⚠️  git diff --cached failed:"
  Write-Host $stagedText
  exit 0
}
$files = S $stagedText -split "`n" | Where-Object { $_ -and (Test-Path $_) }
if(-not $files){ exit 0 }

# Build LFS map via check-attr
# Lines: "path: filter: lfs" or "path: filter: unspecified"
$attrOut = $files | & git.exe check-attr filter --stdin 2>&1
$lfsMap = @{}
foreach($line in (S $attrOut -split "`n")){
  if ($line -match '^(.*?):\s+filter:\s+(.+)$'){
    $lfsMap[$Matches[1]] = $Matches[2]
  }
}

$maxBytes   = $MaxSizeMB * 1MB
$violations = New-Object System.Collections.Generic.List[object]

foreach($f in $files){
  try{
    $len = (Get-Item -LiteralPath $f).Length
    if ($len -gt $maxBytes) {
      $isLfs = $false
      if ($lfsMap.ContainsKey($f)) {
        $isLfs = ($lfsMap[$f] -eq 'lfs')
      }
      if(-not $isLfs){
        $violations.Add([pscustomobject]@{Path=$f; SizeMB=[math]::Round($len/1MB,2)})
      }
    }
  } catch {}
}

if($violations.Count -gt 0){
  Write-Host ""
  Write-Host ("❌ Commit blocked: large files not tracked by LFS (> {0} MB):" -f $MaxSizeMB)
  foreach($v in $violations){
    Write-Host ("  - {0}  ({1} MB)" -f $v.Path,$v.SizeMB)
  }
  Write-Host ""
  Write-Host 'Fix:'
  Write-Host '  1) git lfs track "<pattern>"    (e.g., *.unity, *.prefab, *.png, *.wav, *.psd, *.ttf)'
  Write-Host '  2) git add .gitattributes'
  Write-Host '  3) git add <files> && git commit'
  exit 1
}

exit 0
'@
$guard | Out-File -FilePath $guardAbs -Encoding UTF8 -Force

# 3) Install a real 'pre-commit' (no extension) that invokes the guard
$hooksDir = Join-Path $root ".git\hooks"
Ensure-Dir $hooksDir
$preCommit = Join-Path $hooksDir "pre-commit"

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
# Force LF so /bin/sh is happy:
[IO.File]::WriteAllBytes($preCommit, [Text.Encoding]::UTF8.GetBytes(($sh -replace "`r`n","`n" -replace "`r","`n")))

# Also drop a Windows .cmd fallback (optional)
$cmd = @'
@echo off
set ROOT=%~dp0..\..
pwsh -NoProfile -ExecutionPolicy Bypass -File "%ROOT%\Tools\Hooks\PreCommit-LFSGuard.ps1"
set ERR=%ERRORLEVEL%
exit /B %ERR%
'@
$preCommitCmd = Join-Path $hooksDir "pre-commit.cmd"
$cmd | Out-File -FilePath $preCommitCmd -Encoding ASCII -Force

Write-Host "Guard repaired and hooks installed:"
Write-Host "  $guardRel"
Write-Host "  $preCommit"
Write-Host "  $preCommitCmd"
