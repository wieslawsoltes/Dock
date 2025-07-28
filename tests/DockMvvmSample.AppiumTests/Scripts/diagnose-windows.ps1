# PowerShell script to diagnose Windows driver and Appium 2 issues
param(
    [switch]$Verbose,
    [switch]$FixIssues
)

Write-Host "=== Windows Appium 2.0 Diagnostic Tool ===" -ForegroundColor Cyan
Write-Host ""

# Function to check if a command exists
function Test-Command($cmdname) {
    return [bool](Get-Command -Name $cmdname -ErrorAction SilentlyContinue)
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

# Function to check Windows version
function Get-WindowsVersion {
    try {
        $os = Get-WmiObject -Class Win32_OperatingSystem
        return $os.Caption, $os.Version
    } catch {
        return "Unknown", "Unknown"
    }
}

# Function to check Developer Mode
function Test-DeveloperMode {
    try {
        $registryPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock"
        $value = Get-ItemProperty -Path $registryPath -Name "AllowDevelopmentWithoutDevLicense" -ErrorAction SilentlyContinue
        return $value.AllowDevelopmentWithoutDevLicense -eq 1
    } catch {
        return $false
    }
}

# Function to check WinAppDriver installation
function Test-WinAppDriver {
    $winAppDriverPath = "${env:ProgramFiles(x86)}\Windows Application Driver\WinAppDriver.exe"
    return Test-Path $winAppDriverPath
}

# Function to check Appium installation
function Test-Appium {
    try {
        $appiumVersion = & appium --version 2>$null
        return $LASTEXITCODE -eq 0, $appiumVersion
    } catch {
        return $false, $null
    }
}

# Function to check Windows driver installation
function Test-WindowsDriver {
    try {
        $drivers = & appium driver list 2>$null
        return $drivers -like "*windows*"
    } catch {
        return $false
    }
}

# Function to test element finding
function Test-ElementFinding {
    Write-Host "Testing element finding capabilities..." -ForegroundColor Yellow
    
    # This would require the application to be running
    # For now, we'll just check if the servers are responding
    try {
        $response = Invoke-WebRequest -Uri "http://127.0.0.1:4723/status" -TimeoutSec 5
        if ($response.StatusCode -eq 200) {
            Write-Host "OK Appium server is responding" -ForegroundColor Green
        }
    } catch {
        Write-Host "X Appium server is not responding" -ForegroundColor Red
    }
    
    try {
        $response = Invoke-WebRequest -Uri "http://127.0.0.1:4724/status" -TimeoutSec 5
        if ($response.StatusCode -eq 200) {
            Write-Host "OK WinAppDriver is responding" -ForegroundColor Green
        }
    } catch {
        Write-Host "X WinAppDriver is not responding" -ForegroundColor Red
    }
}

# Main diagnostic routine
Write-Host "1. Checking Windows Environment..." -ForegroundColor Yellow
$windowsCaption, $windowsVersion = Get-WindowsVersion
Write-Host "   Windows Version: $windowsCaption ($windowsVersion)" -ForegroundColor White

$developerMode = Test-DeveloperMode
if ($developerMode) {
    Write-Host "   OK Developer Mode is enabled" -ForegroundColor Green
} else {
    Write-Host "   X Developer Mode is not enabled" -ForegroundColor Red
    if ($FixIssues) {
        Write-Host "   Attempting to enable Developer Mode..." -ForegroundColor Yellow
        try {
            $registryPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock"
            if (-not (Test-Path $registryPath)) {
                New-Item -Path $registryPath -Force | Out-Null
            }
            Set-ItemProperty -Path $registryPath -Name "AllowDevelopmentWithoutDevLicense" -Value 1 -Type DWord
            Write-Host "   OK Developer Mode enabled" -ForegroundColor Green
        } catch {
            Write-Host "   X Failed to enable Developer Mode. Please enable it manually in Settings." -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "2. Checking WinAppDriver..." -ForegroundColor Yellow
$winAppDriverInstalled = Test-WinAppDriver
if ($winAppDriverInstalled) {
    Write-Host "   OK WinAppDriver is installed" -ForegroundColor Green
} else {
    Write-Host "   X WinAppDriver is not installed" -ForegroundColor Red
    if ($FixIssues) {
        Write-Host "   Downloading and installing WinAppDriver..." -ForegroundColor Yellow
        try {
            $downloadUrl = "https://github.com/Microsoft/WinAppDriver/releases/latest/download/WindowsApplicationDriver_1.2.99.msi"
            $msiPath = "$env:TEMP\WindowsApplicationDriver.msi"
            Invoke-WebRequest -Uri $downloadUrl -OutFile $msiPath
            Start-Process msiexec.exe -Wait -ArgumentList "/i `"$msiPath`" /quiet"
            Remove-Item $msiPath -Force
            Write-Host "   OK WinAppDriver installed" -ForegroundColor Green
        } catch {
            Write-Host "   X Failed to install WinAppDriver" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "3. Checking Appium Installation..." -ForegroundColor Yellow
$appiumInstalled, $appiumVersion = Test-Appium
if ($appiumInstalled) {
    Write-Host "   OK Appium is installed (Version: $appiumVersion)" -ForegroundColor Green
} else {
    Write-Host "   X Appium is not installed" -ForegroundColor Red
    if ($FixIssues) {
        Write-Host "   Installing Appium..." -ForegroundColor Yellow
        try {
            npm install -g appium
            Write-Host "   OK Appium installed" -ForegroundColor Green
        } catch {
            Write-Host "   X Failed to install Appium" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "4. Checking Windows Driver..." -ForegroundColor Yellow
$windowsDriverInstalled = Test-WindowsDriver
if ($windowsDriverInstalled) {
    Write-Host "   OK Windows driver is installed" -ForegroundColor Green
} else {
    Write-Host "   X Windows driver is not installed" -ForegroundColor Red
    if ($FixIssues) {
        Write-Host "   Installing Windows driver..." -ForegroundColor Yellow
        try {
            appium driver install windows
            Write-Host "   OK Windows driver installed" -ForegroundColor Green
        } catch {
            Write-Host "   X Failed to install Windows driver" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "5. Checking Server Status..." -ForegroundColor Yellow
$appiumPort = Test-Port -Port 4723
$winAppDriverPort = Test-Port -Port 4724

if ($appiumPort) {
    Write-Host "   OK Appium server is running on port 4723" -ForegroundColor Green
} else {
    Write-Host "   X Appium server is not running on port 4723" -ForegroundColor Red
}

if ($winAppDriverPort) {
    Write-Host "   OK WinAppDriver is running on port 4724" -ForegroundColor Green
} else {
    Write-Host "   X WinAppDriver is not running on port 4724" -ForegroundColor Red
}

Write-Host ""
Write-Host "6. Testing Element Finding..." -ForegroundColor Yellow
Test-ElementFinding

Write-Host ""
Write-Host "=== Diagnostic Summary ===" -ForegroundColor Cyan

$issues = @()

if (-not $developerMode) { $issues += "Developer Mode not enabled" }
if (-not $winAppDriverInstalled) { $issues += "WinAppDriver not installed" }
if (-not $appiumInstalled) { $issues += "Appium not installed" }
if (-not $windowsDriverInstalled) { $issues += "Windows driver not installed" }
if (-not $appiumPort) { $issues += "Appium server not running" }
if (-not $winAppDriverPort) { $issues += "WinAppDriver not running" }

if ($issues.Count -eq 0) {
    Write-Host "OK All checks passed! Your Windows Appium setup should work correctly." -ForegroundColor Green
} else {
    Write-Host "X Found $($issues.Count) issue(s):" -ForegroundColor Red
    foreach ($issue in $issues) {
        Write-Host "  - $issue" -ForegroundColor Red
    }
    
    Write-Host ""
    Write-Host "=== Recommended Solutions ===" -ForegroundColor Yellow
    Write-Host "1. Run this script with -FixIssues parameter to automatically fix common issues" -ForegroundColor White
    Write-Host "2. Ensure you're running PowerShell as Administrator" -ForegroundColor White
    Write-Host "3. Check that your application has proper accessibility identifiers" -ForegroundColor White
    Write-Host "4. Verify that your application is built and accessible" -ForegroundColor White
    Write-Host "5. Try running the tests with explicit waits instead of implicit waits" -ForegroundColor White
}

Write-Host ""
Write-Host "=== Additional Troubleshooting Tips ===" -ForegroundColor Cyan
Write-Host "- Windows driver has known issues with implicit waits - use explicit waits" -ForegroundColor White
Write-Host "- Element finding may require multiple strategies (automationid, name, id)" -ForegroundColor White
Write-Host "- WinAppDriver must run as Administrator" -ForegroundColor White
Write-Host "- Appium 2.0 uses different URL format than 1.x" -ForegroundColor White
Write-Host "- Check application logs for accessibility identifier issues" -ForegroundColor White 