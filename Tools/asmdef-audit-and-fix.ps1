<# 
asmdef-audit-and-fix.ps1  —  Audit-first, surgical reference fixer for Unity .asmdef files.

USAGE (audit only; no writes):
  pwsh -File .\Tools\asmdef-audit-and-fix.ps1 -RepoRoot . 

USAGE (plan a specific rename, show preview only):
  pwsh -File .\Tools\asmdef-audit-and-fix.ps1 -RepoRoot . -Map "AQ30.SharedKernel=AQ.SharedKernel"

USAGE (apply the planned rename once you’re satisfied):
  pwsh -File .\Tools\asmdef-audit-and-fix.ps1 -RepoRoot . -Map "AQ30.SharedKernel=AQ.SharedKernel" -Apply

Notes
- Never guesses: no default renames. You must explicitly supply -Map entries like "old=new".
- Safe by default: No file writes unless you add -Apply.
- Backups: any modified .asmdef gets a .prepatch_YYYYMMDD_HHMMSS.bak alongside it.
- GUID references: resolved against local *.asmdef.meta; we flag unresolved ones (no changes).
- Unity assemblies: names starting with "Unity." are treated as external/OK for audit purposes.
#>

[CmdletBinding()]
param(
  [Parameter()][string]$RepoRoot = ".",
  [Parameter()][string[]]$Map = @(),
  [Parameter()][switch]$Apply,
  [Parameter()][switch]$ResolveGuidRefs = $true
)

$ErrorActionPreference = "Stop"

function Log([string]$msg,[string]$color="Gray"){
  $c = [Console]::ForegroundColor
  try { [Console]::ForegroundColor = $color; Write-Host "[$(Get-Date -Format 'HH:mm:ss')] $msg" }
  finally { [Console]::ForegroundColor = $c }
}

function Ensure-Dir($p){ if(-not (Test-Path $p)){ New-Item -ItemType Directory -Path $p | Out-Null } }

# Resolve paths
$repo = (Resolve-Path -LiteralPath $RepoRoot).Path
Set-Location $repo
$auditDir = Join-Path $repo "_Audit"
Ensure-Dir $auditDir
$ts = Get-Date -Format "yyyyMMdd_HHmmss"

$report = Join-Path $auditDir ("_asmdef_audit_{0}.txt" -f $ts)
$plan   = Join-Path $auditDir ("_asmdef_fix_plan_{0}.txt" -f $ts)

Log "RepoRoot = $repo" "Cyan"

# --- Gather asmdefs ----------------------------------------------------------
$asmdefFiles = Get-ChildItem -Recurse -Filter *.asmdef -File -ErrorAction SilentlyContinue
if(-not $asmdefFiles){ throw "No .asmdef files found below $repo." }

# Build GUID -> name map from .asmdef.meta
$guidToName = @{}
$asmdefs = @()
foreach($f in $asmdefFiles){
  $meta = "$($f.FullName).meta"
  $guid = $null
  if(Test-Path $meta){
    $line = Select-String -Path $meta -Pattern 'guid:\s*([0-9a-fA-F]{32})' -SimpleMatch:$false -ErrorAction SilentlyContinue | Select-Object -First 1
    if($line){ $guid = ($line.Matches[0].Groups[1].Value).ToLowerInvariant() }
  }
  try{
    $json = Get-Content -LiteralPath $f.FullName -Raw | ConvertFrom-Json
  } catch {
    Log "WARN: Failed to parse JSON: $($f.FullName)" "Yellow"
    continue
  }
  $name = $json.name
  if($guid -and $name){ $guidToName[$guid] = $name }
  $asmdefs += [pscustomobject]@{
    Path  = $f.FullName
    Rel   = [IO.Path]::GetRelativePath($repo, $f.FullName)
    Name  = $name
    Json  = $json
  }
}

$localNames = $asmdefs.Name | Where-Object { $_ } | Sort-Object -Unique

# --- Analyze references ------------------------------------------------------
$rows = New-Object System.Collections.Generic.List[object]
foreach($a in $asmdefs){
  $refs = @()
  if($a.Json.PSObject.Properties.Name -contains 'references'){
    $refs = @($a.Json.references)
  }
  foreach($r in $refs){
    $refStr = [string]$r
    $kind   = "name"
    $target = $refStr
    $status = "unknown"

    if($ResolveGuidRefs -and $refStr -match '^GUID:'){
      $kind = "guid"
      $guid = ($refStr -replace '^GUID:\s*','').ToLowerInvariant()
      if($guidToName.ContainsKey($guid)){
        $target = $guidToName[$guid]
        $status = "guid-resolved-local"
      } else {
        $status = "guid-missing"
      }
    } else {
      if($localNames -contains $refStr){
        $status = "local-match"
      } elseif($refStr -like 'Unity.*'){
        $status = "unity-external-ok"
      } else {
        $status = "missing-or-external"
      }
    }

    $rows.Add([pscustomobject]@{
      FromAsmdef = $a.Name
      FromPath   = $a.Rel
      RefRaw     = $refStr
      RefKind    = $kind
      TargetName = $target
      Status     = $status
    })
  }
}

# --- Produce audit report ----------------------------------------------------
"ASMDEF AUDIT  ($ts)
Repo: $repo

Local assemblies ({0}):
  - {1}

Findings:
" -f $localNames.Count, ($localNames -join "`n  - ") | Set-Content -LiteralPath $report -Encoding UTF8

# Summaries
$summary = $rows | Group-Object Status | Sort-Object Count -Descending | ForEach-Object { "{0,5}  {1}" -f $_.Count, $_.Name }
"`nReference status summary:`n$($summary -join "`n")`n" | Add-Content -LiteralPath $report -Encoding UTF8

$missing = $rows | Where-Object { $_.Status -eq 'missing-or-external' } | Sort-Object FromAsmdef, RefRaw -Unique
if($missing){
  "Missing/External (not local & not Unity.*):" | Add-Content $report
  $missing | ForEach-Object { "  * {0}  ->  {1}" -f $_.FromAsmdef, $_.RefRaw } | Add-Content $report
  "" | Add-Content $report
} else {
  "Missing/External: (none)" | Add-Content $report
  "" | Add-Content $report
}

$guidMissing = $rows | Where-Object { $_.Status -eq 'guid-missing' }
if($guidMissing){
  "Unresolved GUID references (no local .asmdef.meta match):" | Add-Content $report
  $guidMissing | ForEach-Object { "  * {0}  ->  {1}" -f $_.FromAsmdef, $_.RefRaw } | Add-Content $report
  "" | Add-Content $report
}

# --- Build plan from -Map (no changes yet) ----------------------------------
$mapDict = @{}
foreach($m in $Map){
  if($m -notmatch '='){ throw "Bad -Map entry '$m'. Use OLD=NEW." }
  $old,$new = $m.Split('=',2)
  $old = $old.Trim()
  $new = $new.Trim()
  if(-not $old -or -not $new){ throw "Bad -Map entry '$m'." }
  $mapDict[$old] = $new
}

$planItems = New-Object System.Collections.Generic.List[object]
if($mapDict.Count -gt 0){
  foreach($a in $asmdefs){
    $refs = @()
    if($a.Json.PSObject.Properties.Name -contains 'references'){
      $refs = @($a.Json.references)
    }
    $i = -1
    foreach($r in $refs){
      $i++
      $refName = [string]$r
      if($refName -like 'GUID:*'){ continue } # we only map name->name
      if($mapDict.ContainsKey($refName)){
        $proposed = $mapDict[$refName]
        $exists = $localNames -contains $proposed
        $planItems.Add([pscustomobject]@{
          File       = $a.Rel
          Assembly   = $a.Name
          Index      = $i
          Old        = $refName
          New        = $proposed
          NewExists  = $exists
        })
      }
    }
  }
}

# write plan preview
"FIX PLAN PREVIEW (no files changed yet)
Mappings: $(
  if($mapDict.Count){ ($mapDict.GetEnumerator() | ForEach-Object { "  - {0} => {1}" -f $_.Key,$_.Value }) -join "`n" } else { "(none)" }
)

Planned edits:" | Set-Content -LiteralPath $plan -Encoding UTF8

if($planItems.Count -eq 0){
  "  (no references matched your -Map set)" | Add-Content -LiteralPath $plan -Encoding UTF8
} else {
  foreach($p in $planItems){
    "{0}`n  - {1}`n  - refs[{2}]: {3}  ->  {4}  (target present: {5})`n" -f $p.File,$p.Assembly,$p.Index,$p.Old,$p.New,$p.NewExists | Add-Content $plan
  }
}

# --- Apply if requested (only if all targets exist locally) ------------------
if($Apply){
  if($planItems.Count -eq 0){
    Log "No applicable plan items. Nothing to apply." "Yellow"
  } else {
    # Fail fast if any planned NEW target doesn't exist (no guesses)
    $badTargets = $planItems | Where-Object { -not $_.NewExists } | Select-Object -ExpandProperty New -Unique
    if($badTargets){
      Log "Refusing to apply: some mapped NEW names do not exist locally:" "Red"
      $badTargets | ForEach-Object { Log "  - $_" "Red" }
      Log "Audit-first rule: create/confirm those assemblies first, or adjust the mapping." "Red"
      exit 2
    }

    # Group by file and rewrite
    $byFile = $planItems | Group-Object File
    foreach($g in $byFile){
      $fileRel = $g.Name
      $fileAbs = Join-Path $repo $fileRel
      $json = Get-Content -LiteralPath $fileAbs -Raw | ConvertFrom-Json

      # Map edits by index
      for($k=0; $k -lt $json.references.Count; $k++){
        $match = $g.Group | Where-Object { $_.Index -eq $k } | Select-Object -First 1
        if($match){
          $json.references[$k] = $match.New
        }
      }

      $bak = "$fileAbs.prepatch_$ts.bak"
      Copy-Item -LiteralPath $fileAbs -Destination $bak -Force
      $out = $json | ConvertTo-Json -Depth 50
      $out | Set-Content -LiteralPath $fileAbs -Encoding UTF8 -NoNewline
      Log "Patched: $fileRel  (backup: $(Split-Path -Leaf $bak))" "Green"
    }
    Log "Apply complete. Re-run your test suite and the general project audit." "Green"
  }
} else {
  Log "No changes applied (audit-only mode)." "Yellow"
}

Log "Wrote audit:   $report" "Cyan"
Log "Wrote plan:    $plan"   "Cyan"