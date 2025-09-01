Param()
$ErrorActionPreference = "Stop"

# Anchor to repo root regardless of where we were launched from
$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

$targets = @('Packages/com.aq.sharedkernel','Packages/com.aq.domain')

# Recursively scan .cs files in pure packages
$files = Get-ChildItem -Path $targets -Filter *.cs -Recurse -ErrorAction SilentlyContinue
$found = $files | Select-String -Pattern 'using\s+(UnityEngine|UnityEditor)\b' -AllMatches

if ($found) {
  Write-Host "ERROR: Unity API found in pure packages (SharedKernel/Domain)." -ForegroundColor Red
  $found | ForEach-Object { Write-Host (" -> {0} (line {1})" -f $_.Path, $_.LineNumber) }
  exit 1
}

exit 0
