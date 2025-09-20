# Tools\AQ-GitStatus.ps1
# Purpose: Read-only Git/GitHub status: auth, origin, branch, upstream, remote presence.
# Platform: PowerShell 7.x. Encoding: UTF-8, CRLF. Non-destructive. Logs: _audit\git\status\<stamp>\
# Usage: pwsh -NoProfile -ExecutionPolicy Bypass .\Tools\AQ-GitStatus.ps1

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$stamp  = Get-Date -Format "yyyyMMdd_HHmmss"
$root   = (git rev-parse --show-toplevel) 2>&1
if($LASTEXITCODE -ne 0){ throw "Not inside a git repository: $root" }
$root   = $root.Trim()
$logDir = Join-Path $root "_audit\git\status\$stamp"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

function Log {
  param([Parameter(Mandatory)][string]$Name,
        [AllowNull()][AllowEmptyString()][string]$Content)
  $path = Join-Path $logDir $Name
  if($null -eq $Content){ $Content = "" }
  $Content | Out-File -FilePath $path -Encoding UTF8 -Force
  $path
}
function S { param($x) return (($x | Out-String) -replace "`r","").Trim() }

$rows = [System.Collections.Generic.List[object]]::new()
function ROW { param($K,$V) $rows.Add([pscustomobject]@{Check=$K; Detail=$V}) }

# Auth (gh)
try {
  $auth = (gh auth status) 2>&1
  Log "gh_auth_status.txt" (S $auth) | Out-Null
  $authed = ($auth -match 'Logged in to github.com')
  ROW "GitHub auth (gh)" ($(if($authed){"Logged in"}else{"Not logged in"}))
} catch { ROW "GitHub auth (gh)" "gh not installed or failed"; }

# Git basics
ROW "PowerShell" $PSVersionTable.PSVersion.ToString()
ROW "git" (S (git --version 2>&1))
$branch = S (git rev-parse --abbrev-ref HEAD 2>&1)
ROW "Current branch" $branch
$origin = S (git remote get-url origin 2>&1)
ROW "origin URL" $origin
Log "remotes.txt" (S (git remote -v 2>&1)) | Out-Null

# Upstream (safe '@{u}')
$revU = (git rev-parse --abbrev-ref --symbolic-full-name '@{u}') 2>&1
if($LASTEXITCODE -eq 0 -and ($revU | Out-String) -notmatch 'fatal:|no upstream'){
  ROW "Upstream" (S $revU)
} else {
  ROW "Upstream" "None"
}

# Remote head presence (read-only probe)
$heads = (git -c credential.interactive=never ls-remote --heads origin "$branch") 2>&1
Log "ls_remote_heads.txt" (S $heads) | Out-Null
$exists = ((S $heads).Length -gt 0)
ROW "Remote branch on origin" ($(if($exists){"exists"}else{"absent"}))

# Output
$rows | Format-Table -AutoSize | Out-String | Write-Host
Write-Host "Logs: $logDir"
