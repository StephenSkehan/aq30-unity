#Requires -Version 7
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$targetPath = "Assets/App/Leads/LeadCardView.cs"
if (!(Test-Path $targetPath)) { throw "File not found: $targetPath" }

# Backup first
$stamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupPath = "$targetPath.bak_$stamp"
Copy-Item $targetPath $backupPath -Force
Write-Host "Backup created: $backupPath" -ForegroundColor Cyan

# Load file
$src = Get-Content $targetPath -Raw

# 1) Rename the class declaration
$src2 = $src -replace '(\bclass\s+)LeadRequirementItem(\b)', '${1}LeadRequirementItemView${2}'

# 2) Rename references INSIDE THIS FILE ONLY
#    (types, fields, generics, "new" expressions, GetComponent<>, arrays)
$patterns = @(
    '(\bnew\s+)LeadRequirementItem(\s*\()',
    '(\bList<\s*)LeadRequirementItem(\s*>)',
    '(\bIEnumerable<\s*)LeadRequirementItem(\s*>)',
    '(\bLeadRequirementItem\s*\[)',                     # arrays
    '(\bGetComponent<\s*)LeadRequirementItem(\s*>)',
    '(\bGetComponentsInChildren<\s*)LeadRequirementItem(\s*>)',
    '(\bTryGetComponent<\s*)LeadRequirementItem(\s*>)',
    '(\bLeadRequirementItem\b)'                         # plain type refs as last resort
)
foreach ($p in $patterns) {
    $src2 = [regex]::Replace($src2, $p, { param($m) $m.Groups[1].Value + "LeadRequirementItemView" + $m.Groups[2].Value })
}

if ($src2 -eq $src) {
    Write-Host "No changes were needed; it may already be renamed." -ForegroundColor Yellow
} else {
    Set-Content -LiteralPath $targetPath -Value $src2 -NoNewline
    Write-Host "Renamed inner class to LeadRequirementItemView and updated references." -ForegroundColor Green
}

Write-Host "Done. Return to Unity and let it recompile." -ForegroundColor Green
