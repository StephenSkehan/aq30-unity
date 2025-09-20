# Tools\Hooks\PreCommit-LFSGuard.ps1
# Purpose: Block commits that stage files > $MaxSizeMB unless the path is LFS-tracked.
# Exit 1 = block commit with a clear message.

param(
  [int]$MaxSizeMB = 10
)

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
foreach($line in ($attrs -split "`n")){
  if($line -match '^\s*(\S+)\s+filter=lfs'){ $globs.Add($Matches[1]) }
}
foreach($line in (S $lfsList -split "`n")){
  if($line -match '^\s*([\S]+)\s+\(.*\)'){ $globs.Add($Matches[1]) }
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
  Write-Host "`n❌ Commit blocked: large files not tracked by LFS (> $MaxSizeMB MB):"
  $violations | ForEach-Object { Write-Host ("  - {0}  ({1} MB)" -f $_.Path,$_.SizeMB) }
  Write-Host "`nFix:"
  Write-Host "  1) git lfs track \"<pattern>\"   (e.g., *.unity, *.prefab, *.png, *.wav, …)"
  Write-Host "  2) git add .gitattributes"
  Write-Host "  3) git add <files> && git commit"
  exit 1
}

exit 0
