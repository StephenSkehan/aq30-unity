param(
  [ValidateSet("EditMode","PlayMode")] [string]$Platform = "EditMode",
  [string]$Filter = "",
  [string]$ResultsDir = ".\_testresults"
)
$ErrorActionPreference='Stop'

# Load Unity locator (defines Get-UnityExe)
. (Join-Path (Get-Location).Path "Tools\Get-UnityExe.ps1")

if(-not (Test-Path $ResultsDir)){ New-Item -ItemType Directory -Path $ResultsDir | Out-Null }
$stamp = Get-Date -Format "yyyyMMdd_HHmmss"
$xml   = Join-Path $ResultsDir ("results_{0}_{1}.xml" -f $Platform,$stamp)
$log   = Join-Path $ResultsDir ("unity_{0}_{1}.log"   -f $Platform,$stamp)

$args = @(
  "-batchmode","-nographics",
  "-projectPath",(Get-Location).Path,
  "-runTests",
  "-testPlatform",$Platform,           # NOTE: Exact casing required
  "-testResults",$xml,
  "-logFile",$log,
  "-quit"
)
if($Filter -and $Filter.Trim().Length -gt 0){ $args += @("-testFilter",$Filter) }

$u = Get-UnityExe
& $u $args
$exit = $LASTEXITCODE

# Friendly reporting: don’t Resolve-Path unless the file exists
if(Test-Path $xml){
  Write-Host ("Test XML : {0}" -f (Resolve-Path $xml))
} else {
  Write-Warning "Unity did not produce a results XML at: $xml"
  if(Test-Path $log){
    Write-Host ("Unity log: {0}" -f (Resolve-Path $log))
    Write-Host "`n--- Tail (last 80 lines) of Unity log ---"
    Get-Content -LiteralPath $log -Tail 80 | Write-Host
  } else {
    Write-Warning "Unity log missing: $log"
  }
  Write-Error "No test results produced. Common causes: wrong -testPlatform casing, no tests discovered for this platform, or early Editor compilation error."
}
exit $exit
