# pre-commit.ps1
# Blocks commits if UnityEngine/UnityEditor are referenced in pure-domain code.
# Runs on Windows via .git/hooks/pre-commit.cmd wrapper.

param(
  [string[]]$PureDomainDirs = @(
    'PureDomain',
    'Packages\com.aq.sharedkernel'
  ),
  [string]$RepoRoot = (Resolve-Path '..\..').Path  # .git/hooks -> repo root
)

Write-Host 'Running pre-commit guard for pure-domain code...'

# Gather staged .cs files to limit scope to whatâ€™s being committed
# Fallback: scan all, if staged list fails.
$staged = git diff --cached --name-only --diff-filter=ACMR 2>
if (-not $LASTEXITCODE -eq 0 -or [string]::IsNullOrWhiteSpace($staged)) {
  Write-Host 'Could not get staged files. Scanning all .cs under pure domains instead.'
  $staged = @()
}

$violations = @()

foreach ($dir in $PureDomainDirs) {
  $full = Join-Path $RepoRoot $dir
  if (-not (Test-Path $full)) { continue }

  # Target files: staged .cs in this dir (or all .cs if staged resolution failed)
  if ($staged.Count -gt 0) {
    $targets = $staged | Where-Object {
      $_.ToLower().EndsWith('.cs') -and
      (Resolve-Path (Join-Path $RepoRoot $_) -ErrorAction SilentlyContinue) -ne $null -and
      (Split-Path (Resolve-Path (Join-Path $RepoRoot $_)).Path -Parent).StartsWith($full, [System.StringComparison]::OrdinalIgnoreCase)
    } | ForEach-Object { Join-Path $RepoRoot $_ }
  } else {
    $targets = Get-ChildItem -Path $full -Recurse -Include *.cs -File | Select-Object -ExpandProperty FullName
  }

  foreach ($file in $targets) {
    $text = Get-Content -Path $file -Raw -Encoding UTF8
    if ($text -match '\bUnityEngine\b' -or $text -match '\bUnityEditor\b') {
      $rel = $file.Substring($RepoRoot.Length).TrimStart('\','/')
      $violations += $rel
    }
  }
}

if ($violations.Count -gt 0) {
  Write-Error 'Pre-commit guard: Unity types referenced in pure-domain code.'
  Write-Host  'The following files violate the rule (no UnityEngine/UnityEditor allowed):'
  $violations | ForEach-Object { Write-Host "  - $_" }
  Write-Host  ''
  Write-Host  'Fix: remove Unity references from domain layers. Use interfaces/adapters in Unity layer instead.'
  exit 1
}

Write-Host 'Pre-commit guard passed.'
exit 0

