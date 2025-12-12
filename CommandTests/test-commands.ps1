# Git Sith Command Tests
# Run from the CommandTests folder (uses parent git repo)

$ErrorActionPreference = "Continue"
$testDir = $PSScriptRoot

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Git Sith Command Tests" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Clean up any previous test files
Write-Host "Setting up test environment..." -ForegroundColor Yellow
if (Test-Path ".gitignore") {
    Remove-Item -Force ".gitignore"
}
if (Test-Path "test-file.txt") {
    Remove-Item -Force "test-file.txt"
}
if (Test-Path "test-file2.txt") {
    Remove-Item -Force "test-file2.txt"
}
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
git log --oneline
Write-Host ""

# Test 9: Purge help
Write-Host "========================================" -ForegroundColor Green
Write-Host "TEST 9: Purge command help" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
git sith help purge
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  All tests completed!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Note: Purge command not tested automatically (destructive operation)" -ForegroundColor Yellow
Write-Host "To test purge manually: git sith purge test-file.txt" -ForegroundColor Yellow
