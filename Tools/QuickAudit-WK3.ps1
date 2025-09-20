# Tools/QuickAudit-WK3.ps1
# Purpose: Zero-guess situational awareness for WK3 handover (PS 5.1/7.x safe)
[CmdletBinding()]
param([string]$Root = ".")

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$stamp = Get-Date -Format "yyyyMMdd_HHmmss"
$out   = "_Audit/WK3_Handoff_$stamp"
New-Item -ItemType Directory -Force $out | Out-Null

function Get-CsFiles([string]$base){
  Get-ChildItem -Path $base -Recurse -Filter *.cs -File -ErrorAction SilentlyContinue
}
function GrepCs([string]$pattern){
  Get-CsFiles $Root | Select-String -Pattern $pattern
}

# 0) Environment snapshot
$envSnap = @(
  "PowerShell: $($PSVersionTable.PSVersion)"
  "OS:        $([System.Environment]::OSVersion.VersionString)"
  "Root:      $(Resolve-Path $Root)"
)
$envSnap | Out-File "$out/env.txt" -Encoding utf8

# 1) Unity + packages snapshot
if(Test-Path "ProjectSettings/ProjectVersion.txt"){
  Get-Content "ProjectSettings/ProjectVersion.txt" | Out-File "$out/unity_version.txt" -Encoding utf8
}
if(Test-Path "Packages/manifest.json"){
  Get-Content "Packages/manifest.json" | Out-File "$out/manifest.json" -Encoding utf8
}
if(Test-Path "Packages/packages-lock.json"){
  Get-Content "Packages/packages-lock.json" | Out-File "$out/packages-lock.json" -Encoding utf8
}

# 2) Asmdef inventory
Get-ChildItem -Path $Root -Recurse -Filter *.asmdef -File |
  ForEach-Object {
    "==== $($_.FullName) ===="; Get-Content $_.FullName
  } | Out-File "$out/asmdefs_dump.txt" -Encoding utf8

# 3) TMPro 'using' audit (Binder B hotspot)
$tmproRefs = Get-CsFiles $Root | Select-String -Pattern '\bTMP_Text\b|\bTextMeshPro(UGUI)?\b' |
             Select-Object -ExpandProperty Path -Unique
$rows = foreach($f in $tmproRefs){
  $hasUsing = Select-String -Path $f -Pattern '^\s*using\s+TMPro;' -Quiet
  [PSCustomObject]@{ UsingTMPro = ($hasUsing ? 'OK' : 'MISSING'); File = $f }
}
$rows | Sort-Object UsingTMPro, File | Export-Csv "$out/tmpro_using_audit.csv" -NoTypeInformation

# 4) Variant binders & runners present?
Get-ChildItem -Path $Root -Recurse -File -Include *Binder*.cs,*Variant*.cs |
  Select-Object FullName | Out-File "$out/variant_binders_inventory.txt" -Encoding utf8
GrepCs 'Binder\s*A\b|Binder\s*B\b|Binder\s*C\b|Verifier|Menu(Items|Runner)|ResolutionOverlay' |
  Sort-Object Path, LineNumber | Out-File "$out/variant_binders_refs.txt" -Encoding utf8

# 5) Analytics plumbing sanity
GrepCs 'class\s+ResolutionContinueMB|AnalyticsLocator|IAnalytics|economy_changed|economy_granted|resolution_continue' |
  Sort-Object Path, LineNumber | Out-File "$out/analytics_plumbing.txt" -Encoding utf8

# 6) Unity 6 API sweep
GrepCs '\bFindObject[s]?OfType\b|\bFindFirstObjectByType\b|\bFindObjectsByType\b|Addressables' |
  Sort-Object Path, LineNumber | Out-File "$out/api_calls_sweep.txt" -Encoding utf8

# 7) Test presence quicklook
$tests = Get-CsFiles $Root | Select-String -Pattern '\[(Unity)?Test\]' |
         Group-Object Path | ForEach-Object {
           [PSCustomObject]@{ File = $_.Name; TestCount = $_.Count }
         }
$tests | Sort-Object File | Export-Csv "$out/tests_count.csv" -NoTypeInformation

"PASS: WK3 quick audit outputs written to $out"
