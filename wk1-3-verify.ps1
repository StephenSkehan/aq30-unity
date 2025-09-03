param(
  [Parameter(Mandatory=$true)][string]$ProjectPath,
  [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.2.2f1\Editor\Unity.exe",
  [switch]$RunHeadless
)

$ErrorActionPreference = 'Stop'
function Ok($m){ Write-Host "[ OK ] $m" -ForegroundColor Green }
function Info($m){ Write-Host "[INFO] $m" -ForegroundColor Cyan }
function Warn($m){ Write-Host "[WARN] $m" -ForegroundColor Yellow }
function Fail($m){ Write-Host "[FAIL] $m" -ForegroundColor Red }

# --- Resolve paths
try{ $root = (Resolve-Path $ProjectPath).Path } catch { Fail "Bad -ProjectPath: $ProjectPath"; exit 1 }
Set-Location $root

# --- 0) Git sanity
Info "Git sanity…"
try { $null = git --version 2>$null } catch { Fail "Git not on PATH"; exit 1 }
$gitDir = (& git -C $root rev-parse --git-dir) 2>$null
if($LASTEXITCODE -ne 0){ Fail "This folder is not a git repo: $root"; exit 1 }
$headShort = (& git -C $root rev-parse --short HEAD).Trim()
$top = (& git -C $root log -1 --oneline --decorate).Trim()
Write-Host "    HEAD = $headShort"
Write-Host "    TOP  = $top"
if($headShort -eq '0b51f941'){ Ok "At WK1-3 baseline commit (develop)." } else { Warn "HEAD != 0b51f941 (WK1-3). If intentional, ignore; else: git switch develop" }

# --- 1) Packages present (WK1-3 has SharedKernel only)
$sk = Join-Path $root 'Packages\com.aq.sharedkernel'
$dm = Join-Path $root 'Packages\com.aq.domain.merge'
if(Test-Path $sk){ Ok "SharedKernel present: $sk" } else { Fail "SharedKernel missing: $sk"; exit 1 }
if(Test-Path $dm){ Warn "Domain.Merge present (unexpected for WK1-3)" } else { Ok "Domain.Merge absent (expected for WK1-3)" }

# --- 2) Quick file listing + robust type presence scan (handles record/readonly/generics)
$skRuntime = Join-Path $sk 'Runtime'
if(!(Test-Path $skRuntime)){ Fail "SharedKernel Runtime folder missing: $skRuntime"; exit 1 }
Info "Listing key files under SharedKernel/Runtime (first 20)…"
Get-ChildItem $skRuntime -Recurse -Filter *.cs -File | Select-Object -First 20 | ForEach-Object { "  - " + $_.FullName }

function Test-TypeDecl {
  param([string]$Root,[string]$TypeName)
  $rx = [regex]::new('(?m)^\s*(public|internal)\s+(?:readonly\s+)?(?:partial\s+)?(?:record\s+struct|record|struct|class)\s+' + [regex]::Escape($TypeName) + '(?:\s*<|[\s:{])')
  foreach($f in (Get-ChildItem $Root -Recurse -Filter *.cs -File)){
    try{
      $t = Get-Content $f.FullName -Raw
      if($rx.IsMatch($t)){ return $true }
    }catch{}
  }
  return $false
}

$need = @('Result','DeterministicRandom','FixedTimeProvider')
foreach($t in $need){
  if(Test-TypeDecl -Root $skRuntime -TypeName $t){ Ok "Type present: $t" } else { Warn "Could not confirm type by scan: $t (may still exist under a generic or different tokenization)" }
}

# --- 3) Ensure tests are NOT disabled (rename *.asmdef.disabled back)
Info "Re-enabling any accidentally disabled test asmdefs…"
$disabled = Get-ChildItem $sk -Recurse -Filter *.asmdef.disabled -File -ErrorAction SilentlyContinue
if($disabled){
  foreach($d in $disabled){
    $new = $d.FullName -replace '\.disabled$',''
    Rename-Item -LiteralPath $d.FullName -NewName (Split-Path $new -Leaf) -Force
    Ok "Restored: $new"
  }
}else{
  Ok "No *.asmdef.disabled found under SharedKernel."
}

# --- 4) Verify Unity Test Framework in manifest
$manifest = Join-Path $root 'Packages\manifest.json'
if(!(Test-Path $manifest)){ Fail "Missing Packages/manifest.json"; exit 1 }
$j = Get-Content $manifest -Raw | ConvertFrom-Json
$depNames = $j.dependencies.PSObject.Properties.Name
if($depNames -contains 'com.unity.test-framework'){
  $ver = $j.dependencies.'com.unity.test-framework'
  Ok "Unity Test Framework present ($ver)."
}else{
  Fail "Unity Test Framework NOT listed in manifest. Add it in Package Manager (e.g., 1.4.x), then re-run."
  if(-not $RunHeadless){ exit 1 }
}

# --- 5) Optional: run EditMode tests headless
if($RunHeadless){
  if(!(Test-Path $UnityExe)){ Fail "Unity.exe not found: $UnityExe"; exit 1 }
  $diag = Join-Path $root 'Diagnostics'; New-Item -ItemType Directory -Force -Path $diag | Out-Null
  $results = Join-Path $diag 'wk1-3-testresults.xml'
  $log     = Join-Path $diag 'wk1-3-editor.log'
  if(Test-Path $results){ Remove-Item $results -Force }
  Info "Running EditMode tests headless… (Unity may take a minute to start)"
  & $UnityExe -projectPath $root -batchmode -nographics -runTests -testPlatform editmode -testResults $results -logFile $log -quit
  $code = $LASTEXITCODE
  if(Test-Path $results){
    $xmlText = Get-Content $results -Raw
    $total  = ([regex]::Matches($xmlText,'<test-case\b').Count)
    $failed = ([regex]::Matches($xmlText,'<test-case\b[^>]*result="Failed"').Count)
    if($failed -eq 0){ Ok ("EditMode tests passed ({0} total, 0 failed). → {1}" -f $total,$results) }
    else{ Fail ("EditMode tests had failures ({0}/{1}). See: {2}`nLog: {3}" -f $failed,$total,$results,$log) }
  } else {
    Fail ("No test results produced. Check log: {0}" -f $log)
  }
  if($code -ne 0){ Warn ("Unity exited with code {0} (may still have produced results)." -f $code) }
}

Ok "WK1-3 verification finished."
