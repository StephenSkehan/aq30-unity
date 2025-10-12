#requires -Version 7.2
<#
 File: Tools\AQ-Release-From-Checkpoint.ps1
 Purpose: Create (or verify) a GitHub Release from an existing checkpoint tag and zip.
 Notes:
   - Idempotent: if the release already exists and the asset is present, it will no-op (unless -ClobberAsset).
   - Uses GitHub CLI (gh). Requires you to be authenticated: `gh auth login`.
   - Does not modify git history; reads existing tag & zip under Backups\.
 Usage examples (from repo root):
   pwsh Tools\AQ-Release-From-Checkpoint.ps1 -Tag ckpt-20250930_170438 -Message 'pre-merge-board'
   pwsh Tools\AQ-Release-From-Checkpoint.ps1 -Tag ckpt-20250930_170438 -ZipPath .\Backups\ckpt-20250930_170438.zip -ClobberAsset
#>
[CmdletBinding()]
param(
  [Parameter(Mandatory=$true)]
  [string]$Tag,

  [string]$ZipPath,

  [string]$Message = "checkpoint",

  [switch]$ClobberAsset
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Info([string]$m){ Write-Host "[INFO] $m" }
function Pass([string]$m){ Write-Host "[PASS] $m" -ForegroundColor Green }
function Warn([string]$m){ Write-Host "[WARN] $m" -ForegroundColor Yellow }
function Fail([string]$m){ Write-Host "[FAIL] $m" -ForegroundColor Red }

function Exec([string]$exe, [string[]]$argv, [string]$label){
  Info $label
  $out = & $exe @argv 2>&1
  $code = $LASTEXITCODE
  if($code -ne 0){
    $joined = if($argv){ $argv -join ' ' } else { '' }
    throw "Command failed ($code): $exe $joined`n$out"
  }
  return ($out -join "`n")
}

try {
  # Preflight: tools (capture absolute paths)
  $gitPath = (Get-Command git -ErrorAction Stop).Source
  $ghPath  = (Get-Command gh  -ErrorAction Stop).Source
  Info "git: $gitPath"
  Info "gh : $ghPath"

  # Resolve repo root using git; fallback to CWD if .git exists
  $repoRoot = $null
  try {
    $repoRoot = Exec $gitPath @('rev-parse','--show-toplevel') 'Resolve repo root'
    $repoRoot = $repoRoot.Trim()
  } catch {
    if(Test-Path -LiteralPath '.git'){
      $repoRoot = (Get-Location).Path
      Warn "git rev-parse failed; using current directory as repo root: $repoRoot"
    } else {
      throw
    }
  }
  Info "Repo: $repoRoot"
  Set-Location -LiteralPath $repoRoot

  # Resolve origin -> owner/repo
  $originUrl = Exec $gitPath @('remote','get-url','origin') 'Get origin URL'
  $originUrl = $originUrl.Trim()
  if($originUrl -match 'github\.com[:/](?<owner>[^/]+)/(?<repo>[^/.]+)'){
    $owner = $Matches['owner']; $repo = $Matches['repo']
  } else {
    throw "Cannot parse GitHub owner/repo from origin URL: $originUrl"
  }
  $repoSlug = "$owner/$repo"
  Info "GitHub repo: $repoSlug"

  # Verify tag exists locally
  $tagList = Exec $gitPath @('tag','--list',$Tag) "Verify tag '$Tag' exists"
  if([string]::IsNullOrWhiteSpace($tagList)){
    throw "Tag '$Tag' not found locally. Aborting."
  }

  # Resolve zip path (default: Backups\<Tag>.zip)
  if([string]::IsNullOrWhiteSpace($ZipPath)){
    $ZipPath = Join-Path -Path $repoRoot -ChildPath ("Backups\{0}.zip" -f $Tag)
  } elseif(-not [System.IO.Path]::IsPathRooted($ZipPath)){
    $ZipPath = Join-Path -Path (Get-Location) -ChildPath $ZipPath
  }
  if(-not (Test-Path -LiteralPath $ZipPath -PathType Leaf)){
    throw "Zip not found: $ZipPath"
  }
  Info "Zip: $ZipPath"

  # Compute SHA256 for notes
  $sha256 = (Get-FileHash -LiteralPath $ZipPath -Algorithm SHA256).Hash
  Info "SHA256: $sha256"

  # Verify gh auth
  try {
    $null = Exec $ghPath @('auth','status') 'Check gh auth'
  } catch {
    throw "GitHub CLI not authenticated. Run: gh auth login"
  }

  # Does release already exist?
  $releaseExists = $false
  $releaseUrl = $null
  try {
    $releaseUrl = Exec $ghPath @('release','view',$Tag,'--json','url','--jq','.url') "Check if release '$Tag' exists"
    if(-not [string]::IsNullOrWhiteSpace($releaseUrl)){ $releaseExists = $true }
  } catch {
    $releaseExists = $false
  }

  # Prepare notes
  $notes  = @()
  $notes += "Checkpoint tag: $Tag"
  $notes += "Zip SHA256: $sha256"
  if(-not [string]::IsNullOrWhiteSpace($Message)){
    $notes += "Message: $Message"
  }
  $notes += ""
  $notes += "Created by AQ-Release-From-Checkpoint.ps1 on $(Get-Date -Format 'u')"

  $notesFile = Join-Path $env:TEMP ("release_notes_{0}.txt" -f $Tag)
  Set-Content -LiteralPath $notesFile -Value ($notes -join "`n") -Encoding UTF8

  if(-not $releaseExists){
    # Create release with asset
    $argsCreate = @('release','create',$Tag,$ZipPath,'--title',("Checkpoint {0}" -f $Tag),'--notes-file',$notesFile,'--verify-tag','--repo',$repoSlug)
    $null = Exec $ghPath $argsCreate "Create release '$Tag' with asset"
    $releaseUrl = "https://github.com/$repoSlug/releases/tag/$Tag"
    Pass "Release created: $releaseUrl"
  }
  else {
    Info "Release already exists: $releaseUrl"
    # Check if the asset is already attached
    $assets = Exec $ghPath @('release','view',$Tag,'--json','assets','--jq','.assets[].name') "List release assets"
    $zipName = [System.IO.Path]::GetFileName($ZipPath)
    $hasAsset = $false
    if(-not [string]::IsNullOrWhiteSpace($assets)){
      $assetList = $assets -split "`r?`n" | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
      $hasAsset = $assetList -contains $zipName
    }
    if($hasAsset -and -not $ClobberAsset){
      Info "Asset already present: $zipName (skipping upload)"
    } else {
      $uploadArgs = @('release','upload',$Tag,$ZipPath,'--repo',$repoSlug)
      if($ClobberAsset){ $uploadArgs += '--clobber' }
      $null = Exec $ghPath $uploadArgs "Upload asset: $zipName"
      Pass "Asset uploaded: $zipName"
    }
    # Append SHA256 to notes if missing
    try {
      $desc = Exec $ghPath @('release','view',$Tag,'--json','body','--jq','.body') "Fetch release notes"
      if($desc -notmatch [regex]::Escape($sha256)){
        Info "Appending SHA256 to release notes"
        $newBody = ($desc + "`n`n" + (Get-Content -LiteralPath $notesFile -Raw -Encoding UTF8))
        $tmpNotes = Join-Path $env:TEMP ("release_notes_update_{0}.txt" -f $Tag)
        Set-Content -LiteralPath $tmpNotes -Value $newBody -Encoding UTF8
        $null = Exec $ghPath @('release','edit',$Tag,'--notes-file',$tmpNotes,'--repo',$repoSlug) "Update release notes"
      } else {
        Info "Notes already include SHA256; leaving as-is"
      }
    } catch {
      Warn "Could not read/update release notes (non-fatal): $($_.Exception.Message)"
    }
    Pass "Release verified: $releaseUrl"
  }

  # Final summary
  Pass "Frozen checkpoint: $Tag"
  Pass "Zip: $ZipPath"
  Pass "SHA256: $sha256"
}
catch {
  Fail ($_.Exception.Message)
  exit 1
}
