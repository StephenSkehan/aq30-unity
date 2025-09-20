# Tools\AQ-GitHealth.ps1
# Purpose: End-to-end Git/GitHub connection health (non-destructive).
# Platform: PowerShell 7.x. Encoding: UTF-8, CRLF. Logs: _audit\git\<stamp>\
# Usage: pwsh -NoProfile -ExecutionPolicy Bypass .\Tools\AQ-GitHealth.ps1

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# --- Setup & logging ---------------------------------------------------------
$stamp  = Get-Date -Format "yyyyMMdd_HHmmss"
$root   = Resolve-Path .
$logDir = Join-Path $root "_audit/git/$stamp"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

function Write-Log {
  param([Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$Content)
  $path = Join-Path $logDir $Name
  $Content | Out-File -FilePath $path -Encoding UTF8 -Force
  return $path
}

$results = [System.Collections.Generic.List[object]]::new()
function PASS { param($Check,$Detail) $results.Add([pscustomobject]@{Check=$Check;Status='PASS';Detail=$Detail}) }
function FAIL { param($Check,$Detail) $results.Add([pscustomobject]@{Check=$Check;Status='FAIL';Detail=$Detail}) }

# --- 0) Environment ----------------------------------------------------------
try { PASS "PowerShell version" $PSVersionTable.PSVersion.ToString() } catch { FAIL "PowerShell version" $_.Exception.Message }

# --- 1) Git / gh / LFS availability -----------------------------------------
$gitOk=$false; $ghOk=$false; $lfsOk=$false

try { $v=(git --version) 2>&1; $gitOk=$LASTEXITCODE -eq 0; if($gitOk){PASS "git available" $v}else{FAIL "git available" $v} } catch { FAIL "git available" $_.Exception.Message }
try { $v=(gh --version) 2>&1;   $ghOk =$LASTEXITCODE -eq 0; if($ghOk) {PASS "GitHub CLI (gh) available" $v}else{FAIL "GitHub CLI (gh) available" $v} } catch { FAIL "GitHub CLI (gh) available" $_.Exception.Message }
try { $v=(git lfs version) 2>&1;$lfsOk=$LASTEXITCODE -eq 0; if($lfsOk){PASS "Git LFS available" $v}else{FAIL "Git LFS available" $v} } catch { FAIL "Git LFS available" $_.Exception.Message }

# --- 2) Repo context ---------------------------------------------------------
$repoRoot = $null
try {
  $repoRoot = (git rev-parse --show-toplevel) 2>&1
  if($LASTEXITCODE -eq 0){ PASS "Git repo detected" $repoRoot } else { FAIL "Git repo detected" $repoRoot }
} catch { FAIL "Git repo detected" $_.Exception.Message }

if($repoRoot){
  Push-Location $repoRoot
  try {
    $branch  = (git rev-parse --abbrev-ref HEAD).Trim()
    $remotes = (git remote -v) | Sort-Object | Get-Unique
    Write-Log -Name "remotes.txt" -Content ($remotes | Out-String) | Out-Null
    PASS "Current branch" $branch
    PASS "Configured remotes" "See remotes.txt"
  } catch { FAIL "Repo basics" $_.Exception.Message }
}

# --- 3) origin URL analysis --------------------------------------------------
$originUrl=$null; $isSSH=$false; $isHTTPS=$false
try {
  $originUrl = (git remote get-url origin) 2>&1
  if($LASTEXITCODE -eq 0){
    if($originUrl -match '^git@github\.com:'){ $isSSH=$true }
    if($originUrl -match '^https://github\.com/'){ $isHTTPS=$true }
    PASS "origin URL" $originUrl
  } else { FAIL "origin URL" $originUrl }
} catch { FAIL "origin URL" $_.Exception.Message }

# --- 4) Network reachability (DNS + ports) ----------------------------------
function Test-Port {
  param(
    [Parameter(Mandatory)][string]$destHost,
    [Parameter(Mandatory)][int]$port
  )
  try {
    $res = Test-NetConnection -ComputerName $destHost -Port $port -WarningAction SilentlyContinue
    if($res.TcpTestSucceeded){
      $lat = $null
      try { $lat = $res.PingReplyDetails.RoundtripTime } catch {}
      PASS "Port $port reachable @ $destHost" ("LatencyMs={0}" -f $lat)
    } else {
      FAIL "Port $port reachable @ $destHost" "Blocked or unreachable"
    }
  } catch {
    FAIL "Port $port reachable @ $destHost" $_.Exception.Message
  }
}

try {
  $dns = (Resolve-DnsName github.com | Out-String)
  Write-Log -Name "dns_github_com.txt" -Content $dns | Out-Null
  PASS "DNS resolves github.com" "Logged"
} catch { FAIL "DNS resolves github.com" $_.Exception.Message }

Test-Port -destHost "github.com"     -port 443
Test-Port -destHost "api.github.com" -port 443
if($isSSH){ Test-Port -destHost "github.com" -port 22 }

# --- 5) HTTPS/TLS probe ------------------------------------------------------
try {
  $resp = Invoke-WebRequest -Uri "https://github.com" -Method Head -TimeoutSec 10 -UseBasicParsing
  PASS "HTTPS TLS to github.com" ("StatusCode={0}" -f $resp.StatusCode)
} catch { FAIL "HTTPS TLS to github.com" $_.Exception.Message }

# --- 6) Auth status via gh ---------------------------------------------------
if($ghOk){
  try {
    $auth = (gh auth status) 2>&1
    Write-Log -Name "gh_auth_status.txt" -Content ($auth | Out-String) | Out-Null
    if($LASTEXITCODE -eq 0){ PASS "GitHub auth (gh)" "See gh_auth_status.txt" } else { FAIL "GitHub auth (gh)" $auth }
  } catch { FAIL "GitHub auth (gh)" $_.Exception.Message }
} else {
  PASS "GitHub auth (gh)" "gh not installed—skipping (HTTPS may work via Git Credential Manager)"
}

# --- 7) SSH auth (if using SSH remote) ---------------------------------------
if($isSSH){
  try {
    $svc = Get-Service -Name ssh-agent -ErrorAction Stop
    if($svc.Status -ne 'Running'){ Start-Service ssh-agent | Out-Null }
    PASS "ssh-agent running" (Get-Service ssh-agent).Status
  } catch { FAIL "ssh-agent running" "Service missing or failed to start: $($_.Exception.Message)" }

  try {
    $keys = (ssh-add -l) 2>&1
    Write-Log -Name "ssh_keys.txt" -Content ($keys | Out-String) | Out-Null
    if($LASTEXITCODE -eq 0){ PASS "ssh-agent keys loaded" "See ssh_keys.txt" } else { FAIL "ssh-agent keys loaded" $keys }
  } catch { FAIL "ssh-agent keys loaded" $_.Exception.Message }

  try {
    $probe = (ssh -T -o BatchMode=yes git@github.com) 2>&1
    Write-Log -Name "ssh_probe.txt" -Content ($probe | Out-String) | Out-Null
    if($probe -match "successfully authenticated" -or $probe -match "Hi .*! You've successfully authenticated" -or $LASTEXITCODE -in 0,1){
      PASS "SSH auth to git@github.com" "Handshake ok (see ssh_probe.txt)"
    } else {
      FAIL "SSH auth to git@github.com" ("Exit={0} (see ssh_probe.txt)" -f $LASTEXITCODE)
    }
  } catch { FAIL "SSH auth to git@github.com" $_.Exception.Message }
}

# --- 8) Remote reach & perms -------------------------------------------------
try {
  $lsRem = (git ls-remote origin HEAD) 2>&1
  Write-Log -Name "ls_remote_origin.txt" -Content ($lsRem | Out-String) | Out-Null
  if($LASTEXITCODE -eq 0){ PASS "Remote read (ls-remote)" "origin reachable" } else { FAIL "Remote read (ls-remote)" $lsRem }
} catch { FAIL "Remote read (ls-remote)" $_.Exception.Message }

try {
  $dry = (git push --dry-run) 2>&1
  Write-Log -Name "push_dry_run.txt" -Content ($dry | Out-String) | Out-Null
  if($LASTEXITCODE -eq 0){ PASS "Remote write (push --dry-run)" "Likely push permissions ok" } else { FAIL "Remote write (push --dry-run)" $dry }
} catch { FAIL "Remote write (push --dry-run)" $_.Exception.Message }

# --- 9) Config hygiene -------------------------------------------------------
try {
  $cfg = (git config --list --show-origin | Sort-Object | Out-String)
  Write-Log -Name "git_config.txt" -Content $cfg | Out-Null
  $user = git config user.name
  $mail = git config user.email
  PASS "Git identity" ("user.name={0}; user.email={1}" -f $user,$mail)
} catch { FAIL "Git identity" $_.Exception.Message }

try {
  $crlf = git config core.autocrlf
  PASS "core.autocrlf" ("value='{0}' (Windows Unity repos often 'true')" -f $crlf)
} catch { PASS "core.autocrlf" "not set" }

# --- 10) LFS health ----------------------------------------------------------
if($lfsOk){
  try {
    $env = (git lfs env) 2>&1
    Write-Log -Name "git_lfs_env.txt" -Content ($env | Out-String) | Out-Null
    PASS "LFS env captured" "See git_lfs_env.txt"
  } catch { FAIL "LFS env captured" $_.Exception.Message }
}

# --- Final summary -----------------------------------------------------------
$summary = $results | ConvertTo-Csv -NoTypeInformation
$summaryPath = Write-Log -Name "summary.csv" -Content ($summary -join [Environment]::NewLine)
$failCount = ($results | Where-Object {$_.Status -eq 'FAIL'}).Count

$results | Format-Table -AutoSize | Out-String | Write-Host
Write-Host ""

if($failCount -eq 0){
  Write-Host ("PASS: Git/GitHub health checks passed. Logs: {0}" -f $logDir)
} else {
  Write-Host ("FAIL: {0} check(s) failed. Inspect: {1}" -f $failCount,$logDir)
  exit 2
}

if($repoRoot){ Pop-Location }
