# Quick test script to verify Windows Appium setup
param(
    [int]$AppiumPort = 4723,
    [int]$WinAppDriverPort = 4724
)

Write-Host "=== Windows Appium Setup Verification ===" -ForegroundColor Cyan
Write-Host ""

# Function to check if a port is in use
function Test-Port {
    param([int]$Port)
    try {
        $tcpConnection = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
        return $tcpConnection.Count -gt 0
    } catch {
        return $false
    }
}

# Function to test server response
function Test-ServerResponse {
    param([string]$Url)
    try {
        $response = Invoke-WebRequest -Uri $Url -TimeoutSec 5 -UseBasicParsing
        return $response.StatusCode -eq 200
    } catch {
        return $false
    }
}

# Check WinAppDriver
Write-Host "1. Checking WinAppDriver (port $WinAppDriverPort)..." -ForegroundColor Yellow
if (Test-Port -Port $WinAppDriverPort) {
    Write-Host "   ✓ WinAppDriver is running on port $WinAppDriverPort" -ForegroundColor Green
    
    if (Test-ServerResponse -Url "http://127.0.0.1:$WinAppDriverPort/status") {
        Write-Host "   ✓ WinAppDriver is responding to requests" -ForegroundColor Green
    } else {
        Write-Host "   ⚠ WinAppDriver port is open but not responding" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ✗ WinAppDriver is not running on port $WinAppDriverPort" -ForegroundColor Red
    Write-Host "   Start with: 'C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe'" -ForegroundColor White
}

Write-Host ""

# Check Appium Server
Write-Host "2. Checking Appium Server (port $AppiumPort)..." -ForegroundColor Yellow
if (Test-Port -Port $AppiumPort) {
    Write-Host "   ✓ Appium server is running on port $AppiumPort" -ForegroundColor Green
    
    if (Test-ServerResponse -Url "http://127.0.0.1:$AppiumPort/status") {
        Write-Host "   ✓ Appium server is responding to requests" -ForegroundColor Green
    } else {
        Write-Host "   ⚠ Appium server port is open but not responding" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ✗ Appium server is not running on port $AppiumPort" -ForegroundColor Red
    Write-Host "   Start with: appium --port $AppiumPort" -ForegroundColor White
}

Write-Host ""

# Check configuration
Write-Host "3. Checking configuration files..." -ForegroundColor Yellow
$configPath = "appsettings.windows.json"
if (Test-Path $configPath) {
    $config = Get-Content $configPath | ConvertFrom-Json
    $configuredUrl = $config.AppiumSettings.ServerUrl
    
    if ($configuredUrl -eq "http://127.0.0.1:$AppiumPort") {
        Write-Host "   ✓ Configuration uses correct Appium server URL: $configuredUrl" -ForegroundColor Green
    } else {
        Write-Host "   ✗ Configuration URL mismatch: $configuredUrl (should be http://127.0.0.1:$AppiumPort)" -ForegroundColor Red
    }
    
    $implicitWait = $config.AppiumSettings.ImplicitWait
    if ($implicitWait -eq 0) {
        Write-Host "   ✓ Implicit wait correctly disabled for Windows: $implicitWait" -ForegroundColor Green
    } else {
        Write-Host "   ⚠ Implicit wait should be 0 for Windows: currently $implicitWait" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ⚠ Configuration file not found: $configPath" -ForegroundColor Yellow
}

Write-Host ""

# Summary
$winAppDriverOk = Test-Port -Port $WinAppDriverPort
$appiumOk = Test-Port -Port $AppiumPort
$configOk = Test-Path $configPath

if ($winAppDriverOk -and $appiumOk -and $configOk) {
    Write-Host "🎉 Setup looks good! Ready to run tests." -ForegroundColor Green
    Write-Host ""
    Write-Host "Run tests with: .\Scripts\run-tests-windows.ps1" -ForegroundColor White
} else {
    Write-Host "⚠ Setup issues detected. Please fix the above problems before running tests." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Quick setup commands:" -ForegroundColor Cyan
    Write-Host "1. Start WinAppDriver: 'C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe'" -ForegroundColor White
    Write-Host "2. Start Appium: appium --port $AppiumPort" -ForegroundColor White
    Write-Host "3. Run tests: .\Scripts\run-tests-windows.ps1" -ForegroundColor White
}
