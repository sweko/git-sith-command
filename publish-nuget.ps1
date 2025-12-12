#!/usr/bin/env pwsh
# Publish GitSith package to NuGet.org
# Reads API key from NUGET_API_KEY environment variable

$ErrorActionPreference = "Stop"

$apiKey = $env:NUGET_API_KEY

if ([string]::IsNullOrWhiteSpace($apiKey)) {
    Write-Host "Error: NUGET_API_KEY environment variable is not set" -ForegroundColor Red
    Write-Host ""
    Write-Host "Set it with:" -ForegroundColor Yellow
    Write-Host '  $env:NUGET_API_KEY = "your-api-key"' -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Get your API key from: https://www.nuget.org/account/apikeys" -ForegroundColor Yellow
    exit 1
}

# Find the package
$package = Get-ChildItem -Path "bin/Release" -Filter "GitSith.*.nupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if (-not $package) {
    Write-Host "Error: No NuGet package found in bin/Release" -ForegroundColor Red
    Write-Host "Run 'dotnet pack -c Release' first" -ForegroundColor Yellow
    exit 1
}

Write-Host "Publishing $($package.Name) to NuGet.org..." -ForegroundColor Cyan
Write-Host ""

dotnet nuget push $package.FullName --source "https://api.nuget.org/v3/index.json" --api-key $apiKey --skip-duplicate

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✓ Package published successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "View at: https://www.nuget.org/packages/GitSith" -ForegroundColor Cyan
} else {
    Write-Host ""
    Write-Host "✗ Failed to publish package" -ForegroundColor Red
    exit 1
}
