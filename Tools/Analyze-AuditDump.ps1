<#
.SYNOPSIS
  Step 0 audit helper for Ally Quinn: True Crime Merge.

.DESCRIPTION
  Runs a focused repository audit to support Step 0:
   - Verifies the contract prefab exists at Assets/Resources/App/UI/Prefabs/DialoguePanel.prefab
   - Lists any duplicate DialoguePanel prefabs elsewhere
   - Reports any .off files or Z._quarantine folders under Assets/Resources
   - Detects duplicate asmdef "name" values by reading JSON
   - Optionally checks that the contract prefab is tracked by git
  Writes a concise report to .\.logs\audit_step0_report_TIMESTAMP.txt and sets a non-zero exit code on critical issues.

.PARAMETER RepoRoot
  Path to the repo root (default: current directory).

.EXAMPLE
  pwsh -File .\Analyze-AuditDump.ps1 -RepoRoot "C:\Users\Steph\Dev\aq30-unity"
#>

[CmdletBinding()]
param(
  [string]$RepoRoot = (Get-Location).Path
)

$ErrorActionPreference = 'Stop'

function New-LogDir {
  param([string]$Root)
  $logDir = Join-Path $Root ".logs"
  if(-not (Test-Path $logDir)){ New-Item -ItemType Directory -Path $logDir | Out-Null }
  return $logDir
}

function Get-GitTracked {
  param([string]$Root, [string]$RelPath)
  try {
    Push-Location $Root
    git ls-files --error-unmatch -- $RelPath 2>$null | Out-Null
    $tracked = $LASTEXITCODE -eq 0
    Pop-Location
    return $tracked
  } catch {
    try{ Pop-Location } catch {}
    return $false
  }
}

$root = (Resolve-Path $RepoRoot).Path
$logDir = New-LogDir -Root $root
$ts = Get-Date -Format "yyyyMMdd_HHmmss"
$reportPath = Join-Path $logDir "audit_step0_report_$ts.txt"

$issues = @()
$warnings = @()
$notes = @()

# 1) Contract prefab checks
$contractPath = "Assets/Resources/App/UI/Prefabs/DialoguePanel.prefab"
$contractFs = Join-Path $root $contractPath
$exists = Test-Path $contractFs

if(-not $exists){
  $issues += "MISSING: Contract prefab not found at $contractPath"
} else {
  $notes += "OK: Contract prefab exists at $contractPath"
  # optional git tracking check
  $tracked = Get-GitTracked -Root $root -RelPath $contractPath
  if($tracked){
    $notes += "OK: Contract prefab is tracked in git."
  } else {
    $warnings += "WARN: Contract prefab exists but is NOT tracked by git."
  }
}

# 2) Duplicate DialoguePanel prefabs elsewhere
$allDialoguePrefabs = Get-ChildItem -Path $root -Recurse -Filter "DialoguePanel.prefab" -ErrorAction SilentlyContinue | Where-Object { <#
.SYNOPSIS
  Step 0 audit helper for Ally Quinn: True Crime Merge.

.DESCRIPTION
  Runs a focused repository audit to support Step 0:
   - Verifies the contract prefab exists at Assets/Resources/App/UI/Prefabs/DialoguePanel.prefab
   - Lists any duplicate DialoguePanel prefabs elsewhere
   - Reports any .off files or Z._quarantine folders under Assets/Resources
   - Detects duplicate asmdef "name" values by reading JSON
   - Optionally checks that the contract prefab is tracked by git
  Writes a concise report to .\.logs\audit_step0_report_TIMESTAMP.txt and sets a non-zero exit code on critical issues.

.PARAMETER RepoRoot
  Path to the repo root (default: current directory).

.EXAMPLE
  pwsh -File .\Analyze-AuditDump.ps1 -RepoRoot "C:\Users\Steph\Dev\aq30-unity"
#>

[CmdletBinding()]
param(
  [string]$RepoRoot = (Get-Location).Path
)

$ErrorActionPreference = 'Stop'

function New-LogDir {
  param([string]$Root)
  $logDir = Join-Path $Root ".logs"
  if(-not (Test-Path $logDir)){ New-Item -ItemType Directory -Path $logDir | Out-Null }
  return $logDir
}

function Get-GitTracked {
  param([string]$Root, [string]$RelPath)
  try {
    Push-Location $Root
    git ls-files --error-unmatch -- $RelPath 2>$null | Out-Null
    $tracked = $LASTEXITCODE -eq 0
    Pop-Location
    return $tracked
  } catch {
    try{ Pop-Location } catch {}
    return $false
  }
}

$root = (Resolve-Path $RepoRoot).Path
$logDir = New-LogDir -Root $root
$ts = Get-Date -Format "yyyyMMdd_HHmmss"
$reportPath = Join-Path $logDir "audit_step0_report_$ts.txt"

$issues = @()
$warnings = @()
$notes = @()

# 1) Contract prefab checks
$contractPath = "Assets/Resources/App/UI/Prefabs/DialoguePanel.prefab"
$contractFs = Join-Path $root $contractPath
$exists = Test-Path $contractFs

if(-not $exists){
  $issues += "MISSING: Contract prefab not found at $contractPath"
} else {
  $notes += "OK: Contract prefab exists at $contractPath"
  # optional git tracking check
  $tracked = Get-GitTracked -Root $root -RelPath $contractPath
  if($tracked){
    $notes += "OK: Contract prefab is tracked in git."
  } else {
    $warnings += "WARN: Contract prefab exists but is NOT tracked by git."
  }
}

# 2) Duplicate DialoguePanel prefabs elsewhere
$allDialoguePrefabs = Get-ChildItem -Path $root -Recurse -Filter "DialoguePanel.prefab" -ErrorAction SilentlyContinue |
  Where-Object { $_.FullName -ne $contractFs }

foreach($dup in $allDialoguePrefabs){
  # Ignore obvious caches/Library folders
  if($dup.FullName -match "\\Library\\|\\Temp\\") { continue }
  $rel = ($dup.FullName.Substring($root.Length)).TrimStart('\','/')
  $warnings += "DUPLICATE: Found DialoguePanel prefab at $rel (outside contract path)"
}

# 3) .off files or quarantine under Assets/Resources
$resourcesDir = Join-Path $root "Assets/Resources"
if(Test-Path $resourcesDir){
  $offFiles = Get-ChildItem -Path $resourcesDir -Recurse -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Extension -eq ".off" }
  foreach($f in $offFiles){
    $rel = ($f.FullName.Substring($root.Length)).TrimStart('\','/')
    $warnings += "QUARANTINE-LEAK: '.off' file found under Resources → $rel"
  }

  $quarantineDirs = Get-ChildItem -Path $resourcesDir -Recurse -Directory -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -like "Z._quarantine*" }
  foreach($d in $quarantineDirs){
    $rel = ($d.FullName.Substring($root.Length)).TrimStart('\','/')
    $warnings += "QUARANTINE-LEAK: quarantine folder under Resources → $rel"
  }
}

# 4) Duplicate asmdef names
$asmdefs = Get-ChildItem -Path $root -Recurse -Filter "*.asmdef" -File -ErrorAction SilentlyContinue |
  Where-Object { $_.FullName -notmatch "\\Library\\|\\Temp\\|\\Packages\\(?!com\.aq\.)" }

$asmNameMap = @{}
foreach($asm in $asmdefs){
  try{
    $json = Get-Content $asm.FullName -Raw | ConvertFrom-Json
    $name = $json.name
    if([string]::IsNullOrWhiteSpace($name)){ continue }
    if(-not $asmNameMap.ContainsKey($name)){ $asmNameMap[$name] = @() }
    $asmNameMap[$name] += $asm
  } catch {
    $warnings += "ASMDEF: Failed to parse JSON → $($asm.FullName)"
  }
}

$dupAsm = $asmNameMap.GetEnumerator() | Where-Object { $_.Value.Count -gt 1 }
foreach($kv in $dupAsm){
  $paths = $kv.Value | ForEach-Object { $_.FullName.Substring($root.Length).TrimStart('\','/') }
  $issues += "ASMDEF-DUPLICATE: name '$($kv.Key)' appears in multiple files:`n  - " + ($paths -join "`n  - ")
}

# 5) Summarize & write report
$summary = @()
$summary += "Ally Quinn: Step 0 Audit — $(Get-Date)"
$summary += "Repo: $root"
$summary += ""
if($issues.Count -gt 0){
  $summary += "CRITICAL ISSUES:"
  $summary += $issues | ForEach-Object { " - $_" }
  $summary += ""
}
if($warnings.Count -gt 0){
  $summary += "WARNINGS:"
  $summary += $warnings | ForEach-Object { " - $_" }
  $summary += ""
}
if($notes.Count -gt 0){
  $summary += "NOTES:"
  $summary += $notes | ForEach-Object { " - $_" }
  $summary += ""
}

$summary -join "`r`n" | Tee-Object -FilePath $reportPath | Out-Host

if($issues.Count -gt 0){ exit 1 } else { exit 0 }
.FullName -notmatch "\\Library\\|\\Temp\\|\\Z\._quarantine_" -and <#
.SYNOPSIS
  Step 0 audit helper for Ally Quinn: True Crime Merge.

.DESCRIPTION
  Runs a focused repository audit to support Step 0:
   - Verifies the contract prefab exists at Assets/Resources/App/UI/Prefabs/DialoguePanel.prefab
   - Lists any duplicate DialoguePanel prefabs elsewhere
   - Reports any .off files or Z._quarantine folders under Assets/Resources
   - Detects duplicate asmdef "name" values by reading JSON
   - Optionally checks that the contract prefab is tracked by git
  Writes a concise report to .\.logs\audit_step0_report_TIMESTAMP.txt and sets a non-zero exit code on critical issues.

.PARAMETER RepoRoot
  Path to the repo root (default: current directory).

.EXAMPLE
  pwsh -File .\Analyze-AuditDump.ps1 -RepoRoot "C:\Users\Steph\Dev\aq30-unity"
#>

[CmdletBinding()]
param(
  [string]$RepoRoot = (Get-Location).Path
)

$ErrorActionPreference = 'Stop'

function New-LogDir {
  param([string]$Root)
  $logDir = Join-Path $Root ".logs"
  if(-not (Test-Path $logDir)){ New-Item -ItemType Directory -Path $logDir | Out-Null }
  return $logDir
}

function Get-GitTracked {
  param([string]$Root, [string]$RelPath)
  try {
    Push-Location $Root
    git ls-files --error-unmatch -- $RelPath 2>$null | Out-Null
    $tracked = $LASTEXITCODE -eq 0
    Pop-Location
    return $tracked
  } catch {
    try{ Pop-Location } catch {}
    return $false
  }
}

$root = (Resolve-Path $RepoRoot).Path
$logDir = New-LogDir -Root $root
$ts = Get-Date -Format "yyyyMMdd_HHmmss"
$reportPath = Join-Path $logDir "audit_step0_report_$ts.txt"

$issues = @()
$warnings = @()
$notes = @()

# 1) Contract prefab checks
$contractPath = "Assets/Resources/App/UI/Prefabs/DialoguePanel.prefab"
$contractFs = Join-Path $root $contractPath
$exists = Test-Path $contractFs

if(-not $exists){
  $issues += "MISSING: Contract prefab not found at $contractPath"
} else {
  $notes += "OK: Contract prefab exists at $contractPath"
  # optional git tracking check
  $tracked = Get-GitTracked -Root $root -RelPath $contractPath
  if($tracked){
    $notes += "OK: Contract prefab is tracked in git."
  } else {
    $warnings += "WARN: Contract prefab exists but is NOT tracked by git."
  }
}

# 2) Duplicate DialoguePanel prefabs elsewhere
$allDialoguePrefabs = Get-ChildItem -Path $root -Recurse -Filter "DialoguePanel.prefab" -ErrorAction SilentlyContinue |
  Where-Object { $_.FullName -ne $contractFs }

foreach($dup in $allDialoguePrefabs){
  # Ignore obvious caches/Library folders
  if($dup.FullName -match "\\Library\\|\\Temp\\") { continue }
  $rel = ($dup.FullName.Substring($root.Length)).TrimStart('\','/')
  $warnings += "DUPLICATE: Found DialoguePanel prefab at $rel (outside contract path)"
}

# 3) .off files or quarantine under Assets/Resources
$resourcesDir = Join-Path $root "Assets/Resources"
if(Test-Path $resourcesDir){
  $offFiles = Get-ChildItem -Path $resourcesDir -Recurse -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Extension -eq ".off" }
  foreach($f in $offFiles){
    $rel = ($f.FullName.Substring($root.Length)).TrimStart('\','/')
    $warnings += "QUARANTINE-LEAK: '.off' file found under Resources → $rel"
  }

  $quarantineDirs = Get-ChildItem -Path $resourcesDir -Recurse -Directory -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -like "Z._quarantine*" }
  foreach($d in $quarantineDirs){
    $rel = ($d.FullName.Substring($root.Length)).TrimStart('\','/')
    $warnings += "QUARANTINE-LEAK: quarantine folder under Resources → $rel"
  }
}

# 4) Duplicate asmdef names
$asmdefs = Get-ChildItem -Path $root -Recurse -Filter "*.asmdef" -File -ErrorAction SilentlyContinue |
  Where-Object { $_.FullName -notmatch "\\Library\\|\\Temp\\|\\Packages\\(?!com\.aq\.)" }

$asmNameMap = @{}
foreach($asm in $asmdefs){
  try{
    $json = Get-Content $asm.FullName -Raw | ConvertFrom-Json
    $name = $json.name
    if([string]::IsNullOrWhiteSpace($name)){ continue }
    if(-not $asmNameMap.ContainsKey($name)){ $asmNameMap[$name] = @() }
    $asmNameMap[$name] += $asm
  } catch {
    $warnings += "ASMDEF: Failed to parse JSON → $($asm.FullName)"
  }
}

$dupAsm = $asmNameMap.GetEnumerator() | Where-Object { $_.Value.Count -gt 1 }
foreach($kv in $dupAsm){
  $paths = $kv.Value | ForEach-Object { $_.FullName.Substring($root.Length).TrimStart('\','/') }
  $issues += "ASMDEF-DUPLICATE: name '$($kv.Key)' appears in multiple files:`n  - " + ($paths -join "`n  - ")
}

# 5) Summarize & write report
$summary = @()
$summary += "Ally Quinn: Step 0 Audit — $(Get-Date)"
$summary += "Repo: $root"
$summary += ""
if($issues.Count -gt 0){
  $summary += "CRITICAL ISSUES:"
  $summary += $issues | ForEach-Object { " - $_" }
  $summary += ""
}
if($warnings.Count -gt 0){
  $summary += "WARNINGS:"
  $summary += $warnings | ForEach-Object { " - $_" }
  $summary += ""
}
if($notes.Count -gt 0){
  $summary += "NOTES:"
  $summary += $notes | ForEach-Object { " - $_" }
  $summary += ""
}

$summary -join "`r`n" | Tee-Object -FilePath $reportPath | Out-Host

if($issues.Count -gt 0){ exit 1 } else { exit 0 }
.FullName -ne $contractFs }
  Where-Object { $_.FullName -ne $contractFs }

foreach($dup in $allDialoguePrefabs){
  # Ignore obvious caches/Library folders
  if($dup.FullName -match "\\Library\\|\\Temp\\") { continue }
  $rel = ($dup.FullName.Substring($root.Length)).TrimStart('\','/')
  $warnings += "DUPLICATE: Found DialoguePanel prefab at $rel (outside contract path)"
}

# 3) .off files or quarantine under Assets/Resources
$resourcesDir = Join-Path $root "Assets/Resources"
if(Test-Path $resourcesDir){
  $offFiles = Get-ChildItem -Path $resourcesDir -Recurse -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Extension -eq ".off" }
  foreach($f in $offFiles){
    $rel = ($f.FullName.Substring($root.Length)).TrimStart('\','/')
    $warnings += "QUARANTINE-LEAK: '.off' file found under Resources → $rel"
  }

  $quarantineDirs = Get-ChildItem -Path $resourcesDir -Recurse -Directory -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -like "Z._quarantine*" }
  foreach($d in $quarantineDirs){
    $rel = ($d.FullName.Substring($root.Length)).TrimStart('\','/')
    $warnings += "QUARANTINE-LEAK: quarantine folder under Resources → $rel"
  }
}

# 4) Duplicate asmdef names
$asmdefs = Get-ChildItem -Path $root -Recurse -Filter "*.asmdef" -File -ErrorAction SilentlyContinue |
  Where-Object { $_.FullName -notmatch "\\Library\\|\\Temp\\|\\Packages\\(?!com\.aq\.)" }

$asmNameMap = @{}
foreach($asm in $asmdefs){
  try{
    $json = Get-Content $asm.FullName -Raw | ConvertFrom-Json
    $name = $json.name
    if([string]::IsNullOrWhiteSpace($name)){ continue }
    if(-not $asmNameMap.ContainsKey($name)){ $asmNameMap[$name] = @() }
    $asmNameMap[$name] += $asm
  } catch {
    $warnings += "ASMDEF: Failed to parse JSON → $($asm.FullName)"
  }
}

$dupAsm = $asmNameMap.GetEnumerator() | Where-Object { $_.Value.Count -gt 1 }
foreach($kv in $dupAsm){
  $paths = $kv.Value | ForEach-Object { $_.FullName.Substring($root.Length).TrimStart('\','/') }
  $issues += "ASMDEF-DUPLICATE: name '$($kv.Key)' appears in multiple files:`n  - " + ($paths -join "`n  - ")
}

# 5) Summarize & write report
$summary = @()
$summary += "Ally Quinn: Step 0 Audit — $(Get-Date)"
$summary += "Repo: $root"
$summary += ""
if($issues.Count -gt 0){
  $summary += "CRITICAL ISSUES:"
  $summary += $issues | ForEach-Object { " - $_" }
  $summary += ""
}
if($warnings.Count -gt 0){
  $summary += "WARNINGS:"
  $summary += $warnings | ForEach-Object { " - $_" }
  $summary += ""
}
if($notes.Count -gt 0){
  $summary += "NOTES:"
  $summary += $notes | ForEach-Object { " - $_" }
  $summary += ""
}

$summary -join "`r`n" | Tee-Object -FilePath $reportPath | Out-Host

if($issues.Count -gt 0){ exit 1 } else { exit 0 }

