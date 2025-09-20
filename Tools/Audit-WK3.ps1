<# 
AQ30 — Week 3 audit (tightened)
Layer 1 = filesystem-only heuristics (safe, fast, stricter)
Layer 2 = optional Unity scene-wiring audit (run Tools/Run-WK3-UnityAudit.ps1)
Outputs in: _audit\wk3_audit\<timestamp>\
#>

[CmdletBinding()]
param(
  [string]$ProjectPath = (Resolve-Path ".").Path,
  [switch]$VerboseLog
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$PSDefaultParameterValues["Out-File:Encoding"] = "utf8"

function New-Dir([string]$p){ if(!(Test-Path $p)){ New-Item -ItemType Directory -Force -Path $p | Out-Null } }
function Log([string]$msg){ $msg | Tee-Object -FilePath $findingsTxt -Append | Out-Host }
function Head([string]$msg){ Log ("`n=== $msg ===") }

$ts = Get-Date -Format "yyyyMMdd_HHmmss"
$outRoot = Join-Path $ProjectPath "_audit\wk3_audit\$ts"
New-Dir $outRoot
$findingsTxt = Join-Path $outRoot "wk3_findings_$ts.txt"
$summaryJson = Join-Path $outRoot "wk3_summary_$ts.json"

# Limit to app + AQ packages; exclude samples, TMP examples, tests by default
$roots = @(
  (Join-Path $ProjectPath "Assets\App"),
  (Join-Path $ProjectPath "Assets\Editor\AQ"),
  (Join-Path $ProjectPath "Packages\com.aq.sharedkernel"),
  (Join-Path $ProjectPath "Packages\com.aq.domain.merge")
) | Where-Object { Test-Path $_ }

$excludeRegex = '(?i)(\\TextMesh Pro\\|\\Samples\\|\\Sample\\|\\Example\\|\\Examples\\|\\Demo\\|\\Tests\\|\\Test\\|\\Editor\\AQ\\Debug\\)'
$csFiles = @()
foreach($r in $roots){
  $csFiles += @(Get-ChildItem -Path $r -Recurse -Include *.cs -ErrorAction SilentlyContinue | Where-Object { $_.FullName -notmatch $excludeRegex })
}

function GrepDef($regexArray){
  $hits = @()
  foreach($f in $csFiles){
    $txt = ""
    try { $txt = Get-Content -Raw -LiteralPath $f.FullName -ErrorAction Stop } catch { continue }
    foreach($rx in $regexArray){
      if($txt -match $rx){ $hits += $f.FullName; break }
    }
  }
  return ,(@($hits | Sort-Object -Unique))
}

function Score($points,$cond){ if($cond){ $points } else { 0 } }

# ────────────────────────────────────────────────────────────────────────────
# WK3-1 Economy & Rewards (Wallet, Reward, persistence, overlay binder)
Head "WK3-1 Economy & Rewards"

# Require real interface/class defs with word boundaries
$walletIface = GrepDef @('(?m)^\s*public\s+interface\s+IWallet\b')
$walletClass = GrepDef @('(?m)^\s*public\s+class\s+Wallet(Service|)\b')
$rewardTypes = GrepDef @('(?m)^\s*public\s+(struct|class)\s+Reward(Bundle|)\b')
$persistence = GrepDef @('\bSaveBlob\b','\bJsonSaveService\b','\bSerialize\(')
$overlayBind = GrepDef @('(?m)^\s*public\s+class\s+ResolutionRewardsBinder\b','\bRewardsLine\b')

$wk31Score = 0
$wk31Score += Score 2 ($walletIface.Count -gt 0 -and $walletClass.Count -gt 0)
$wk31Score += Score 2 ($persistence.Count -gt 0)
$wk31Score += Score 1 ($rewardTypes.Count -gt 0)
$wk31Score += Score 1 ($overlayBind.Count -gt 0)
$wk31Percent = [int](100*$wk31Score/6)

# ────────────────────────────────────────────────────────────────────────────
# WK3-2 Minigame Framework + Scrub
Head "WK3-2 Minigame Framework + Surveillance Scrub"

$iMini     = GrepDef @('(?m)^\s*public\s+interface\s+IMinigame\b')
$miniHost  = GrepDef @('(?m)^\s*public\s+class\s+MinigameHost\b')
$scrub     = GrepDef @('(?i)\bMinigame_Scrub\b','(?m)^\s*public\s+class\s+Scrub\b','\bScrubHotspot\b')
$advanceBtn= GrepDef @('\bCaseFlowAdvanceOnEventMB\.Advance\(')

$wk32Score = 0
$wk32Score += Score 2 ($iMini.Count    -gt 0)
$wk32Score += Score 2 ($miniHost.Count -gt 0)
$wk32Score += Score 2 ($scrub.Count    -gt 0)
$wk32Score += Score 1 ($advanceBtn.Count -gt 0)
$wk32Percent = [int](100*$wk32Score/7)

# ────────────────────────────────────────────────────────────────────────────
# WK3-3 CaseFlow Orchestrator
Head "WK3-3 CaseFlow Orchestrator"

$orch = GrepDef @('(?m)^\s*public\s+class\s+CaseFlowOrchestrator(MB|)\b')
$state= GrepDef @('(?m)^\s*public\s+enum\s+CaseFlowState\b')
$bus  = GrepDef @('(?m)^\s*public\s+class\s+InMemoryEventBus\b','\bIEventBus\b')
$wire = GrepDef @('\bResolutionContinueMB\.OnResolve\(','\bAdvanceOnceMB\b','\bCaseFlowAdvanceOnEventMB\b')

$wk33Score = 0
$wk33Score += Score 2 ($orch.Count -gt 0 -and $state.Count -gt 0)
$wk33Score += Score 2 ($bus.Count  -gt 0)
$wk33Score += Score 1 ($wire.Count -gt 0)
$wk33Percent = [int](100*$wk33Score/5)

# ────────────────────────────────────────────────────────────────────────────
# WK3-4 Firebase Analytics & Crashlytics (heuristic: explicit calls present)
Head "WK3-4 Firebase Analytics & Crashlytics"

$firebase = GrepDef @('\bFirebaseAnalytics\b','\bCrashlytics\b','\bSetCustomKey\b','\bLogEvent\(')
$events   = GrepDef @('\bftue_start\b','\breward_granted\b','\bminigame_(start|end)\b','\bchoice_selected\b')

$wk34Score = 0
$wk34Score += Score 2 ($firebase.Count -gt 0)
$wk34Score += Score 1 ($events.Count   -gt 0)
$wk34Percent = [int](100*$wk34Score/3)

# ────────────────────────────────────────────────────────────────────────────
# WK3-5 Ads & IAP Stubs
Head "WK3-5 Ads & IAP Stubs"

$rewarded = GrepDef @('(?m)^\s*public\s+(class|interface)\s+(RewardedAds|IRewardedAds)\b','\bShowRewarded\b')
$noads    = GrepDef @('\bNoAds(Entitlement|)\b','\bHasNoAds\b')
$iap      = GrepDef @('\bUnityPurchasing\b','\bIAP\b','\bPurchase\b')

$wk35Score = 0
$wk35Score += Score 2 ($rewarded.Count -gt 0)
$wk35Score += Score 1 ($noads.Count   -gt 0)
$wk35Score += Score 1 ($iap.Count     -gt 0)
$wk35Percent = [int](100*$wk35Score/4)

# ────────────────────────────────────────────────────────────────────────────
# Output & roll-up
$report = [ordered]@{
  "WK3-1" = @{ Percent=$wk31Percent; Evidence=@($walletIface+$walletClass+$persistence+$rewardTypes+$overlayBind) }
  "WK3-2" = @{ Percent=$wk32Percent; Evidence=@($iMini+$miniHost+$scrub+$advanceBtn) }
  "WK3-3" = @{ Percent=$wk33Percent; Evidence=@($orch+$state+$bus+$wire) }
  "WK3-4" = @{ Percent=$wk34Percent; Evidence=@($firebase+$events) }
  "WK3-5" = @{ Percent=$wk35Percent; Evidence=@($rewarded+$noads+$iap) }
}

if($VerboseLog){
  foreach($k in $report.Keys){
    Log "$k evidence:"
    (@($report[$k].Evidence) | Select-Object -Unique | ForEach-Object { Log "  $_" }) | Out-Null
  }
}

$overall = [int](($report.Values | ForEach-Object { $_.Percent } | Measure-Object -Average).Average)
Log "`n=== Week 3 Progress Summary (filesystem layer) ==="
foreach($k in $report.Keys){ Log "$k : $($report[$k].Percent)%" }
Log "==> Overall (fs): $overall%"

$artifact = [ordered]@{ Timestamp=$ts; Layer="filesystem"; Overall=$overall; WK3=$report }
$artifact | ConvertTo-Json -Depth 6 | Out-File $summaryJson -Force

Write-Host "PASS: Filesystem audit done. Findings: $findingsTxt | JSON: $summaryJson"
