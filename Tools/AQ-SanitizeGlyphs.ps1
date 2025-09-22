#Requires -Version 7
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root   = (Get-Location).Path
$targets = @('Assets','Packages') | ForEach-Object { Join-Path $root $_ }
$exts   = '*.cs','*.asset','*.prefab','*.unity','*.json','*.txt'
$stamp  = Get-Date -Format yyyyMMdd_HHmmss
$log    = Join-Path $root "_audit\tmp_sanitize_$stamp.txt"

$logDir = Split-Path -Parent $log
if(-not (Test-Path $logDir)){ New-Item -ItemType Directory -Force -Path $logDir | Out-Null }

"== AQ Sanitize Glyphs (✓ -> OK) @ $stamp ==" | Tee-Object -FilePath $log
$files = Get-ChildItem $targets -Recurse -Include $exts | Where-Object { $_.FullName -notmatch '\\Library\\' }
$changed = 0

foreach($f in $files){
  $raw = Get-Content $f.FullName -Raw
  if($raw -match '✓' -or $raw -match '\\u2713'){
    $bak = "$($f.FullName).bak_$stamp"
    Copy-Item $f.FullName $bak -Force
    $new = $raw.Replace('✓','OK').Replace('\u2713','OK')
    if($new -ne $raw){
      Set-Content $f.FullName $new -NoNewline
      "CHANGED: $($f.FullName)" | Tee-Object -FilePath $log -Append
      $changed++
    }
  }
}

"$changed file(s) changed. Log: $log" | Tee-Object -FilePath $log -Append
Write-Host "PASS: Sanitizer completed. Changed=$changed"
