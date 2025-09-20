$ErrorActionPreference = "Stop"

# Resolve repo and DotNetBuild folder relative to this script
$repo      = Split-Path -Parent $PSScriptRoot
$dotnetDir = Join-Path $repo 'DotNetBuild'
Set-Location $dotnetDir

dotnet --version

Write-Host '--- Building AQ.SharedKernel (.NET) ---'
dotnet build .\AQ.SharedKernel.csproj -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host '--- Building AQ.Domain.Merge (.NET) ---'
dotnet build .\AQ.Domain.Merge.csproj -c Release
exit $LASTEXITCODE
