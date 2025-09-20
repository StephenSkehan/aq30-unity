[CmdletBinding()]
param(
  [Parameter()][string]$RepoRoot = "."
)

$ErrorActionPreference = "Stop"
function Write-Header([string]$t){ Write-Host ("`n=== {0} ===" -f $t) -ForegroundColor Cyan }
function Info([string]$t){ Write-Host $t -ForegroundColor Gray }
function Good([string]$t){ Write-Host $t -ForegroundColor Green }
function Warn([string]$t){ Write-Host $t -ForegroundColor Yellow }

$repo = (Resolve-Path -LiteralPath $RepoRoot).Path
Set-Location $repo

$ts = Get-Date -Format "yyyyMMdd_HHmmss"
$auditDir = Join-Path $repo "_Audit"
if(-not (Test-Path $auditDir)){ New-Item -ItemType Directory -Path $auditDir | Out-Null }
$out = Join-Path $auditDir ("_gameplay_data_audit_{0}.txt" -f $ts)

$resourcesGameplay = "Assets\Resources\App\Gameplay"
$addrGroupsDir = "Assets\AddressableAssetsData\AssetGroups"
$iconsDir = "Assets\Resources\App\UI\Icons"

Write-Header "Inputs"
Info "RepoRoot: $repo"
Info "Gameplay dir: $resourcesGameplay"
Info "Addressables groups: $addrGroupsDir"
Info "Icons dir: $iconsDir"

$lines = New-Object System.Collections.Generic.List[string]

function Get-Guid($path){
  $meta = "$path.meta"
  if(-not (Test-Path -LiteralPath $meta)){ return $null }
  $m = Select-String -Path $meta -Pattern 'guid:\s*([0-9a-f]{32})' | Select-Object -First 1
  if($m){ return $m.Matches[0].Groups[1].Value } else { return $null }
}

function Find-InAddressables([string]$guid){
  if(-not $guid){ return @() }
  if(-not (Test-Path -LiteralPath $addrGroupsDir)){ return @() }
  Get-ChildItem -LiteralPath $addrGroupsDir -Recurse -Filter *.asset -File |
    Select-String -SimpleMatch $guid -List | Select-Object -ExpandProperty Path
}

$targets = @("RecipeBook.asset","SpawnPolicy.asset")
$report = @()

Write-Header "Gameplay assets under $resourcesGameplay"
if(Test-Path -LiteralPath $resourcesGameplay){
  foreach($t in $targets){
    $p = Join-Path $resourcesGameplay $t
    $exists = Test-Path -LiteralPath $p
    $guid = if($exists){ Get-Guid $p } else { $null }
    $addrHits = if($exists){ Find-InAddressables $guid } else { @() }
    $report += [pscustomobject]@{
      AssetName = $t
      Exists    = $exists
      Path      = if($exists){ $p } else { "" }
      GUID      = $guid
      AddressablesRefs = ($addrHits -join "; ")
    }
  }
  $report | Format-Table -AutoSize | Out-String | %{ Write-Host $_ }
  $lines.Add(($report | ConvertTo-Csv -NoTypeInformation | Out-String))
} else {
  Warn "Gameplay folder not found. (This is OK if not yet created.)"
}

Write-Header "Icon sprites sanity"
$icons = @()
if(Test-Path -LiteralPath $iconsDir){
  $icons = Get-ChildItem -LiteralPath $iconsDir -Recurse -Include *.png,*.psd,*.sprite,*.asset -File -ErrorAction SilentlyContinue
  Info ("Found {0} icon files." -f $icons.Count)
} else {
  Warn "No icons folder at $iconsDir"
}
$lines.Add("IconCount=" + $icons.Count)

Write-Header "Addressables coverage for gameplay assets"
if($report){
  foreach($row in $report){
    if($row.Exists){
      if([string]::IsNullOrWhiteSpace($row.AddressablesRefs)){
        Warn ("{0} exists but is NOT in Addressables." -f $row.AssetName)
      } else {
        Good ("{0} present in Addressables: {1}" -f $row.AssetName,$row.AddressablesRefs)
      }
    }
  }
}

# write machine-friendly log
$lines | Set-Content -LiteralPath $out -Encoding UTF8
Good "Wrote audit: $out"