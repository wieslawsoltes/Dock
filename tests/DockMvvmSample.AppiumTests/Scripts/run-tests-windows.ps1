# PowerShell script to run Appium tests on Windows
param(
    [string]$TestFilter = "",
    [switch]$Verbose,
    [int]$Port = 4723
)

Write-Host "Running Appium tests on Windows..." -ForegroundColor Green

# Check if Appium is installed
$appiumAvailable = $false
try {
    $appiumVersion = & appium --version 2>$null
    if ($LASTEXITCODE -eq 0) {
        $appiumAvailable = $true
        Write-Host "Appium found: $appiumVersion" -ForegroundColor Green
    }
} catch {
    $appiumAvailable = $false
}

if (-not $appiumAvailable) {
    Write-Host "Appium is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Please install Appium: npm install -g appium" -ForegroundColor Yellow
    Write-Host "Or run setup-windows.ps1 first" -ForegroundColor Yellow
    exit 1
}

# Check if WinAppDriver is installed (needed by Appium)
$winAppDriverPath = "${env:ProgramFiles(x86)}\Windows Application Driver\WinAppDriver.exe"
if (-not (Test-Path $winAppDriverPath)) {
    Write-Host "WinAppDriver not found at: $winAppDriverPath" -ForegroundColor Red
    Write-Host "Please run setup-windows.ps1 first to install WinAppDriver" -ForegroundColor Yellow
    exit 1
}

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

# Check if port is already in use
if (Test-Port -Port $Port) {
    Write-Host "Port $Port is already in use. Appium server might already be running." -ForegroundColor Yellow
    $useExisting = Read-Host "Do you want to use the existing server? (y/n)"
    if ($useExisting -ne "y" -and $useExisting -ne "Y") {
        Write-Host "Please stop the existing server or use a different port with -Port parameter" -ForegroundColor Yellow
        exit 1
    }
    $startNewServer = $false
} else {
    $startNewServer = $true
}

# Start Appium server if needed
$appiumProcess = $null
if ($startNewServer) {
    Write-Host "Starting Appium server on port $Port..." -ForegroundColor Yellow
    try {
        # Start Appium server with Windows driver support using cmd
        $appiumArgs = "/c appium --base-path /wd/hub --port $Port"
        $appiumProcess = Start-Process -FilePath "cmd.exe" -ArgumentList $appiumArgs -PassThru -NoNewWindow
        Write-Host "Appium server started (PID: $($appiumProcess.Id))" -ForegroundColor Green
        
        # Wait for Appium to start
        Write-Host "Waiting for Appium server to start..." -ForegroundColor Yellow
        Start-Sleep -Seconds 5
        
        # Verify it's running
        if ($appiumProcess.HasExited) {
            Write-Host "Appium server failed to start or exited immediately" -ForegroundColor Red
            exit 1
        }
    } catch {
        Write-Host "Failed to start Appium server: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "Using existing Appium server on port $Port" -ForegroundColor Yellow
}

# Build test arguments
$testArgs = @("test")
if ($TestFilter) {
    $testArgs += "--filter"
    $testArgs += $TestFilter
}
if ($Verbose) {
    $testArgs += "--logger"
    $testArgs += "console;verbosity=detailed"
}

# Run tests
Write-Host "Running tests..." -ForegroundColor Yellow
try {
    & dotnet @testArgs
    $testExitCode = $LASTEXITCODE
} catch {
    Write-Host "Error running tests: $($_.Exception.Message)" -ForegroundColor Red
    $testExitCode = 1
}

# Cleanup: Stop Appium server if we started it
if ($appiumProcess -and -not $appiumProcess.HasExited) {
    Write-Host "Stopping Appium server..." -ForegroundColor Yellow
    try {
        $appiumProcess.Kill()
        $appiumProcess.WaitForExit(5000)
        Write-Host "Appium server stopped" -ForegroundColor Green
    } catch {
        Write-Host "Warning: Could not stop Appium server gracefully" -ForegroundColor Yellow
    }
}

# Exit with test result
if ($testExitCode -eq 0) {
    Write-Host "All tests completed successfully!" -ForegroundColor Green
} else {
    Write-Host "Some tests failed or there were errors" -ForegroundColor Red
}

exit $testExitCode 