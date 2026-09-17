# Convert-MdToPdf.ps1
# Renders a SAS markdown file to .html (same style as the premise/pipeline exports) and .pdf via headless Chrome.
# Usage: powershell -File SAS/tools/Convert-MdToPdf.ps1 -Md SAS/some-file.md
# No Python on this machine; this replaces the earlier python builders for plain markdown docs.
param([Parameter(Mandatory=$true)][string]$Md)

$ErrorActionPreference = 'Stop'
$mdPath = (Resolve-Path $Md).Path
$htmlPath = [System.IO.Path]::ChangeExtension($mdPath, '.html')
$pdfPath  = [System.IO.Path]::ChangeExtension($mdPath, '.pdf')
$lines = [System.IO.File]::ReadAllLines($mdPath, [System.Text.Encoding]::UTF8)

$title = [System.IO.Path]::GetFileNameWithoutExtension($mdPath)
foreach ($l in $lines) { if ($l -match '<!--\s*pdf-title:\s*(.+?)\s*-->') { $title = $Matches[1]; break } }

function Esc([string]$s) { return $s.Replace('&','&amp;').Replace('<','&lt;').Replace('>','&gt;') }
function Inline([string]$s) {
  $s = Esc $s
  $s = [regex]::Replace($s, '`([^`]+)`', '<code>$1</code>')
  $s = [regex]::Replace($s, '\*\*(.+?)\*\*', '<strong>$1</strong>')
  $s = [regex]::Replace($s, '(?<![\w*])\*(?!\s)(.+?)(?<!\s)\*(?![\w*])', '<em>$1</em>')
  return $s
}

$out = New-Object System.Text.StringBuilder
$para = New-Object System.Collections.Generic.List[string]
$inTable = $false; $tableRow = 0; $listType = $null; $inQuote = $false
function FlushPara { if ($para.Count -gt 0) { [void]$out.Append('<p>' + (Inline ($para -join ' ')) + '</p>' + "`n"); $para.Clear() } }
function CloseTable { if ($script:inTable) { [void]$out.Append("</tbody></table>`n"); $script:inTable = $false; $script:tableRow = 0 } }
function CloseList { if ($script:listType) { [void]$out.Append("</$($script:listType)>`n"); $script:listType = $null } }
function CloseQuote { if ($script:inQuote) { [void]$out.Append("</blockquote>`n"); $script:inQuote = $false } }

foreach ($raw in $lines) {
  $line = $raw.TrimEnd()
  if ($line -match '^\s*<!--.*-->\s*$') { continue }
  $isQuote = $line -match '^>\s?'
  if ($isQuote) { $line = $line -replace '^>\s?', ''; if (-not $inQuote) { FlushPara; CloseTable; CloseList; [void]$out.Append("<blockquote>`n"); $inQuote = $true } }
  elseif ($inQuote -and $line -ne '') { FlushPara; CloseQuote }
  if ($line -eq '') { FlushPara; CloseTable; CloseList; if ($inQuote -and -not $isQuote) { CloseQuote }; continue }
  if ($line -match '^(#{1,4})\s+(.*)$') { FlushPara; CloseTable; CloseList; $n = $Matches[1].Length; [void]$out.Append("<h$n>" + (Inline $Matches[2]) + "</h$n>`n"); continue }
  if ($line -match '^-{3,}$') { FlushPara; CloseTable; CloseList; [void]$out.Append("<hr />`n"); continue }
  if ($line -match '^\|(.*)\|$') {
    FlushPara; CloseList
    $cells = $Matches[1].Split('|') | ForEach-Object { $_.Trim() }
    if (($cells | Where-Object { $_ -notmatch '^:?-{2,}:?$' }).Count -eq 0) { continue }
    if (-not $inTable) { [void]$out.Append("<table><thead><tr>" + (($cells | ForEach-Object { '<th>' + (Inline $_) + '</th>' }) -join '') + "</tr></thead><tbody>`n"); $inTable = $true; continue }
    [void]$out.Append('<tr>' + (($cells | ForEach-Object { '<td>' + (Inline $_) + '</td>' }) -join '') + "</tr>`n"); continue
  }
  if ($line -match '^\s*[-*]\s+(.*)$') { FlushPara; CloseTable; if ($listType -ne 'ul') { CloseList; [void]$out.Append("<ul>`n"); $listType = 'ul' }; [void]$out.Append('<li>' + (Inline $Matches[1]) + "</li>`n"); continue }
  if ($line -match '^\s*\d+\.\s+(.*)$') { FlushPara; CloseTable; if ($listType -ne 'ol') { CloseList; [void]$out.Append("<ol>`n"); $listType = 'ol' }; [void]$out.Append('<li>' + (Inline $Matches[1]) + "</li>`n"); continue }
  CloseTable; CloseList
  $para.Add($line.Trim())
}
FlushPara; CloseTable; CloseList; CloseQuote

$css = @'
  @page { size: A4; margin: 16mm 15mm 18mm; }
  body { font-family: Georgia, 'Times New Roman', serif; color: #1c1a17; margin: 0; font-size: 10.5pt; line-height: 1.55; }
  h1 { font-size: 20pt; letter-spacing: .2px; border-bottom: 3px solid #1c1a17; padding-bottom: 8px; margin: 0 0 14px; }
  h2 { font-size: 13.5pt; margin: 26px 0 8px; padding-bottom: 4px; border-bottom: 1.5px solid #1c1a17; page-break-after: avoid; }
  h3 { font-size: 11.5pt; margin: 18px 0 6px; page-break-after: avoid; }
  p { margin: 0 0 9px; }
  ul, ol { margin: 0 0 10px; padding-left: 20px; }
  li { margin-bottom: 4px; }
  hr { border: 0; border-top: 1px solid #ccc5b8; margin: 22px 0; }
  table { border-collapse: collapse; width: 100%; margin: 6px 0 14px; font-size: 8.6pt; line-height: 1.35; page-break-inside: auto; }
  th, td { border: 1px solid #b9b2a4; padding: 4px 6px; vertical-align: top; text-align: left; }
  th { background: #e9e4d8; font-weight: bold; }
  td { background: #faf8f2; }
  tr { page-break-inside: avoid; }
  blockquote { border-left: 3px solid #b9b2a4; margin: 8px 0 12px; padding: 4px 14px; background: #f4f1ea; }
  code { font-family: Consolas, monospace; font-size: 9pt; }
'@
$html = "<!DOCTYPE html><html><head><meta charset=`"utf-8`">`n<title>" + (Esc $title) + "</title>`n<style>`n$css`n</style></head><body>`n" + $out.ToString() + "</body></html>`n"
[System.IO.File]::WriteAllText($htmlPath, $html, (New-Object System.Text.UTF8Encoding($false)))

$chrome = 'C:\Program Files\Google\Chrome\Application\chrome.exe'
if (Test-Path $pdfPath) { Remove-Item $pdfPath -Force }
& $chrome --headless=new --disable-gpu --no-pdf-header-footer --print-to-pdf="$pdfPath" "file:///$($htmlPath -replace '\\','/')" 2>$null | Out-Null
$deadline = (Get-Date).AddSeconds(60)
while (-not (Test-Path $pdfPath) -and (Get-Date) -lt $deadline) { Start-Sleep -Milliseconds 500 }
if (Test-Path $pdfPath) { Write-Output "wrote $htmlPath and $pdfPath ($((Get-Item $pdfPath).Length) bytes)" } else { Write-Output "wrote $htmlPath; pdf not produced" }
