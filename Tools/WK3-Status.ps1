param([string]$RepoRoot = (Resolve-Path ".").Path)
$root = $RepoRoot
# --- WK1 & WK2 quick signals ---
$wk1 = @(
  "{0,-28}: {1}" -f "SharedKernel present", (Test-Path "$root\Packages\com.aq.sharedkernel\Runtime")
  "{0,-28}: {1}" -f "Domain.Merge present", (Test-Path "$root\Packages\com.aq.domain.merge\Runtime")
)
$wk2 = @(
  "{0,-28}: {1}" -f "UI scaffolding folder", (Test-Path "$root\Assets\App\UI")
  "{0,-28}: {1}" -f "ThemeSO canonical",     (Test-Path "$root\Assets\App\UI\Theme\ThemeSO.cs")
  "{0,-28}: {1}" -f "Addressables (code)",   (Get-ChildItem $root -Recurse -Filter *.cs -EA SilentlyContinue | Select-String -SimpleMatch "Unity.Addressables" -Quiet)
  "{0,-28}: {1}" -f "Input System (code)",   (Get-ChildItem $root -Recurse -Filter *.cs -EA SilentlyContinue | Select-String -SimpleMatch "UnityEngine.InputSystem" -Quiet)
)
# --- WK3 heuristic % (presence meters) ---
$files = Get-ChildItem $root -Recurse -Include *.cs,*.json -File -EA SilentlyContinue
function pct($hits,$total){ [int](100*([int]$hits)/[double]$total) }

$economyHits = @(
  ($files | Select-String -SimpleMatch "RewardBundle"    -Quiet),
  ($files | Select-String -SimpleMatch "IWallet"         -Quiet),
  ($files | Select-String -SimpleMatch "WalletService"   -Quiet),
  ($files | Select-String -SimpleMatch "RewardGranted"   -Quiet)
).Where({$_}).Count
$minigameHits = @(
  ($files | Select-String -SimpleMatch "interface IMinigame" -Quiet),
  ($files | Select-String -SimpleMatch "MinigameResult"      -Quiet),
  ($files | Select-String -SimpleMatch "Surveillance"        -Quiet),
  ($files | Select-String -SimpleMatch "CCTV"                -Quiet)
).Where({$_}).Count
$caseflowHits = @(
  ($files | Select-String -SimpleMatch "CaseFlowOrchestrator" -Quiet),
  ($files | Select-String -SimpleMatch "CaseFlow"             -Quiet),
  ($files | Select-String -SimpleMatch "Episode"              -Quiet)
).Where({$_}).Count
$firebaseHits = @(
  ($files | Select-String -SimpleMatch "FirebaseAnalytics" -Quiet),
  ($files | Select-String -SimpleMatch "Firebase.Analytics" -Quiet),
  ((Test-Path "$root\Packages\manifest.json") -and (Select-String -Path "$root\Packages\manifest.json" -SimpleMatch "firebase" -Quiet))
).Where({$_}).Count
$adsHits = @(
  ($files | Select-String -SimpleMatch "UnityEngine.Purchasing"    -Quiet),
  ($files | Select-String -SimpleMatch "com.unity.purchasing"      -Quiet),
  ($files | Select-String -SimpleMatch "UnityEngine.Advertisements" -Quiet),
  ($files | Select-String -SimpleMatch "Unity.Services.Mediation"  -Quiet)
).Where({$_}).Count

Write-Host "`n=== AQ30 Status @ $(Get-Date -Format u) ==="
Write-Host "WK1 — Foundations";   $wk1 | Write-Host
Write-Host "`nWK2 — App Integration"; $wk2 | Write-Host
Write-Host "`nWK3 — Sprint Items (presence meters)"
"{0,-40}: {1,3}%" -f "Economy & Rewards (lite)",                 (pct $economyHits 4) | Write-Host
"{0,-40}: {1,3}%" -f "Minigame framework + Surveillance Scrub",  (pct $minigameHits 4) | Write-Host
"{0,-40}: {1,3}%" -f "CaseFlow orchestrator",                     (pct $caseflowHits 3) | Write-Host
"{0,-40}: {1,3}%" -f "Firebase Analytics",                        (pct $firebaseHits 3) | Write-Host
"{0,-40}: {1,3}%" -f "Ads & IAP stubs",                           (pct $adsHits 4) | Write-Host
