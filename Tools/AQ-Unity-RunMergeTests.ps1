#requires -Version 7.2
<#
 File: Tools\AQ-Unity-RunMergeTests.ps1
 Purpose: Run Unity EditMode tests (or at least compile) headlessly and capture results/logs.
 Usage:
   pwsh Tools\AQ-Unity-RunMergeTests.ps1
#>
[CmdletBinding()]
param(
  [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.2.2f1\Editor\Unity.exe",
  [string]$ProjectPath = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Info($m){ Write-Host "[INFO] $m" }
function Pass($m){ Write-Host "[PASS] $m" -ForegroundColor Green }
function Warn($m){ Write-Host "[WARN] $m" -ForegroundColor Yellow }
function Fail($m){ Write-Host "[FAIL] $m" -ForegroundColor Red }

try{
  if(-not (Test-Path -LiteralPath $UnityExe)){ throw "Unity not found: $UnityExe" }
  if(-not (Test-Path -LiteralPath $ProjectPath)){ throw "Project not found: $ProjectPath" }

  $logs = Join-Path $ProjectPath '_logs'
  New-Item -ItemType Directory -Force -Path $logs | Out-Null
  $stamp = Get-Date -Format 'yyyyMMdd_HHmmss'
  $logFile = Join-Path $logs ("unity_editmode_{0}.log" -f $stamp)
  $trxFile = Join-Path $logs ("unity_editmode_{0}.xml" -f $stamp)

  Info "Unity: $UnityExe"
  Info "Proj : $ProjectPath"
  Info "Log : $logFile"
  Info "TRX : $trxFile"

  $args = @(
    '-batchmode','-nographics',
    '-projectPath',"`"$ProjectPath`"",
    '-runTests',
    '-testPlatform','EditMode',
    '-testResults',"`"$trxFile`"",
    '-logFile',"`"$logFile`"",
    '-quit'
  )

  Info "Launching Unity test runner..."
  & $UnityExe $args
  $code = $LASTEXITCODE

  $tail = if(Test-Path -LiteralPath $logFile){ Get-Content -LiteralPath $logFile -Tail 200 -ErrorAction SilentlyContinue } else { @() }
  $summary = ($tail | Where-Object { $_ -match 'Passed:' -or $_ -match 'Failed:' -or $_ -match 'tests completed' })

  if($summary){ $summary | ForEach-Object { Write-Host $_ } }
  else { Warn "No explicit test summary found; possibly zero tests. See log for details." }

  if($code -ne 0){
    throw "Unity exited with code $code. Inspect log: $logFile"
  }

  Pass "Unity test/compile pass completed."
  Pass ("Log: {0}" -f $logFile)
  if(Test-Path -LiteralPath $trxFile){ Pass ("XML: {0}" -f $trxFile) }
}
catch{
  Fail $_
  exit 1
}
