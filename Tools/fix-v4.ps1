<# 
fix-v4.ps1
-----------
Purpose: Minimal, safe fixer for a specific compile issue where
         Assets\App\Presentation\EventBusInstaller.cs may be missing:
             using AQ.SharedKernel.Events;

Notes:
- Idempotent: running multiple times is safe.
- ASCII-only. No smart quotes or Unicode punctuation.
- Uses correct escaping for $1 and explicit CRLF insertion.

Usage:
  pwsh -NoProfile -ExecutionPolicy Bypass -File .\Tools\fix-v4.ps1 -RepoRoot .
#>

[CmdletBinding(SupportsShouldProcess=$true)]
param(
    [Parameter(Mandatory=$false)]
    [string]$RepoRoot = "."
)

# -----------------------
# Helpers (ASCII only)
# -----------------------
function Info([string]$msg) { Write-Host "[INFO] $msg" -ForegroundColor Cyan }
function Good([string]$msg) { Write-Host "[ OK ] $msg" -ForegroundColor Green }
function Warn([string]$msg) { Write-Host "[WARN] $msg" -ForegroundColor Yellow }
function Bad ([string]$msg) { Write-Host "[FAIL] $msg" -ForegroundColor Red }

function Resolve-RepoPath([string]$root, [string]$rel) {
    $base = Resolve-Path -LiteralPath $root -ErrorAction Stop
    return (Join-Path -Path $base -ChildPath $rel)
}

# Safe file read/write with UTF8 (no BOM)
function Read-AllText([string]$path) {
    return Get-Content -Raw -LiteralPath $path -ErrorAction Stop
}
function Write-AllText([string]$path, [string]$text) {
    Set-Content -LiteralPath $path -Value $text -Encoding UTF8 -ErrorAction Stop
}

# Insert a using before the first "namespace" if the using is missing
function Ensure-Using-Before-Namespace {
    param(
        [Parameter(Mandatory=$true)] [string]$FilePath,
        [Parameter(Mandatory=$true)] [string]$UsingNamespace
    )

    if (-not (Test-Path -LiteralPath $FilePath)) {
        Bad "File not found: $FilePath"
        return $false
    }

    $content = Read-AllText -path $FilePath

    # If already present, nothing to do
    $usingPattern = "^\s*using\s+$([Regex]::Escape($UsingNamespace));"
    if ($content -match $usingPattern) {
        Good "Already imports $UsingNamespace in: $FilePath"
        return $true
    }

    # Insert using immediately before first 'namespace' line.
    # We capture the newline preceding 'namespace' so we can preserve it via `$1`
    # and then add an explicit CRLF followed by the using statement.
    $pattern = '(\r?\n)namespace'
    $replacement = "`$1using $UsingNamespace;`r`nnamespace"

    if ($content -notmatch '^\s*namespace' -and $content -notmatch $pattern) {
        Warn "No 'namespace' token found; will prepend using at top in: $FilePath"
        $newContent = "using $UsingNamespace;`r`n" + $content
    } else {
        $newContent = [Regex]::Replace($content, $pattern, $replacement, 
            [System.Text.RegularExpressions.RegexOptions]::Multiline)
    }

    if ($PSCmdlet.ShouldProcess($FilePath, "Insert 'using $UsingNamespace;'")) {
        Write-AllText -path $FilePath -text $newContent
        Good "Patched to include using $UsingNamespace in: $FilePath"
    }

    return $true
}

# -----------------------
# Main
# -----------------------
try {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot -ErrorAction Stop).Path
} catch {
    Bad "RepoRoot does not exist: $RepoRoot"
    exit 1
}

Info "RepoRoot: $RepoRoot"

# Target file and namespace
$eventBusInstaller = Resolve-RepoPath $RepoRoot 'Assets\App\Presentation\EventBusInstaller.cs'
$targetNamespace   = 'AQ.SharedKernel.Events'

# Apply patch
$ok = Ensure-Using-Before-Namespace -FilePath $eventBusInstaller -UsingNamespace $targetNamespace

if ($ok) {
    Good "fix-v4 completed."
    exit 0
} else {
    Bad "fix-v4 encountered an issue."
    exit 1
}
