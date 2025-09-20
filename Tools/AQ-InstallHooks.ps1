# Tools\AQ-InstallHooks.ps1
# Purpose: Install pre-commit hook that calls our LFS guard (works in PowerShell on Windows).

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = (& git.exe rev-parse --show-toplevel) 2>&1
if($LASTEXITCODE -ne 0){ throw "Not inside a git repository: $root" }
$root = $root.Trim()

# Ensure the guard exists
$guardRel = "Tools\Hooks\PreCommit-LFSGuard.ps1"
$guardAbs = Join-Path $root $guardRel
if(!(Test-Path $guardAbs)){ throw "Guard script missing: $guardRel (run this after adding it)" }

$hooksDir = Join-Path $root ".git\hooks"
New-Item -ItemType Directory -Force -Path $hooksDir | Out-Null

# Windows .cmd shim that Git will run
$preCommitCmd = @"
@echo off
REM Git hook shim → PowerShell guard
pwsh -NoProfile -ExecutionPolicy Bypass -File "%~dp0..\..\Tools\Hooks\PreCommit-LFSGuard.ps1"
set ERR=%ERRORLEVEL%
exit /B %ERR%
"@

$hookPath = Join-Path $hooksDir "pre-commit.cmd"
$preCommitCmd | Out-File -FilePath $hookPath -Encoding ASCII -Force

Write-Host "Installed pre-commit hook:"
Write-Host "  $hookPath"
