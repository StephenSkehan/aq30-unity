param([string]\ = "C:\Users\Steph\Dev\aq30-unity")
\ = @(
  Join-Path \ 'Packages\com.aq.sharedkernel\Runtime'
  Join-Path \ 'Packages\com.aq.domain.merge\Runtime'
)
\ = @()
foreach (\ in \) {
  if (Test-Path \) {
    Get-ChildItem -Path \ -Filter *.cs -Recurse | ForEach-Object {
      \ = Get-Content -Raw -Path \.FullName
      if (\ -match '^\s*using\s+UnityEngine\s*;' -or \ -match 'UnityEngine\.') {
        \ += \.FullName
      }
    }
  }
}
if (\.Count -gt 0) {
  Write-Host '[FAIL] UnityEngine reference(s) found in domain/sharedkernel:' -ForegroundColor Red
  \ | ForEach-Object { Write-Host '  ' \ }
  exit 1
}
Write-Host '[OK] No UnityEngine refs in domain/sharedkernel.'
