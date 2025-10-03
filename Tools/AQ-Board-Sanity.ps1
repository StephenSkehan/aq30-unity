<#PSScriptInfo
.VERSION 1.0.1
.GUID 6f8b5a9c-b8d7-4ae2-8e52-7d2e5e2d5b01
.AUTHOR AQ30 Build/Tools
#>

<#
.SYNOPSIS
Read-only sanity audit of a Unity merge-board scene (slots, DragLayer, EventSystem, Grid spacing).

.DESCRIPTION
Parses a Unity .unity YAML scene directly (no Unity process). Verifies:
- Scene presence (explicit or discovered)
- 63 pre-placed slots named slot_rr_cc for Rows x Cols (default 9 x 7)
- Presence of DragLayer and EventSystem GameObjects
- GridLayoutGroup spacing near "MergeBoard" == {x:2,y:2}

Writes _audit\board_sanity_*.txt and .json with INFO/WARN/FAIL lines and structured data.

.PARAMETER ScenePath
Relative path to a .unity file (e.g., Assets/Scenes/MergeBoard_Demo.unity). If omitted, discovers.

.PARAMETER Root
Repo root (default ".").

.PARAMETER Rows
Expected rows (default 9).

.PARAMETER Cols
Expected cols (default 7).

.PARAMETER Quiet
Suppress INFO in console; WARN/FAIL still print.

.EXAMPLE
pwsh Tools\AQ-Board-Sanity.ps1

.EXAMPLE
pwsh Tools\AQ-Board-Sanity.ps1 -ScenePath 'Assets/Scenes/MergeBoard_Demo.unity'
#>

[CmdletBinding()]
param(
    [string]$ScenePath,
    [string]$Root = ".",
    [int]$Rows = 9,
    [int]$Cols = 7,
    [switch]$Quiet
)

Set-StrictMode -Version 2
$ErrorActionPreference = 'Stop'

# ------------------------------ logging --------------------------------------
$script:LogLines = New-Object System.Collections.Generic.List[string]
function Write-Log {
    param(
        [ValidateSet('INFO','WARN','FAIL')][string]$Level,
        [Parameter(Mandatory)][string]$Message
    )
    $line = "{0}: {1}" -f $Level, $Message
    $script:LogLines.Add($line) | Out-Null
    if ($Level -eq 'INFO' -and $Quiet) { return }
    $color = @{ INFO='Gray'; WARN='Yellow'; FAIL='Red' }[$Level]
    Write-Host $line -ForegroundColor $color
}

# ------------------------------ helpers --------------------------------------
function Resolve-RepoPath([Parameter(Mandatory)][string]$p) {
    (Resolve-Path -LiteralPath $p).Path
}

function As-Array($value) {
    if ($null -eq $value) { return @() }
    if ($value -is [System.Collections.IEnumerable] -and -not ($value -is [string])) { return @($value) }
    return @($value)
}

function Find-Scene {
    param(
        [Parameter(Mandatory)][string]$Base,
        [string]$ExplicitPath
    )
    $assets = Join-Path $Base 'Assets'

    if ($ExplicitPath) {
        $candidate = Join-Path $Base $ExplicitPath
        if (Test-Path -LiteralPath $candidate) { return $candidate }
        Write-Log FAIL "Scene not found: $ExplicitPath"
        return $null
    }

    $candidates = @(
        'Assets/Scenes/MergeBoard_Demo.unity',
        'Assets/Scenes/MergeBoard.unity'
    ) | ForEach-Object { Join-Path $Base $_ }

    foreach ($c in $candidates) {
        if (Test-Path -LiteralPath $c) { return $c }
    }

    $mergeHits = As-Array (Get-ChildItem -LiteralPath $assets -Recurse -Filter *.unity -File | Where-Object { $_.Name -match 'MergeBoard' })
    if ($mergeHits.Count -gt 0) { return $mergeHits[0].FullName }

    $any = Get-ChildItem -LiteralPath $assets -Recurse -Filter *.unity -File | Select-Object -First 1
    if ($any) {
        Write-Log WARN "No MergeBoard scene found; falling back to first scene: $($any.FullName.Substring($Base.Length+1))"
        return $any.FullName
    }

    Write-Log FAIL "No .unity scene files found under Assets/."
    return $null
}

function Read-SceneYaml([Parameter(Mandatory)][string]$sceneFullPath) {
    Get-Content -LiteralPath $sceneFullPath -Raw -Encoding UTF8
}

function Parse-Slots {
    param(
        [Parameter(Mandatory)][string]$Yaml,
        [Parameter(Mandatory)][int]$Rows,
        [Parameter(Mandatory)][int]$Cols
    )
    # Match "m_Name: slot_rr_cc"
    $matches = [regex]::Matches($Yaml, 'm_Name:\s*slot_(\d{2})_(\d{2})')
    $names = New-Object System.Collections.Generic.List[string]
    foreach ($m in $matches) {
        $names.Add(($m.Groups[0].Value -replace 'm_Name:\s*','')) | Out-Null
    }
    $unique = As-Array ($names | Select-Object -Unique)

    $expected = New-Object System.Collections.Generic.List[string]
    for ($r=0; $r -lt $Rows; $r++) {
        for ($c=0; $c -lt $Cols; $c++) {
            $expected.Add( ('slot_{0}_{1}' -f $r.ToString('00'), $c.ToString('00')) ) | Out-Null
        }
    }

    $missing = As-Array ($expected | Where-Object { $_ -notin $unique })
    $extra   = As-Array ($unique   | Where-Object { $_ -notin $expected })

    [pscustomobject]@{
        CountFound  = $unique.Count
        Expected    = $expected
        Missing     = $missing
        Extra       = $extra
        AllFound    = ($missing.Count -eq 0 -and $extra.Count -eq 0)
        Names       = $unique
    }
}

function Find-Name {
    param(
        [Parameter(Mandatory)][string]$Yaml,
        [Parameter(Mandatory)][string]$Name
    )
    [regex]::IsMatch($Yaml, ('m_Name:\s*{0}\b' -f [regex]::Escape($Name)))
}

function Parse-GridSpacingNearMergeBoard {
    param([Parameter(Mandatory)][string]$Yaml)
    $rxMB = [regex]'m_Name:\s*MergeBoard\b'
    $m = $rxMB.Match($Yaml)
    if (-not $m.Success) { return $null }

    $start = $m.Index
    $window = $Yaml.Substring($start, [Math]::Min(8000, $Yaml.Length - $start)) # expand window to be safe

    $rxSpacing = [regex]'m_Spacing:\s*{x:\s*([-\d\.]+),\s*y:\s*([-\d\.]+)}'
    $sm = $rxSpacing.Match($window)
    if (-not $sm.Success) { return $null }

    [pscustomobject]@{
        X = [double]$sm.Groups[1].Value
        Y = [double]$sm.Groups[2].Value
        Source = 'MergeBoard'
    }
}

# ------------------------------- main ----------------------------------------
try {
    $repoRoot = Resolve-RepoPath $Root
    $sceneFull = Find-Scene -Base $repoRoot -ExplicitPath $ScenePath
    if (-not $sceneFull) { exit 1 }

    $sceneRel = $sceneFull.Substring($repoRoot.Length + 1)
    Write-Log INFO "Scene: $sceneRel"

    $yaml = Read-SceneYaml -sceneFullPath $sceneFull

    # Slots
    $slots = Parse-Slots -Yaml $yaml -Rows $Rows -Cols $Cols
    if ($slots.AllFound) {
        Write-Log INFO "Slots: $($slots.CountFound) found (expected $($Rows*$Cols)) — OK"
    } else {
        Write-Log WARN "Slots: $($slots.CountFound) found (expected $($Rows*$Cols))"
        if ($slots.Missing.Count -gt 0) { Write-Log FAIL ("Missing: " + ($slots.Missing -join ', ')) }
        if ($slots.Extra.Count   -gt 0) { Write-Log WARN ("Extra: "   + ($slots.Extra   -join ', ')) }
    }

    # DragLayer / EventSystem
    $hasDragLayer   = Find-Name -Yaml $yaml -Name 'DragLayer'
    $hasEventSystem = Find-Name -Yaml $yaml -Name 'EventSystem'
    Write-Log (@{ $true='INFO'; $false='FAIL'}[$hasDragLayer])   ("DragLayer: "   + (@{ $true='present'; $false='missing' }[$hasDragLayer]))
    Write-Log (@{ $true='INFO'; $false='FAIL'}[$hasEventSystem]) ("EventSystem: " + (@{ $true='present'; $false='missing' }[$hasEventSystem]))

    # Grid spacing
    $spacing = Parse-GridSpacingNearMergeBoard -Yaml $yaml
    $x=$null; $y=$null; $spacingOK=$false
    if ($null -eq $spacing) {
        Write-Log WARN "GridLayoutGroup spacing not found near 'MergeBoard' — verify object name/component."
    } else {
        $x = $spacing.X; $y = $spacing.Y
        $spacingOK = ($x -eq 2 -and $y -eq 2)
        Write-Log (@{ $true='INFO'; $false='WARN'}[$spacingOK]) "Grid spacing (near MergeBoard): x=$x y=$y (expected 2,2)"
    }

    # Report object
    $report = [ordered]@{
        Timestamp          = (Get-Date).ToString('o')
        RepoRoot           = $repoRoot
        ScenePath          = $sceneRel
        ExpectedRows       = $Rows
        ExpectedCols       = $Cols
        ExpectedSlots      = $Rows * $Cols
        SlotCountFound     = $slots.CountFound
        SlotsAllFound      = $slots.AllFound
        MissingSlots       = $slots.Missing
        ExtraSlots         = $slots.Extra
        DragLayerPresent   = $hasDragLayer
        EventSystemPresent = $hasEventSystem
        GridSpacing        = @{ x = $x; y = $y; Source = if ($spacing) { $spacing.Source } else { $null } }
        GridSpacingIs2x2   = $spacingOK
        Log                = $script:LogLines
    }

    # Output files
    $auditDir = Join-Path $repoRoot '_audit'
    if (-not (Test-Path -LiteralPath $auditDir)) { New-Item -ItemType Directory -Path $auditDir | Out-Null }
    $stamp = (Get-Date).ToString('yyyyMMdd_HHmmss')
    $base = Join-Path $auditDir ("board_sanity_{0}" -f $stamp)

    # TXT
    $header = "=== BOARD SANITY AUDIT: {0} @ {1} ===" -f $report.ScenePath, (Get-Date).ToString('yyyy-MM-dd HH:mm:ss')
    ($header + [Environment]::NewLine + ($script:LogLines -join [Environment]::NewLine)) |
        Out-File -LiteralPath ($base + '.txt') -Encoding UTF8

    # JSON
    ($report | ConvertTo-Json -Depth 8) | Out-File -LiteralPath ($base + '.json') -Encoding UTF8

    # Exit codes
    $hardFail = (-not $hasDragLayer) -or (-not $hasEventSystem) -or (-not $slots.AllFound)
    if ($hardFail) {
        Write-Log FAIL "Audit complete with failures. See $($base.Substring($repoRoot.Length+1)).txt and .json"
        exit 2
    }
    if (-not $spacingOK) {
        Write-Log WARN "Audit complete with warnings (grid spacing). See _audit output for details."
        exit 0
    }

    Write-Log INFO "Audit PASS. See _audit output for details."
    exit 0
}
catch {
    Write-Log FAIL $_.Exception.Message
    exit 1
}
