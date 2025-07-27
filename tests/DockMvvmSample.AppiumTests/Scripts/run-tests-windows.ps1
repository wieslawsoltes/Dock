# PowerShell script to run Appium tests on Windows
param(
    [string]$TestFilter = "",
    [switch]$Verbose,
    [int]$Port = 4723
)

Write-Host "Running Appium tests on Windows..." -ForegroundColor Green

# Check if WinAppDriver is installed
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

# Start WinAppDriver if needed
$winAppDriverProcess = $null
if ($startNewServer) {
    Write-Host "Starting WinAppDriver on port $Port..." -ForegroundColor Yellow
    try {
        $winAppDriverProcess = Start-Process -FilePath $winAppDriverPath -ArgumentList $Port -PassThru -NoNewWindow
        Write-Host "✓ WinAppDriver started (PID: $($winAppDriverProcess.Id))" -ForegroundColor Green
        
        # Wait for WinAppDriver to start
        Start-Sleep -Seconds 3
        
        # Verify it's running
        if ($winAppDriverProcess.HasExited) {
            Write-Host "WinAppDriver failed to start or exited immediately" -ForegroundColor Red
            exit 1
        }
    } catch {
        Write-Host "Failed to start WinAppDriver: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "Using existing Appium/WinAppDriver server on port $Port" -ForegroundColor Yellow
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

# Cleanup: Stop WinAppDriver if we started it
if ($winAppDriverProcess -and -not $winAppDriverProcess.HasExited) {
    Write-Host "Stopping WinAppDriver..." -ForegroundColor Yellow
    try {
        $winAppDriverProcess.Kill()
        $winAppDriverProcess.WaitForExit(5000)
        Write-Host "✓ WinAppDriver stopped" -ForegroundColor Green
    } catch {
        Write-Host "Warning: Could not stop WinAppDriver gracefully" -ForegroundColor Yellow
    }
}

# Exit with test result
if ($testExitCode -eq 0) {
    Write-Host "✓ All tests completed successfully!" -ForegroundColor Green
} else {
    Write-Host "✗ Some tests failed or there were errors" -ForegroundColor Red
}

exit $testExitCode 