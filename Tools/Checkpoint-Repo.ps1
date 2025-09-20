#requires -Version 7.0
[CmdletBinding()]
param(
  [string]$Message       = "checkpoint (green)",
  [string]$RepoRoot      = (Get-Location).Path,
  [string]$BackupsDir    = "Backups",
  [switch]$ZipTrackedOnly,          # compatibility switch (no-op; we always make a delta zip)
  [switch]$CreateTag,               # create ckpt-$stamp tag
  [switch]$PushTag,                 # push the tag to remote
  [string]$Remote        = "origin",
  [int]$MaxPushRetries   = 3,
  [int]$RetrySecondsBase = 5         # exponential backoff base (seconds)
)

# ---------------- helpers ----------------
$ErrorActionPreference = 'Stop'

function Write-Step { param([string]$Msg)
  $ts = Get-Date -Format 'HH:mm:ss'
  Write-Host "[$ts] $Msg"
}

function Format-ArgForLog { param([string]$Arg)
  if ($null -eq $Arg) { return '""' }
  if ($Arg -match '[\s"`$^&|<>()]') { return '"' + ($Arg -replace '"','\"') + '"' }
  $Arg
}

function Invoke-Git {
  param([string[]]$GitArgs, [switch]$IgnoreErrors)
  if (-not $GitArgs -or $GitArgs.Count -eq 0) { throw "Internal: Invoke-Git called with no arguments." }
  $display = ($GitArgs | ForEach-Object { Format-ArgForLog $_ }) -join ' '
  Write-Step "git $display"

  $psi = [System.Diagnostics.ProcessStartInfo]::new()
  $psi.FileName = "git"
  foreach ($a in $GitArgs) { [void]$psi.ArgumentList.Add($a) }
  $psi.RedirectStandardOutput = $true
  $psi.RedirectStandardError  = $true
  $psi.UseShellExecute        = $false
  $psi.CreateNoWindow         = $true
  $psi.Environment['GIT_TERMINAL_PROMPT'] = '0'
  $psi.Environment['GIT_HTTP_VERSION']    = 'HTTP/1.1'

  $proc = [System.Diagnostics.Process]::new()
  $proc.StartInfo = $psi
  [void]$proc.Start()
  $stdout = $proc.StandardOutput.ReadToEnd()
  $stderr = $proc.StandardError.ReadToEnd()
  $proc.WaitForExit()
  $code = $proc.ExitCode

  if ($stdout.Trim().Length -gt 0) { Write-Host $stdout.Trim() }
  if ($code -ne 0 -and -not $IgnoreErrors) {
    $msg = if ($stderr) { $stderr.Trim() } else { "(no stderr)" }
    throw "git $display failed (exit $code)`n$msg"
  }
  [PSCustomObject]@{ Code = $code; Out = $stdout; Err = $stderr }
}

function Invoke-WithRetry {
  param(
    [scriptblock]$Script,         # must accept one param: attempt index (1..N)
    [string]$What = "operation",
    [int]$MaxRetries = 3,
    [int]$DelayBaseSec = 5
  )
  $attempt = 0
  while ($true) {
    try {
      $attempt++
      Write-Step "$What (attempt $attempt/$MaxRetries)"
      & $Script $attempt
      Write-Step "$What succeeded"
      break
    }
    catch {
      $err = $_.Exception.Message
      Write-Warning "$What failed: $err"
      if ($attempt -ge $MaxRetries) { throw }
      $delay = [Math]::Pow(2, ($attempt - 1)) * $DelayBaseSec
      Write-Step "Retrying in $delay second(s)..."
      Start-Sleep -Seconds $delay
    }
  }
}

function Ensure-ZipApisLoaded {
  try { $null = [System.IO.Compression.ZipFile] } catch {
    try { Add-Type -AssemblyName System.IO.Compression.FileSystem -ErrorAction SilentlyContinue } catch {}
    try { Add-Type -AssemblyName System.IO.Compression -ErrorAction SilentlyContinue } catch {}
  }
  if (-not ([type]::GetType("System.IO.Compression.ZipFile"))) {
    throw "Zip APIs unavailable; cannot create archive."
  }
}

function New-DeltaZip {
  param(
    [string]$ZipPath,
    [string]$RepoRoot,
    [string[]]$ExtraAnchors
  )
  Ensure-ZipApisLoaded

  Write-Step "Calculating delta file set (staged + unstaged + untracked)"
  $staged    = (Invoke-Git @("diff","--cached","--name-only","--diff-filter=ACMR")).Out -split "`n" | Where-Object { $_ }
  $unstaged  = (Invoke-Git @("diff","--name-only","--diff-filter=ACMR")).Out         -split "`n" | Where-Object { $_ }
  $untracked = (Invoke-Git @("ls-files","--others","--exclude-standard")).Out        -split "`n" | Where-Object { $_ }

  # Deleted paths (for manifest only)
  $deletedIdx  = (Invoke-Git @("diff","--cached","--name-only","--diff-filter=D")).Out -split "`n" | Where-Object { $_ }
  $deletedWork = (Invoke-Git @("diff","--name-only","--diff-filter=D")).Out            -split "`n" | Where-Object { $_ }
  $deleted = @($deletedIdx + $deletedWork | Select-Object -Unique)

  $files = @($staged + $unstaged + $untracked + $ExtraAnchors | Select-Object -Unique)

  # Prune obvious heavies / generated stuff
  $excludes = @('^Backups/','^Library/','^Temp/','^Obj/','^Logs/','^\.git/','^\.vs/','^\.idea/')
  $files = $files | Where-Object {
    $keep = $true
    foreach($ex in $excludes){ if ($_ -match $ex){ $keep = $false; break } }
    $keep
  }

  # Ensure parents exist
  $zipDir = Split-Path $ZipPath -Parent
  if (-not (Test-Path $zipDir)) { New-Item -ItemType Directory -Path $zipDir | Out-Null }

  $stage = Join-Path ([System.IO.Path]::GetTempPath()) ("aq30_delta_" + [System.Guid]::NewGuid().ToString("N"))
  New-Item -ItemType Directory -Path $stage | Out-Null

  # Copy files preserving tree
  $copied = New-Object System.Collections.Generic.List[string]
  foreach ($rel in $files) {
    $src = Join-Path $RepoRoot $rel
    if (Test-Path -LiteralPath $src) {
      $dest = Join-Path $stage $rel
      $dir  = Split-Path $dest -Parent
      if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir | Out-Null }
      Copy-Item -LiteralPath $src -Destination $dest -Force
      [void]$copied.Add($rel)
    }
  }

  # Write manifest + README
  $headShort = (Invoke-Git @("rev-parse","--short","HEAD")).Out.Trim()
  $branch    = (Invoke-Git @("symbolic-ref","-q","--short","HEAD") -IgnoreErrors).Out.Trim()
  $manifest = [PSCustomObject]@{
    created_utc   = (Get-Date).ToUniversalTime().ToString("o")
    branch        = if ($branch) { $branch } else { "(detached)" }
    head_short    = $headShort
    file_count    = $copied.Count
    files         = $copied
    deleted_count = $deleted.Count
    deleted       = $deleted
  } | ConvertTo-Json -Depth 6
  $manifestPath = Join-Path $stage "manifest.json"
  $manifest | Out-File -FilePath $manifestPath -Encoding UTF8 -Force

  $readme = @"
AQ30 Delta Backup
=================

This zip contains ONLY files that were added/modified/untracked at backup time
plus a few critical anchors (manifest, packages, settings). To recover after a
local wipe:

1) Clone or pull your remote to a fresh working directory.
2) Unzip this archive OVER the repo root, keeping folder structure.
3) Review manifest.json (deleted entries) and remove any paths you want gone.
4) git add -A
5) git commit -m "Restore from AQ30 delta backup @ $headShort"
6) (optional) git push

If you have NO remote and this delta is all you have:
- Unzip to an empty folder, then initialize a new repo:
    git init
    git add -A
    git commit -m "Import from AQ30 delta backup"
- You’ll be missing files that weren’t changed in this delta; pull from any
  previous backups or reimport Unity packages as needed.
"@
  $readmePath = Join-Path $stage "README-RESTORE.txt"
  $readme | Out-File -FilePath $readmePath -Encoding UTF8 -Force

  Write-Step ("Delta copy complete → {0} file(s), {1} deleted listed" -f $copied.Count, $deleted.Count)

  # Create zip
  Add-Type -AssemblyName System.IO.Compression.FileSystem
  [System.IO.Compression.ZipFile]::CreateFromDirectory($stage, $ZipPath, [System.IO.Compression.CompressionLevel]::Optimal, $false)

  Remove-Item -Recurse -Force $stage
  Write-Step "Delta zip created: $ZipPath"
}

# ---------------- preflight ----------------
Write-Step "Preflight: checking tools and repo"
if (-not (Get-Command git -ErrorAction SilentlyContinue)) { throw "git not found on PATH" }
if (Get-Command git-lfs -ErrorAction SilentlyContinue) {
  [void](Invoke-Git @("config","--local","lfs.concurrenttransfers","3") -IgnoreErrors)
  [void](Invoke-Git @("config","--local","lfs.transfer.maxretries","3") -IgnoreErrors)
}
$env:GIT_HTTP_VERSION = "HTTP/1.1"

Set-Location $RepoRoot
$root = (Get-Location).Path
Write-Step "Repo: $root"

$stamp   = Get-Date -Format 'yyyyMMdd_HHmmss'
$branch  = "ckpt-$stamp"
$tag     = "ckpt-$stamp"
$backups = Join-Path $RepoRoot $BackupsDir
if (-not (Test-Path $backups)) { New-Item -ItemType Directory -Path $backups | Out-Null }
Write-Step "Backups dir: $backups"

# ---------------- commit & branch ----------------
$headRef = (Invoke-Git @("symbolic-ref","-q","--short","HEAD") -IgnoreErrors).Out.Trim()
$onDetached = [string]::IsNullOrWhiteSpace($headRef)
if ($onDetached) {
  Write-Step "HEAD is detached; creating branch $branch at current commit"
  [void](Invoke-Git @("checkout","-b",$branch))
} else {
  $branch = $headRef
  Write-Step "On branch: $branch"
}

# Stage *only* source roots + Tools (no explicit excludes; ignores already handle Backups/)
Write-Step "Staging changes (Assets, Packages, ProjectSettings, Tools)"
[void](Invoke-Git @("add","-A","--","Assets","Packages","ProjectSettings","Tools"))

$hasStaged = (Invoke-Git @("diff","--cached","--name-only")).Out.Trim().Length -gt 0
if ($hasStaged) {
  Write-Step "Committing with message: $Message"
  [void](Invoke-Git @("commit","-m",$Message))
} else {
  Write-Step "No staged changes to commit"
}

# ---------------- push branch (retry; small & safe) ----------------
Invoke-WithRetry -What "Push branch '$branch' to origin" -MaxRetries $MaxPushRetries -DelayBaseSec $RetrySecondsBase -Script {
  param($attempt)
  [void](Invoke-Git @("push",$Remote,$branch))
}

# ---------------- tag (optional) ----------------
if ($CreateTag.IsPresent) {
  Write-Step "Creating tag $tag"
  $exists = (Invoke-Git @("tag","--list",$tag)).Out.Trim().Length -gt 0
  if ($exists) {
    $i = 1
    while ((Invoke-Git @("tag","--list","$tag-$i")).Out.Trim().Length -gt 0) { $i++ }
    $tag = "$tag-$i"
    Write-Step "Adjusted tag name to unique: $tag"
  }
  [void](Invoke-Git @("tag","-a",$tag,"-m",$Message))
  if ($PushTag.IsPresent) {
    Invoke-WithRetry -What "Push tag '$tag' to origin" -MaxRetries $MaxPushRetries -DelayBaseSec $RetrySecondsBase -Script {
      param($attempt)
      [void](Invoke-Git @("push",$Remote,$tag))
    }
  }
}

# ---------------- delta zip ----------------
$zipPath = Join-Path $backups ("delta-$($tag)-$($branch).zip")

# Always include a few anchors in the delta, even if not changed
$extra = @(
  "Packages/manifest.json",
  "Packages/packages-lock.json",
  "ProjectSettings/ProjectVersion.txt",
  ".editorconfig"
) + (Get-ChildItem -Path $RepoRoot -Recurse -Filter *.asmdef | ForEach-Object { $_.FullName.Substring($RepoRoot.Length + 1) }) `
  + (Get-ChildItem -Path (Join-Path $RepoRoot "Tools") -Recurse -Filter *.ps1 | ForEach-Object { $_.FullName.Substring($RepoRoot.Length + 1) })

# Normalize and de-duplicate
$extra = $extra | Where-Object { $_ -and (Test-Path (Join-Path $RepoRoot $_)) } | Select-Object -Unique

New-DeltaZip -ZipPath $zipPath -RepoRoot $RepoRoot -ExtraAnchors $extra

# ---------------- summary ----------------
$headShort = (Invoke-Git @("rev-parse","--short","HEAD")).Out.Trim()
Write-Host ""
Write-Host "Checkpoint Summary"
Write-Host "  Branch : $branch"
Write-Host "  Tag    : $tag"
Write-Host "  Commit : $headShort"
Write-Host "  Zip    : $zipPath"
Write-Host "  Remote : $Remote"
Write-Step "Done."
