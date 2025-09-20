param(
  [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.2.2f1\Editor\Unity.exe",
  [string]$ProjectPath = (Resolve-Path ".").Path
)

& $UnityExe -batchmode -quit -projectPath $ProjectPath -executeMethod "AQ.EditorTools.Audit.WK3Audit.RunBatch" -logFile -
if($LASTEXITCODE -ne 0){
  Write-Error "WK3 Unity scene audit failed. Check Editor logs."
}else{
  Write-Host "WK3 Unity scene audit completed."
}
