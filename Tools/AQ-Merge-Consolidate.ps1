#requires -Version 7.2
<#
 File: Tools\AQ-Merge-Consolidate.ps1
 Purpose: Make the package RecipeBook authoritative by retiring the Assets duplicate, with a safe dry-run first.
 Usage (dry-run):
   pwsh Tools\AQ-Merge-Consolidate.ps1
 Apply (move Assets copy to _recycle and stage):
   pwsh Tools\AQ-Merge-Consolidate.ps1 -ApplyDelete
 Add commit & push:
   pwsh Tools\AQ-Merge-Consolidate.ps1 -ApplyDelete -Commit -Push
#>
[CmdletBinding()]
param(
  [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
  [string]$AssetsRecipe = 'Assets\App\Gameplay\Data\RecipeBook.cs',
  [string]$PackageRecipe = 'Packages\com.aq.domain.merge\Runtime\RecipeBook.cs',
  [switch]$ApplyDelete,
  [switch]$Commit,
  [switch]$Push
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Info([string]$m){ Write-Host "[INFO] $m" }
function Pass([string]$m){ Write-Host "[PASS] $m" -ForegroundColor Green }
function Warn([string]$m){ Write-Host "[WARN] $m" -ForegroundColor Yellow }
function Fail([string]$m){ Write-Host "[FAIL] $m" -ForegroundColor Red }

function Git([string[]]$args, [string]$display){
  $git = (Get-Command git).Source
  $psi = New-Object System.Diagnostics.ProcessStartInfo
  $psi.FileName = $git
  $psi.WorkingDirectory = $RepoRoot
  $psi.RedirectStandardOutput = $true
  $psi.RedirectStandardError = $true
  $psi.UseShellExecute = $false
  $psi.Arguments = ($args -join ' ')
  $p = [System.Diagnostics.Process]::Start($psi)
  $out = $p.StandardOutput.ReadToEnd()
  $err = $p.StandardError.ReadToEnd()
  $p.WaitForExit()
  if($p.ExitCode -ne 0){ throw "git $display failed (exit $($p.ExitCode))`n$err" }
  return $out
}

Push-Location $RepoRoot
try{
  Info "Repo: $RepoRoot"
  $assetsPath = Join-Path $RepoRoot $AssetsRecipe
  $pkgPath    = Join-Path $RepoRoot $PackageRecipe

  if(-not (Test-Path -LiteralPath $pkgPath)){
    throw "Package RecipeBook not found: $PackageRecipe"
  }
  if(-not (Test-Path -LiteralPath $assetsPath)){
    Warn "Assets duplicate not found (already consolidated?): $AssetsRecipe"
  }

  $meta = @()
  foreach($p in @($assetsPath,$pkgPath) | Where-Object { $_ -and (Test-Path -LiteralPath $_) }){
    $txt = Get-Content -LiteralPath $p -Raw -Encoding UTF8
    $sha = (Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash
    $firstNs = ([regex]::Match($txt,'(?m)^\s*namespace\s+([A-Za-z0-9_.]+)')).Groups[1].Value
    $firstClass = ([regex]::Match($txt,'(?m)^\s*(public|internal)?\s*(partial\s+)?class\s+RecipeBook\b')).Value
    $meta += [pscustomobject]@{
      Path = $p.Replace($RepoRoot+'\','')
      Bytes = (Get-Item -LiteralPath $p).Length
      SHA256 = $sha
      Namespace = $firstNs
      DeclaresRecipeBook = [bool]($firstClass)
    }
  }
  Info "Targets summary:"
  $meta | Format-Table -AutoSize | Out-String | ForEach-Object { $_.TrimEnd() } | Write-Host

  # Find usages (skip the two declaration files)
  $allCs = Get-ChildItem -LiteralPath $RepoRoot -Recurse -Include *.cs -File |
           Where-Object { $_.FullName -ne $assetsPath -and $_.FullName -ne $pkgPath }
  $hits = @()
  foreach($f in $allCs){
    $m = Select-String -LiteralPath $f.FullName -Pattern '\bRecipeBook\b' -SimpleMatch -AllMatches -ErrorAction SilentlyContinue
    if($m){
      $hits += [pscustomobject]@{
        File = $f.FullName.Replace($RepoRoot+'\','')
        Count = $m.Matches.Count
      }
    }
  }
  Info ("Usages of 'RecipeBook' in repo (excl. declarations): {0}" -f $hits.Count)
  if($hits.Count){ $hits | Sort-Object Count -Descending | Format-Table -AutoSize | Out-String | ForEach-Object { $_.TrimEnd() } | Write-Host }

  if(-not $ApplyDelete){
    Pass "Dry-run only. No changes made. Re-run with -ApplyDelete to retire the Assets duplicate."
    return
  }

  if(-not (Test-Path -LiteralPath $assetsPath)){
    Pass "Nothing to delete; Assets duplicate already gone."
  } else {
    $recycle = Join-Path $RepoRoot "_recycle"
    New-Item -ItemType Directory -Force -Path $recycle | Out-Null
    $stamp = Get-Date -Format 'yyyyMMdd_HHmmss'
    $dest = Join-Path $recycle ("RecipeBook.cs.assets_copy_{0}.bak" -f $stamp)
    Info "Moving Assets copy -> $dest"
    Move-Item -LiteralPath $assetsPath -Destination $dest -Force
    # If a .meta exists beside it, move too
    $metaPath = $assetsPath + '.meta'
    if(Test-Path -LiteralPath $metaPath){
      Move-Item -LiteralPath $metaPath -Destination ($dest + '.meta') -Force
    }
    Info "Staging changes"
    Git @('add','-A','--','Assets','Packages') 'add'
  }

  if($Commit){
    $msg = 'Discovery: make package RecipeBook authoritative; retire Assets duplicate'
    Info "Committing: $msg"
    Git @('commit','-m',('"{0}"' -f $msg)) 'commit'
  } else {
    Warn "Commit not requested. Use -Commit to create a commit."
  }

  if($Push){
    Info "Pushing current branch"
    Git @('push') 'push'
  } else {
    Warn "Push not requested. Use -Push to push the branch."
  }

  Pass "Consolidation step completed."
}
catch{
  Fail $_
  exit 1
}
finally{
  Pop-Location
}
