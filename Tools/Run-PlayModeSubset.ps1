[CmdletBinding()]
param(
  [string]$Filter = "MergeBoardInteractionTests,ThemeSmokeTests",
  [ValidateSet("PlayMode","EditMode")][string]$Platform = "PlayMode",
  [string]$UnityExe,                               # Optional explicit path
  [string]$ProjectPath = (Get-Location).Path,
  [string]$ResultsPath = $(Join-Path (Get-Location).Path ("TestResults_{0}_" -f $Platform) + (Get-Date -Format "yyyyMMdd_HHmmss") + ".xml"),
  [switch]$NoGraphics
)

$ErrorActionPreference = 'Stop'

function Get-UnityExe {
  param([string]$Hint)
  if ($Hint -and (Test-Path $Hint)) { return $Hint }
  $preferred = 'C:\Program Files\Unity\Hub\Editor\6000.2.2f1\Editor\Unity.exe'
  if (Test-Path $preferred) { return $preferred }
  $hub = 'C:\Program Files\Unity\Hub\Editor'
  if (-not (Test-Path $hub)) { throw "Unity Hub Editor path not found: $hub (and no -UnityExe provided)" }
  foreach($v in (Get-ChildItem $hub -Directory | Sort-Object Name -Descending)) {
    $cand = Join-Path $v.FullName 'Editor\Unity.exe'
    if (Test-Path $cand) { return $cand }
  }
  throw "Unity.exe not found under $hub and no -UnityExe provided."
}

$unity = Get-UnityExe -Hint $UnityExe

# Build args
$args = @(
  '-batchmode',
  '-projectPath', $ProjectPath,
  '-runTests',
  '-testPlatform', $Platform,
  '-testFilter', $Filter,
  '-logFile', '-' ,
  '-testResults', $ResultsPath
)
if ($NoGraphics) { $args = @('-nographics') + $args }

Write-Host "Unity :" $unity
Write-Host "Filter:" $Filter
Write-Host "Platf :" $Platform
Write-Host "Results ->" $ResultsPath

$proc = Start-Process -FilePath $unity -ArgumentList $args -NoNewWindow -PassThru -Wait
if ($proc.ExitCode -ne 0) {
  Write-Warning "Unity exited with code $($proc.ExitCode). See results/log for details."
  exit $proc.ExitCode
}
Write-Host "Done. Results: $ResultsPath"
