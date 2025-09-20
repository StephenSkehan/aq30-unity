# Tools\AQ-UnityGitBootstrap.ps1
# Purpose: Bootstrap a clean Unity repo with correct .gitignore, .gitattributes (LFS), and safe Git settings.
# Platform: PowerShell 7.x · Encoding: UTF-8 · Newlines: CRLF · Logs: _audit\git\bootstrap\<stamp>\

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# --- setup logging ------------------------------------------------------------
$stamp  = Get-Date -Format "yyyyMMdd_HHmmss"
$root   = (& git.exe rev-parse --show-toplevel) 2>&1
if($LASTEXITCODE -ne 0){ throw "Not inside a git repository: $root" }
$root   = $root.Trim()
$logDir = Join-Path $root "_audit\git\bootstrap\$stamp"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null
function Log { param([string]$name,[string]$content="") ($content|Out-File -Enc UTF8 -FilePath (Join-Path $logDir $name)); }

# --- canonical Unity .gitignore ----------------------------------------------
$gitignore = @"
# --- UNITY GENERATED/LOCAL ---
/[Ll]ibrary/
/[Tt]emp/
/[Oo]bj/
/[Bb]uild/
/[Bb]uilds/
/[Ll]ogs/
/[Uu]ser[Ss]ettings/
/MemoryCaptures/
/Assets/StreamingAssets/aa/*/*.hash
/.vs/
/.idea/
/*.code-workspace

# --- OS JUNK ---
.DS_Store
Thumbs.db

# --- UNITY CACHE & REPORTS ---
/Artifacts/
/ScriptAssemblies/
/CrashReports/
/_UpgradeReport_Files/
/SysInfo.txt

# --- PACKAGES CACHE ---
/Packages/PackageCache/

# --- TEXTURE/IMPORTER META LOCKS ---
*.pidb
*.booproj

# --- TEST/LOCAL OUTPUT ---
/_testresults/
/*.log

# --- OPTIONAL: KEEP BINARY EXPORTS OUT OF GIT ---
*.unitypackage
"@

# --- robust LFS rules (.gitattributes) ---------------------------------------
$gitattributes = @"
* text=auto

# Large Unity assets to LFS
*.unity     filter=lfs diff=lfs merge=lfs -text
*.prefab    filter=lfs diff=lfs merge=lfs -text
*.fbx       filter=lfs diff=lfs merge=lfs -text
*.obj       filter=lfs diff=lfs merge=lfs -text
*.tga       filter=lfs diff=lfs merge=lfs -text
*.tif       filter=lfs diff=lfs merge=lfs -text
*.tiff      filter=lfs diff=lfs merge=lfs -text
*.png       filter=lfs diff=lfs merge=lfs -text
*.jpg       filter=lfs diff=lfs merge=lfs -text
*.jpeg      filter=lfs diff=lfs merge=lfs -text
*.psd       filter=lfs diff=lfs merge=lfs -text
*.mp3       filter=lfs diff=lfs merge=lfs -text
*.wav       filter=lfs diff=lfs merge=lfs -text
*.ogg       filter=lfs diff=lfs merge=lfs -text
*.mp4       filter=lfs diff=lfs merge=lfs -text
*.mov       filter=lfs diff=lfs merge=lfs -text
*.avi       filter=lfs diff=lfs merge=lfs -text
*.ttf       filter=lfs diff=lfs merge=lfs -text
*.otf       filter=lfs diff=lfs merge=lfs -text
*.exr       filter=lfs diff=lfs merge=lfs -text
"@

# --- write files idempotently -------------------------------------------------
$giPath = Join-Path $root ".gitignore"
$gaPath = Join-Path $root ".gitattributes"
if(!(Test-Path $giPath) -or ((Get-Content $giPath -Raw) -ne $gitignore)){ $gitignore | Out-File -Enc UTF8 -FilePath $giPath }
if(!(Test-Path $gaPath) -or ((Get-Content $gaPath -Raw) -ne $gitattributes)){ $gitattributes | Out-File -Enc UTF8 -FilePath $gaPath }
Log "gitignore.txt" (Get-Content $giPath -Raw)
Log "gitattributes.txt" (Get-Content $gaPath -Raw)

# --- git + lfs settings (repo-local) -----------------------------------------
& git.exe lfs install | Out-Null
& git.exe config --local core.longpaths true
& git.exe config --local push.autoSetupRemote true
# more reliable big pushes over flakey links
& git.exe config --local http.version HTTP/1.1
& git.exe config --local lfs.concurrenttransfers 2
& git.exe config --local http.lowSpeedTime 300
& git.exe config --local http.lowSpeedLimit 1
& git.exe config --local http.expect 100-continue

# ensure LFS rules registered (safe if already tracked)
$patterns = @("*.unity","*.prefab","*.fbx","*.obj","*.tga","*.tif","*.tiff","*.png","*.jpg","*.jpeg","*.psd","*.mp3","*.wav","*.ogg","*.mp4","*.mov","*.avi","*.ttf","*.otf","*.exr")
foreach($p in $patterns){ & git.exe lfs track $p | Out-Null }

# stage and commit bootstrap files if changed
& git.exe add .gitattributes .gitignore | Out-Null
$pending = (& git.exe diff --cached --name-only) 2>&1
if($pending){ & git.exe commit -m "chore(repo): bootstrap Unity .gitignore + .gitattributes (LFS) and safe settings" | Out-Null }

Write-Host "Bootstrap complete. Review _audit\git\bootstrap\$stamp for details."
