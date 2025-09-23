#Requires -Version 7.0
<#
AQ-Editor-Audit.ps1
Walks all Editor directories, inventories files, and audits C# editor scripts for common issues.
Emits artifacts under _audit/editor/

Safe (read-only). Tested on PowerShell 7.x.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ---------------- Config ----------------
$Stamp        = Get-Date -Format "yyyyMMdd_HHmmss"
$RepoRoot     = (Resolve-Path ".").Path
$OutDir       = Join-Path $RepoRoot "_audit/editor"
$ListPath     = Join-Path $OutDir ("editor_file_list_{0}.txt"     -f $Stamp)
$FindingsPath = Join-Path $OutDir ("editor_findings_{0}.txt"      -f $Stamp)
$ConcatPath   = Join-Path $OutDir ("editor_sources_concat_{0}.txt"-f $Stamp)

# What to scan
$IncludeExt = @(".cs",".asmdef",".uxml",".uss",".shader",".ps1",".json",".md")
$MaxConcatBytesPerFile = 250KB   # sanity cap for sources_concat

# Directories to skip (wildcard match on normalized forward-slash paths)
$SkipDirs = @("/.git/","/Library/","/Obj/","/Bin/","/_audit/","/Logs/")

# ---------------- Helpers ----------------
function Test-SkipPath([string]$Path){
  if ([string]::IsNullOrWhiteSpace($Path)) { return $false }
  $norm = $Path -replace '\\','/'  # normalize to forward slashes
  foreach($p in $SkipDirs){
    if ($norm -like "*$p*") { return $true }
  }
  return $false
}

function Get-EditorRoots {
  $roots = New-Object System.Collections.Generic.List[System.IO.DirectoryInfo]
  if(Test-Path "Assets"){
    Get-ChildItem -Path "Assets" -Recurse -Directory -Force |
      Where-Object { $_.Name -ieq "Editor" -and -not (Test-SkipPath $_.FullName) } |
      ForEach-Object { [void]$roots.Add($_) }
  }
  if(Test-Path "Packages"){
    Get-ChildItem -Path "Packages" -Recurse -Directory -Force |
      Where-Object { $_.Name -ieq "Editor" -and -not (Test-SkipPath $_.FullName) } |
      ForEach-Object { [void]$roots.Add($_) }
  }
  return ,$roots.ToArray()
}

function Read-FileHead([string]$Path, [int]$Bytes){
  try {
    $fs = [System.IO.File]::Open($Path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
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

  $rxM = [System.Text.RegularExpressions.RegexOptions]::Multiline

  if([regex]::IsMatch($text, '^\s*using\s+UnityEditor\s*;', $rxM)){ $r.UsingUnityEditor = $true }
  if([regex]::IsMatch($text, '^\s*#\s*if\s+UNITY_EDITOR',   $rxM)){ $r.HasUnityEditorGuard = $true }
  if([regex]::IsMatch($text, 'class\s+[^\{:]+\s*:\s*AssetPostprocessor', $rxM)){ $r.IsAssetPostprocessor = $true }

  $menuMatches = [System.Text.RegularExpressions.Regex]::Matches($text, '\[MenuItem\("([^"]+)"[^\]]*\)\)')
  if($menuMatches.Count -gt 0){
    $r.HasMenuItems = $menuMatches | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique
  }

  if([regex]::IsMatch($text, '\b(enableWordWrapping)\b')){ $r.UsesDeprecatedTMP = $true }
  if([regex]::IsMatch($text, 'FindObjectsOfType\s*<'))   { $r.UsesDeprecatedFind = $true }

  $ns = [regex]::Match($text,'^\s*namespace\s+([A-Za-z0-9_.]+)', $rxM)
  if($ns.Success){ $r.Namespace = $ns.Groups[1].Value }

  $cls = [regex]::Matches($text,
    '^\s*(public|internal|protected|private)?\s*(static\s+)?(partial\s+)?class\s+([A-Za-z0-9_]+)',
    $rxM)
  if($cls.Count -gt 0){ $r.Classes = $cls | ForEach-Object { $_.Groups[4].Value } }

  return $r
}

# ---------------- Main ----------------
if(!(Test-Path $OutDir)){ New-Item -ItemType Directory -Force -Path $OutDir | Out-Null }

$editorRoots = @(Get-EditorRoots)
if(-not $editorRoots -or $editorRoots.Count -eq 0){
  # Still produce empty artifacts so the pipeline doesn’t break.
  "AQ Editor Audit — File Inventory`nRepo: $RepoRoot`nStamp: $Stamp`nEditor roots: (none)`nFiles found: 0" | Out-File -FilePath $ListPath -Encoding utf8
  "AQ Editor Audit — Findings`nRepo: $RepoRoot`nStamp: $Stamp`n`nSUMMARY`n  C# editor files: 0`n  MenuItem-bearing files: 0`n  AssetPostprocessor files: 0`n  Using UnityEditor (no #if UNITY_EDITOR): 0`n  Deprecated TMP (enableWordWrapping): 0`n  Deprecated FindObjectsOfType<T>: 0`n  Potential misplacements (UnityEditor usage outside Editor-only asmdef): 0" | Out-File -FilePath $FindingsPath -Encoding utf8
  "// AQ Editor Audit — Sources Concat  [$Stamp]`n// (no sources)" | Out-File -FilePath $ConcatPath -Encoding utf8
  Write-Host "PASS: Editor audit ready (no Editor folders found)."
  Write-Host "  - $ListPath"
  Write-Host "  - $FindingsPath"
  Write-Host "  - $ConcatPath"
  exit 0
}

$files = New-Object System.Collections.Generic.List[System.IO.FileInfo]
foreach($root in $editorRoots){
  Get-ChildItem -Path $root.FullName -Recurse -File -Force |
    Where-Object {
      -not (Test-SkipPath $_.FullName) -and
      $IncludeExt -contains ([System.IO.Path]::GetExtension($_.Name).ToLower())
    } | ForEach-Object { [void]$files.Add($_) }
}

# Inventory + hashes
$inv = foreach($f in ($files | Sort-Object FullName)){
  $hash = try { (Get-FileHash -LiteralPath $f.FullName -Algorithm SHA1).Hash } catch { "" }
  [PSCustomObject]@{
    Path        = $f.FullName.Replace($RepoRoot, "").TrimStart("\","/")
    Name        = $f.Name
    Ext         = $f.Extension.ToLower()
    SizeKB      = [Math]::Round($f.Length/1KB,2)
    Modified    = $f.LastWriteTime
    SHA1        = $hash
  }
}

# Per-file analyses for .cs and .asmdef
$analyses = New-Object System.Collections.Generic.List[object]
$asmdefEditorOnly = @{}

foreach($f in $files){
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

# Correlate: C# files using UnityEditor but not under an editor-only asmdef (heuristic)
$editorMisplacements = @()
$analysesArr = @($analyses.ToArray())  # normalize to array early
foreach($a in $analysesArr){
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

# ---------- Write artifacts ----------
# 1) File list
$header = @"
AQ Editor Audit — File Inventory
Repo: $RepoRoot
Stamp: $Stamp
Editor roots:
  $(($editorRoots | ForEach-Object { " - " + $_.FullName }) -join "`n")
Files found: $($inv.Count)

Columns: Path | Name | Ext | SizeKB | Modified | SHA1
"@
$header | Out-File -FilePath $ListPath -Encoding utf8
$inv | Sort-Object Path | Format-Table -AutoSize | Out-String -Width 4096 | Add-Content -Path $ListPath

# 2) Findings report (normalize all to arrays before Count)
$deprecatedTMP    = @($analysesArr | Where-Object { $_.UsesDeprecatedTMP })
$deprecatedFind   = @($analysesArr | Where-Object { $_.UsesDeprecatedFind })
$noGuardButEditor = @($analysesArr | Where-Object { $_.UsingUnityEditor -and -not $_.HasUnityEditorGuard })
$assetPost        = @($analysesArr | Where-Object { $_.IsAssetPostprocessor })
$menuItems        = @($analysesArr | Where-Object { $_.HasMenuItems -ne "" })

$report = New-Object System.Text.StringBuilder
[void]$report.AppendLine("AQ Editor Audit — Findings")
[void]$report.AppendLine("Repo: $RepoRoot")
[void]$report.AppendLine("Stamp: $Stamp")
[void]$report.AppendLine("")
[void]$report.AppendLine("SUMMARY")
[void]$report.AppendLine(("  C# editor files: {0}" -f ($analysesArr.Count)))
[void]$report.AppendLine(("  MenuItem-bearing files: {0}" -f ($menuItems.Count)))
[void]$report.AppendLine(("  AssetPostprocessor files: {0}" -f ($assetPost.Count)))
[void]$report.AppendLine(("  Using UnityEditor (no #if UNITY_EDITOR): {0}" -f ($noGuardButEditor.Count)))
[void]$report.AppendLine(("  Deprecated TMP (enableWordWrapping): {0}" -f ($deprecatedTMP.Count)))
[void]$report.AppendLine(("  Deprecated FindObjectsOfType<T>: {0}" -f ($deprecatedFind.Count)))
[void]$report.AppendLine(("  Potential misplacements (UnityEditor usage outside Editor-only asmdef): {0}" -f ($editorMisplacements.Count)))
[void]$report.AppendLine("")
[void]$report.AppendLine("MENU ITEMS")
foreach($m in $menuItems){
  [void]$report.AppendLine(("  {0}  ->  {1}" -f $m.Path, $m.HasMenuItems))
}
[void]$report.AppendLine("")
[void]$report.AppendLine("ASSET POSTPROCESSORS")
foreach($p in $assetPost){ [void]$report.AppendLine(("  {0}" -f $p.Path)) }
[void]$report.AppendLine("")
[void]$report.AppendLine("USING UnityEditor WITHOUT #if UNITY_EDITOR")
foreach($x in $noGuardButEditor){ [void]$report.AppendLine(("  {0}" -f $x.Path)) }
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

# 3) Sources concat (for quick grepping / manual review)
"// AQ Editor Audit — Sources Concat  [$Stamp]" | Out-File -FilePath $ConcatPath -Encoding utf8
foreach($f in ($files | Sort-Object FullName)){
  if($f.Extension.ToLower() -eq ".cs"){
    "
// ===== BEGIN: {0} =====" -f ($f.FullName.Replace($RepoRoot,"").TrimStart("\","/")) | Add-Content -Path $ConcatPath -Encoding utf8
    $len = (Get-Item $f.FullName).Length
    if($len -gt $MaxConcatBytesPerFile){
      "# [NOTE] Truncated to $MaxConcatBytesPerFile bytes (file size $len bytes)" | Add-Content -Path $ConcatPath
    }
    Read-FileHead -Path $f.FullName -Bytes $MaxConcatBytesPerFile | Add-Content -Path $ConcatPath -Encoding utf8
    "// ===== END =====" | Add-Content -Path $ConcatPath -Encoding utf8
  }
}

Write-Host "PASS: Editor audit ready."
Write-Host "  - $ListPath"
Write-Host "  - $FindingsPath"
Write-Host "  - $ConcatPath"
