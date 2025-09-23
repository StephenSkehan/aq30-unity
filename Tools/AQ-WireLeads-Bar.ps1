#Requires -Version 7.0
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Config
$UnityExe = "${env:ProgramFiles}\Unity\Hub\Editor\6000.2.2f1\Editor\Unity.exe"
$ProjectPath = (Resolve-Path ".").Path
$Stamp = Get-Date -Format "yyyyMMdd_HHmmss"
$AuditDir = "_audit\logs"; if (!(Test-Path $AuditDir)) { New-Item -ItemType Directory -Force -Path $AuditDir | Out-Null }
$Log = Join-Path $AuditDir "wire_leads_$Stamp.log"

# Run Unity in batchmode to execute our menu method
& $UnityExe -batchmode -projectPath $ProjectPath -quit `
  -executeMethod "AQ.Editor.Scenes.LeadsBarWire.ExecuteWireHLGBatch" `
  -logFile "$Log"

if ($LASTEXITCODE -ne 0) {
  Write-Error "Unity batch wiring failed (exit $LASTEXITCODE). See $Log"
} else {
  Write-Host "PASS: LeadsBar wired (HLG). Log: $Log"
}

# Optional: lightweight checkpoint tag
$tag = "leads_wire_$Stamp"
git add -A
git commit -m "Wire LeadsBar (HLG) via batch runner $Stamp" | Out-Null
git tag $tag
git push
git push --tags
Write-Host "Tagged: $tag"
