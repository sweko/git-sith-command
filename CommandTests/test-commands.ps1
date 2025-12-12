# Git Sith Command Tests
# Creates a temporary 'youngling' branch, runs all tests (including purge), then cleans up

$ErrorActionPreference = "Continue"
$testDir = $PSScriptRoot
$testBranch = "youngling"
$originalBranch = git rev-parse --abbrev-ref HEAD

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Git Sith Command Tests" -ForegroundColor Cyan
Write-Host "  (The younglings will be sacrificed)" -ForegroundColor DarkRed
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Function to clean up on exit (success or failure)
function Cleanup-Youngling {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Magenta
    Write-Host "  Cleanup: Executing Order 66" -ForegroundColor Magenta
    Write-Host "========================================" -ForegroundColor Magenta
    
    # Return to original branch
    Write-Host "Returning to branch '$originalBranch'..." -ForegroundColor Yellow
    git checkout $originalBranch 2>&1 | Out-Null
    
    # Delete local branch
    Write-Host "Deleting local branch '$testBranch'..." -ForegroundColor Yellow
    git branch -D $testBranch 2>&1 | Out-Null
    
    # Delete remote branch
    Write-Host "Deleting remote branch '$testBranch'..." -ForegroundColor Yellow
    git push origin --delete $testBranch 2>&1 | Out-Null
    
    # Clean up test files in CommandTests folder
    Set-Location $testDir
    if (Test-Path ".gitignore") { Remove-Item -Force ".gitignore" }
    if (Test-Path "test-file.txt") { Remove-Item -Force "test-file.txt" }
    if (Test-Path "test-file2.txt") { Remove-Item -Force "test-file2.txt" }
    if (Test-Path "purge-victim.txt") { Remove-Item -Force "purge-victim.txt" }
    
    Write-Host "The younglings have been eliminated" -ForegroundColor DarkRed
    Write-Host ""
}

# Create and checkout test branch
Write-Host "Creating test branch '$testBranch'..." -ForegroundColor Yellow
git checkout -b $testBranch
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to create branch '$testBranch'. It may already exist." -ForegroundColor Red
    Write-Host "Run: git branch -D $testBranch" -ForegroundColor Yellow
    exit 1
}

# Push the branch to remote
Write-Host "Pushing '$testBranch' to origin..." -ForegroundColor Yellow
git push -u origin $testBranch
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to push branch to origin." -ForegroundColor Red
    git checkout $originalBranch
    git branch -D $testBranch
    exit 1
}
Write-Host ""

# Clean up any previous test files
Write-Host "Setting up test environment..." -ForegroundColor Yellow
if (Test-Path ".gitignore") { Remove-Item -Force ".gitignore" }
if (Test-Path "test-file.txt") { Remove-Item -Force "test-file.txt" }
if (Test-Path "test-file2.txt") { Remove-Item -Force "test-file2.txt" }
if (Test-Path "purge-victim.txt") { Remove-Item -Force "purge-victim.txt" }
Write-Host ""

# Test 1: Help command
Write-Host "========================================" -ForegroundColor Green
Write-Host "TEST 1: Help command" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
git sith help
Write-Host ""

# Test 2: Ignore with no arguments (dark side gitignore)
Write-Host "========================================" -ForegroundColor Green
Write-Host "TEST 2: Ignore everything (no args)" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
git sith ignore
Write-Host ""
Write-Host "Contents of .gitignore:" -ForegroundColor Yellow
Get-Content .gitignore
Write-Host ""

# Clean up for next test
Remove-Item -Force ".gitignore"

# Test 3: Ignore with templates
Write-Host "========================================" -ForegroundColor Green
Write-Host "TEST 3: Ignore with templates (csharp, node)" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
git sith ignore csharp node
Write-Host ""

# Test 4: Ignore idempotency check
Write-Host "========================================" -ForegroundColor Green
Write-Host "TEST 4: Ignore idempotency (adding same templates again)" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
git sith ignore csharp node
Write-Host ""

# Test 5: Aliases list
Write-Host "========================================" -ForegroundColor Green
Write-Host "TEST 5: Ignore --aliases" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
git sith ignore --aliases
Write-Host ""

# Test 6: Force push (create some files first)
Write-Host "========================================" -ForegroundColor Green
Write-Host "TEST 6: Force push with message" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
"Test file content" | Out-File -FilePath "test-file.txt"
git sith force-push "Test commit from the dark side"
Write-Host ""

# Test 7: Force push with no message (random Sith quote)
Write-Host "========================================" -ForegroundColor Green
Write-Host "TEST 7: Force push with no message (random Sith quote)" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
"Another test" | Out-File -FilePath "test-file2.txt"
git sith push
Write-Host ""

# Test 8: Show git log
Write-Host "========================================" -ForegroundColor Green
Write-Host "TEST 8: Git log (verify commits)" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
git log --oneline -10
Write-Host ""

# Test 9: Purge command help
Write-Host "========================================" -ForegroundColor Green
Write-Host "TEST 9: Purge command help" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
git sith help purge
Write-Host ""

# Test 10: Purge command (now safe to test on youngling branch!)
Write-Host "========================================" -ForegroundColor Green
Write-Host "TEST 10: Purge command (Order 66)" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

# Stash any uncommitted changes first (filter-branch requires clean working directory)
Write-Host "Stashing uncommitted changes..." -ForegroundColor Yellow
git stash push -m "test-stash" 2>&1 | Out-Null

# Create a file, commit it, then purge it
Write-Host "Creating victim file..." -ForegroundColor Yellow
"This youngling will be purged from history" | Out-File -FilePath "purge-victim.txt"
git add purge-victim.txt
git commit -m "Adding youngling to be purged"
git push

Write-Host ""
Write-Host "Verifying file exists in history..." -ForegroundColor Yellow
$commitCount = git log --oneline --all -- purge-victim.txt | Measure-Object | Select-Object -ExpandProperty Count
Write-Host "Found $commitCount commit(s) containing purge-victim.txt" -ForegroundColor Cyan

Write-Host ""
Write-Host "Executing purge (Order 66)..." -ForegroundColor Red
git sith order-66 purge-victim.txt

Write-Host ""
Write-Host "Verifying file is purged from history..." -ForegroundColor Yellow
$commitCountAfter = git log --oneline --all -- purge-victim.txt | Measure-Object | Select-Object -ExpandProperty Count
if ($commitCountAfter -eq 0) {
    Write-Host "File successfully purged from history!" -ForegroundColor Green
} else {
    Write-Host "File still appears in $commitCountAfter commit(s)" -ForegroundColor Yellow
}

# Force push the rewritten history to remote
Write-Host ""
Write-Host "Force pushing rewritten history..." -ForegroundColor Yellow
git push --force
Write-Host ""

# Test 11: Git log after purge
Write-Host "========================================" -ForegroundColor Green
Write-Host "TEST 11: Git log (after purge)" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
git log --oneline -10
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  All tests completed!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Cleanup
Cleanup-Youngling

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Test run complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
