# Tools\AQ-GitFixUpstream.ps1
# Purpose: Fix "no upstream branch" safely without hangs: set upstream via config (no network),
#          verify upstream, optionally push non-interactively with a hard timeout.
# Platform: PowerShell 7.x · Encoding: UTF-8 · Newlines: CRLF
# Logs: _audit\git\upstream\<stamp>\
# Usage: pwsh -NoProfile -ExecutionPolicy Bypass .\Tools\AQ-GitFixUpstream.ps1

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# --- toggles -----------------------------------------------------------------
$DoRealPush   = $true     # set to $false to skip the actual push
$GitTimeoutMs = 30000     # per-command timeout (ms) to prevent hangs

# --- logging -----------------------------------------------------------------
$stamp  = Get-Date -Format "yyyyMMdd_HHmmss"
$root   = (& git.exe rev-parse --show-toplevel) 2>&1
if($LASTEXITCODE -ne 0){ throw "Not inside a git repository: $root" }
$root   = $root.Trim()
$logDir = Join-Path $root "_audit\git\upstream\$stamp"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

function Write-Log {
  param(
    [Parameter(Mandatory)][string]$Name,
    [AllowNull()][AllowEmptyString()][string]$Content
  )
  $path = Join-Path $logDir $Name
  if ($null -eq $Content) { $Content = "" }
  $Content | Out-File -FilePath $path -Encoding UTF8 -Force
  return $path
}

# Normalize any (string|string[]) → single trimmed string
function S { param($x) return (($x | Out-String) -replace "`r","").Trim() }

# --- process runner with timeout (never hangs) -------------------------------
function Invoke-Proc {
  param(
    [Parameter(Mandatory)][string]$FilePath,
    [Parameter(Mandatory)][string[]]$Args,
    [int]$TimeoutMs = 20000
  )
  if(-not $Args -or $Args.Count -eq 0){ throw "Invoke-Proc: Args may not be empty." }

  $psi = [System.Diagnostics.ProcessStartInfo]::new()
  $psi.FileName               = $FilePath
  $psi.ArgumentList.Clear()
  foreach($a in $Args){ [void]$psi.ArgumentList.Add($a) }
  $psi.RedirectStandardOutput = $true
  $psi.RedirectStandardError  = $true
  $psi.UseShellExecute        = $false
  $psi.CreateNoWindow         = $true

  $p = [System.Diagnostics.Process]::new()
  $p.StartInfo = $psi
  $null = $p.Start()
  $ok = $p.WaitForExit($TimeoutMs)

  if(-not $ok){
    try { $p.Kill($true) } catch {}
    return [pscustomobject]@{
      Code = 124; TimedOut = $true;
      Out = S $p.StandardOutput.ReadToEnd(); Err = S $p.StandardError.ReadToEnd();
      Cmd = "$FilePath " + ($Args -join ' ')
    }
  }
  [pscustomobject]@{
    Code = $p.ExitCode; TimedOut = $false;
    Out = S $p.StandardOutput.ReadToEnd(); Err = S $p.StandardError.ReadToEnd();
    Cmd = "$FilePath " + ($Args -join ' ')
  }
}

# Explicit helpers (do NOT shadow 'git')
function Invoke-Git {
  param(
    [Parameter(Mandatory)][string[]]$Args,
    [int]$TimeoutMs = $GitTimeoutMs
  )
  if(-not $Args -or $Args.Count -eq 0){ throw "Invoke-Git: Args may not be empty." }
  $r = Invoke-Proc -FilePath "git.exe" -Args $Args -TimeoutMs $TimeoutMs
  [pscustomobject]@{ Code=$r.Code; Text=S($r.Out+"`n"+$r.Err); Cmd=$r.Cmd; TimedOut=$r.TimedOut }
}
function Invoke-GitNI {
  param(
    [Parameter(Mandatory)][string[]]$Args,
    [int]$TimeoutMs = $GitTimeoutMs
  )
  if(-not $Args -or $Args.Count -eq 0){ throw "Invoke-GitNI: Args may not be empty." }
  $niArgs = @('-c','credential.interactive=never') + $Args
  Invoke-Git -Args $niArgs -TimeoutMs $TimeoutMs
}

# --- tabular result helpers --------------------------------------------------
$results = [System.Collections.Generic.List[object]]::new()
function PASS { param($Check,$Detail) $results.Add([pscustomobject]@{Check=$Check;Status='PASS';Detail=$Detail}) }
function FAIL { param($Check,$Detail) $results.Add([pscustomobject]@{Check=$Check;Status='FAIL';Detail=$Detail}) }

# --- context -----------------------------------------------------------------
try { PASS "PowerShell" $PSVersionTable.PSVersion.ToString() } catch { FAIL "PowerShell" $_.Exception.Message }

$gitVer = S ((& git.exe --version) 2>&1)
if($LASTEXITCODE -eq 0){ PASS "git" $gitVer } else { FAIL "git" $gitVer }

$branch = S ((& git.exe rev-parse --abbrev-ref HEAD) 2>&1)
if($LASTEXITCODE -ne 0){ throw "Unable to get current branch: $branch" }

$origin = S ((& git.exe remote get-url origin) 2>&1)
if($LASTEXITCODE -ne 0){ throw "No 'origin' remote configured: $origin" }

Write-Log "remotes.txt" (S ((& git.exe remote -v) 2>&1)) | Out-Null
PASS "Repo root" $root
PASS "Branch"    $branch
PASS "origin"    $origin

# --- STEP 1: Set upstream via config (no network) ----------------------------
# branch.<name>.remote = origin
# branch.<name>.merge  = refs/heads/<name>
$cfgA = Invoke-Git -Args @('config','--local',"branch.$branch.remote",'origin')
$cfgB = Invoke-Git -Args @('config','--local',"branch.$branch.merge","refs/heads/$branch")
Write-Log "config_set.txt" ($cfgA.Text + "`n" + $cfgB.Text) | Out-Null
if($cfgA.Code -eq 0 -and $cfgB.Code -eq 0){
  PASS "Set upstream (config)" "branch.$branch.remote=origin; branch.$branch.merge=refs/heads/$branch"
} else {
  FAIL "Set upstream (config)" "See config_set.txt"
  $results | Format-Table -AutoSize | Out-String | Write-Host
  exit 2
}

# --- STEP 2: Verify upstream locally ----------------------------------------
$rev = Invoke-Git -Args @('rev-parse','--abbrev-ref','--symbolic-full-name','@{u}')
Write-Log "rev_parse_upstream.txt" $rev.Text | Out-Null
if($rev.Code -eq 0 -and $rev.Text -match '^origin/'){
  PASS "Verify upstream" $rev.Text
} else {
  $msg = if([string]::IsNullOrWhiteSpace($rev.Text)){"None"}else{$rev.Text}
  FAIL "Verify upstream" $msg
  $results | Format-Table -AutoSize | Out-String | Write-Host
  exit 2
}

# --- STEP 3: Remote reachability (read; non-interactive) ---------------------
$ls = Invoke-GitNI -Args @('ls-remote','origin','HEAD')
Write-Log "ls_remote_origin.txt" $ls.Text | Out-Null
if($ls.TimedOut){ FAIL "Remote read (ls-remote)" "Timed out: $($ls.Cmd)"; exit 2 }
if($ls.Code -eq 0){ PASS "Remote read (ls-remote)" "origin reachable" } else { FAIL "Remote read (ls-remote)" $ls.Text; exit 2 }

# --- STEP 4: Dry-run push (no transfer; non-interactive) ---------------------
$dry = Invoke-GitNI -Args @('push','--dry-run')
Write-Log "push_dry_run.txt" $dry.Text | Out-Null
if($dry.TimedOut){ FAIL "Push --dry-run" "Timed out: $($dry.Cmd)"; exit 2 }
if($dry.Code -eq 0){ PASS "Push --dry-run" "OK" } else { FAIL "Push --dry-run" $dry.Text; exit 2 }

# --- STEP 5: Real push (optional), no hooks, timeout guarded -----------------
if($DoRealPush){
  $push = Invoke-GitNI -Args @('push','--no-verify','--porcelain')
  Write-Log "push_result.txt" $push.Text | Out-Null
  if($push.TimedOut){
    FAIL "Push" "Timed out after ${GitTimeoutMs}ms. See push_result.txt"
    $results | Format-Table -AutoSize | Out-String | Write-Host
    exit 2
  }
  if($push.Code -eq 0){
    PASS "Push" "origin/$branch updated"
  } else {
    FAIL "Push" $push.Text
    $results | Format-Table -AutoSize | Out-String | Write-Host
    Write-Host "Hint: If this is auth, run: gh auth status; gh auth login (HTTPS); then re-run."
    exit 2
  }
} else {
  PASS "Push (skipped)" "Set DoRealPush=\$true to push"
}

# --- summary -----------------------------------------------------------------
$summary = $results | ConvertTo-Csv -NoTypeInformation
Write-Log "summary.csv" ($summary -join [Environment]::NewLine) | Out-Null
$fails = ($results | Where-Object {$_.Status -eq 'FAIL'}).Count

$results | Format-Table -AutoSize | Out-String | Write-Host
if($fails -eq 0){
  Write-Host "PASS: Upstream configured and verified$(if($DoRealPush){"; push ok"}). Logs: $logDir"
} else {
  Write-Host "FAIL: $fails check(s) failed. See: $logDir"
  exit 2
}
