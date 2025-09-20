<# ========================================================================
  Tools\Content-ScriptDump.ps1
  Produces:
    - _audit\_project_structure_YYYYMMDD-HHMMss.txt
    - _audit\_project_sources_concat_YYYYMMDD-HHMMss.txt

  Safe for Windows PowerShell 5.x (no PS7 features).
  Usage:
    powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\Content-ScriptDump.ps1 -RepoRoot "C:\Path\to\repo"

  Optional params:
    -ActiveDirs "Assets","Packages","ProjectSettings","Tools","DotNetBuild"
    -OutDir ".\_audit"
    -IncludeExt ".cs",".asmdef",".json",".yaml",".yml",".xml",".shader",".txt",".md",".ps1",".uxml",".uss",".asmref",".rsp",".meta"
    -MaxFileSizeMB 2
========================================================================= #>

[CmdletBinding()]
param(
  [Parameter(Mandatory=$true)]
  [string]$RepoRoot,

  [string[]]$ActiveDirs = @("Assets","Packages","ProjectSettings","Tools","DotNetBuild"),

  [string]$OutDir = ".\_audit",

  [string[]]$IncludeExt = @(
    ".cs",".asmdef",".json",".yaml",".yml",".xml",".shader",".txt",".md",".ps1",
    ".uxml",".uss",".asmref",".rsp",".meta",".cginc",".hlsl",".glsl",".compute"
  ),

  [int]$MaxFileSizeMB = 2
)

function Resolve-ExistingDirs {
  param([string]$Root,[string[]]$Dirs)
  $list = @()
  foreach($d in $Dirs){
    $p = Join-Path -Path $Root -ChildPath $d
    if(Test-Path $p){ $list += (Resolve-Path -LiteralPath $p).Path }
  }
  return $list
}

function New-SafeDirectory {
  param([string]$Path)
  if(!(Test-Path $Path)){ New-Item -ItemType Directory -Path $Path | Out-Null }
}

function Get-RelativePath {
  param([string]$Root,[string]$Full)
  $rootFixed = (Resolve-Path -LiteralPath $Root).Path
  $fullFixed = (Resolve-Path -LiteralPath $Full).Path
  if($fullFixed.StartsWith($rootFixed)){
    return $fullFixed.Substring($rootFixed.Length).TrimStart('\','/')
  }
  return $Full
}

function Should-ExcludePath {
  param([string]$FullPath)
  # Common heavy/noisy folders
  $excludeNames = @(
    "\.git", "\.vs", "\.idea", "Library", "Temp", "Obj", "obj", "Logs",
    "Build", "Builds", "UserSettings", "MemoryCaptures",
    "PackageCache", "node_modules", "External", "DerivedData"
  )
  $pattern = ($excludeNames -join "|")
  if($FullPath -match $pattern){ return $true }
  return $false
}

function Is-IncludedTextFile {
  param([System.IO.FileInfo]$File,[string[]]$AllowExt)
  $ext = ($File.Extension).ToLowerInvariant()
  if($AllowExt -contains $ext){ return $true }
  return $false
}

try{
  if(!(Test-Path $RepoRoot)){ throw "RepoRoot not found: $RepoRoot" }

  $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
  $auditDir  = Join-Path -Path $RepoRoot -ChildPath $OutDir
  New-SafeDirectory -Path $auditDir

  $structurePath = Join-Path -Path $auditDir -ChildPath ("_project_structure_{0}.txt" -f $timestamp)
  $sourcesPath   = Join-Path -Path $auditDir -ChildPath ("_project_sources_concat_{0}.txt" -f $timestamp)

  $roots = Resolve-ExistingDirs -Root $RepoRoot -Dirs $ActiveDirs
  if($roots.Count -eq 0){ throw "None of the ActiveDirs exist under $RepoRoot. Checked: $($ActiveDirs -join ', ')" }

  # Gather candidate files (restricted to included extensions, excluding noisy dirs)
  $allFiles = @()
  foreach($r in $roots){
    $items = Get-ChildItem -LiteralPath $r -Recurse -File -Force -ErrorAction SilentlyContinue
    foreach($it in $items){
      if(Should-ExcludePath -FullPath $it.FullName){ continue }
      if(Is-IncludedTextFile -File $it -AllowExt $IncludeExt){ $allFiles += $it }
    }
  }

  # ---------------- Structure Report ----------------
  $sb = New-Object System.Text.StringBuilder
  [void]$sb.AppendLine(("AQ30 Project Structure Report  ::  {0}" -f (Get-Date)))
  [void]$sb.AppendLine(("RepoRoot: {0}" -f (Resolve-Path -LiteralPath $RepoRoot).Path))
  [void]$sb.AppendLine(("ActiveDirs: {0}" -f ($roots -join ", ")))
  [void]$sb.AppendLine(("Included Extensions: {0}" -f ($IncludeExt -join ", ")))
  [void]$sb.AppendLine(("Excluded Folders: .git, .vs, .idea, Library, Temp, Obj/obj, Logs, Build(s), UserSettings, PackageCache, node_modules, External, DerivedData"))
  [void]$sb.AppendLine("")

  $sorted = $allFiles | Sort-Object FullName
  $currentDir = ""
  foreach($f in $sorted){
    $dir = Split-Path -Parent $f.FullName
    if($dir -ne $currentDir){
      [void]$sb.AppendLine(("DIR  {0}" -f (Get-RelativePath -Root $RepoRoot -Full $dir)))
      $currentDir = $dir
    }
    $rel = Get-RelativePath -Root $RepoRoot -Full $f.FullName
    $sizeKB = [Math]::Round(($f.Length/1KB),2)
    $hash = ""
    try{
      $h = Get-FileHash -Algorithm SHA1 -LiteralPath $f.FullName -ErrorAction SilentlyContinue
      if($h){ $hash = $h.Hash.Substring(0,12) }
    }catch{}
    [void]$sb.AppendLine(("  - {0}  ({1} KB, {2}, sha1:{3})" -f $rel,$sizeKB,$f.LastWriteTime.ToString("yyyy-MM-dd HH:mm"),$hash))
  }
  $sb.ToString() | Set-Content -LiteralPath $structurePath -Encoding UTF8

  # ---------------- Sources Concatenation ----------------
  $maxBytes = $MaxFileSizeMB * 1MB
  $sep = ("=" * 78)

  $sw = New-Object System.IO.StreamWriter($sourcesPath,$false,[System.Text.UTF8Encoding]::new($false))
  try{
    $sw.WriteLine("AQ30 Project Sources Concatenated  ::  {0}" -f (Get-Date))
    $sw.WriteLine("RepoRoot: {0}" -f (Resolve-Path -LiteralPath $RepoRoot).Path)
    $sw.WriteLine("Files included: {0}" -f $sorted.Count)
    $sw.WriteLine($sep)

    foreach($f in $sorted){
      $rel = Get-RelativePath -Root $RepoRoot -Full $f.FullName
      $sw.WriteLine("FILE: {0}" -f $rel)
      $sw.WriteLine("SIZE: {0} bytes" -f $f.Length)
      $sw.WriteLine("MTIME: {0}" -f $f.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"))
      $sw.WriteLine($sep)

      if($f.Length -le $maxBytes){
        try{
          $content = Get-Content -LiteralPath $f.FullName -Raw -Encoding UTF8 -ErrorAction Stop
          $content = $content -replace "`r`n","`n"   # normalize endings for clean diffs
          $sw.WriteLine($content)
        }catch{
          $sw.WriteLine("<<< ERROR READING FILE >>>")
          $sw.WriteLine($_.Exception.Message)
        }
      } else {
        $sw.WriteLine("<<< SKIPPED: file exceeds MaxFileSizeMB ({0} MB). Current size: {1} MB >>>" -f $MaxFileSizeMB, [Math]::Round(($f.Length/1MB),2))
      }
      $sw.WriteLine("") ; $sw.WriteLine($sep) ; $sw.WriteLine("")
    }
  } finally {
    $sw.Flush(); $sw.Close()
  }

  Write-Host ""
  Write-Host "Structure report : $structurePath"
  Write-Host "Sources concat   : $sourcesPath"
  Write-Host ""
  Write-Host "Done. Feed BOTH files back into the chat for situational awareness."
}
catch{
  Write-Error $_
  exit 1
}
