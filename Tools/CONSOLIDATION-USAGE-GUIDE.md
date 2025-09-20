# AQ30 Assembly Consolidation Usage Guide

## Prerequisites

1. **Git repository** - Script requires Git for safety backups
2. **Unity closed** - Close Unity Editor before running
3. **Clean working directory** - Commit or stash any pending changes
4. **PowerShell 5.1+** - Run from PowerShell (not Command Prompt)

## Usage

### 1. Dry Run (Recommended First)
```powershell
cd C:\Users\Steph\Dev\aq30-unity
.\Tools\AQ30-Assembly-Consolidation.ps1 -RepoRoot . -DryRun
```

### 2. Full Execution
```powershell
cd C:\Users\Steph\Dev\aq30-unity
.\Tools\AQ30-Assembly-Consolidation.ps1 -RepoRoot .
```

### 3. Force Mode (Skip Confirmations)
```powershell
.\Tools\AQ30-Assembly-Consolidation.ps1 -RepoRoot . -Force
```

## What the Script Does

### Phase 1: Safety Backup
- Creates Git commit with timestamp
- Ensures you can rollback if needed

### Phase 2: Cleanup Backup Files
- Removes all `.bak`, `.prepatch`, `.off` files
- These confuse Unity's assembly resolution

### Phase 3: Remove Orphaned Packages
- Deletes `com.aq.domain`, `com.aq.infrastructure`, `com.aq.presentation`
- These have no active asmdefs and create noise

### Phase 4: Consolidate Packages to Assets
- Moves `com.aq.sharedkernel` → `Assets/SharedKernel/`
- Moves `com.aq.domain.merge` → `Assets/Domain.Merge/`
- Maintains all source code and structure

### Phase 5: Create Canonical Assembly Definitions
- Creates exactly 5 asmdefs as per Technical Seed Doc v3:
  - `AQ.SharedKernel` (pure C#, no Unity)
  - `AQ.Domain.Merge` (pure C#, references SharedKernel)
  - `AQ.App` (Unity integration, references all Unity packages)
  - `AQ.Editor` (Unity Editor tools)
  - `AQ.Tests` (all tests, references everything)

### Phase 6: Consolidate Tests
- Moves all tests to single `Assets/Tests/` location
- Creates `EditMode/` and `PlayMode/` subdirectories
- Preserves existing test code

### Phase 7: Find Orphaned Scripts
- Identifies any `.cs` files not covered by asmdefs
- Lists them for manual placement

### Phase 8: Verification
- Confirms all canonical asmdefs exist
- Checks that no AQ packages remain in Packages/
- Reports success/failure status

## After Running the Script

### 1. Open Unity
- Unity will reimport all assets (may take 2-3 minutes)
- Watch Console for compilation errors

### 2. Check Compilation
- **Success:** Console shows no errors
- **Issues:** See troubleshooting below

### 3. Run Project Audit
```powershell
.\Tools\project-audit.ps1 -RepoRoot .
```

### 4. Test Your Project
- Run existing tests to ensure nothing broke
- Test basic functionality (scene loading, UI, etc.)

### 5. Commit Consolidated Structure
```powershell
git add -A
git commit -m "Consolidated to canonical assembly architecture"
```

## Troubleshooting

### Missing References
If Unity shows missing reference errors:
1. Select the affected `.asmdef` file in Unity
2. Add missing references in Inspector
3. Common missing refs: `Unity.TextMeshPro`, `Unity.Addressables`

### Orphaned Scripts
If script reports orphaned scripts:
1. Check the log file for specific files
2. Move scripts based on their dependencies:
   - Pure C# logic → `SharedKernel` or `Domain.Merge`
   - Unity-dependent → `App`
   - Tests → `Tests`

### Compilation Errors
If Unity won't compile:
1. Check that all asmdefs have correct references
2. Look for namespace issues in moved files
3. Check that Unity packages are installed (Window → Package Manager)

## Rollback Instructions

### If Something Goes Wrong

**Option 1: Git Reset (Recommended)**
```powershell
git reset --hard HEAD~1
```

**Option 2: Restore from Backup**
```powershell
# The script creates timestamped backups in case of partial failure
# Check Tools\ directory for backup files
```

### Re-run After Fixes
If you need to modify and re-run:
```powershell
# First rollback
git reset --hard HEAD~1

# Fix any issues, then re-run
.\Tools\AQ30-Assembly-Consolidation.ps1 -RepoRoot .
```

## Expected Results

### Before (Current State)
```
Packages/
├── com.aq.sharedkernel/
├── com.aq.domain.merge/
├── com.aq.domain/ (orphaned)
├── com.aq.infrastructure/ (orphaned)
└── com.aq.presentation/ (orphaned)

Assets/
├── App/ (with AQ.App.asmdef)
├── Editor/ (with AQ.Editor.asmdef)
└── Tests/ (with AQ.Tests.asmdef)
```

### After (Consolidated State)
```
Assets/
├── SharedKernel/ (with AQ.SharedKernel.asmdef)
├── Domain.Merge/ (with AQ.Domain.Merge.asmdef)
├── App/ (with AQ.App.asmdef)
├── Editor/ (with AQ.Editor.asmdef)
└── Tests/ (with AQ.Tests.asmdef)

Packages/
└── (no AQ packages - only Unity packages)
```

### Key Benefits After Consolidation
- ✅ No Assembly-CSharp compilation
- ✅ Clean linear dependency chain
- ✅ All Unity package references resolved
- ✅ PowerShell tools work against predictable structure
- ✅ Easy to maintain and extend

## Support

If you encounter issues:
1. Check the log file: `Tools/consolidation_log_TIMESTAMP.txt`
2. Review the dry run output first
3. Ensure all prerequisites are met
4. Use Git rollback if needed