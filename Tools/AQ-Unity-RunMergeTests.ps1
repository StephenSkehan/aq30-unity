#requires -Version 7.2
<#
 File: Tools\AQ-Unity-RunMergeTests.ps1
 Purpose: Run Unity EditMode/PlayMode tests headlessly, write log + results, and
          produce a concise, reliable summary (no $LASTEXITCODE dependence).
 Usage:
   pwsh Tools\AQ-Unity-RunMergeTests.ps1
   pwsh Tools\AQ-Unity-RunMergeTests.ps1 -TestPlatform PlayMode
   pwsh Tools\AQ-Unity-RunMergeTests.ps1 -UnityExe "C:\Path\To\Unity.exe"
 Notes:
   - Auto-detects the latest Hub editor if -UnityExe is not supplied.
   - Results are NUnit XML; we summarize totals and first failures.
#>

[CmdletBinding()]
param(
  # Optional explicit path to Unity.exe
  [string]$UnityExe,

  # Project path (defaults to repo root)
  [string]$Project,

  # EditMode or PlayMode
  [ValidateSet('EditMode','PlayMode')]
  [string]$TestPlatform = 'EditMode',

  # If set, fail the run when no tests are discovered
  [switch]$Strict
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Info([string]$m){ Write-Host "[INFO] $m" }
function Pass([string]$m){ Write-Host "[PASS] $m" -ForegroundColor Green }
function Warn([string]$m){ Write-Host "[WARN] $m" -ForegroundColor Yellow }
function Fail([string]$m){ Write-Host "[FAIL] $m" -ForegroundColor Red }

function Get-RepoRoot {
  # Assume this script lives under <repo>\Tools\
  $root = Resolve-Path (Join-Path $PSScriptRoot '..')
  return $root.Path
}

function Resolve-UnityExe([string]$hint){
  if($hint){
    if(Test-Path -LiteralPath $hint){ return (Resolve-Path -LiteralPath $hint).Path }
    throw "Unity not found at: $hint"
  }

  if($env:UNITY_EDITOR_PATH -and (Test-Path -LiteralPath $env:UNITY_EDITOR_PATH)){
    return (Resolve-Path -LiteralPath $env:UNITY_EDITOR_PATH).Path
  }

  # Try Unity Hub default installations (pick the highest version folder)
  $hubRoot = 'C:\Program Files\Unity\Hub\Editor'
  if(Test-Path -LiteralPath $hubRoot){
    $cands = Get-ChildItem -LiteralPath $hubRoot -Directory -ErrorAction SilentlyContinue |
             ForEach-Object {
               $exe = Join-Path $_.FullName 'Editor\Unity.exe'
               if(Test-Path -LiteralPath $exe){
                 $ver = [version]'0.0.0.0'
                 try { [void][version]::TryParse($_.Name, [ref]$ver) } catch {}
                 [pscustomobject]@{
                   Path = $exe
                   Ver  = $ver
                 }
               }
             } | Sort-Object Ver
    if($cands){ return $cands[-1].Path }
  }

  # Fallback to PATH
  try { return (Get-Command Unity.exe -ErrorAction Stop).Source } catch {}
  throw "Unity Editor not found. Supply -UnityExe or set UNITY_EDITOR_PATH."
}

# ------------------------------------------------------------------------------

$repo = Get-RepoRoot
if(-not $Project){ $Project = $repo }

$unity = Resolve-UnityExe -hint:$UnityExe

$logsDir = Join-Path $repo '_logs'
New-Item -ItemType Directory -Force -Path $logsDir | Out-Null

$stamp     = Get-Date -Format 'yyyyMMdd_HHmmss'
$platLower = $TestPlatform.ToLowerInvariant()
$logPath   = Join-Path $logsDir ("unity_{0}_{1}.log" -f $platLower, $stamp)
$xmlPath   = Join-Path $logsDir ("unity_{0}_{1}.xml" -f $platLower, $stamp)

Info "Unity: $unity"
Info "Proj : $Project"
Info "Log : $logPath"
Info "XML : $xmlPath"
Info "Launching Unity test runner..."

# Build Unity arguments. Use an array so quoting is handled correctly.
$args = @(
  '-batchmode',
  '-nographics',
  '-quit',
  '-projectPath', $Project,
  '-runTests',
  '-testPlatform', $TestPlatform,
  '-logFile', $logPath,
  '-testResults', $xmlPath
)

# Run Unity and capture the true exit code (do NOT rely on $LASTEXITCODE).
$proc = Start-Process -FilePath $unity -ArgumentList $args -Wait -PassThru
$code = $proc.ExitCode

# ---- Summarize results -------------------------------------------------------
$summaryPrinted = $false
$total = $passed = $failed = $skipped = 0

if(Test-Path -LiteralPath $xmlPath){
  try {
    [xml]$xml = Get-Content -LiteralPath $xmlPath -Raw -Encoding UTF8

    # NUnit3 format: <test-run total=".." passed=".." failed=".." inconclusive=".." skipped="..">
    $tr = $xml.'test-run'
    if($tr){
      $total   = [int]$tr.total
      $passed  = [int]$tr.passed
      $failed  = [int]$tr.failed
      $skipped = [int]$tr.skipped

      if($failed -gt 0){
        Fail ("Results: total={0} passed={1} failed={2} skipped={3}" -f $total,$passed,$failed,$skipped)
      } else {
        Pass ("Results: total={0} passed={1} failed={2} skipped={3}" -f $total,$passed,$failed,$skipped)
      }

      # List first few failing test cases (if any)
      $failedCases = $xml.SelectNodes('//test-case[@result="Failed"]')
      if($failedCases -and $failedCases.Count -gt 0){
        $max = [Math]::Min(10, $failedCases.Count)
        Warn "First $max failing tests:"
        0..($max-1) | ForEach-Object {
          $n = $failedCases.Item($_)
          $name = $n.fullname
          $msg  = $n.failure.message.'#text'
          $msg  = if($msg){ ($msg -split "`r?`n")[0] } else { '(no message)' }
          Write-Host ("  - {0}`n      {1}" -f $name, $msg)
        }
        Write-Host ""
        Write-Host "See full log: $logPath"
        Write-Host "See full XML: $xmlPath"
      }

      $summaryPrinted = $true
    }
  } catch {
    Warn "Could not parse results XML: $($_.Exception.Message)"
  }
} else {
  Warn "No results XML produced at: $xmlPath"
}

# Handle no-tests case
if(($total -eq 0) -and $Strict){
  Fail "No tests discovered/executed."
  $code = 2
  $summaryPrinted = $true
}

if(-not $summaryPrinted){
  if($code -eq 0){
    Pass "Unity exited 0 but no results summary was produced. Check log: $logPath"
  } else {
    Fail "Unity exit code: $code. Check log: $logPath"
  }
}

# Return a clear process exit code for CI/automation.
if($failed -gt 0 -and $code -eq 0){ $code = 2 }
exit $code
