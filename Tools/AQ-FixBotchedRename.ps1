#Requires -Version 7
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$path = "Assets/App/Leads/LeadCardView.cs"
if (!(Test-Path $path)) { throw "Not found: $path" }

# Backup first
$stamp = Get-Date -Format "yyyyMMdd_HHmmss"
Copy-Item $path "$path.bak_$stamp" -Force
Write-Host "Backup: $path.bak_$stamp" -ForegroundColor Cyan

# Load
$src = Get-Content $path -Raw

# 1) Fix double-replaced token
$src = $src -replace 'LeadRequirementItemLeadRequirementItemView', 'LeadRequirementItemView'

# 2) Ensure we target ONLY the inner helper type inside this file.
#    Keep the public MonoBehaviour 'AQ.App.Leads.LeadRequirementItem' defined in its own file.
#    If the class declaration here is still the old name, rename it.
$src = [regex]::Replace($src, '(\bclass\s+)LeadRequirementItem(\b)', '${1}LeadRequirementItemView${2}')

# 3) Update *this file's* references to the inner type (lists, news, generics).
$repls = @(
  '(\bnew\s+)LeadRequirementItem(\s*\()',
  '(\bList<\s*)LeadRequirementItem(\s*>)',
  '(\bIEnumerable<\s*)LeadRequirementItem(\s*>)',
  '(\bLeadRequirementItem\s*\[)',
  '(\bGetComponent<\s*)LeadRequirementItem(\s*>)',
  '(\bGetComponentsInChildren<\s*)LeadRequirementItem(\s*>)',
  '(\bTryGetComponent<\s*)LeadRequirementItem(\s*>)',
  '(\bLeadRequirementItem\b)'  # last resort inside this file only
)
foreach ($p in $repls) {
  $src = [regex]::Replace($src, $p, { param($m) $m.Groups[1].Value + "LeadRequirementItemView" + $m.Groups[2].Value })
}

Set-Content -LiteralPath $path -Value $src -NoNewline
Write-Host "Patched bad tokens in $path" -ForegroundColor Green
