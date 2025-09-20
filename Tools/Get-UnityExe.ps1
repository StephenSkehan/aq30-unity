param([switch]$Print)
function Get-UnityExe {
  $pref = "C:\Program Files\Unity\Hub\Editor\6000.2.2f1\Editor\Unity.exe"
  if (Test-Path $pref) { return $pref }
  $hub = "C:\Program Files\Unity\Hub\Editor"
  if (Test-Path $hub) {
    $dirs = Get-ChildItem $hub -Directory | Sort-Object Name -Descending
    foreach ($d in $dirs) {
      $cand = Join-Path $d.FullName "Editor\Unity.exe"
      if (Test-Path $cand) { return $cand }
    }
  }
  throw "Unity.exe not found via Unity Hub."
}
$u = Get-UnityExe
if ($Print) { Write-Host $u } else { $u }
