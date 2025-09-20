# Tools\AQ-PushUpstream.ps1
# Purpose: Push current branch to origin, create/set upstream if needed, then verify (no hangs).
# Platform: PowerShell 7.x · Encoding: UTF-8 · Newlines: CRLF
# Logs: _audit\git\push\<stamp>\
# Usage: pwsh -NoProfile -ExecutionPolicy Bypass .\Tools\AQ-PushUpstream.ps1

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ---- knobs -------------------------------------------------------------------
$GitTimeoutMs = 30000   # per-command timeout (ms)
$UseNoVerify  = $true   # skip git hooks that can hang; set $false if you rely on pre-push hooks

# ---- logging -----------------------------------------------------------------
$stamp  = Get-Date -Format "yyyyMMdd_HHmmss"
$root   = (& git.exe rev-parse --show-toplevel) 2>&1
if($LASTEXITCODE -ne 0){ throw "Not inside a git repository: $root" }
$root   = $root.Trim()
$logDir = Join-Path $root "_audit\git\push\$stamp"
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
function S { param($x) return (($x | Out-String) -replace "`r","").Trim() }

# ---- proc runner (timeout; never hangs) --------------------------------------
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

# ---- explicit git helpers (no shadowing) -------------------------------------
function Invoke-Git {
  param([Parameter(Mandatory)][string[]]$Args,[int]$TimeoutMs=$GitTimeoutMs)
  $r = Invoke-Proc -FilePath "git.exe" -Args $Args -TimeoutMs $TimeoutMs
  [pscustomobject]@{ Code=$r.Code; Text=S($r.Out+"`n"+$r.Err); Cmd=$r.Cmd; TimedOut=$r.TimedOut }
}
function Invoke-GitNI {
  param([Parameter(Mandatory)][string[]]$Args,[int]$TimeoutMs=$GitTimeoutMs)
  Invoke-Git -Args (@('-c','credential.interactive=never') + $Args) -TimeoutMs $TimeoutMs
}

# ---- table helpers -----------------------------------------------------------
$rows = [System.Collections.Generic.List[object]]::new()
function ROW { param($K,$S,$D) $rows.Add([pscustomobject]@{Check=$K;Status=$S;Detail=$D}) }

# ---- context -----------------------------------------------------------------
$branch = S ((& git.exe rev-parse --abbrev-ref HEAD) 2>&1)
if($LASTEXITCODE -ne 0){ throw "Unable to get current branch: $branch" }
$origin = S ((& git.exe remote get-url origin) 2>&1)
if($LASTEXITCODE -ne 0){ throw "No 'origin' remote configured: $origin" }

Write-Log "remotes.txt" (S ((& git.exe remote -v) 2>&1)) | Out-Null
ROW "Repo root" "PASS" $root
ROW "Branch"    "PASS" $branch
ROW "origin"    "PASS" $origin

# ---- upstream configured locally? -------------------------------------------
$u = ( & git.exe rev-parse --abbrev-ref --symbolic-full-name '@{u}' ) 2>&1
$hasUpstream = ($LASTEXITCODE -eq 0 -and ($u | Out-String) -notmatch 'fatal:|no upstream')
if($hasUpstream){ ROW "Local upstream" "PASS" (S $u) } else { ROW "Local upstream" "FAIL" "None" }

# ---- remote branch exists? (read-only) --------------------------------------
$heads = Invoke-GitNI -Args @('ls-remote','--heads','origin',"$branch")
Write-Log "ls_remote_heads.txt" $heads.Text | Out-Null
$remoteExists = ($heads.Code -eq 0 -and ($heads.Text.Length -gt 0))
ROW "Remote branch presence" "PASS" ($(if($remoteExists){"exists"}else{"absent"}))

# ---- ensure local upstream keys (no network) --------------------------------
if(-not $hasUpstream){
  $cfgA = Invoke-Git -Args @('config','--local',"branch.$branch.remote",'origin')
  $cfgB = Invoke-Git -Args @('config','--local',"branch.$branch.merge","refs/heads/$branch")
  Write-Log "config_set.txt" ($cfgA.Text + "`n" + $cfgB.Text) | Out-Null
  if($cfgA.Code -eq 0 -and $cfgB.Code -eq 0){
    ROW "Set upstream (config)" "PASS" "branch.$branch.remote=origin; branch.$branch.merge=refs/heads/$branch"
    $hasUpstream = $true
  } else {
    ROW "Set upstream (config)" "FAIL" "See config_set.txt"
    $rows | Format-Table -AutoSize | Out-String | Write-Host
    throw "Could not set local upstream."
  }
}

# ---- decide push command -----------------------------------------------------
$pushArgs = @('push')
if($UseNoVerify){ $pushArgs += '--no-verify' }
$pushArgs += '--porcelain'

# If remote branch does not exist, force creation and tracking
if(-not $remoteExists){
  $pushArgs = @('push','--set-upstream','origin',"$branch")
  if($UseNoVerify){ $pushArgs += '--no-verify' }
  $pushArgs += '--porcelain'
}

# ---- dry-run first (fast-fail auth/perm) ------------------------------------
$dry = Invoke-GitNI -Args (@('push','--dry-run'))
Write-Log "push_dry_run.txt" $dry.Text | Out-Null
if($dry.TimedOut){ ROW "Push --dry-run" "FAIL" "Timed out: $($dry.Cmd)"; $rows | Format-Table -AutoSize | Out-String | Write-Host; exit 2 }
if($dry.Code -eq 0){ ROW "Push --dry-run" "PASS" "OK" } else { ROW "Push --dry-run" "FAIL" $dry.Text; $rows | Format-Table -AutoSize | Out-String | Write-Host; exit 2 }

# ---- real push (non-interactive, timeout) -----------------------------------
$push = Invoke-GitNI -Args $pushArgs
Write-Log "push_result.txt" $push.Text | Out-Null
if($push.TimedOut){ ROW "Push" "FAIL" "Timed out: $($push.Cmd)"; $rows | Format-Table -AutoSize | Out-String | Write-Host; exit 2 }
if($push.Code -eq 0){ ROW "Push" "PASS" ("Executed: " + ($pushArgs -join ' ')) } else { ROW "Push" "FAIL" $push.Text; $rows | Format-Table -AutoSize | Out-String | Write-Host; exit 2 }

# ---- verify upstream again ---------------------------------------------------
$u2 = ( & git.exe rev-parse --abbrev-ref --symbolic-full-name '@{u}' ) 2>&1
if($LASTEXITCODE -eq 0 -and ($u2 | Out-String) -match '^origin/'){
  ROW "Verify upstream" "PASS" (S $u2)
} else {
  ROW "Verify upstream" "FAIL" (S $u2)
  $rows | Format-Table -AutoSize | Out-String | Write-Host
  exit 2
}

# ---- verify remote head exists now ------------------------------------------
$heads2 = Invoke-GitNI -Args @('ls-remote','--heads','origin',"$branch")
Write-Log "ls_remote_heads_after.txt" $heads2.Text | Out-Null
$remoteExists2 = ($heads2.Code -eq 0 -and ($heads2.Text.Length -gt 0))
ROW "Remote branch presence (after)" ($(if($remoteExists2){"PASS"}else{"FAIL"})) ($(if($remoteExists2){"exists"}else{"absent"}))

# ---- summary ----------------------------------------------------------------
$rows | Format-Table -AutoSize | Out-String | Write-Host
Write-Log "summary.txt" ($rows | Format-Table -AutoSize | Out-String) | Out-Null
Write-Host "Logs: $logDir"
if(-not $remoteExists2){ exit 2 }
