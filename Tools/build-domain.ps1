param([string]\ = "C:\Users\Steph\Dev\aq30-unity")
Set-Location "C:\Users\Steph\Dev\aq30-unity\DotNetBuild"
dotnet --version
Write-Host '--- Building AQ.SharedKernel (.NET) ---'
dotnet build .\AQ.SharedKernel.csproj -c Release
if (\ -ne 0) { exit \ }
Write-Host '--- Building AQ.Domain.Merge (.NET) ---'
dotnet build .\AQ.Domain.Merge.csproj -c Release
exit \
