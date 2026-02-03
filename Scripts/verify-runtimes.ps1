#!/usr/bin/env pwsh
# Script to verify all runtimes build successfully
# Run this before pushing to ensure CI will pass

$ErrorActionPreference = "Stop"

$runtimes = @(
    @{ Name = "Windows x64"; RID = "win-x64"; Supported = $IsWindows }
    @{ Name = "Linux x64"; RID = "linux-x64"; Supported = $IsLinux }
    @{ Name = "macOS x64"; RID = "osx-x64"; Supported = $IsMacOS }
    @{ Name = "macOS ARM64"; RID = "osx-arm64"; Supported = $IsMacOS }
    @{ Name = "Browser WASM"; RID = "browser-wasm"; Supported = $true }
)

$projects = @(
    "Zylance.Core/Zylance.Core.csproj"
    "Zylance.Contract/Zylance.Contract.csproj"
    "Zylance.UI/Zylance.UI.csproj"
)

Write-Host "🔍 Verifying runtime builds..." -ForegroundColor Cyan
Write-Host ""

$failed = $false

foreach ($runtime in $runtimes) {
    Write-Host "Testing $($runtime.Name) ($($runtime.RID))..." -ForegroundColor Yellow

    if (-not $runtime.Supported) {
        Write-Host "  ⏭️  Skipped (not supported on this OS)" -ForegroundColor Gray
        continue
    }

    # Install WASM workload if needed
    if ($runtime.RID -eq "browser-wasm") {
        Write-Host "  📦 Ensuring WASM workload is installed..." -ForegroundColor Gray
        dotnet workload install wasm-tools 2>&1 | Out-Null
    }

    $runtimeFailed = $false

    foreach ($project in $projects) {
        $projectName = Split-Path $project -Leaf
        $projectName = $projectName -replace '\.csproj$', ''

        Write-Host "    Building $projectName..." -NoNewline -ForegroundColor Gray

        $output = dotnet build $project -c Release -r $runtime.RID 2>&1

        if ($LASTEXITCODE -eq 0) {
            Write-Host " ✅" -ForegroundColor Green
        } else {
            Write-Host " ❌" -ForegroundColor Red
            Write-Host "      Error output:" -ForegroundColor Red
            $output | Select-Object -Last 10 | ForEach-Object { Write-Host "      $_" -ForegroundColor Red }
            $runtimeFailed = $true
            $failed = $true
        }
    }

    if (-not $runtimeFailed) {
        Write-Host "  ✅ $($runtime.Name) builds successfully!" -ForegroundColor Green
    } else {
        Write-Host "  ❌ $($runtime.Name) build failed!" -ForegroundColor Red
    }

    Write-Host ""
}

Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan

if ($failed) {
    Write-Host "❌ Some runtime builds failed!" -ForegroundColor Red
    Write-Host "Fix the errors above before pushing to CI." -ForegroundColor Yellow
    exit 1
} else {
    Write-Host "✅ All runtime builds succeeded!" -ForegroundColor Green
    Write-Host "Ready to push to CI! 🚀" -ForegroundColor Cyan
    exit 0
}
