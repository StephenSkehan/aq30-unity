<# 
AQ-Editor-Prune.ps1
Scans Assets\Editor (and nested Editor dirs), classifies .cs files into KEEP vs CANDIDATE, 
writes reports, and produces an optional prune action that MOVES candidates to Backups\Pruned.

Usage:
  pwsh -NoProfile -ExecutionPolicy Bypass .\Tools\AQ-Editor-Prune.ps1
Options:
  -Root "<path>"      : Project root (default: ".")
  -Apply              : If present, moves candidate files into Backups\Pruned\<timestamp>\
  -WhatIf             : Simulate actions (default PowerShell WhatIf works on Move-Item)
  -Verbose            : Detailed console output
#>

[CmdletBinding(SupportsShouldProcess=$true)]
param(
  [string]$Root = ".",
  [switch]$Apply
)

$ErrorActionPreference = "Stop"

# --- Helpers ---------------------------------------------------------------

function New-ReportDir {
  param([string]$Base)
  $dir = Join-Path $Base "Tools\Reports"
  if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir | Out-Null }
  return $dir
}

function Get-ProjectPaths {
  param([string]$root)
  $full = Resolve-Path $root
  return $full.Path
}

function Get-EditorRoots {
  param([string]$projectRoot)

  # Find any folder named "Editor" under Assets (including nested)
  $assets = Join-Path $projectRoot "Assets"
  if (-not (Test-Path $assets)) { throw "Assets folder not found at: $assets" }

  $editorDirs = Get-ChildItem -Path $assets -Recurse -Directory -ErrorAction SilentlyContinue |
                Where-Object { $_.Name -ieq "Editor" } |
                Select-Object -ExpandProperty FullName

  # Always include top-level Assets\Editor if present
  $topEditor = Join-Path $assets "Editor"
  if (Test-Path $topEditor) {
    if (-not ($editorDirs -contains $topEditor)) { $editorDirs += $topEditor }
  }

  # Unique, sorted
  $editorDirs = $editorDirs | Sort-Object -Unique
  return ,$editorDirs  # ensure array
}

function Read-FileText {
  param([string]$path)
  try {
    return [System.IO.File]::ReadAllText($path)
  } catch {
    return ""
  }
}

function Extract-ClassNames {
  param([string]$text)
  # Very simple class name extractor: looks for "class Name"
  $names = @()
  foreach($m in [regex]::Matches($text, 'class\s+([A-Za-z_][A-Za-z0-9_]*)')) {
    $names += $m.Groups[1].Value
  }
  return $names | Sort-Object -Unique
}

function Is-KeepByHeuristics {
  param(
    [string]$text,
    [string]$fileName
  )
  # Strong keep signals
  $keepSignals = @(
    '\[MenuItem\s*\(',
    ':\s*AssetPostprocessor',
    ':\s*EditorWindow',
    ':\s*ScriptableWizard',
    ':\s*Editor\b',                 # CustomEditor-derived editors
    '\[CustomEditor\s*\(',
    '\[InitializeOnLoad(Attribute)?\]',
    'DidReloadScripts',
    'OnPostprocessTexture',
    'OnPreprocessTexture',
    'OnPostprocessAllAssets',
    'OnPreprocessAsset',
    'AssetPostprocessor'
  )

  foreach($pat in $keepSignals){
    if([regex]::IsMatch($text, $pat)) { return $true }
  }

  # Critical filenames (current project canon)
  $canonical = @(
    # Art / TopBar / Audits
    'TopBarNukeRebuild.cs','TopBarFinalize.cs','AuditCanvasBoard.cs','UIImportGuard.cs',
    # Leads
    'LeadsBarWire.cs','LeadsBarCleanup.cs','LeadCard_FloatActors.cs',
    # Requirements
    'RequirementsHUDWire.cs',
    # Diagnostics we still lean on
    'FindDotItem.cs'
  )
  if ($canonical -contains (Split-Path $fileName -Leaf)) { return $true }

  return $false
}

function Is-LegacyHint {
  param([string]$path,[string]$text)
  $name = Split-Path $path -Leaf

  $nameHints = @('legacy','old','proto','prototype','backup','bak','temp','scratch','test','experi','wip','_old','_bak','.old')
  foreach($h in $nameHints){
    if ($name.ToLower().Contains($h)) { return $true }
  }

  $textHints = @('ONE-OFF','ONE OFF','OBSOLETE','DEPRECATED','TEMP','SCRATCH','EXPERIMENT','TEST-ONLY','throwaway','remove me','TODO remove')
  foreach($h in $textHints){
    if ($text -match [regex]::Escape($h)) { return $true }
  }

  return $false
}

function Build-SymbolIndex {
  param([string]$projectRoot)

  $allCs = Get-ChildItem -Path $projectRoot -Recurse -Filter *.cs -ErrorAction SilentlyContinue |
           Where-Object { $_.FullName -notmatch '\\Library\\' -and $_.FullName -notmatch '\\Logs\\' -and $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\Temp\\' }

  $index = [System.Collections.Generic.Dictionary[string, System.Collections.Generic.List[string]]]::new()
  foreach($f in $allCs){
    $t = Read-FileText $f.FullName
    $names = Extract-ClassNames $t
    foreach($n in $names){
      if(-not $index.ContainsKey($n)){ $index[$n] = [System.Collections.Generic.List[string]]::new() }
      $null = $index[$n].Add($f.FullName)
    }
  }
  return $index
}

function Is-Referenced-Somewhere {
  param(
    [string[]]$classNames,
    [System.Collections.Generic.Dictionary[string, System.Collections.Generic.List[string]]]$symbolIndex
  )
  foreach($n in $classNames){
    if($symbolIndex.ContainsKey($n)){ return $true }
  }
  return $false
}

# --- Main ------------------------------------------------------------------

$projectRoot = Get-ProjectPaths -root $Root
$reportDir   = New-ReportDir -Base $projectRoot
$stamp       = Get-Date -Format "yyyyMMdd_HHmmss"
$pruneDir    = Join-Path $projectRoot ("Backups\Pruned\{0}" -f $stamp)
$deletePlan  = Join-Path $reportDir ("editor_prune_plan_{0}.ps1" -f $stamp)
$listReport  = Join-Path $reportDir ("editor_prune_list_{0}.txt" -f $stamp)
$keepReport  = Join-Path $reportDir ("editor_keep_list_{0}.txt" -f $stamp)
$summaryPath = Join-Path $reportDir ("editor_prune_summary_{0}.txt" -f $stamp)

Write-Host "🔎 Scanning Editor scripts under: $projectRoot" -ForegroundColor Cyan

$editorRoots = Get-EditorRoots -projectRoot $projectRoot
if(-not $editorRoots -or $editorRoots.Count -eq 0){
  Write-Warning "No 'Editor' folders found under Assets. Nothing to do."
  return
}

# Build a crude symbol index of the whole project for reference checks
Write-Host "📚 Building symbol index (project-wide class name map)..." -ForegroundColor DarkCyan
$symbolIndex = Build-SymbolIndex -projectRoot $projectRoot

# Collect all .cs files under the editor roots
$editorFiles = @()
foreach($dir in $editorRoots){
  $editorFiles += Get-ChildItem -Path $dir -Recurse -Filter *.cs -ErrorAction SilentlyContinue
}
$editorFiles = $editorFiles | Sort-Object FullName
$total = $editorFiles.Count

if($total -eq 0){
  Write-Warning "No .cs files found under Editor folders. Nothing to do."
  return
}

# Analysis structures
$candidates = New-Object System.Collections.Generic.List[System.IO.FileInfo]
$keepers    = New-Object System.Collections.Generic.List[System.IO.FileInfo]

# Progress loop
$idx = 0
foreach($file in $editorFiles){
  $idx++
  $pct = [int](($idx / [double]$total) * 100)
  Write-Progress -Activity "Classifying Editor files..." -Status ("{0}/{1} {2}" -f $idx,$total,(Split-Path $file.FullName -Leaf)) -PercentComplete $pct

  $text = Read-FileText $file.FullName

  $keep = Is-KeepByHeuristics -text $text -fileName $file.FullName
  if(-not $keep){
    $classes = Extract-ClassNames $text
    $refd    = Is-Referenced-Somewhere -classNames $classes -symbolIndex $symbolIndex
    $legacy  = Is-LegacyHint -path $file.FullName -text $text

    # Candidate if legacy hint OR no references to its classes
    if($legacy -or (-not $refd)){
      [void]$candidates.Add($file)
      continue
    }
  }

  [void]$keepers.Add($file)
}

# Write reports
$candidates = $candidates | Sort-Object FullName
$keepers    = $keepers    | Sort-Object FullName

$candCount = $candidates.Count
$keepCount = $keepers.Count

"--- EDITOR PRUNE SUMMARY ($stamp) ---" | Set-Content -Encoding UTF8 $summaryPath
("Project: {0}" -f $projectRoot)            | Add-Content -Encoding UTF8 $summaryPath
("Editor roots: {0}" -f ($editorRoots -join '; ')) | Add-Content -Encoding UTF8 $summaryPath
("Total Editor .cs: {0}" -f $total)         | Add-Content -Encoding UTF8 $summaryPath
("KEEP: {0}" -f $keepCount)                 | Add-Content -Encoding UTF8 $summaryPath
("CANDIDATES: {0}" -f $candCount)           | Add-Content -Encoding UTF8 $summaryPath

$keepers  | ForEach-Object { $_.FullName }   | Set-Content -Encoding UTF8 $keepReport
$candidates | ForEach-Object { $_.FullName } | Set-Content -Encoding UTF8 $listReport

# Generate action plan (move to Backups\Pruned)
@"
# Auto-generated by AQ-Editor-Prune.ps1 at $stamp
# This plan MOVES candidate files into: $pruneDir
# Review the list before running. Usage:
#   pwsh -NoProfile -ExecutionPolicy Bypass "$deletePlan" [-WhatIf]
param([switch]$WhatIf)
`$dest = "$pruneDir"
if(-not (Test-Path `$dest)) { New-Item -ItemType Directory -Path `$dest | Out-Null }
"@ | Set-Content -Encoding UTF8 $deletePlan

# Chunk moves to keep command file readable
$candidates | ForEach-Object {
  $src = $_.FullName
  $rel = $src.Substring($projectRoot.Length).TrimStart('\','/')
  $dst = Join-Path $pruneDir $rel
  $dstDir = Split-Path $dst -Parent
  @"
if(-not (Test-Path "$dstDir")) { New-Item -ItemType Directory -Path "$dstDir" | Out-Null }
Write-Host "Moving: $rel" -ForegroundColor Yellow
Move-Item -LiteralPath "$src" -Destination "$dst" -Force @WhatIf
"@ | Add-Content -Encoding UTF8 $deletePlan
}

Write-Host ""
Write-Host "✅ Analysis complete." -ForegroundColor Green
Write-Host ("   KEEP:        {0}" -f $keepCount)
Write-Host ("   CANDIDATES:  {0}" -f $candCount)
Write-Host ""
Write-Host ("📄 Reports:")
Write-Host (" - {0}" -f $summaryPath)
Write-Host (" - {0}" -f $keepReport)
Write-Host (" - {0}" -f $listReport)
Write-Host ("🧰 Delete plan (safe move): {0}" -f $deletePlan) 

if($Apply){
  Write-Host ""
  Write-Host "⚠️  APPLY requested: moving candidates to Backups\Pruned..." -ForegroundColor Red
  if(-not (Test-Path $pruneDir)) { New-Item -ItemType Directory -Path $pruneDir | Out-Null }
  foreach($c in $candidates){
    $src = $c.FullName
    $rel = $src.Substring($projectRoot.Length).TrimStart('\','/')
    $dst = Join-Path $pruneDir $rel
    $dstDir = Split-Path $dst -Parent
    if(-not (Test-Path $dstDir)) { New-Item -ItemType Directory -Path $dstDir | Out-Null }
    if($PSCmdlet.ShouldProcess($src,"Move -> $dst")){
      Move-Item -LiteralPath $src -Destination $dst -Force
      Write-Host ("Moved: {0}" -f $rel) -ForegroundColor Yellow
    }
  }
  Write-Host "✅ Apply complete. You can restore from: $pruneDir" -ForegroundColor Green
}
