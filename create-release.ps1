#!/usr/bin/env pwsh
# Create a GitHub release with all standalone executables

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [string]$ReleaseNotes = ""
)

$ErrorActionPreference = "Stop"

# Validate version format
if ($Version -notmatch '^\d+\.\d+\.\d+$') {
    Write-Host "Error: Version must be in format X.Y.Z (e.g., 0.0.1)" -ForegroundColor Red
    exit 1
}

$tag = "v$Version"
$distDir = "dist"
$releaseDir = "release"

Write-Host "Creating GitHub release $tag" -ForegroundColor Cyan
Write-Host ""

# Check if gh is available
if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    Write-Host "Error: GitHub CLI (gh) is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Install with: winget install GitHub.cli" -ForegroundColor Yellow
    exit 1
}

# Check if dist directory exists
if (-not (Test-Path $distDir)) {
    Write-Host "Error: dist directory not found. Run build-standalone.ps1 first." -ForegroundColor Red
    exit 1
}

# Create release directory and copy/rename files
Write-Host "Preparing release files..." -ForegroundColor Yellow
if (Test-Path $releaseDir) {
    Remove-Item $releaseDir -Recurse -Force
}
New-Item -ItemType Directory -Path $releaseDir | Out-Null

$platforms = @(
    @{ Runtime = "win-x64"; Ext = ".exe"; Name = "git-sith-$tag-win-x64.exe" },
    @{ Runtime = "win-arm64"; Ext = ".exe"; Name = "git-sith-$tag-win-arm64.exe" },
    @{ Runtime = "linux-x64"; Ext = ""; Name = "git-sith-$tag-linux-x64" },
    @{ Runtime = "linux-arm64"; Ext = ""; Name = "git-sith-$tag-linux-arm64" },
    @{ Runtime = "osx-x64"; Ext = ""; Name = "git-sith-$tag-macos-x64" },
    @{ Runtime = "osx-arm64"; Ext = ""; Name = "git-sith-$tag-macos-arm64" }
)

$files = @()
foreach ($platform in $platforms) {
    $source = "$distDir/$($platform.Runtime)/git-sith$($platform.Ext)"
    $dest = "$releaseDir/$($platform.Name)"
    
    if (Test-Path $source) {
        Copy-Item $source $dest
        $files += $dest
        Write-Host "  ✓ $($platform.Name)" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Missing: $source" -ForegroundColor Red
    }
}

if ($files.Count -eq 0) {
    Write-Host "Error: No executable files found in dist directory" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Read release notes from file if not provided
if ([string]::IsNullOrWhiteSpace($ReleaseNotes)) {
    if (Test-Path "RELEASE_NOTES.md") {
        $ReleaseNotes = Get-Content "RELEASE_NOTES.md" -Raw
    } else {
        $ReleaseNotes = "Release $tag"
    }
}

# Create GitHub release
Write-Host "Creating GitHub release..." -ForegroundColor Yellow
Write-Host ""

try {
    $ghArgs = @(
        "release", "create", $tag,
        "--title", "GitSith $tag",
        "--notes", $ReleaseNotes
    )
    $ghArgs += $files
    
    & gh @ghArgs
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✓ Release $tag created successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "View at: https://github.com/sweko/git-sith-command/releases/tag/$tag" -ForegroundColor Cyan
    } else {
        Write-Host "Error: Failed to create release" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "Error creating release: $_" -ForegroundColor Red
    exit 1
}

# Clean up release directory
Write-Host ""
Write-Host "Cleaning up..." -ForegroundColor Yellow
Remove-Item $releaseDir -Recurse -Force
Write-Host "Done!" -ForegroundColor Green
