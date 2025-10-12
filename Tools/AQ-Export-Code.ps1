#requires -Version 7.2
<#
 File: Tools\AQ-Export-Code.ps1
 Purpose: Concatenate all C# source files into two deterministic text bundles:
          1) Assets\App  ->  _audit\code\code_assets_app_<stamp>.txt
          2) Packages    ->  _audit\code\code_packages_<stamp>.txt
          Also writes a JSON manifest with per-file hashes, line counts, and sizes.

 Usage (from repo root):
   pwsh Tools\AQ-Export-Code.ps1
   # or with explicit paths:
   pwsh Tools\AQ-Export-Code.ps1 `
     -AssetsApp "C:\Users\Steph\Dev\aq30-unity\Assets\App" `
     -Packages  "C:\Users\Steph\Dev\aq30-unity\Packages"

 Notes:
   - Read-only; idempotent; deterministic ordering.
   - Adds clear BEGIN/END headers between files; code body is unmodified.
   - Encoding UTF-8. Large projects supported (streamed append).
   - Designed to avoid churn: stable sort (by full path), explicit stamp, and manifest.
#>
[CmdletBinding()]
param(
  [Parameter()]
  [string]$AssetsApp = (Join-Path (Resolve-Path (Join-Path $PSScriptRoot '..')).Path 'Assets\App'),

  [Parameter()]
  [string]$Packages  = (Join-Path (Resolve-Path (Join-Path $PSScriptRoot '..')).Path 'Packages'),

  [Parameter()]
  [string]$OutDir    = (Join-Path (Resolve-Path (Join-Path $PSScriptRoot '..')).Path '_audit\code'),

  [switch]$NoTimestamp
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Info($m){ Write-Host "[INFO] $m" }
function Pass($m){ Write-Host "[PASS] $m" -ForegroundColor Green }
function Warn($m){ Write-Host "[WARN] $m" -ForegroundColor Yellow }
function Fail($m){ Write-Host "[FAIL] $m" -ForegroundColor Red }

function Resolve-Existing([string]$p, [string]$label){
  if(-not (Test-Path -LiteralPath $p)){ throw "$label not found: $p" }
  return (Resolve-Path -LiteralPath $p).Path
}

function Ensure-Dir([string]$p){
  New-Item -ItemType Directory -Path $p -Force | Out-Null
  return (Resolve-Path -LiteralPath $p).Path
}

function New-EmptyUtf8([string]$path){
  # Create/overwrite an empty UTF-8 file (no BOM churn).
  Set-Content -LiteralPath $path -Value '' -NoNewline -Encoding UTF8
}

function Append-Line([string]$path, [string]$text){
  Add-Content -LiteralPath $path -Value $text -Encoding UTF8
}

function Dump-CodeSet {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory)][string]$Root,
    [Parameter(Mandatory)][string]$OutPath,
    [Parameter(Mandatory)][string]$Label
  )

  $rootResolved = Resolve-Existing $Root $Label
  $files = Get-ChildItem -LiteralPath $rootResolved -Recurse -File -Filter *.cs |
           Sort-Object FullName, Length

  Info "${Label}: found $($files.Count) .cs files under $rootResolved"
  New-EmptyUtf8 -path $OutPath

  $header = @(
    "//// CODE DUMP",
    "//// SourceRoot: $rootResolved",
    "//// Label     : $Label",
    "//// Generated : $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss K')",
    "//// Files     : $($files.Count)",
    "//// ---------------------------------------------------------------------------",
    ""
  ) -join "`r`n"
  Append-Line -path $OutPath -text $header

  $index = [System.Collections.Generic.List[pscustomobject]]::new()
  $fileNum = 0
  foreach($f in $files){
    $fileNum++
    $rel = $f.FullName.Substring($rootResolved.Length).TrimStart('\','/')
    $sha = (Get-FileHash -Algorithm SHA256 -LiteralPath $f.FullName).Hash
    $text = Get-Content -LiteralPath $f.FullName -Raw -Encoding UTF8
    # count lines without altering content
    $lineCount = if([string]::IsNullOrEmpty($text)){ 0 } else { ($text -split "`r`n|`n|`r").Count }

    # file prologue
    $prologue = @(
      "//// BEGIN FILE [$fileNum/$($files.Count)]: $rel",
      "//// SIZE=$($f.Length) bytes | LINES=$lineCount | SHA256=$sha",
      "//// ---------------------------------------------------------------------------"
    ) -join "`r`n"
    Append-Line -path $OutPath -text $prologue
    Append-Line -path $OutPath -text "`r`n"

    # body (exact)
    Append-Line -path $OutPath -text $text
    if(-not $text.EndsWith("`r`n") -and -not $text.EndsWith("`n")){ Append-Line -path $OutPath -text "`r`n" }

    # epilogue
    Append-Line -path $OutPath -text "//// END FILE: $rel"
    Append-Line -path $OutPath -text "`r`n"

    $index.Add([pscustomobject]@{
      path     = $rel
      size     = $f.Length
      lines    = $lineCount
      sha256   = $sha
      modified = $f.LastWriteTimeUtc.ToString('u')
    })
  }

  # Totals for this set (append)
  $mo = $index | Measure-Object -Property lines -Sum
  $totalLines = if($mo -and $mo.Sum) { [int]$mo.Sum } else { 0 }

  $summary = @(
    "//// ---------------------------------------------------------------------------",
    "//// SUMMARY ${Label}: files=$($files.Count) lines=$totalLines",
    "//// END OF DUMP ($Label)",
    ""
  ) -join "`r`n"
  Append-Line -path $OutPath -text $summary

  return [pscustomobject]@{
    label      = $Label
    root       = $rootResolved
    outPath    = $OutPath
    files      = $index
    fileCount  = $files.Count
    lineTotal  = $totalLines
  }
}

try {
  $assetsRoot   = Resolve-Existing $AssetsApp  'Assets\App'
  $packagesRoot = Resolve-Existing $Packages   'Packages'
  $outRoot      = Ensure-Dir $OutDir

  $stamp = if($NoTimestamp){ 'latest' } else { Get-Date -Format 'yyyyMMdd_HHmmss' }
  $assetsOut   = Join-Path $outRoot ("code_assets_app_{0}.txt" -f $stamp)
  $packagesOut = Join-Path $outRoot ("code_packages_{0}.txt"  -f $stamp)
  $manifestOut = Join-Path $outRoot ("code_manifest_{0}.json" -f $stamp)

  Info "Output: $outRoot"
  $assetsSummary   = Dump-CodeSet -Root $assetsRoot   -OutPath $assetsOut   -Label 'Assets\App'
  $packagesSummary = Dump-CodeSet -Root $packagesRoot -OutPath $packagesOut -Label 'Packages'

  $manifest = [ordered]@{
    generatedUtc = (Get-Date).ToUniversalTime().ToString('u')
    stamp        = $stamp
    outputs      = @(
      [ordered]@{
        label = $assetsSummary.label
        out   = (Resolve-Path -LiteralPath $assetsSummary.outPath).Path
        files = $assetsSummary.fileCount
        lines = $assetsSummary.lineTotal
        index = $assetsSummary.files
      },
      [ordered]@{
        label = $packagesSummary.label
        out   = (Resolve-Path -LiteralPath $packagesSummary.outPath).Path
        files = $packagesSummary.fileCount
        lines = $packagesSummary.lineTotal
        index = $packagesSummary.files
      }
    )
  }

  ($manifest | ConvertTo-Json -Depth 8) | Set-Content -LiteralPath $manifestOut -Encoding UTF8

  Pass "Assets\App dump -> $(Split-Path -Leaf $assetsOut)  (files=$($assetsSummary.fileCount) lines=$($assetsSummary.lineTotal))"
  Pass "Packages dump  -> $(Split-Path -Leaf $packagesOut) (files=$($packagesSummary.fileCount) lines=$($packagesSummary.lineTotal))"
  Pass "Manifest       -> $(Split-Path -Leaf $manifestOut)"
  Info "Ready to hand the two TXT files back for evaluation."
}
catch {
  Fail $_.Exception.Message
  exit 1
}
