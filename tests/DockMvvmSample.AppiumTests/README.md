# DockMvvmSample Appium Tests

This project contains automated UI tests for the DockMvvmSample application using Appium.

## Overview

The tests use Appium to automate the DockMvvmSample application on multiple platforms:
- **Windows**: Uses WinAppDriver with Windows automation
- **macOS**: Uses Mac2 driver with macOS automation

## Platform-Specific Configuration

### Windows Configuration (`appsettings.windows.json`)
- Uses WinAppDriver with `automationName: "Windows"`
- Includes enhanced capabilities for better app launching:
  - `ms:waitForAppLaunch`: Waits for app to launch before connecting
  - `ms:experimental-webdriver`: Enables experimental WebDriver features
  - Extended timeouts for slower Windows environments

### macOS Configuration (`appsettings.macos.json`)
- Uses Mac2 driver with `automationName: "Mac2"`
- Uses bundle ID for app identification
- Configured for app bundle management

## Enhanced Application Loading (Windows Compatibility Fix)

The `WaitForApplicationToLoad()` method has been significantly enhanced to address Windows-specific issues:

### Multi-Stage Loading Detection
1. **Application Process Ready**: Verifies WinAppDriver can communicate with the app
2. **Main Window Visible**: Checks if the main window is accessible and ready
3. **Essential Elements Ready**: Validates that core UI elements are available and interactable
4. **Final Stabilization**: Platform-specific wait time (2s for Windows, 1s for others)

### Windows-Specific Improvements
- **Enhanced Element Detection**: Uses Windows-specific XPath patterns (`@AutomationId`, `@Name`, `@ControlType`)
- **State Validation**: Checks both `Displayed` and `Enabled` properties on Windows
- **Better Error Handling**: Provides detailed diagnostic information on failures
- **Improved App Lifecycle**: Enhanced startup detection with main window handle verification

### Diagnostic Features
- Comprehensive error logging with platform detection
- Element enumeration for debugging
- Window handle information
- Detailed failure diagnostics

## Running Tests

### Prerequisites
1. **Windows**: 
   - Install WinAppDriver from Microsoft
   - Start WinAppDriver: `WinAppDriver.exe`
   
2. **macOS**:
   - Install Appium with Mac2 driver
   - Start Appium: `appium --base-path /wd/hub --port 4723`

### Build the Test Application
```bash
# Build DockMvvmSample
cd ../../samples/DockMvvmSample
dotnet build
```

### Run Tests
```bash
# Run all tests
dotnet test

# Run specific test
dotnet test --filter "ApplicationStartsSuccessfully"

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"
```

## Common Issues and Solutions

### Windows-Specific Issues

#### Issue: `WaitForApplicationToLoad()` Timeout
**Solution**: The enhanced implementation now provides better Windows compatibility with:
- Multi-stage loading detection
- Platform-specific element detection strategies
- Enhanced diagnostic information on failures

#### Issue: Element Not Found
**Solution**: The improved element detection now uses multiple strategies:
1. Automation ID lookup
2. Name attribute search
3. Windows-specific XPath patterns
4. Fallback element enumeration

#### Issue: Application Startup Delays
**Solution**: Enhanced application lifecycle management:
- Main window handle detection
- Process exit monitoring
- Configurable timeout values
- Better error reporting

### Debugging Tips

1. **Enable Detailed Logging**: Check test output for diagnostic information
2. **Check App State**: Verify the application starts correctly outside of tests
3. **Validate Configuration**: Ensure paths in `appsettings.windows.json` are correct
4. **WinAppDriver Status**: Confirm WinAppDriver is running and accessible

## Test Structure

### Page Objects
- `MainWindowPage`: Represents the main application window with enhanced Windows compatibility
- Enhanced element detection with fallback strategies
- Platform-specific wait logic

### Base Classes
- `BaseTest`: Provides common test infrastructure
- `AppiumDriverFactory`: Creates platform-specific drivers with enhanced Windows support

### Test Categories
- `BasicDockTests`: Core application functionality
- `DebugTests`: Development and debugging features
- `AutomationPropertiesTests`: UI automation verification

## Configuration

### Test Settings
- **CommandTimeout**: Maximum time for individual commands (60s)
- **ImplicitWait**: Default element wait time (10s)
- **WaitForApplicationToLoad**: Enhanced multi-stage timeout (30s total)

### Platform Detection
The test framework automatically detects the platform and applies appropriate:
- Driver configuration
- Element detection strategies
- Timeout values
- Diagnostic approaches

## Recent Windows Compatibility Improvements

### Enhanced WaitForApplicationToLoad Method
- **Multi-stage detection**: Process ready → Window visible → Elements ready → Stabilization
- **Platform-specific strategies**: Different approaches for Windows vs other platforms
- **Better error handling**: Detailed diagnostic information on failures
- **Improved reliability**: Reduced false positives and timeout issues

### Windows-Specific Element Detection
- Uses proper Windows automation attributes (`@AutomationId`, `@Name`, `@ControlType`)
- Validates element state (`Displayed` and `Enabled`)
- Fallback strategies for different Windows versions
- Enhanced XPath patterns for WinAppDriver

### Enhanced Application Lifecycle Management
- Better startup detection with main window handle verification
- Process monitoring during startup
- Extended timeout values for Windows environments
- Improved error reporting and diagnostics

These improvements significantly enhance the reliability of Appium tests on Windows platforms, addressing common issues with application loading detection and element accessibility.

## Running the Example Application

Before running tests, you can manually verify the application works:

```bash
cd ../../samples/DockMvvmSample/bin/Debug/net9.0
./DockMvvmSample.exe  # Windows
./DockMvvmSample      # macOS/Linux
```

The application should start and display the main dock interface with tools and document areas.

## Architecture

The test project follows the Page Object Model pattern:
- **Page Objects**: Represent UI components and interactions
- **Test Classes**: Contain test logic and assertions  
- **Infrastructure**: Handles driver creation, configuration, and utilities
- **Configuration**: Platform-specific settings and capabilities

This architecture provides maintainable, readable tests that can run reliably across different platforms with platform-specific optimizations. 