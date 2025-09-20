#Requires -Version 7
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$PSStyle.OutputRendering = 'PlainText'

function Write-Head($text) { Write-Host "`n=== $text ===" -ForegroundColor Cyan }
function Mark($ok){ if($ok){ "✅" } else { "⚠️" } }

# Resolve Assets folder from script location or current dir
$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not (Test-Path $repoRoot)) { $repoRoot = Get-Location }
$assets = Join-Path $repoRoot "..\Assets" | Resolve-Path -ErrorAction SilentlyContinue
if (-not $assets) { $assets = Join-Path $repoRoot "Assets" }
if (-not (Test-Path $assets)) { throw "Could not find Assets/ folder from '$repoRoot'." }

Write-Host "AQ Audit — $(Get-Date -Format s)" -ForegroundColor Green
Write-Host "Assets: $assets" -ForegroundColor DarkGray

# ---------- 1) Code audit: LeadRequirementItem presence ----------
Write-Head "Scripts: LeadRequirementItem"
$csHits = Get-ChildItem -Path $assets -Filter *.cs -Recurse |
  Select-String -Pattern '\bclass\s+LeadRequirementItem\b' -List

if ($csHits) {
  $paths = $csHits | ForEach-Object { $_.Path } | Sort-Object -Unique
  $count = $paths.Count
  Write-Host "$(Mark($true)) Found $count definition(s):"
  $paths | ForEach-Object { Write-Host "  - $_" }
} else {
  Write-Host "$(Mark($false)) No class named 'LeadRequirementItem' found under Assets/."
}

# ---------- 2) Prefab audit: Missing Scripts ----------
Write-Head "Prefabs with Missing Script components"
$missing = Get-ChildItem -Path $assets -Filter *.prefab -Recurse |
  Select-String -Pattern 'm_Script:\s*\{fileID:\s*0' | Group-Object Path | Select-Object Name

if ($missing) {
  $missing | ForEach-Object { Write-Host "  ⚠️  $_.Name" }
} else {
  Write-Host "$(Mark($true)) No prefab with 'Missing Script' components detected."
}

# ---------- 3) ReqItem.prefab structure ----------
$reqItemPath = Join-Path $assets "UI\Prefabs\ReqItem.prefab"
Write-Head "ReqItem.prefab structure"
if (Test-Path $reqItemPath) {
  $reqText = Get-Content $reqItemPath -Raw
  $hasIcon  = $reqText -match 'm_Name:\s*Icon'
  $hasLabel = $reqText -match 'm_Name:\s*Label'
  $hasCheck = $reqText -match 'm_Name:\s*Check'
  $hasReqMB = $reqText -match 'LeadRequirementItem' -or $reqText -match 'm_Script: \{fileID: -?[0-9]+, guid: .+, type: 3\}'
  Write-Host "  Path: $reqItemPath"
  Write-Host "  Children: Icon=$(Mark($hasIcon)) Label=$(Mark($hasLabel)) Check=$(Mark($hasCheck))"
  Write-Host "  Has component (by name/guid presence): $(Mark($hasReqMB))"
} else {
  Write-Host "$(Mark($false)) Missing prefab: $reqItemPath"
}

# ---------- 4) LeadCardView.prefab structure ----------
$cardPath = Join-Path $assets "UI\Prefabs\LeadCardView.prefab"
Write-Head "LeadCardView.prefab structure"
if (Test-Path $cardPath) {
  $cardText = Get-Content $cardPath -Raw
  $hasRequirements = $cardText -match 'm_Name:\s*Requirements'
  $hasPortrait     = $cardText -match 'm_Name:\s*Portrait'
  $hasTitle        = $cardText -match 'm_Name:\s*Title'
  $hasProceed      = $cardText -match 'm_Name:\s*Proceed'
  # Best-effort check that ReqItem is referenced somewhere (by file name)
  $refsReqItem     = $cardText -match 'ReqItem'
  Write-Host "  Path: $cardPath"
  Write-Host "  Children: Requirements=$(Mark($hasRequirements)) Portrait=$(Mark($hasPortrait)) Title=$(Mark($hasTitle)) Proceed=$(Mark($hasProceed))"
  Write-Host "  Mentions ReqItem: $(Mark($refsReqItem))"
} else {
  Write-Host "$(Mark($false)) Missing prefab: $cardPath"
}

# ---------- 5) Scenes: expected objects present by name ----------
Write-Head "Scenes: expected object names present"
$sceneHits = @()
$sceneFiles = Get-ChildItem -Path $assets -Filter *.unity -Recurse
$needles = @('Canvas_Board','LeadsBar','ScrollLeads','RequirementsHUD','StatusRow')
foreach ($s in $sceneFiles) {
  $txt = Get-Content $s.FullName -Raw
  $found = foreach ($n in $needles) { if ($txt -match "m_Name:\s*$n") { $n } }
  if ($found) {
    $sceneHits += [pscustomobject]@{ Scene=$s.FullName; Found=($found -join ', ') }
  }
}
if ($sceneHits) {
  $sceneHits | ForEach-Object { Write-Host "  ✅ $($_.Scene)" ; Write-Host "     → $($_.Found)" }
} else {
  Write-Host "  ⚠️  No scenes with those names found (this only matches YAML object names)."
}

# ---------- 6) Summary ----------
Write-Head "Summary"
$okClass = $csHits.Count -ge 1
$okReq   = (Test-Path $reqItemPath)
$okCard  = (Test-Path $cardPath)
$okMissing = -not $missing

"{0} LeadRequirementItem class present"  -f (Mark($okClass))   | Write-Host
"{0} ReqItem.prefab present"             -f (Mark($okReq))     | Write-Host
"{0} LeadCardView.prefab present"        -f (Mark($okCard))    | Write-Host
"{0} No prefabs with Missing Script"     -f (Mark($okMissing)) | Write-Host

Write-Host "`nDone." -ForegroundColor Green
