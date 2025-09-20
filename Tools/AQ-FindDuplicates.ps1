#Requires -Version 7
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$assets = Resolve-Path -LiteralPath (Join-Path (Get-Location) "Assets")
$pattern = '^\s*(public|internal|protected|private)?\s*(sealed\s+|abstract\s+)?(partial\s+)?class\s+LeadRequirementItem(\s|:)'
$hits = Get-ChildItem $assets -Recurse -Filter *.cs |
  Select-String -Pattern $pattern -SimpleMatch:$false

if (-not $hits) { Write-Host "No duplicate class declarations found." ; exit 0 }

Write-Host "LeadRequirementItem declarations:" -ForegroundColor Cyan
$hits | ForEach-Object {
  "{0}:{1}  ->  {2}" -f $_.Path, $_.LineNumber, $_.Line.Trim()
}

$grouped = $hits | Group-Object Path
Write-Host "`nSummary:" -ForegroundColor Cyan
$grouped | ForEach-Object {
  "{0}  ({1} hit{2})" -f $_.Name, $_.Count, $(if($_.Count -gt 1){"s"}else{""})
}

if ($grouped.Count -gt 1) {
  Write-Host "`n⚠️  Multiple files declare 'LeadRequirementItem'. Keep the one in Assets/App/Leads/LeadRequirementItem.cs and rename the others." -ForegroundColor Yellow
}
