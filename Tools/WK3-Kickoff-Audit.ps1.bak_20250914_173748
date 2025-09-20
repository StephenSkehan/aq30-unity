<#  Tools/WK3-Kickoff-Audit.ps1  (WK3 patch 2025-09-14)
    - Finds _project_* artifacts recursively (handles _audit\…).
    - Canonical asmdef check now accepts Domain/Kernel under Packages/.
    - Null-safe list handling (no Count crash).
#>

[CmdletBinding()]
param(
  [string]$RepoRoot = (Resolve-Path ".").Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function New-Timestamp { (Get-Date).ToString("yyyyMMdd_HHmmss") }
$ts      = New-Timestamp
$OutDir  = Join-Path $RepoRoot "_Audit/WK3_Kickoff_$ts"
$CheckDir= Join-Path $OutDir "checks"
$null = New-Item -Force -ItemType Directory -Path $OutDir, $CheckDir | Out-Null

function W($t){ $t | Tee-Object -FilePath (Join-Path $OutDir "WK3_Summary.txt") -Append }

function Pass($n,$d){ $m="PASS: $n — $d"; $m | Set-Content (Join-Path $CheckDir "$($n)_PASS.txt"); W $m }
function Fail($n,$d){ $m="FAIL: $n — $d"; $m | Set-Content (Join-Path $CheckDir "$($n)_FAIL.txt"); W $m }
function Warn($n,$d){ $m="WARN: $n — $d"; $m | Set-Content (Join-Path $CheckDir "$($n)_WARN.txt"); W $m }

W "=== WK3 Kickoff Audit @ $((Get-Date).ToString('u')) ==="
W "RepoRoot: $RepoRoot"
W ""

# 0) Run authoritative project audit
$toolsAudit = Join-Path $RepoRoot "Tools\project-audit.ps1"
if(Test-Path $toolsAudit){
  try{
    & $toolsAudit | Tee-Object -FilePath (Join-Path $OutDir "project-audit.log")
  } catch {
    Warn "project-audit.ps1" "Invocation error: $($_.Exception.Message)"
  }

  # find latest artifacts *recursively* (your audit writes to _audit\…)
  $struct = Get-ChildItem -Path $RepoRoot -Filter "_project_structure_*.txt" -File -Recurse | Sort-Object LastWriteTime -Descending | Select-Object -First 1
  $finds  = Get-ChildItem -Path $RepoRoot -Filter "_project_findings_*.txt"  -File -Recurse | Sort-Object LastWriteTime -Descending | Select-Object -First 1
  $concat = Get-ChildItem -Path $RepoRoot -Filter "_project_sources_concat_*.txt" -File -Recurse | Sort-Object LastWriteTime -Descending | Select-Object -First 1

  foreach($f in @($struct,$finds,$concat)){ if($f){ Copy-Item $f.FullName -Destination $OutDir -Force } }
  if($struct -and $finds -and $concat){
    Pass "project-audit.ps1" "Captured structure/findings/sources_concat"
  } else {
    Warn "project-audit.ps1" "Audit ran but could not locate one or more *_project_* outputs (searched recursively)"
  }
} else {
  Fail "project-audit.ps1" "Missing Tools\project-audit.ps1"
}

# 1) ScriptAssemblies presence signal
$asmDir = Join-Path $RepoRoot "Library\ScriptAssemblies"
if(Test-Path $asmDir){
  $aqAsms = @( Get-ChildItem $asmDir -Filter "*AQ*" -Name | Sort-Object )
  $aqAsms | Set-Content (Join-Path $CheckDir "script_assemblies.txt")
  if($aqAsms.Count -ge 3){ Pass "ScriptAssemblies" ("Found: " + ($aqAsms -join ", ")) }
  else{ Warn "ScriptAssemblies" "Fewer than expected AQ assemblies; open Unity once to force import" }
} else { Warn "ScriptAssemblies" "No Library\ScriptAssemblies (fresh clone / cleaned Library?)" }

# 2) Collect asmdefs from Assets/ and Packages/ (domain/ker often reside in Packages)
$asmdefFiles = @()
$assetsAsm   = Join-Path $RepoRoot "Assets"
$packagesAsm = Join-Path $RepoRoot "Packages"
if(Test-Path $assetsAsm){   $asmdefFiles += Get-ChildItem -Path $assetsAsm   -Recurse -Filter *.asmdef -File }
if(Test-Path $packagesAsm){ $asmdefFiles += Get-ChildItem -Path $packagesAsm -Recurse -Filter *.asmdef -File }

$asmdefInfo = foreach($a in $asmdefFiles){
  try{
    $json = Get-Content $a.FullName -Raw | ConvertFrom-Json
    [PSCustomObject]@{ Name=$json.name; Path=$a.FullName }
  } catch {
    [PSCustomObject]@{ Name="(invalid json)"; Path=$a.FullName }
  }
}
$asmdefInfo | Sort-Object Name | Format-Table | Out-String | Set-Content (Join-Path $CheckDir "all_asmdefs.txt")

# Canonical presence (anywhere): App, Editor, Tests, Domain.Merge, SharedKernel
$expected = @('AQ.App','AQ.Editor','AQ.Tests','AQ.Domain.Merge','AQ.SharedKernel')
$names    = @($asmdefInfo.Name | Sort-Object -Unique)
$missing  = @($expected | Where-Object { $_ -notin $names })
$extras   = @() # names not in expected are fine; we don't fail on extras in sprint mode

if($missing.Count -eq 0){
  Pass "Asmdefs-Canonical" "All canonical assemblies present across Assets/ and Packages/"
} else {
  Fail "Asmdefs-Canonical" ("Missing: " + ($missing -join ", "))
}

# 3) Assembly-CSharp leak scan (null-safe)
# Project-only asmdef leak scan (patched: scope = Assets/ and Packages/com.aq.* only, JSON-based)
$acLeakCode = @()
$assetsPath   = Join-Path $RepoRoot "Assets"
$packagesPath = Join-Path $RepoRoot "Packages"
$projectAsmdefs = @()
if(Test-Path $assetsPath){   $projectAsmdefs += Get-ChildItem -Path $assetsPath   -Recurse -Filter *.asmdef -File }
if(Test-Path $packagesPath){ $projectAsmdefs += Get-ChildItem -Path $packagesPath -Recurse -Filter *.asmdef -File | Where-Object { <#  Tools/WK3-Kickoff-Audit.ps1  (WK3 patch 2025-09-14)
    - Finds _project_* artifacts recursively (handles _audit\…).
    - Canonical asmdef check now accepts Domain/Kernel under Packages/.
    - Null-safe list handling (no Count crash).
#>

[CmdletBinding()]
param(
  [string]$RepoRoot = (Resolve-Path ".").Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function New-Timestamp { (Get-Date).ToString("yyyyMMdd_HHmmss") }
$ts      = New-Timestamp
$OutDir  = Join-Path $RepoRoot "_Audit/WK3_Kickoff_$ts"
$CheckDir= Join-Path $OutDir "checks"
$null = New-Item -Force -ItemType Directory -Path $OutDir, $CheckDir | Out-Null

function W($t){ $t | Tee-Object -FilePath (Join-Path $OutDir "WK3_Summary.txt") -Append }

function Pass($n,$d){ $m="PASS: $n — $d"; $m | Set-Content (Join-Path $CheckDir "$($n)_PASS.txt"); W $m }
function Fail($n,$d){ $m="FAIL: $n — $d"; $m | Set-Content (Join-Path $CheckDir "$($n)_FAIL.txt"); W $m }
function Warn($n,$d){ $m="WARN: $n — $d"; $m | Set-Content (Join-Path $CheckDir "$($n)_WARN.txt"); W $m }

W "=== WK3 Kickoff Audit @ $((Get-Date).ToString('u')) ==="
W "RepoRoot: $RepoRoot"
W ""

# 0) Run authoritative project audit
$toolsAudit = Join-Path $RepoRoot "Tools\project-audit.ps1"
if(Test-Path $toolsAudit){
  try{
    & $toolsAudit | Tee-Object -FilePath (Join-Path $OutDir "project-audit.log")
  } catch {
    Warn "project-audit.ps1" "Invocation error: $($_.Exception.Message)"
  }

  # find latest artifacts *recursively* (your audit writes to _audit\…)
  $struct = Get-ChildItem -Path $RepoRoot -Filter "_project_structure_*.txt" -File -Recurse | Sort-Object LastWriteTime -Descending | Select-Object -First 1
  $finds  = Get-ChildItem -Path $RepoRoot -Filter "_project_findings_*.txt"  -File -Recurse | Sort-Object LastWriteTime -Descending | Select-Object -First 1
  $concat = Get-ChildItem -Path $RepoRoot -Filter "_project_sources_concat_*.txt" -File -Recurse | Sort-Object LastWriteTime -Descending | Select-Object -First 1

  foreach($f in @($struct,$finds,$concat)){ if($f){ Copy-Item $f.FullName -Destination $OutDir -Force } }
  if($struct -and $finds -and $concat){
    Pass "project-audit.ps1" "Captured structure/findings/sources_concat"
  } else {
    Warn "project-audit.ps1" "Audit ran but could not locate one or more *_project_* outputs (searched recursively)"
  }
} else {
  Fail "project-audit.ps1" "Missing Tools\project-audit.ps1"
}

# 1) ScriptAssemblies presence signal
$asmDir = Join-Path $RepoRoot "Library\ScriptAssemblies"
if(Test-Path $asmDir){
  $aqAsms = @( Get-ChildItem $asmDir -Filter "*AQ*" -Name | Sort-Object )
  $aqAsms | Set-Content (Join-Path $CheckDir "script_assemblies.txt")
  if($aqAsms.Count -ge 3){ Pass "ScriptAssemblies" ("Found: " + ($aqAsms -join ", ")) }
  else{ Warn "ScriptAssemblies" "Fewer than expected AQ assemblies; open Unity once to force import" }
} else { Warn "ScriptAssemblies" "No Library\ScriptAssemblies (fresh clone / cleaned Library?)" }

# 2) Collect asmdefs from Assets/ and Packages/ (domain/ker often reside in Packages)
$asmdefFiles = @()
$assetsAsm   = Join-Path $RepoRoot "Assets"
$packagesAsm = Join-Path $RepoRoot "Packages"
if(Test-Path $assetsAsm){   $asmdefFiles += Get-ChildItem -Path $assetsAsm   -Recurse -Filter *.asmdef -File }
if(Test-Path $packagesAsm){ $asmdefFiles += Get-ChildItem -Path $packagesAsm -Recurse -Filter *.asmdef -File }

$asmdefInfo = foreach($a in $asmdefFiles){
  try{
    $json = Get-Content $a.FullName -Raw | ConvertFrom-Json
    [PSCustomObject]@{ Name=$json.name; Path=$a.FullName }
  } catch {
    [PSCustomObject]@{ Name="(invalid json)"; Path=$a.FullName }
  }
}
$asmdefInfo | Sort-Object Name | Format-Table | Out-String | Set-Content (Join-Path $CheckDir "all_asmdefs.txt")

# Canonical presence (anywhere): App, Editor, Tests, Domain.Merge, SharedKernel
$expected = @('AQ.App','AQ.Editor','AQ.Tests','AQ.Domain.Merge','AQ.SharedKernel')
$names    = @($asmdefInfo.Name | Sort-Object -Unique)
$missing  = @($expected | Where-Object { $_ -notin $names })
$extras   = @() # names not in expected are fine; we don't fail on extras in sprint mode

if($missing.Count -eq 0){
  Pass "Asmdefs-Canonical" "All canonical assemblies present across Assets/ and Packages/"
} else {
  Fail "Asmdefs-Canonical" ("Missing: " + ($missing -join ", "))
}

# 3) Assembly-CSharp leak scan (null-safe)
$acLeakCode = @( Get-ChildItem -Path $RepoRoot -Recurse -Include *.cs,*.asmdef -File |
                 Select-String -SimpleMatch "Assembly-CSharp" -List | Select-Object -ExpandProperty Path -Unique )
$acLeakCode | Set-Content -Path (Join-Path $CheckDir "assembly_csharp_refs.txt")
if($acLeakCode.Count -gt 0){
  Fail "Assembly-CSharp-Leaks" "$($acLeakCode.Count) references found (see checks/assembly_csharp_refs.txt)"
} else {
  Pass "Assembly-CSharp-Leaks" "No references found"
}

# 4) Verify AQ.App.asmdef references Unity packages (TMP/UI/Addressables/InputSystem)
$appAsm = $asmdefInfo | Where-Object { $_.Name -eq 'AQ.App' } | Select-Object -First 1
if($appAsm){
  try{
    $appJson = Get-Content $appAsm.Path -Raw | ConvertFrom-Json
    $refs = @($appJson.references) + @($appJson.precompiledReferences) | Where-Object { $_ } | Sort-Object -Unique
    $need = @('Unity.TextMeshPro','UnityEngine.UI','Unity.Addressables','Unity.ResourceManager','Unity.InputSystem')
    $missingRefs = @($need | Where-Object { $_ -notin $refs })
    ("Found: " + ($refs -join ", ") + "`nMissing: " + ($missingRefs -join ", ")) |
      Set-Content -Path (Join-Path $CheckDir "AQ.App_references.txt")
    if($missingRefs.Count -eq 0){ Pass "AQ.App-UnityRefs" "App asmdef contains required Unity package references" }
    else{ Fail "AQ.App-UnityRefs" ("Missing: " + ($missingRefs -join ", ")) }
  } catch {
    Warn "AQ.App-UnityRefs" "Parse failed: $($_.Exception.Message)"
  }
} else { Fail "AQ.App-UnityRefs" "AQ.App.asmdef not found" }

# 5) Deprecated API probe (null-safe)
$deprHits = @( Get-ChildItem -Path (Join-Path $RepoRoot "Assets") -Recurse -Include *.cs -File |
               Select-String -Pattern 'FindObjectOfType<' -SimpleMatch )
$deprHits | ForEach-Object { $_.Path } | Sort-Object -Unique |
  Set-Content -Path (Join-Path $CheckDir "deprecated_api_hits.txt")
if($deprHits.Count -gt 0){
  Warn "Deprecated-API" "Found legacy FindObjectOfType usage (see checks/deprecated_api_hits.txt)"
} else {
  Pass "Deprecated-API" "No legacy FindObjectOfType usage detected"
}

W ""
W "WK3 Kickoff Audit complete. Review $OutDir for details."
.FullName -match "\\com\.aq\." } }
foreach($asmdef in $projectAsmdefs){
  try{
    $json = Get-Content $asmdef.FullName -Raw | ConvertFrom-Json
    if($json.references -contains "Assembly-CSharp"){
      $acLeakCode += ($asmdef.FullName.Substring($RepoRoot.Length).TrimStart('\','/'))
    }
  } catch {
    Write-Host "WARN: Failed to parse $($asmdef.FullName)"
  }
}
$acLeakCode | Set-Content -Path (Join-Path $CheckDir "assembly_csharp_refs.txt")
if($acLeakCode.Count -gt 0){
  Fail "Assembly-CSharp-Leaks" "$($acLeakCode.Count) references found (see checks/assembly_csharp_refs.txt)"
} else {
  Pass "Assembly-CSharp-Leaks" "No references found"
}

# 4) Verify AQ.App.asmdef references Unity packages (TMP/UI/Addressables/InputSystem)
$appAsm = $asmdefInfo | Where-Object { $_.Name -eq 'AQ.App' } | Select-Object -First 1
if($appAsm){
  try{
    $appJson = Get-Content $appAsm.Path -Raw | ConvertFrom-Json
    $refs = @($appJson.references) + @($appJson.precompiledReferences) | Where-Object { $_ } | Sort-Object -Unique
    $need = @('Unity.TextMeshPro','UnityEngine.UI','Unity.Addressables','Unity.ResourceManager','Unity.InputSystem')
    $missingRefs = @($need | Where-Object { $_ -notin $refs })
    ("Found: " + ($refs -join ", ") + "`nMissing: " + ($missingRefs -join ", ")) |
      Set-Content -Path (Join-Path $CheckDir "AQ.App_references.txt")
    if($missingRefs.Count -eq 0){ Pass "AQ.App-UnityRefs" "App asmdef contains required Unity package references" }
    else{ Fail "AQ.App-UnityRefs" ("Missing: " + ($missingRefs -join ", ")) }
  } catch {
    Warn "AQ.App-UnityRefs" "Parse failed: $($_.Exception.Message)"
  }
} else { Fail "AQ.App-UnityRefs" "AQ.App.asmdef not found" }

# 5) Deprecated API probe (null-safe)
$deprHits = @( Get-ChildItem -Path (Join-Path $RepoRoot "Assets") -Recurse -Include *.cs -File |
               Select-String -Pattern 'FindObjectOfType<' -SimpleMatch )
$deprHits | ForEach-Object { $_.Path } | Sort-Object -Unique |
  Set-Content -Path (Join-Path $CheckDir "deprecated_api_hits.txt")
if($deprHits.Count -gt 0){
  Warn "Deprecated-API" "Found legacy FindObjectOfType usage (see checks/deprecated_api_hits.txt)"
} else {
  Pass "Deprecated-API" "No legacy FindObjectOfType usage detected"
}

W ""
W "WK3 Kickoff Audit complete. Review $OutDir for details."

