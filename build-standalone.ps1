#!/usr/bin/env pwsh
# Build standalone executables for all platforms

param(
    [string]$Version = "0.0.1"
)

$ErrorActionPreference = "Stop"

Write-Host "Building GitSith standalone executables v$Version" -ForegroundColor Cyan
Write-Host ""

$platforms = @(
    @{ Runtime = "win-x64"; Name = "Windows x64" },
    @{ Runtime = "win-arm64"; Name = "Windows ARM64" },
    @{ Runtime = "linux-x64"; Name = "Linux x64" },
    @{ Runtime = "linux-arm64"; Name = "Linux ARM64" },
    @{ Runtime = "osx-x64"; Name = "macOS x64" },
    @{ Runtime = "osx-arm64"; Name = "macOS ARM64" }
)

# Create output directory
$outputDir = "dist"
if (Test-Path $outputDir) {
    Remove-Item $outputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $outputDir | Out-Null

foreach ($platform in $platforms) {
    Write-Host "Building for $($platform.Name)..." -ForegroundColor Yellow
    
    dotnet publish GitSith.csproj `
        -c Release `
        -r $platform.Runtime `
        --self-contained `
        -p:PublishSingleFile=true `
        -p:PublishTrimmed=true `
        -p:EnableCompressionInSingleFile=true `
        -p:DebugType=None `
        -p:DebugSymbols=false `
        -p:Version=$Version `
        -o "$outputDir/$($platform.Runtime)"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ Success" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Failed" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "Build complete! Executables are in the '$outputDir' directory." -ForegroundColor Green
Write-Host ""
Write-Host "Distribution files:"
Get-ChildItem -Path $outputDir -Recurse -Include "git-sith*" -Exclude "*.pdb","*.xml" | ForEach-Object {
    $size = [math]::Round($_.Length / 1MB, 2)
    Write-Host "  $($_.FullName.Replace((Get-Location).Path, '.')) ($size MB)"
}
