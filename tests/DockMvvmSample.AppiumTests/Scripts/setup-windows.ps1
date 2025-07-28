# PowerShell script to set up Appium testing environment on Windows
param(
    [switch]$SkipAppiumInstall,
    [switch]$SkipBuild
)

Write-Host "Setting up Appium testing environment for Windows..." -ForegroundColor Green

# Check if running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "This script requires Administrator privileges. Please run as Administrator." -ForegroundColor Red
    exit 1
}

# Function to check if a command exists
function Test-Command($cmdname) {
    return [bool](Get-Command -Name $cmdname -ErrorAction SilentlyContinue)
}

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Yellow

# Check Node.js
if (-not (Test-Command "node")) {
    Write-Host "Node.js is not installed. Please install Node.js from https://nodejs.org/" -ForegroundColor Red
    exit 1
}
Write-Host "Node.js is installed: $(node --version)" -ForegroundColor Green

# Check npm
if (-not (Test-Command "npm")) {
    Write-Host "npm is not installed. Please install npm (usually comes with Node.js)" -ForegroundColor Red
    exit 1
}
Write-Host "npm is installed: $(npm --version)" -ForegroundColor Green

# Check .NET
if (-not (Test-Command "dotnet")) {
    Write-Host ".NET is not installed. Please install .NET 9.0 SDK from https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
}
Write-Host ".NET is installed: $(dotnet --version)" -ForegroundColor Green

# Install Appium if not skipped
if (-not $SkipAppiumInstall) {
    Write-Host "Installing Appium..." -ForegroundColor Yellow
    npm install -g appium
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to install Appium" -ForegroundColor Red
        exit 1
    }
    Write-Host "Appium installed successfully" -ForegroundColor Green

    # Install Windows driver
    Write-Host "Installing Appium Windows driver..." -ForegroundColor Yellow
    appium driver install windows
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to install Appium Windows driver" -ForegroundColor Red
        exit 1
    }
    Write-Host "Appium Windows driver installed successfully" -ForegroundColor Green
} else {
    Write-Host "Skipping Appium installation (SkipAppiumInstall parameter specified)" -ForegroundColor Yellow
}

# Download and install WinAppDriver
Write-Host "Checking for WinAppDriver..." -ForegroundColor Yellow
$winAppDriverPath = "${env:ProgramFiles(x86)}\Windows Application Driver\WinAppDriver.exe"
if (-not (Test-Path $winAppDriverPath)) {
    Write-Host "WinAppDriver not found. Downloading and installing..." -ForegroundColor Yellow
    
    $downloadUrl = "https://github.com/Microsoft/WinAppDriver/releases/latest/download/WindowsApplicationDriver_1.2.99.msi"
    $msiPath = "$env:TEMP\WindowsApplicationDriver.msi"
    
    try {
        Invoke-WebRequest -Uri $downloadUrl -OutFile $msiPath
        Start-Process msiexec.exe -Wait -ArgumentList "/i `"$msiPath`" /quiet"
        Remove-Item $msiPath -Force
        Write-Host "WinAppDriver installed successfully" -ForegroundColor Green
    } catch {
        Write-Host "Failed to download/install WinAppDriver: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Please download and install WinAppDriver manually from:" -ForegroundColor Yellow
        Write-Host "https://github.com/Microsoft/WinAppDriver/releases" -ForegroundColor Yellow
    }
} else {
    Write-Host "WinAppDriver is already installed" -ForegroundColor Green
}

# Enable Developer Mode (required for WinAppDriver)
Write-Host "Enabling Developer Mode..." -ForegroundColor Yellow
try {
    $registryPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock"
    if (-not (Test-Path $registryPath)) {
        New-Item -Path $registryPath -Force | Out-Null
    }
    Set-ItemProperty -Path $registryPath -Name "AllowDevelopmentWithoutDevLicense" -Value 1 -Type DWord
    Write-Host "Developer Mode enabled" -ForegroundColor Green
} catch {
    Write-Host "Warning: Failed to enable Developer Mode. You may need to enable it manually in Settings." -ForegroundColor Yellow
}

# Build DockMvvmSample if not skipped
if (-not $SkipBuild) {
    Write-Host "Building DockMvvmSample..." -ForegroundColor Yellow
    $samplePath = "..\..\samples\DockMvvmSample"
    if (Test-Path $samplePath) {
        Push-Location $samplePath
        try {
            dotnet build -c Debug
            if ($LASTEXITCODE -ne 0) {
                Write-Host "Failed to build DockMvvmSample" -ForegroundColor Red
                exit 1
            }
            Write-Host "DockMvvmSample built successfully" -ForegroundColor Green
        } finally {
            Pop-Location
        }
    } else {
        Write-Host "Warning: DockMvvmSample path not found: $samplePath" -ForegroundColor Yellow
    }
} else {
    Write-Host "Skipping build (SkipBuild parameter specified)" -ForegroundColor Yellow
}

# Build test project
Write-Host "Building test project..." -ForegroundColor Yellow
dotnet restore
dotnet build -c Debug
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to build test project" -ForegroundColor Red
    exit 1
}
Write-Host "Test project built successfully" -ForegroundColor Green

Write-Host ""
Write-Host "Setup completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "To run the tests:" -ForegroundColor Yellow
Write-Host "1. Start WinAppDriver: `"${winAppDriverPath}`"" -ForegroundColor Cyan
Write-Host "2. Run tests: dotnet test" -ForegroundColor Cyan
Write-Host ""
Write-Host "Note: Make sure to run WinAppDriver as Administrator" -ForegroundColor Yellow 