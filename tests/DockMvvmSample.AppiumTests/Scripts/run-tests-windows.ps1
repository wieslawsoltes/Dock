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

# Function to kill existing Appium processes
function Stop-AppiumProcesses {
    try {
        $appiumProcesses = Get-Process -Name "node" -ErrorAction SilentlyContinue | Where-Object { $_.CommandLine -like "*appium*" }
        if ($appiumProcesses) {
            Write-Host "Found existing Appium processes. Stopping them..." -ForegroundColor Yellow
            $appiumProcesses | ForEach-Object { 
                Write-Host "Stopping Appium process (PID: $($_.Id))" -ForegroundColor Yellow
                Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
            }
            Start-Sleep -Seconds 2
            return $true
        }
        return $false
    } catch {
        return $false
    }
}

# Check if port is already in use
if (Test-Port -Port $Port) {
    Write-Host "WARNING Port $Port is already in use. Appium server might already be running." -ForegroundColor Yellow
    
    # Try to stop existing Appium processes automatically
    $stopped = Stop-AppiumProcesses
    if ($stopped) {
        Write-Host "Stopped existing Appium processes. Waiting for port to be free..." -ForegroundColor Yellow
        Start-Sleep -Seconds 3
        
        # Check if port is now free
        if (-not (Test-Port -Port $Port)) {
            Write-Host "OK Port $Port is now free. Will start new Appium server." -ForegroundColor Green
            $startNewServer = $true
        } else {
            Write-Host "X Port $Port is still in use after stopping processes." -ForegroundColor Red
            Write-Host "Please manually stop the process using port $Port or use a different port with -Port parameter" -ForegroundColor Yellow
            exit 1
        }
    } else {
        Write-Host "No Appium processes found to stop. Port might be used by another service." -ForegroundColor Yellow
        Write-Host "Please manually stop the process using port $Port or use a different port with -Port parameter" -ForegroundColor Yellow
        exit 1
    }
} else {
    $startNewServer = $true
}

# Start WinAppDriver first (required for Windows automation)
$winAppDriverProcess = $null
$winAppDriverPort = 4724  # Default WinAppDriver port

# Check if WinAppDriver is already running
if (Test-Port $winAppDriverPort) {
    Write-Host "WARNING Port $winAppDriverPort is already in use. WinAppDriver might already be running." -ForegroundColor Yellow
} else {
    Write-Host "Starting WinAppDriver on port $winAppDriverPort..." -ForegroundColor Yellow
    try {
        # Start WinAppDriver using cmd
        $winAppDriverArgs = "/c `"$winAppDriverPath`""
        $winAppDriverProcess = Start-Process -FilePath "cmd.exe" -ArgumentList $winAppDriverArgs -PassThru -NoNewWindow
        Write-Host "OK WinAppDriver started (PID: $($winAppDriverProcess.Id))" -ForegroundColor Green
        
        # Wait for WinAppDriver to start
        Write-Host "Waiting for WinAppDriver to start..." -ForegroundColor Yellow
        Start-Sleep -Seconds 3
        
        # Verify it's running
        if ($winAppDriverProcess.HasExited) {
            Write-Host "X WinAppDriver failed to start or exited immediately" -ForegroundColor Red
            Write-Host "Note: WinAppDriver requires Developer Mode and should run as Administrator" -ForegroundColor Yellow
            exit 1
        }
    } catch {
        Write-Host "Failed to start WinAppDriver: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Note: WinAppDriver requires Developer Mode and should run as Administrator" -ForegroundColor Yellow
        exit 1
    }
}

# Start Appium server if needed
$appiumProcess = $null
if ($startNewServer) {
    Write-Host "Starting Appium server on port $Port..." -ForegroundColor Yellow
    try {
        # Start Appium server with Windows driver support using cmd
        $appiumArgs = "/c appium --port $Port"
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

# Cleanup: Stop servers if we started them
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

if ($winAppDriverProcess -and -not $winAppDriverProcess.HasExited) {
    Write-Host "Stopping WinAppDriver..." -ForegroundColor Yellow
    try {
        $winAppDriverProcess.Kill()
        $winAppDriverProcess.WaitForExit(5000)
        Write-Host "WinAppDriver stopped" -ForegroundColor Green
    } catch {
        Write-Host "Warning: Could not stop WinAppDriver gracefully" -ForegroundColor Yellow
    }
}

# Exit with test result
if ($testExitCode -eq 0) {
    Write-Host "All tests completed successfully!" -ForegroundColor Green
} else {
    Write-Host "Some tests failed or there were errors" -ForegroundColor Red
}

exit $testExitCode 