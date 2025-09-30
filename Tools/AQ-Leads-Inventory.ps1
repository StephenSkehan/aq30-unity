# PowerShell 7.x, UTF-8, CRLF
Set-StrictMode -Version Latest
$root = "Assets\App\Leads\Data"
if (-not (Test-Path $root)) { Write-Error "Not found: $root"; exit 1 }
Get-ChildItem -Path $root -Filter "Lead_*.asset" -Recurse |
  Select-Object FullName, Length |
  Format-Table -Auto
