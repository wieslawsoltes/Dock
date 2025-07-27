# PowerShell script to verify Appium test setup
Write-Host "Verifying Appium test setup..." -ForegroundColor Green

$allGood = $true

# Check .NET
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ .NET SDK not found" -ForegroundColor Red
    $allGood = $false
}

# Check Node.js
try {
    $nodeVersion = node --version
    Write-Host "✓ Node.js: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ Node.js not found" -ForegroundColor Red
    $allGood = $false
}

# Check npm
try {
    $npmVersion = npm --version
    Write-Host "✓ npm: $npmVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ npm not found" -ForegroundColor Red
    $allGood = $false
}

# Check Appium
try {
    $appiumVersion = appium --version
    Write-Host "✓ Appium: $appiumVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ Appium not found" -ForegroundColor Red
    $allGood = $false
}

# Check WinAppDriver (Windows only)
if ($IsWindows -or $env:OS -eq "Windows_NT") {
    $winAppDriverPath = "${env:ProgramFiles(x86)}\Windows Application Driver\WinAppDriver.exe"
    if (Test-Path $winAppDriverPath) {
        Write-Host "✓ WinAppDriver: Found at $winAppDriverPath" -ForegroundColor Green
    } else {
        Write-Host "✗ WinAppDriver not found" -ForegroundColor Red
        $allGood = $false
    }
}

# Check if DockMvvmSample is built
$samplePath = "..\..\samples\DockMvvmSample\bin\Debug\net9.0\DockMvvmSample.exe"
if (Test-Path $samplePath) {
    Write-Host "✓ DockMvvmSample executable found" -ForegroundColor Green
} else {
    Write-Host "✗ DockMvvmSample executable not found at: $samplePath" -ForegroundColor Red
    Write-Host "  Run: dotnet build samples/DockMvvmSample -c Debug" -ForegroundColor Yellow
    $allGood = $false
}

# Check test project build
try {
    Write-Host "Building test project..." -ForegroundColor Yellow
    dotnet build -c Debug --verbosity quiet
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Test project builds successfully" -ForegroundColor Green
    } else {
        Write-Host "✗ Test project failed to build" -ForegroundColor Red
        $allGood = $false
    }
} catch {
    Write-Host "✗ Error building test project: $($_.Exception.Message)" -ForegroundColor Red
    $allGood = $false
}

# Check configuration files
$configFiles = @("appsettings.json", "appsettings.windows.json", "appsettings.macos.json")
foreach ($configFile in $configFiles) {
    if (Test-Path $configFile) {
        Write-Host "✓ Configuration file: $configFile" -ForegroundColor Green
    } else {
        Write-Host "✗ Missing configuration file: $configFile" -ForegroundColor Red
        $allGood = $false
    }
}

Write-Host "`n" -NoNewline
if ($allGood) {
    Write-Host "🎉 All checks passed! You're ready to run the tests." -ForegroundColor Green
    Write-Host "`nNext steps:" -ForegroundColor Yellow
    Write-Host "  1. Run: .\Scripts\run-tests-windows.ps1" -ForegroundColor Cyan
    Write-Host "  2. Or manually start WinAppDriver and run: dotnet test" -ForegroundColor Cyan
} else {
    Write-Host "❌ Some checks failed. Please fix the issues above before running tests." -ForegroundColor Red
    Write-Host "`nTo fix issues:" -ForegroundColor Yellow
    Write-Host "  1. Run setup script: .\Scripts\setup-windows.ps1" -ForegroundColor Cyan
    Write-Host "  2. Build DockMvvmSample: dotnet build samples/DockMvvmSample -c Debug" -ForegroundColor Cyan
} 