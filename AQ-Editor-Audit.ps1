#Requires -Version 7.0
<#
AQ-Editor-Audit.progress.ps1
Scans all "Editor" folders (Assets/**/Editor, Packages/**/Editor),
inventories files, audits Editor C# scripts, and writes artifacts under _audit\editor\.
Adds visible progress bars + stage timings. Read-only.

Usage:
  pwsh -NoProfile -ExecutionPolicy Bypass .\Tools\AQ-Editor-Audit.progress.ps1
  pwsh ... .\Tools\AQ-Editor-Audit.progress.ps1 -SkipHash   # faster (no SHA1)
#>

param(
  [switch]$SkipHash,
  [int]$MaxConcatBytesPerFile = 256KB
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$swAll = [System.Diagnostics.Stopwatch]::StartNew()

# ---------------- Config ----------------
$Stamp        = Get-Date -Format "yyyyMMdd_HHmmss"
$RepoRoot     = (Resolve-Path ".").Path
$OutDir       = Join-Path $RepoRoot "_audit\editor"
$ListPath     = Join-Path $OutDir ("editor_file_list_{0}.txt"     -f $Stamp)
$FindingsPath = Join-Path $OutDir ("editor_findings_{0}.txt"      -f $Stamp)
$ConcatPath   = Join-Path $OutDir ("editor_sources_concat_{0}.txt"-f $Stamp)

$IncludeExt = @(".cs",".asmdef",".uxml",".uss",".shader",".ps1",".json",".md")
$SkipDirs = @("\.git","\Library","\Obj","\Bin","_audit","\Logs")

# ---------------- Helpers ----------------
function Test-SkipPath([string]$Path){
  $norm = $Path -replace '\\','/'
  foreach($p in $SkipDirs){ if($norm -match $p){ return $true } }
  return $false
}

function Get-EditorRoots {
  $roots = @()
  if(Test-Path "Assets"){
    $roots += Get-ChildItem -Path "Assets" -Recurse -Directory -Force |
      Where-Object { $_.Name -ieq "Editor" -and -not (Test-SkipPath $_.FullName) }
  }
  if(Test-Path "Packages"){
    $roots += Get-ChildItem -Path "Packages" -Recurse -Directory -Force |
      Where-Object { $_.Name -ieq "Editor" -and -not (Test-SkipPath $_.FullName) }
  }
  return $roots
}

function Read-FileHead([string]$Path, [int]$Bytes){
  try {
    $fs = [System.IO.File]::Open($Path, 'Open', 'Read', 'ReadWrite')
    try {
      $len = [Math]::Min($Bytes, $fs.Length)
      $buf = New-Object byte[] $len
      [void]$fs.Read($buf,0,$len)
      return [System.Text.Encoding]::UTF8.GetString($buf)
    } finally { $fs.Dispose() }
  } catch { return "" }
}

function Scan-CsFile([string]$Path){
  $text = Get-Content -LiteralPath $Path -Raw -ErrorAction SilentlyContinue
  $r = [ordered]@{
    UsingUnityEditor     = $false
    HasUnityEditorGuard  = $false
    HasMenuItems         = @()
    IsAssetPostprocessor = $false
    UsesDeprecatedTMP    = $false
    UsesDeprecatedFind   = $false
    Namespace            = $null
    Classes              = @()
  }
  if($text -match '^\s*using\s+UnityEditor\s*;'m){ $r.UsingUnityEditor = $true }
  if($text -match '^\s*#\s*if\s+UNITY_EDITOR'm){   $r.HasUnityEditorGuard = $true }
  if($text -match 'class\s+[^\{:]+\s*:\s*AssetPostprocessor'){ $r.IsAssetPostprocessor = $true }
  $menuMatches = [regex]::Matches($text, '\[MenuItem\("([^"]+)"[^\]]*\)\)')
  if($menuMatches.Count -gt 0){ $r.HasMenuItems = $menuMatches | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique }
  if($text -match '\b(enableWordWrapping)\b'){ $r.UsesDeprecatedTMP = $true }
  if($text -match 'FindObjectsOfType\s*<'){   $r.UsesDeprecatedFind = $true }
  $ns = [regex]::Match($text,'^\s*namespace\s+([A-Za-z0-9_.]+)', 'Multiline')
  if($ns.Success){ $r.Namespace = $ns.Groups[1].Value }
  $cls = [regex]::Matches($text, '^\s*(public|internal|protected|private)?\s*(static\s+)?(partial\s+)?class\s+([A-Za-z0-9_]+)', 'Multiline')
  if($cls.Count -gt 0){ $r.Classes = $cls | ForEach-Object { $_.Groups[4].Value } }
  return $r
}

# ---------- Stage 1: discover roots ----------
$sw = [System.Diagnostics.Stopwatch]::StartNew()
Write-Progress -Activity "AQ Editor Audit" -Status "Discovering Editor folders..." -PercentComplete 3
if(!(Test-Path $OutDir)){ New-Item -ItemType Directory -Force -Path $OutDir | Out-Null }
$editorRoots = Get-EditorRoots
Write-Progress -Activity "AQ Editor Audit" -Status "Found $($editorRoots.Count) Editor folder(s)" -PercentComplete 8
$sw.Stop(); $t1 = $sw.Elapsed

# ---------- Stage 2: enumerate files ----------
$sw.Restart()
Write-Progress -Activity "AQ Editor Audit" -Status "Enumerating files..." -PercentComplete 12
$files = @()
foreach($root in $editorRoots){
  $files += Get-ChildItem -Path $root.FullName -Recurse -File -Force |
    Where-Object {
      -not (Test-SkipPath $_.FullName) -and
      $IncludeExt -contains ([System.IO.Path]::GetExtension($_.Name).ToLower())
    }
}
$files = $files | Sort-Object FullName
Write-Progress -Activity "AQ Editor Audit" -Status "Enumerated $($files.Count) files" -PercentComplete 20
$sw.Stop(); $t2 = $sw.Elapsed

# ---------- Stage 3: inventory + hashes ----------
$sw.Restart()
$inv = New-Object System.Collections.Generic.List[object]
$idx = 0
foreach($f in $files){
  $idx++
  $pct = 20 + [int](($idx / [Math]::Max(1,$files.Count)) * 25)  # 20→45%
  Write-Progress -Activity "AQ Editor Audit" -Status "Hashing & inventory ($idx / $($files.Count)): $($f.Name)" -PercentComplete $pct
  $hash = ""
  if(-not $SkipHash){
    try { $hash = (Get-FileHash -LiteralPath $f.FullName -Algorithm SHA1).Hash } catch { $hash = "" }
  }
  $inv.Add([PSCustomObject]@{
    Path     = $f.FullName.Replace($RepoRoot, "").TrimStart("\","/")
    Name     = $f.Name
    Ext      = $f.Extension.ToLower()
    SizeKB   = [Math]::Round($f.Length/1KB,2)
    Modified = $f.LastWriteTime
    SHA1     = $hash
  })
}
$sw.Stop(); $t3 = $sw.Elapsed

# ---------- Stage 4: analyze .cs + .asmdef ----------
$sw.Restart()
$analyses = New-Object System.Collections.Generic.List[object]
$asmdefEditorOnly = @{}
$filesCount = $files.Count; $i = 0
foreach($f in $files){
  $i++
  $pct = 45 + [int](($i / [Math]::Max(1,$filesCount)) * 35)  # 45→80%
  Write-Progress -Activity "AQ Editor Audit" -Status "Analyzing ($i / $filesCount): $($f.Name)" -PercentComplete $pct

  switch($f.Extension.ToLower()){
    ".cs" {
      $scan = Scan-CsFile $f.FullName
      $analyses.Add([PSCustomObject]@{
        Path        = $f.FullName.Replace($RepoRoot, "").TrimStart("\","/")
        UsingUnityEditor     = $scan.UsingUnityEditor
        HasUnityEditorGuard  = $scan.HasUnityEditorGuard
        IsAssetPostprocessor = $scan.IsAssetPostprocessor
        HasMenuItems         = ($scan.HasMenuItems -join "; ")
        UsesDeprecatedTMP    = $scan.UsesDeprecatedTMP
        UsesDeprecatedFind   = $scan.UsesDeprecatedFind
        Namespace            = $scan.Namespace
        Classes              = ($scan.Classes -join ", ")
      }) | Out-Null
    }
    ".asmdef" {
      try {
        $json = Get-Content -LiteralPath $f.FullName -Raw | ConvertFrom-Json -ErrorAction Stop
        $name = $json.name
        $incl = @($json.includePlatforms) -join ","
        $isEditorOnly = ($incl -match 'Editor') -or ($name -match '\.Editor$')
        $asmdefEditorOnly[$f.Directory.FullName] = $isEditorOnly
      } catch { }
    }
  }
}
$sw.Stop(); $t4 = $sw.Elapsed

# ---------- Stage 5: correlate misplacements ----------
$sw.Restart()
Write-Progress -Activity "AQ Editor Audit" -Status "Correlating asmdef/editor usage..." -PercentComplete 82
$editorMisplacements = @()
foreach($a in $analyses){
  if($a.UsingUnityEditor -and -not $a.HasUnityEditorGuard){
    $absPath = Join-Path $RepoRoot $a.Path
    $dir = Split-Path $absPath -Parent
    $isEditorAsm = $false
    foreach($kvp in $asmdefEditorOnly.GetEnumerator()){
      if($dir -like "$($kvp.Key)*"){ if($kvp.Value){ $isEditorAsm = $true } }
    }
    if(-not $isEditorAsm){ $editorMisplacements += $a }
  }
}
$sw.Stop(); $t5 = $sw.Elapsed

# ---------- Stage 6: write artifacts ----------
$sw.Restart()
Write-Progress -Activity "AQ Editor Audit" -Status "Writing artifacts..." -PercentComplete 88

if(!(Test-Path $OutDir)){ New-Item -ItemType Directory -Force -Path $OutDir | Out-Null }

# File list
$header = @"
AQ Editor Audit — File Inventory
Repo: $RepoRoot
Stamp: $Stamp
Editor roots:
  $(($editorRoots | ForEach-Object { " - " + $_.FullName }) -join "`n")
Files found: $($inv.Count)
SkipHash: $SkipHash

Columns: Path | Name | Ext | SizeKB | Modified | SHA1
"@
$header | Out-File -FilePath $ListPath -Encoding utf8
$inv | Sort-Object Path | Format-Table -AutoSize | Out-String -Width 4096 | Add-Content -Path $ListPath

# Findings report
$deprecatedTMP   = $analyses | Where-Object { $_.UsesDeprecatedTMP }
$deprecatedFind  = $analyses | Where-Object { $_.UsesDeprecatedFind }
$noGuardButEd    = $analyses | Where-Object { $_.UsingUnityEditor -and -not $_.HasUnityEditorGuard }
$assetPost       = $analyses | Where-Object { $_.IsAssetPostprocessor }
$menuItems       = $analyses | Where-Object { $_.HasMenuItems -ne "" }

$report = New-Object System.Text.StringBuilder
[void]$report.AppendLine("AQ Editor Audit — Findings")
[void]$report.AppendLine("Repo: $RepoRoot")
[void]$report.AppendLine("Stamp: $Stamp")
[void]$report.AppendLine("")
[void]$report.AppendLine("SUMMARY")
[void]$report.AppendLine(("  C# editor files: {0}" -f ($analyses.Count)))
[void]$report.AppendLine(("  MenuItem-bearing files: {0}" -f ($menuItems.Count)))
[void]$report.AppendLine(("  AssetPostprocessor files: {0}" -f ($assetPost.Count)))
[void]$report.AppendLine(("  Using UnityEditor (no #if UNITY_EDITOR): {0}" -f ($noGuardButEd.Count)))
[void]$report.AppendLine(("  Deprecated TMP (enableWordWrapping): {0}" -f ($deprecatedTMP.Count)))
[void]$report.AppendLine(("  Deprecated FindObjectsOfType<T>: {0}" -f ($deprecatedFind.Count)))
[void]$report.AppendLine(("  Potential misplacements (UnityEditor usage outside Editor-only asmdef): {0}" -f ($editorMisplacements.Count)))
[void]$report.AppendLine("")
[void]$report.AppendLine("MENU ITEMS")
foreach($m in $menuItems){ [void]$report.AppendLine(("  {0}  ->  {1}" -f $m.Path, $m.HasMenuItems)) }
[void]$report.AppendLine("")
[void]$report.AppendLine("ASSET POSTPROCESSORS")
foreach($p in $assetPost){ [void]$report.AppendLine(("  {0}" -f $p.Path)) }
[void]$report.AppendLine("")
[void]$report.AppendLine("USING UnityEditor WITHOUT #if UNITY_EDITOR")
foreach($x in $noGuardButEd){ [void]$report.AppendLine(("  {0}" -f $x.Path)) }
[void]$report.AppendLine("")
[void]$report.AppendLine("DEPRECATED — TMP enableWordWrapping")
foreach($x in $deprecatedTMP){ [void]$report.AppendLine(("  {0}" -f $x.Path)) }
[void]$report.AppendLine("")
[void]$report.AppendLine("DEPRECATED — FindObjectsOfType<T>")
foreach($x in $deprecatedFind){ [void]$report.AppendLine(("  {0}" -f $x.Path)) }
[void]$report.AppendLine("")
[void]$report.AppendLine("POTENTIAL MISPLACEMENTS (UnityEditor usage not in Editor-only asmdef)")
foreach($x in $editorMisplacements){ [void]$report.AppendLine(("  {0}" -f $x.Path)) }

$report.ToString() | Out-File -FilePath $FindingsPath -Encoding utf8

# Sources concat
"// AQ Editor Audit — Sources Concat  [$Stamp]" | Out-File -FilePath $ConcatPath -Encoding utf8
$k = 0
foreach($f in ($files | Where-Object {$_.Extension -eq ".cs"})){
  $k++
  $pct = 88 + [int](($k / [Math]::Max(1,($files | Where-Object {$_.Extension -eq ".cs"}).Count)) * 12)  # 88→100%
  Write-Progress -Activity "AQ Editor Audit" -Status "Concatenating sources ($k): $($f.Name)" -PercentComplete $pct

  "
// ===== BEGIN: {0} =====" -f ($f.FullName.Replace($RepoRoot,"").TrimStart("\","/")) | Add-Content -Path $ConcatPath -Encoding utf8
  $len = (Get-Item $f.FullName).Length
  if($len -gt $MaxConcatBytesPerFile){
    "# [NOTE] Truncated to $MaxConcatBytesPerFile bytes (file size $len bytes)" | Add-Content -Path $ConcatPath
  }
  Read-FileHead -Path $f.FullName -Bytes $MaxConcatBytesPerFile | Add-Content -Path $ConcatPath -Encoding utf8
  "// ===== END =====" | Add-Content -Path $ConcatPath -Encoding utf8
}

Write-Progress -Activity "AQ Editor Audit" -Completed

$swStage = @{
  "Roots discovered"       = $t1
  "Files enumerated"       = $t2
  "Inventory & hashes"     = $t3
  "Code analysis"          = $t4
  "Correlate asmdef usage" = $t5
  "Write artifacts"        = $sw.Elapsed
}
$swAll.Stop()

Write-Host "PASS: Editor audit ready."
Write-Host "  - $ListPath"
Write-Host "  - $FindingsPath"
Write-Host "  - $ConcatPath"
Write-Host ""
Write-Host "Stage timings:"
$swStage.GetEnumerator() | ForEach-Object { "{0,-26} {1}" -f $_.Key, $_.Value } | Write-Host
Write-Host ("Total elapsed: {0}" -f $swAll.Elapsed)
