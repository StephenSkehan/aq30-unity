param(
  [string]$RepoRoot = "."
)

# --- Helpers ---
function Read-Asmdef($path){
  if(!(Test-Path $path -PathType Leaf)){
    throw "Missing asmdef: $path"
  }
  (Get-Content -Raw $path | ConvertFrom-Json)
}
function Write-Asmdef($path, $obj){
  $json = $obj | ConvertTo-Json -Depth 20
  # Preserve CRLF to keep Unity happy on Windows
  [System.IO.File]::WriteAllText((Resolve-Path $path), $json, [System.Text.Encoding]::UTF8)
}

function Ensure-Refs([object]$asmdef, [string[]]$names){
  if(-not $asmdef.references){ $asmdef | Add-Member -NotePropertyName references -NotePropertyValue @() }
  $set = [System.Collections.Generic.HashSet[string]]::new([string[]]$asmdef.references)
  foreach($n in $names){ $null = $set.Add($n) }
  $asmdef.references = @($set)
  return $asmdef
}

# --- Canonical paths (do not create new files; only edit if present) ---
$appAsm   = Join-Path $RepoRoot "Assets\App\AQ.App.asmdef"
$presAsm  = Join-Path $RepoRoot "Packages\com.aq.presentation\Runtime\AQ30.Presentation.asmdef"
$editAsm  = Join-Path $RepoRoot "Assets\Editor\AQ.Editor.asmdef"

Write-Host "Wiring asmdefs..." -ForegroundColor Cyan

# 1) Presentation must see Domain + SharedKernel
try {
  $pres = Read-Asmdef $presAsm
  $pres = Ensure-Refs $pres @("AQ.Domain.Merge","AQ.SharedKernel")
  Write-Asmdef $presAsm $pres
  Write-Host "  Updated: $presAsm -> refs Domain.Merge, SharedKernel"
} catch {
  Write-Warning $_
}

# 2) App must see Presentation + Domain + SharedKernel
try {
  $app = Read-Asmdef $appAsm
  $app = Ensure-Refs $app @("AQ30.Presentation","AQ.Domain.Merge","AQ.SharedKernel")
  Write-Asmdef $appAsm $app
  Write-Host "  Updated: $appAsm -> refs Presentation, Domain.Merge, SharedKernel"
} catch {
  Write-Warning $_
}

# 3) Editor (no changes to references—just sanity print if present)
if(Test-Path $editAsm -PathType Leaf){
  Write-Host "  Found editor asmdef: $editAsm (no changes made)"
} else {
  Write-Host "  Editor asmdef not found (that’s OK if you already have one elsewhere)."
}

Write-Host ""
Write-Host "Done. Next:" -ForegroundColor Green
Write-Host "  1) git status"
Write-Host "  2) git add -A && git commit -m 'Wire asmdefs: App->Presentation/Domain/SharedKernel; Presentation->Domain/SharedKernel'"
Write-Host "  3) Re-open Unity or let it recompile"
Write-Host "  4) Re-run audits:" 
Write-Host "       pwsh -NoProfile -ExecutionPolicy Bypass -File .\Tools\project-audit.ps1 -RepoRoot ."
Write-Host "       pwsh -NoProfile -ExecutionPolicy Bypass -File .\Tools\wk3-stabilize-audit.ps1 -RepoRoot ."
