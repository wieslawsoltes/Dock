# DockMvvmSample Appium Tests

This project contains Appium-based UI tests for the DockMvvmSample Avalonia application, supporting both Windows and macOS platforms with **platform-optimized element finding strategies**.

## Key Features

- **Cross-platform compatibility** with automatic platform detection
- **Enhanced element finding** with multiple fallback strategies  
- **Platform-optimized wait strategies** (Windows: 500ms polling, macOS: 250ms polling)
- **Comprehensive diagnostic tools** for setup validation
- **Automatic server configuration** handling Windows dual-server architecture
- **Robust error handling** with detailed troubleshooting guidance

## Prerequisites

### Common Requirements
- **.NET 9.0 SDK** - [Download](https://dotnet.microsoft.com/download)
- **Node.js** (v16 or later) - [Download](https://nodejs.org/)
- **npm** (comes with Node.js)

### Windows-Specific Requirements
- **Windows 10/11**
- **Administrator privileges** for setup
- **Developer Mode enabled** (handled by setup script)

### macOS-Specific Requirements
- **macOS 10.15 or later**
- **Xcode Command Line Tools**
- **Accessibility permissions** for automation apps

## Quick Setup

### Windows Setup (Enhanced)
```powershell
# Run PowerShell as Administrator
cd tests/DockMvvmSample.AppiumTests

# Run diagnostics and auto-fix common issues
.\Scripts\diagnose-windows.ps1 -FixIssues

# Setup everything (if diagnostics found issues)
.\Scripts\setup-windows.ps1
```

### macOS Setup (Unchanged)
```bash
cd tests/DockMvvmSample.AppiumTests
chmod +x Scripts/setup-macos.sh
./Scripts/setup-macos.sh
```

## Running Tests

### Windows
```powershell
# Option 1: Using the run script (recommended)
.\Scripts\run-tests-windows.ps1

# Option 2: Manual approach
# Terminal 1: Start WinAppDriver (run as Administrator)
"C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe"

# Terminal 2: Start Appium (in separate terminal)
appium --port 4723

# Terminal 3: Run tests
dotnet test
```

### macOS
```bash
# Option 1: Using the run script (recommended)
./Scripts/run-tests-macos.sh

# Option 2: Manual approach
# Terminal 1: Start Appium server
appium --port 4723

# Terminal 2: Run tests
dotnet test
```

## Test Configuration

The tests use configuration files to support different platforms:

- `appsettings.json` - Common settings
- `appsettings.windows.json` - Windows-specific settings
- `appsettings.macos.json` - macOS-specific settings

### Customizing Paths

If your DockMvvmSample executable is in a different location, update the appropriate configuration file:

```json
{
  "DockMvvmSample": {
    "ExecutablePath": "path/to/your/DockMvvmSample.exe",
    "WorkingDirectory": "path/to/working/directory/"
  }
}
```

## Test Structure

```
DockMvvmSample.AppiumTests/
├── Configuration/          # Configuration models
├── Infrastructure/         # Base classes and utilities
├── PageObjects/           # Page object models
├── Scripts/              # Setup, verification, and run scripts
│   ├── setup-windows.ps1     # Windows setup
│   ├── setup-macos.sh        # macOS setup
│   ├── verify-setup.ps1      # Windows verification
│   ├── verify-setup-macos.sh # macOS verification
│   ├── run-tests-windows.ps1 # Windows test runner
│   ├── run-tests-macos.sh    # macOS test runner
│   └── diagnose-windows.ps1  # Windows diagnostic tool
├── Tests/                # Actual test classes
└── appsettings*.json     # Configuration files
```

## Available Tests

### BasicDockTests
- **ApplicationStartsSuccessfully** - Verifies the app launches and main window is visible
- **MainMenuIsAccessible** - Tests File and Window menu accessibility
- **DocumentTabsAreVisible** - Checks for default document tabs
- **ToolWindowsAreVisible** - Verifies tool windows are present
- **CanCreateNewLayout** - Tests the "New Layout" functionality
- **DocumentTabsAreClickable** - Verifies document tab interaction

## Troubleshooting

### Windows Issues

#### **NEW: Windows Diagnostic Tool**
First, run the diagnostic tool to identify issues:
```powershell
# Run as Administrator
.\Scripts\diagnose-windows.ps1

# To automatically fix common issues
.\Scripts\diagnose-windows.ps1 -FixIssues
```

#### **Element Not Found Issues (Most Common)**
The Windows driver has known issues with element finding. Our enhanced ElementHelper includes multiple workarounds:

**Symptoms:**
- `NoSuchElementException` even when elements exist
- Elements found inconsistently
- Implicit waits not working

**Root Cause:**
- **Windows requires TWO servers**: WinAppDriver (port 4724) + Appium (port 4723)
- **Server URL must point to Appium server** (port 4723), NOT WinAppDriver (port 4724)
- **Implicit waits don't work** on Windows - only explicit waits work
- **AutomationId attribute casing** matters (use @AutomationId, not @automationid)

**Solutions:**
1. **Correct server configuration** (fixed in appsettings.windows.json)
2. **Multiple element finding strategies** with platform-specific optimizations
3. **Enhanced error handling** with fallback strategies
4. **Windows-specific capabilities** in driver configuration

**Code Example:**
```csharp
// Enhanced element finding with automatic platform optimization:
var element = Elements.FindByAccessibilityIdWithWait("MyElement", 10);

// Platform-specific wait strategies are automatically applied
var element = Elements.WaitForClickable("MyElement", 10);
```

#### **WinAppDriver fails to start**
- Ensure you're running as Administrator
- Check that Developer Mode is enabled in Windows Settings
- Verify WinAppDriver is installed correctly

#### **Tests can't find the application**
- Build the DockMvvmSample project: `dotnet build samples/DockMvvmSample -c Debug`
- Verify the executable path in `appsettings.windows.json`

#### **Port already in use**
- Use a different port: `.\Scripts\run-tests-windows.ps1 -Port 4724`
- Or stop the existing WinAppDriver process

#### **Session Creation Fails**
**Error:** `SessionNotCreatedException: UiAutomation not connected`

**Solutions:**
1. **Ensure both servers are running:**
   - WinAppDriver on port 4724 (as Administrator)
   - Appium server on port 4723

2. **Check Windows version compatibility:**
   - Windows 10 version 1607 or later required
   - Windows 11 recommended

3. **Verify Developer Mode:**
   ```powershell
   # Check if Developer Mode is enabled
   Get-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock" -Name "AllowDevelopmentWithoutDevLicense"
   ```

4. **Restart services:**
   ```powershell
   # Stop existing processes
   Stop-Process -Name "WinAppDriver" -Force -ErrorAction SilentlyContinue
   Stop-Process -Name "node" -Force -ErrorAction SilentlyContinue
   
   # Restart in correct order
   Start-Process "C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe" -Verb RunAs
   Start-Sleep -Seconds 3
   appium --port 4723
   ```

#### **Implicit Wait Issues**
**Problem:** Implicit waits don't work properly with Windows driver

**Solution:** Use explicit waits (already implemented):
```csharp
// ✅ Good - explicit wait
var element = Elements.WaitForClickable("MyElement", 10);

// ❌ Bad - implicit wait (doesn't work on Windows)
driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
var element = driver.FindElement(By.Id("MyElement"));
```

### macOS Issues

**Permission denied errors**
- Grant Accessibility permissions to Terminal, Appium, and DockMvvmSample
- Go to System Preferences > Security & Privacy > Privacy > Accessibility

**App fails to launch or automation elements not found**
- **Use the test runner script**: Always use `./Scripts/run-tests-macos.sh` instead of `dotnet test` directly
- **App bundling required**: The script automatically creates a proper macOS app bundle (`DockMvvmSample.app`) needed for automation
- If building manually: `cd samples/DockMvvmSample && ./build-app-bundle.sh`
- Check the executable path in `appsettings.macos.json` points to the `.app` bundle
- Verify the app has necessary permissions

**Appium connection issues**
- Ensure Appium server is running on the correct port
- Check firewall settings
- Try restarting the Appium server

### General Issues

**Element not found errors**
- The UI automation selectors might need adjustment for your specific Avalonia version
- Check if the application UI has changed
- Increase timeout values in configuration

**Build errors**
- Ensure all NuGet packages are restored: `dotnet restore`
- Check .NET version compatibility
- Update package versions if needed

## Windows-Specific Best Practices

### 1. **Element Finding Strategies**
Our enhanced ElementHelper uses platform-optimized strategies:

**Windows (Automatic Detection):**
```csharp
// Multiple fallback strategies with proper casing:
// 1. @AutomationId attribute (case-sensitive)
// 2. @Name attribute  
// 3. Combined AutomationId or Name
// 4. Legacy approaches for compatibility
// 5. AppiumBy fallback
```

**macOS (Automatic Detection):**
```csharp
// Mac2 driver optimized strategies:
// 1. @identifier attribute (primary)
// 2. @name attribute
// 3. Combined approach
// 4. AppiumBy fallback
```

### 2. **Wait Strategies**
```csharp
// Platform-optimized wait strategies (automatic detection)
var element = Elements.WaitForClickable("MyElement", 10);  // Uses optimized polling
var element = Elements.WaitForVisible("MyElement", 10);    // Platform-specific timing
var element = Elements.WaitForCondition("MyElement", e => e.Enabled, 10);

// Windows: 500ms polling intervals for WinAppDriver stability
// macOS: 250ms polling intervals for responsive Mac2 driver
```

### 3. **Error Handling**
```csharp
// Enhanced error handling with multiple strategies
try
{
    var element = Elements.FindByAccessibilityId("MyElement");
}
catch (NoSuchElementException)
{
    // Element not found with any strategy
    // Log detailed information for debugging
}
```

### 4. **Server Management**
```powershell
# CORRECT startup order for Windows:
# 1. WinAppDriver (as Administrator) on port 4724
"C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe"

# 2. Appium server on port 4723 (connects to WinAppDriver)
appium --port 4723

# 3. Tests connect to Appium (port 4723), which proxies to WinAppDriver
dotnet test

# Use the automated script for proper server management:
.\Scripts\run-tests-windows.ps1
```

## Extending Tests

### Adding New Tests
1. Create new test classes in the `Tests/` folder
2. Inherit from `BaseTest` for driver management
3. Use the `[Collection("AppiumTests")]` attribute for proper test isolation

### Adding New Page Objects
1. Create page object classes in `PageObjects/` folder
2. Follow the existing pattern for element location and interaction methods
3. Use explicit waits for better reliability

### Platform-Specific Tests
Use the `ConfigurationHelper` to detect platform:
```csharp
[Fact]
public void WindowsSpecificTest()
{
    if (!ConfigurationHelper.IsWindows)
    {
        Skip.If(true, "This test is Windows-specific");
    }
    // Test implementation
}
```

## Script Options

### Windows Script Options
```powershell
# Setup script
.\Scripts\setup-windows.ps1 -SkipAppiumInstall -SkipBuild

# Verification script
.\Scripts\verify-setup.ps1

# Diagnostic script (NEW)
.\Scripts\diagnose-windows.ps1 -FixIssues

# Run script
.\Scripts\run-tests-windows.ps1 -TestFilter "BasicDockTests" -Verbose -Port 4724
```

### macOS Script Options
```bash
# Setup script
./Scripts/setup-macos.sh --skip-appium-install --skip-build

# Verification script
./Scripts/verify-setup-macos.sh

# Run script
./Scripts/run-tests-macos.sh --filter "BasicDockTests" --verbose --port 4724
```

## Contributing

When adding new tests:
1. Follow the existing naming conventions
2. Add appropriate documentation
3. Test on both platforms if possible
4. Update this README if needed
5. Use explicit waits instead of implicit waits for Windows compatibility

## License

This project follows the same license as the main Dock project. 