<# 
project-audit.ps1
Creates an audit of a Unity project (Assets/, Packages/, ProjectSettings/):
 - Writes a sorted, deduplicated file list to _audit/_project_structure_TIMESTAMP.txt
 - Optionally concatenates key source/config files to _audit/_project_sources_concat_TIMESTAMP.txt
 - Emits findings: duplicate asmdef names, missing asmdef references, stray Samples~ asmdefs
Idempotent, re-runnable, verbose.

Usage:
  pwsh -File .\Tools\project-audit.ps1 -RepoRoot . [-NoSources]
#>

[CmdletBinding()]
param(
  [string]$RepoRoot = ".",
  [switch]$NoSources
)

$ErrorActionPreference = "Stop"

function Write-Log([string]$msg) {
  Write-Host "[$(Get-Date -Format 'HH:mm:ss')] $msg"
}

function Ensure-Dir([string]$path) {
  if (-not (Test-Path -LiteralPath $path)) {
    New-Item -ItemType Directory -Force -Path $path | Out-Null
  }
}

# 0) Resolve and validate
$repo = (Resolve-Path -LiteralPath $RepoRoot).Path
Write-Log "RepoRoot = $repo"

$targets = @("Assets","Packages","ProjectSettings") | ForEach-Object { Join-Path $repo $_ }
$missing = @()
foreach ($d in $targets) { if (-not (Test-Path -LiteralPath $d)) { $missing += Split-Path -Leaf $d } }
if ($missing.Count -gt 0) {
  Write-Log "WARNING: Missing expected folders: $($missing -join ', ')"
}

# Unity editor version (if available)
$unityVersionFile = Join-Path $repo "ProjectSettings/ProjectVersion.txt"
$unityVersion = "unknown"
if (Test-Path -LiteralPath $unityVersionFile) {
  $line = Get-Content -LiteralPath $unityVersionFile | Where-Object { $_ -match "m_EditorVersion:" } | Select-Object -First 1
  if ($line) { $unityVersion = ($line -split ":",2)[1].Trim() }
}

# 1) Prepare output
$auditDir = Join-Path $repo "_audit"
Ensure-Dir $auditDir
$ts = Get-Date -Format "yyyyMMdd_HHmmss"
$structurePath = Join-Path $auditDir "_project_structure_${ts}.txt"
$sourcesPath   = Join-Path $auditDir "_project_sources_concat_${ts}.txt"
$reportPath    = Join-Path $auditDir "_project_findings_${ts}.txt"

Write-Log "Output dir = $auditDir"
Write-Log "Unity Version = $unityVersion"

# 2) Collect file inventory
Write-Log "Scanning file inventory..."
$allFiles = @()
foreach ($root in $targets) {
  if (Test-Path -LiteralPath $root) {
    $allFiles += (Get-ChildItem -LiteralPath $root -Recurse -File -ErrorAction SilentlyContinue)
  }
}
# Relative, normalized, sorted list
$rel = $allFiles | ForEach-Object { $_.FullName.Substring($repo.Length).TrimStart("\","/") } |
       Sort-Object -Unique

# 3) Write structure file
Write-Log "Writing structure: $structurePath"
"PROJECT STRUCTURE   ($ts)
RepoRoot: $repo
Unity:    $unityVersion

=== FILE LIST (Assets/, Packages/, ProjectSettings/) ===
" | Set-Content -LiteralPath $structurePath -Encoding UTF8

$rel | Add-Content -LiteralPath $structurePath -Encoding UTF8

# Extension summary
"`n=== SUMMARY BY EXTENSION ===" | Add-Content -LiteralPath $structurePath -Encoding UTF8
$rel | ForEach-Object {
  [IO.Path]::GetExtension($_).ToLowerInvariant()
} | Group-Object | Sort-Object Count -Descending |
ForEach-Object { "{0,6}  {1}" -f $_.Count, ($_.Name -ne "" ? $_.Name : "<no ext>") } |
Add-Content -LiteralPath $structurePath -Encoding UTF8

# 4) asmdef analysis
Write-Log "Analyzing asmdefs..."
$asmdefFiles = $allFiles | Where-Object { $_.Extension -ieq ".asmdef" }
$asmdefs = @()
foreach ($f in $asmdefFiles) {
  try {
    $json = Get-Content -LiteralPath $f.FullName -Raw | ConvertFrom-Json
    $asmdefs += [pscustomobject]@{
      Path            = $f.FullName
      RelPath         = $f.FullName.Substring($repo.Length).TrimStart("\","/")
      Name            = $json.name
      References      = @($json.references)
      TestAssemblies  = [bool]$json.testAssemblies
      IncludePlatforms= @($json.includePlatforms)
    }
  } catch {
    Write-Log "WARN: Failed to parse asmdef JSON: $($f.FullName)"
  }
}
$names = $asmdefs.Name | Sort-Object -Unique
$dupes = $asmdefs | Group-Object Name | Where-Object { $_.Count -gt 1 }
$allRefs = @()
foreach ($a in $asmdefs) {
  foreach ($r in $a.References) {
    if ($null -ne $r -and $r -ne "") {
      $allRefs += [pscustomobject]@{ From=$a.Name; To=$r }
    }
  }
}
$missingRefs = $allRefs | Where-Object { $_.To -notin $names } | Sort-Object To, From -Unique

# Samples~ / sample asmdefs not in quarantine
$sampleAsmdefs = $asmdefs | Where-Object {
  $_.Path -match "(?i)Samples~|Sample|Examples" -and $_.Path -notmatch "(?i)_Quarantine"
}

# 5) Write findings report
Write-Log "Writing findings: $reportPath"
@"
PROJECT FINDINGS   ($ts)
RepoRoot: $repo
Unity:    $unityVersion

-- Duplicate asmdef names --
$(
  if ($dupes) {
    ($dupes | ForEach-Object {
      "  * {0} ({1} copies)" -f $_.Name, $_.Count
      $_.Group | ForEach-Object { "      - {0}" -f $_.RelPath }
    }) -join "`n"
  } else { "  (none found)" }
)

-- Missing asmdef references (reference -> missing target) --
$(
  if ($missingRefs) {
    ($missingRefs | ForEach-Object { "  * {0} -> {1}" -f $_.From, $_.To }) -join "`n"
  } else { "  (none found)" }
)

-- Stray asmdefs in Samples~/Sample/Examples (not quarantined) --
$(
  if ($sampleAsmdefs) {
    ($sampleAsmdefs | ForEach-Object { "  * {0}  [{1}]" -f $_.Name, $_.RelPath }) -join "`n"
  } else { "  (none found)" }
)
"@ | Set-Content -LiteralPath $reportPath -Encoding UTF8

# 6) Optional: concatenate sources/configs
if (-not $NoSources) {
  Write-Log "Building sources concat: $sourcesPath"

  $sourceFiles = @()

  # Core text sources
  $sourceFiles += ($allFiles | Where-Object { $_.Extension -iin ".cs",".asmdef",".asmref",".rsp" })

  # Important config files
  $extras = @(
    (Join-Path $repo "Packages\manifest.json"),
    (Join-Path $repo "Packages\packages-lock.json"),
    (Join-Path $repo "csc.rsp")
  ) + @(
    "ProjectSettings.asset","EditorBuildSettings.asset","GraphicsSettings.asset",
    "TagManager.asset","InputManager.asset","PackageManagerSettings.asset"
  ) | ForEach-Object { Join-Path (Join-Path $repo "ProjectSettings") $_ }

  foreach ($p in $extras) { if (Test-Path -LiteralPath $p) { $sourceFiles += (Get-Item -LiteralPath $p) } }

  # Dedup & sort
  $sourceFiles = $sourceFiles | Sort-Object FullName -Unique

  # Write with headers
  "# PROJECT SOURCES CONCAT ($ts)  Unity: $unityVersion`n" | Set-Content -LiteralPath $sourcesPath -Encoding UTF8
  foreach ($f in $sourceFiles) {
    $relp = $f.FullName.Substring($repo.Length).TrimStart("\","/")
    "===== BEGIN: $relp =====" | Add-Content -LiteralPath $sourcesPath -Encoding UTF8
    try {
      Get-Content -LiteralPath $f.FullName -Raw -ErrorAction Stop | Add-Content -LiteralPath $sourcesPath -Encoding UTF8
    } catch {
      "<<< Unable to read as text >>>" | Add-Content -LiteralPath $sourcesPath -Encoding UTF8
    }
    "===== END: $relp =====`n" | Add-Content -LiteralPath $sourcesPath -Encoding UTF8
  }
} else {
  Write-Log "Skipping sources concat (-NoSources)."
}

# 7) Final summary to console
Write-Host ""
Write-Host "Audit complete." -ForegroundColor Green
Write-Host "  Structure: $structurePath"
Write-Host "  Findings : $reportPath"
if (-not $NoSources) { Write-Host "  Sources  : $sourcesPath" }
Write-Host ""