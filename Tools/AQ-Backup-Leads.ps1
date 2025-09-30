# PowerShell 7.x, UTF-8, CRLF
Set-StrictMode -Version Latest
$stamp = Get-Date -Format "yyyyMMdd_HHmmss"
$src = "Assets\App\Leads"
$dst = "Backups\Leads_$stamp"
New-Item -ItemType Directory -Force -Path $dst | Out-Null
Copy-Item $src $dst -Recurse -Force
Write-Host "PASS: Leads folder backed up to $dst"
