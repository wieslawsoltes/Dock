# DockMvvmSample Appium Tests

This project contains Appium-based UI tests for the DockMvvmSample Avalonia application, supporting both Windows and macOS platforms.

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

### Windows Setup
```powershell
# Run PowerShell as Administrator
cd tests/DockMvvmSample.AppiumTests
.\Scripts\setup-windows.ps1
```

### macOS Setup
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
# Terminal 1: Start WinAppDriver
"C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe"

# Terminal 2: Run tests
dotnet test
```

### macOS
```bash
# Option 1: Using the run script (recommended)
./Scripts/run-tests-macos.sh

# Option 2: Manual approach
# Terminal 1: Start Appium server
appium --base-path /wd/hub --port 4723

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
│   └── run-tests-macos.sh    # macOS test runner
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

**WinAppDriver fails to start**
- Ensure you're running as Administrator
- Check that Developer Mode is enabled in Windows Settings
- Verify WinAppDriver is installed correctly

**Tests can't find the application**
- Build the DockMvvmSample project: `dotnet build samples/DockMvvmSample -c Debug`
- Verify the executable path in `appsettings.windows.json`

**Port already in use**
- Use a different port: `.\Scripts\run-tests-windows.ps1 -Port 4724`
- Or stop the existing WinAppDriver process

### macOS Issues

**Permission denied errors**
- Grant Accessibility permissions to Terminal, Appium, and DockMvvmSample
- Go to System Preferences > Security & Privacy > Privacy > Accessibility

**App fails to launch**
- Ensure DockMvvmSample is built: `dotnet build samples/DockMvvmSample -c Debug`
- Check the executable path in `appsettings.macos.json`
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

## License

This project follows the same license as the main Dock project. 