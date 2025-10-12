#requires -Version 7.2
<#
 File: Tools\AQ-Git-Commit-And-Push.ps1
 Purpose: Stage, commit and push with a provided message (explicit helper).
 Usage:
   pwsh Tools\AQ-Git-Commit-And-Push.ps1 -Message "Discovery: consolidate RecipeBook"
#>
[CmdletBinding()]
param(
  [Parameter(Mandatory=$true)][string]$Message,
  [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
function Info($m){ Write-Host "[INFO] $m" }
function Pass($m){ Write-Host "[PASS] $m" -ForegroundColor Green }
function Fail($m){ Write-Host "[FAIL] $m" -ForegroundColor Red }

Push-Location $RepoRoot
try{
  Info "Repo: $RepoRoot"
  & git add -A -- Assets Packages ProjectSettings Tools
  if($LASTEXITCODE -ne 0){ throw "git add failed" }
  & git commit -m $Message
  if($LASTEXITCODE -ne 0){ throw "git commit failed (nothing to commit?)" }
  & git push
  if($LASTEXITCODE -ne 0){ throw "git push failed" }
  Pass "Committed and pushed: $Message"
}
catch{
  Fail $_
  exit 1
}
finally{ Pop-Location }
