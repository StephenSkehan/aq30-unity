#Requires -Version 7
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root   = (Get-Location).Path
$assets = Join-Path $root 'Assets'
$outDir = Join-Path $root "_audit\merge"
$stamp  = Get-Date -Format 'yyyyMMdd_HHmmss'
New-Item -Force -ItemType Directory $outDir | Out-Null

$codeOut   = Join-Path $outDir "merge_code_index_$stamp.txt"
$assetOut  = Join-Path $outDir "merge_assets_index_$stamp.txt"
$findings  = Join-Path $outDir "merge_findings_$stamp.txt"

# --- 1) Code index (C#)
$codeFiles = Get-ChildItem $assets -Recurse -Include *.cs -File | Where-Object { $_.FullName -notmatch '\\Library\\' }

$codeTerms = @(
  'class\s+Merge', 'interface\s+IMerge', 'MergeBoard', 'MergeGrid', 'MergeCell', 'MergeSlot',
  'MergeItem', 'TryMerge', 'CanMerge', 'OnMerge', 'BoardPresenter', 'MergePresenter',
  'Evidence', 'LeadsBar', 'ProceedRequested'
)

"== CODE MATCHES ==" | Set-Content $codeOut -Encoding UTF8
foreach($f in $codeFiles){
  $hits = Select-String -Path $f.FullName -Pattern $codeTerms -SimpleMatch:$false -CaseSensitive:$false
  if($hits){
    "FILE: $($f.FullName)" | Add-Content $codeOut
    $hits | ForEach-Object { "  L$($_.LineNumber): $($_.Line.Trim())" | Add-Content $codeOut }
    "" | Add-Content $codeOut
  }
}

# --- 2) Asset index (prefabs/scenes/SOs) scanning YAML text
$assetFiles = Get-ChildItem $assets -Recurse -Include *.prefab,*.unity,*.asset -File | Where-Object { $_.FullName -notmatch '\\Library\\' }
$assetTerms = @('Merge', 'Board', 'Cell', 'Slot', 'Tile', 'TryMerge', 'IMerge')

"== ASSET TEXT MATCHES ==" | Set-Content $assetOut -Encoding UTF8
foreach($f in $assetFiles){
  $hit = $false
  $raw = Get-Content $f.FullName -Raw -Encoding UTF8
  foreach($t in $assetTerms){
    if($raw -match [regex]::Escape($t)){
      if(-not $hit){
        "FILE: $($f.FullName)" | Add-Content $assetOut
        $hit = $true
      }
    }
  }
  if($hit){ "" | Add-Content $assetOut }
}

# --- 3) Findings summary
$codeCount  = (Select-String -Path $codeFiles.FullName -Pattern $codeTerms -CaseSensitive:$false | Measure-Object).Count
$assetCount = (Get-Content $assetOut -TotalCount 999999 | Where-Object { $_ -like 'FILE:*' } | Measure-Object).Count

$rows = @()
$rows += "AQ Merge Audit — $stamp"
$rows += "Assets: $assets"
$rows += ""
$rows += "Code hits:   $codeCount   (see $(Resolve-Path $codeOut))"
$rows += "Asset files mentioning Merge-ish terms: $assetCount   (see $(Resolve-Path $assetOut))"
$rows += ""
$rows += "Heuristics:"
$rows += "- If you see classes like MergeBoardPresenter / MergeItem / MergeCell, we re-use them."
$rows += "- If only scattered references exist (no presenter), we can layer in a thin presenter without churn."
$rows += "- If nothing shows up, we proceed with the minimal presenter scaffold."
$rows | Set-Content $findings -Encoding UTF8

Write-Host "PASS: Merge audit complete."
Write-Host "  Code index:   $codeOut"
Write-Host "  Asset index:  $assetOut"
Write-Host "  Findings:     $findings"
