# AQ30 Assembly Architecture Consolidation Script
# Consolidates hybrid package/assets architecture to canonical Assets-based structure
# Based on Technical Seed Doc v3 + sprint pragmatism

param(
    [Parameter(Mandatory=$true)]
    [string]$RepoRoot,
    
    [switch]$DryRun = $false,
    [switch]$SkipBackup = $false,
    [switch]$Force = $false
)

$ErrorActionPreference = "Stop"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$logFile = Join-Path $RepoRoot "Tools\consolidation_log_$timestamp.txt"

# Ensure Tools directory exists
$toolsDir = Join-Path $RepoRoot "Tools"
if (!(Test-Path $toolsDir)) {
    New-Item -Path $toolsDir -ItemType Directory -Force | Out-Null
}

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $logEntry = "[$timestamp] [$Level] $Message"
    Write-Host $logEntry
    Add-Content -Path $logFile -Value $logEntry
}

function Test-UnityProject {
    param([string]$Path)
    return (Test-Path (Join-Path $Path "Assets")) -and (Test-Path (Join-Path $Path "ProjectSettings"))
}

function Test-GitRepo {
    param([string]$Path)
    return Test-Path (Join-Path $Path ".git")
}

function Confirm-Action {
    param([string]$Message)
    if ($Force) { return $true }
    $response = Read-Host "$Message (y/N)"
    return $response -match "^[Yy]"
}

function Backup-CurrentState {
    Write-Log "=== PHASE 1: CREATING SAFETY BACKUP ==="
    
    if (!(Test-GitRepo $RepoRoot)) {
        Write-Log "ERROR: Not a Git repository. Git backup required for safety." "ERROR"
        throw "Git repository required for safe operation"
    }
    
    if (!$SkipBackup) {
        Write-Log "Creating Git commit backup..."
        if (!$DryRun) {
            Push-Location $RepoRoot
            try {
                git add -A
                git commit -m "Pre-consolidation backup - $timestamp" --allow-empty
                Write-Log "Git backup created successfully"
            }
            catch {
                Write-Log "Git backup failed: $_" "ERROR"
                throw
            }
            finally {
                Pop-Location
            }
        } else {
            Write-Log "[DRY RUN] Would create Git backup"
        }
    }
}

function Remove-BackupFiles {
    Write-Log "=== PHASE 2: CLEANING BACKUP FILES ==="
    
    $backupPatterns = @("*.bak*", "*.prepatch*", "*.off")
    $cleanupPaths = @("Assets", "Packages")
    
    foreach ($cleanupPath in $cleanupPaths) {
        $fullPath = Join-Path $RepoRoot $cleanupPath
        if (Test-Path $fullPath) {
            foreach ($pattern in $backupPatterns) {
                $backupFiles = Get-ChildItem -Path $fullPath -Recurse -Name $pattern -ErrorAction SilentlyContinue
                foreach ($file in $backupFiles) {
                    $fullFilePath = Join-Path $fullPath $file
                    Write-Log "Removing backup file: $file"
                    if (!$DryRun) {
                        Remove-Item -Path $fullFilePath -Force -ErrorAction SilentlyContinue
                    }
                }
            }
        }
    }
}

function Remove-OrphanedPackages {
    Write-Log "=== PHASE 3: REMOVING ORPHANED PACKAGES ==="
    
    $orphanedPackages = @(
        "com.aq.domain",
        "com.aq.infrastructure", 
        "com.aq.presentation"
    )
    
    foreach ($package in $orphanedPackages) {
        $packagePath = Join-Path $RepoRoot "Packages\$package"
        if (Test-Path $packagePath) {
            Write-Log "Removing orphaned package: $package"
            if (!$DryRun) {
                Remove-Item -Path $packagePath -Recurse -Force
            }
        } else {
            Write-Log "Package not found (already clean): $package"
        }
    }
}

function Move-PackageToAssets {
    param(
        [string]$PackageName,
        [string]$TargetAssetFolder
    )
    
    $packagePath = Join-Path $RepoRoot "Packages\$PackageName"
    $targetPath = Join-Path $RepoRoot "Assets\$TargetAssetFolder"
    $runtimePath = Join-Path $packagePath "Runtime"
    
    if (Test-Path $packagePath) {
        Write-Log "Moving $PackageName to Assets/$TargetAssetFolder"
        
        if (!$DryRun) {
            # Create target directory
            New-Item -Path $targetPath -ItemType Directory -Force | Out-Null
            
            # Copy runtime files
            if (Test-Path $runtimePath) {
                Get-ChildItem -Path $runtimePath -Recurse | ForEach-Object {
                    if ($_.PSIsContainer) {
                        $destDir = $_.FullName.Replace($runtimePath, $targetPath)
                        New-Item -Path $destDir -ItemType Directory -Force | Out-Null
                    } else {
                        $destFile = $_.FullName.Replace($runtimePath, $targetPath)
                        $destDir = Split-Path $destFile -Parent
                        if (!(Test-Path $destDir)) {
                            New-Item -Path $destDir -ItemType Directory -Force | Out-Null
                        }
                        Copy-Item -Path $_.FullName -Destination $destFile -Force
                    }
                }
            }
            
            # Remove original package
            Remove-Item -Path $packagePath -Recurse -Force
        }
        
        return $true
    } else {
        Write-Log "Package not found: $PackageName"
        return $false
    }
}

function Consolidate-PackagesToAssets {
    Write-Log "=== PHASE 4: CONSOLIDATING PACKAGES TO ASSETS ==="
    
    # Move SharedKernel
    Move-PackageToAssets "com.aq.sharedkernel" "SharedKernel"
    
    # Move Domain.Merge  
    Move-PackageToAssets "com.aq.domain.merge" "Domain.Merge"
}

function Create-CanonicalAsmDefs {
    Write-Log "=== PHASE 5: CREATING CANONICAL ASSEMBLY DEFINITIONS ==="
    
    # SharedKernel asmdef
    $sharedKernelAsmdef = @{
        name = "AQ.SharedKernel"
        references = @()
        includePlatforms = @()
        excludePlatforms = @()
        allowUnsafeCode = $false
        overrideReferences = $false
        precompiledReferences = @()
        autoReferenced = $true
        defineConstraints = @()
        versionDefines = @()
        noEngineReferences = $true
    } | ConvertTo-Json -Depth 10
    
    # Domain.Merge asmdef
    $domainMergeAsmdef = @{
        name = "AQ.Domain.Merge"
        references = @("AQ.SharedKernel")
        includePlatforms = @()
        excludePlatforms = @()
        allowUnsafeCode = $false
        overrideReferences = $false
        precompiledReferences = @()
        autoReferenced = $true
        defineConstraints = @()
        versionDefines = @()
        noEngineReferences = $true
    } | ConvertTo-Json -Depth 10
    
    # App asmdef
    $appAsmdef = @{
        name = "AQ.App"
        references = @(
            "AQ.SharedKernel",
            "AQ.Domain.Merge",
            "Unity.Addressables",
            "Unity.ResourceManager", 
            "Unity.InputSystem",
            "Unity.TextMeshPro"
        )
        includePlatforms = @()
        excludePlatforms = @()
        allowUnsafeCode = $false
        overrideReferences = $false
        precompiledReferences = @()
        autoReferenced = $true
        defineConstraints = @()
        versionDefines = @()
        noEngineReferences = $false
    } | ConvertTo-Json -Depth 10
    
    # Editor asmdef
    $editorAsmdef = @{
        name = "AQ.Editor"
        references = @(
            "AQ.SharedKernel",
            "AQ.Domain.Merge", 
            "AQ.App"
        )
        includePlatforms = @("Editor")
        excludePlatforms = @()
        allowUnsafeCode = $false
        overrideReferences = $false
        precompiledReferences = @()
        autoReferenced = $true
        defineConstraints = @()
        versionDefines = @()
        noEngineReferences = $false
    } | ConvertTo-Json -Depth 10
    
    # Tests asmdef
    $testsAsmdef = @{
        name = "AQ.Tests"
        references = @(
            "AQ.SharedKernel",
            "AQ.Domain.Merge",
            "AQ.App",
            "UnityEngine.TestRunner",
            "UnityEditor.TestRunner"
        )
        includePlatforms = @("Editor")
        excludePlatforms = @()
        allowUnsafeCode = $false
        overrideReferences = $true
        precompiledReferences = @("nunit.framework.dll")
        autoReferenced = $false
        defineConstraints = @("UNITY_INCLUDE_TESTS")
        versionDefines = @()
        noEngineReferences = $false
    } | ConvertTo-Json -Depth 10
    
    $asmdefConfigs = @(
        @{ Path = "Assets\SharedKernel\AQ.SharedKernel.asmdef"; Content = $sharedKernelAsmdef },
        @{ Path = "Assets\Domain.Merge\AQ.Domain.Merge.asmdef"; Content = $domainMergeAsmdef },
        @{ Path = "Assets\App\AQ.App.asmdef"; Content = $appAsmdef },
        @{ Path = "Assets\Editor\AQ.Editor.asmdef"; Content = $editorAsmdef },
        @{ Path = "Assets\Tests\AQ.Tests.asmdef"; Content = $testsAsmdef }
    )
    
    foreach ($config in $asmdefConfigs) {
        $fullPath = Join-Path $RepoRoot $config.Path
        $directory = Split-Path $fullPath -Parent
        
        Write-Log "Creating asmdef: $($config.Path)"
        
        if (!$DryRun) {
            if (!(Test-Path $directory)) {
                New-Item -Path $directory -ItemType Directory -Force | Out-Null
            }
            Set-Content -Path $fullPath -Value $config.Content -Encoding UTF8
        }
    }
}

function Consolidate-Tests {
    Write-Log "=== PHASE 6: CONSOLIDATING TESTS ==="
    
    $testsPath = Join-Path $RepoRoot "Assets\Tests"
    $existingTests = Join-Path $RepoRoot "Assets\Tests"
    
    # Remove existing test structure to start clean
    if (Test-Path $existingTests) {
        Write-Log "Backing up existing tests..."
        if (!$DryRun) {
            $testBackup = Join-Path $RepoRoot "Tests_backup_$timestamp"
            Move-Item -Path $existingTests -Destination $testBackup
        }
    }
    
    # Create clean test structure
    Write-Log "Creating consolidated test structure..."
    if (!$DryRun) {
        New-Item -Path $testsPath -ItemType Directory -Force | Out-Null
        New-Item -Path (Join-Path $testsPath "EditMode") -ItemType Directory -Force | Out-Null
        New-Item -Path (Join-Path $testsPath "PlayMode") -ItemType Directory -Force | Out-Null
        
        # Restore backed up tests
        $testBackup = Join-Path $RepoRoot "Tests_backup_$timestamp"
        if (Test-Path $testBackup) {
            Get-ChildItem -Path $testBackup -Recurse -Include "*.cs" | ForEach-Object {
                $relativePath = $_.FullName.Replace($testBackup, "")
                $destPath = Join-Path $testsPath $relativePath
                $destDir = Split-Path $destPath -Parent
                
                if (!(Test-Path $destDir)) {
                    New-Item -Path $destDir -ItemType Directory -Force | Out-Null
                }
                Copy-Item -Path $_.FullName -Destination $destPath
            }
            
            Remove-Item -Path $testBackup -Recurse -Force
        }
    }
}

function Find-OrphanedScripts {
    Write-Log "=== PHASE 7: FINDING ORPHANED SCRIPTS ==="
    
    $assetsPath = Join-Path $RepoRoot "Assets"
    $orphanedScripts = @()
    
    Get-ChildItem -Path $assetsPath -Recurse -Include "*.cs" | ForEach-Object {
        $scriptPath = $_.FullName
        $directory = Split-Path $scriptPath -Parent
        
        # Check if directory has an asmdef
        $asmdefExists = Get-ChildItem -Path $directory -Include "*.asmdef" | Where-Object { $_.Name -notmatch "\.bak" }
        
        if (!$asmdefExists) {
            $relativePath = $scriptPath.Replace($RepoRoot, "").TrimStart('\')
            $orphanedScripts += $relativePath
            Write-Log "Orphaned script found: $relativePath" "WARNING"
        }
    }
    
    if ($orphanedScripts.Count -gt 0) {
        Write-Log "Found $($orphanedScripts.Count) orphaned scripts that need manual placement" "WARNING"
        foreach ($script in $orphanedScripts) {
            Write-Log "  - $script" "WARNING"
        }
    } else {
        Write-Log "No orphaned scripts found - all covered by asmdefs"
    }
    
    return $orphanedScripts
}

function Verify-Consolidation {
    Write-Log "=== PHASE 8: VERIFICATION ==="
    
    $canonicalAsmdefs = @(
        "Assets\SharedKernel\AQ.SharedKernel.asmdef",
        "Assets\Domain.Merge\AQ.Domain.Merge.asmdef", 
        "Assets\App\AQ.App.asmdef",
        "Assets\Editor\AQ.Editor.asmdef",
        "Assets\Tests\AQ.Tests.asmdef"
    )
    
    $success = $true
    foreach ($asmdef in $canonicalAsmdefs) {
        $fullPath = Join-Path $RepoRoot $asmdef
        if (Test-Path $fullPath) {
            Write-Log "✓ Found canonical asmdef: $asmdef"
        } else {
            Write-Log "✗ Missing canonical asmdef: $asmdef" "ERROR"
            $success = $false
        }
    }
    
    # Check for remaining packages
    $packagesPath = Join-Path $RepoRoot "Packages"
    $remainingAQPackages = Get-ChildItem -Path $packagesPath -Directory | Where-Object { $_.Name.StartsWith("com.aq.") }
    
    if ($remainingAQPackages.Count -eq 0) {
        Write-Log "✓ All AQ packages successfully moved to Assets"
    } else {
        Write-Log "✗ Remaining AQ packages found:" "WARNING"
        foreach ($pkg in $remainingAQPackages) {
            Write-Log "  - $($pkg.Name)" "WARNING"
        }
    }
    
    return $success
}

function Show-NextSteps {
    Write-Log "=== CONSOLIDATION COMPLETE ==="
    Write-Log ""
    Write-Log "NEXT STEPS:"
    Write-Log "1. Open Unity and let it reimport all assets"
    Write-Log "2. Check for compilation errors in Console"
    Write-Log "3. Run: Tools\project-audit.ps1 -RepoRoot '$RepoRoot'"
    Write-Log "4. Test that existing tests still pass"
    Write-Log "5. Commit the consolidated structure"
    Write-Log ""
    Write-Log "If there are issues, restore from Git backup:"
    Write-Log "  git reset --hard HEAD~1"
    Write-Log ""
    Write-Log "Log file saved to: $logFile"
}

# Main execution
try {
    Write-Log "Starting AQ30 Assembly Consolidation"
    Write-Log "Repository: $RepoRoot"
    Write-Log "Dry Run: $DryRun"
    Write-Log "Timestamp: $timestamp"
    
    if (!(Test-UnityProject $RepoRoot)) {
        throw "Invalid Unity project directory: $RepoRoot"
    }
    
    if (!$DryRun -and !(Confirm-Action "This will restructure your assembly architecture. Continue?")) {
        Write-Log "Operation cancelled by user"
        exit 0
    }
    
    # Execute phases
    Backup-CurrentState
    Remove-BackupFiles
    Remove-OrphanedPackages
    Consolidate-PackagesToAssets
    Create-CanonicalAsmDefs
    Consolidate-Tests
    
    $orphanedScripts = Find-OrphanedScripts
    $verificationSuccess = Verify-Consolidation
    
    if ($verificationSuccess) {
        Write-Log "✓ CONSOLIDATION COMPLETED SUCCESSFULLY" "SUCCESS"
        if ($orphanedScripts.Count -gt 0) {
            Write-Log "⚠ Manual action required: $($orphanedScripts.Count) orphaned scripts need placement" "WARNING"
        }
    } else {
        Write-Log "✗ CONSOLIDATION COMPLETED WITH WARNINGS" "WARNING"
    }
    
    Show-NextSteps
    
} catch {
    Write-Log "CONSOLIDATION FAILED: $_" "ERROR"
    Write-Log "You can restore from Git backup if needed: git reset --hard HEAD~1" "ERROR"
    throw
}