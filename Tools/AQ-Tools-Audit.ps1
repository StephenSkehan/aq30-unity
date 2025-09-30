#requires -Version 7.2
<#
  File: Tools\AQ-Tools-Audit.ps1
  Purpose: Read-only inventory of the Tools folder; highlight likely backup/checkpoint scripts to REUSE.
  Guarantees:
    - PowerShell 7.x, StrictMode Latest, idempotent.
    - .NET Regex only (no PCRE flags). No ambiguous -join usage. No TrimStart pitfalls.
    - Null-safe joins and counts; safe relative-path computation.
  Outputs:
    - _audit\tools_inventory_YYYYMMDD_HHmmss.txt
    - _audit\tools_inventory_YYYYMMDD_HHmmss.json
  Usage:
    pwsh Tools\AQ-Tools-Audit.ps1 -Root "C:\\Users\\Steph\\Dev\\aq30-unity\\Tools"
#>
[CmdletBinding()]
param(
  [string]$Root = (Join-Path $PSScriptRoot '.')
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Info($m){ Write-Host "INFO: $m" }
function Pass($m){ Write-Host "PASS: $m" -ForegroundColor Green }
function Warn($m){ Write-Host "WARN: $m" -ForegroundColor Yellow }
function Fail($m){ Write-Host "FAIL: $m" -ForegroundColor Red }

try {
  if(-not (Test-Path -LiteralPath $Root)) { throw "Tools path not found: $Root" }
  $rootResolved = (Resolve-Path -LiteralPath $Root).Path
  Info "Auditing Tools folder: $rootResolved"

  # Place artifacts under repo/_audit (relative to provided Tools root)
  $repoRoot = Split-Path -Path $rootResolved -Parent
  $stamp = Get-Date -Format 'yyyyMMdd_HHmmss'
  $auditDir = Join-Path $repoRoot '_audit'
  New-Item -ItemType Directory -Force -Path $auditDir | Out-Null
  $txtPath = Join-Path $auditDir ("tools_inventory_{0}.txt" -f $stamp)
  $jsonPath = Join-Path $auditDir ("tools_inventory_{0}.json" -f $stamp)

  $files = Get-ChildItem -LiteralPath $rootResolved -Recurse -File -ErrorAction Stop
  $byExt = @($files | Group-Object Extension | Sort-Object Count -Descending)
  $ps1 = @($files | Where-Object { $_.Extension -ieq '.ps1' })
  $cmd = @($files | Where-Object { $_.Extension -in @('.cmd','.bat') })
  $sh  = @($files | Where-Object { $_.Extension -ieq '.sh' })

  # Heuristics to detect backup/checkpoint tools
  $heur = @('backup','checkpoint','zip','archive','compress-archive','7z','git tag','git  tag','lfs','snapshot','export','bundle')

  $results = New-Object System.Collections.Generic.List[hashtable]
  $candidates = New-Object System.Collections.Generic.List[hashtable]

  foreach($f in $files){
    # Cross-platform relative path
    $rel = [System.IO.Path]::GetRelativePath($rootResolved, $f.FullName)

    $entry = [ordered]@{
      path       = $rel
      ext        = $f.Extension
      sizeKB     = [math]::Round($f.Length/1KB,1)
      modified   = $f.LastWriteTime.ToString('u')
      sha256     = (Get-FileHash -Algorithm SHA256 -LiteralPath $f.FullName).Hash
      header     = ''
      parameters = @()
      hits       = @()
    }

    if($f.Extension -ieq '.ps1'){
      $head = @(Get-Content -LiteralPath $f.FullName -TotalCount 120 -Encoding UTF8)
      $headerLines = @($head | Where-Object { $_ -match '^(#|//)' })
      $entry['header'] = if($headerLines.Count -gt 0){ [string]::Join("`n", $headerLines) } else { '' }

      $text = Get-Content -LiteralPath $f.FullName -Raw -Encoding UTF8
      # Multi-line param(...) via .NET Regex with Singleline+Multiline
      $m = [System.Text.RegularExpressions.Regex]::Match($text, '(?sm)^\s*param\s*\((?<pb>.*?)\)\s*')
      if($m.Success){
        $pb = $m.Groups['pb'].Value
        $names = [regex]::Matches($pb,'\[(?:[^\]]+)\]\s*\$(\w+)') | ForEach-Object { $_.Groups[1].Value }
        $entry['parameters'] = $names | Select-Object -Unique
      }

      foreach($h in $heur){ if([string]::IsNullOrEmpty($h) -eq $false -and $text -imatch [regex]::Escape($h)){ $entry['hits'] = @($entry['hits']) + $h } }

      if(($entry['hits']).Count -gt 0 -or $f.Name -match '(?i)backup|checkpoint|archive|zip|snapshot'){
        $candidates.Add($entry)
      }
    }

    $results.Add($entry)
  }

  $lines = New-Object System.Collections.Generic.List[string]
  $lines.Add("Tools inventory @ $rootResolved") | Out-Null
  $lines.Add("Scanned files: $($files.Count)") | Out-Null
  foreach($g in $byExt){ $lines.Add("  - $($g.Name): $($g.Count)") | Out-Null }
  $lines.Add("") | Out-Null
  $lines.Add("Top PowerShell scripts:") | Out-Null
  foreach($p in ($ps1 | Sort-Object LastWriteTime -Descending | Select-Object -First 15)){
    $relPs = [System.IO.Path]::GetRelativePath($rootResolved, $p.FullName)
    $lines.Add("  - $relPs  ($([math]::Round($p.Length/1KB,1)) KB)") | Out-Null
  }
  $lines.Add("") | Out-Null

  if($candidates.Count -gt 0){
    $lines.Add("Backup/Checkpoint candidates (by name/content):") | Out-Null
    foreach($c in $candidates){
      $hitArr = @($c['hits'] | Sort-Object -Unique)
      $hitStr = if($hitArr.Count -gt 0){ " [hits: " + ([string]::Join(', ', $hitArr)) + "]" } else { '' }
      $lines.Add("  - $($c['path'])$hitStr") | Out-Null
    }
  } else {
    $lines.Add("No backup/checkpoint candidates detected via heuristics.") | Out-Null
  }

  Set-Content -Encoding UTF8 -Path $txtPath -Value $lines
  ($results.ToArray() | ConvertTo-Json -Depth 8) | Set-Content -Encoding UTF8 -Path $jsonPath

  Pass ("Wrote {0} and {1} to _audit\\" -f ([System.IO.Path]::GetFileName($txtPath)), ([System.IO.Path]::GetFileName($jsonPath)))
  Pass ("PS1 files: {0}; CMD/BAT: {1}; SH: {2}" -f $ps1.Count, $cmd.Count, $sh.Count)

  if($candidates.Count -gt 0){
    $first = $candidates[0]
    Info ("Likely candidate: {0}" -f $first['path'])
    Info "Tip: inspect params and usage header, then run that existing tool for backups."
  }
}
catch {
  Fail $_
  exit 1
}
