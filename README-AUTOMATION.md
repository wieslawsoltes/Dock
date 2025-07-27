# Automation Testing Setup for Dock Project

This document explains how to set up and run automation tests for the Dock project using proper macOS app bundles and automation properties, similar to [Avalonia's IntegrationTestApp](https://github.com/AvaloniaUI/Avalonia/tree/master/samples/IntegrationTestApp).

## Overview

The automation testing setup includes:
1. **Automation Properties** - Added to UI elements for proper identification
2. **App Bundle Creation** - Proper macOS `.app` bundle structure  
3. **Appium Integration** - Configured to work with the app bundle
4. **Test Infrastructure** - Comprehensive test framework

## Key Components

### 1. Automation Properties

UI elements in `DockMvvmSample` now have proper automation properties:

```xml
<!-- Main Window -->
<Window AutomationProperties.AutomationId="MainWindow"
        AutomationProperties.Name="Dock Avalonia Demo" />

<!-- Menu System -->
<Menu AutomationProperties.AutomationId="MainMenu"
      AutomationProperties.Name="Main Menu">
  <MenuItem AutomationProperties.AutomationId="FileMenu"
            AutomationProperties.Name="File Menu" />
</Menu>

<!-- Toolbar Buttons -->
<Button AutomationProperties.AutomationId="BackButton"
        AutomationProperties.Name="Back" />
<Button AutomationProperties.AutomationId="DashboardButton"
        AutomationProperties.Name="Dashboard" />
```

### 2. App Bundle Configuration

The `DockMvvmSample.csproj` includes macOS-specific properties:

```xml
<PropertyGroup Condition="'$(RuntimeIdentifier)' == 'osx-x64' OR '$(RuntimeIdentifier)' == 'osx-arm64'">
  <UseAppHost>true</UseAppHost>
  <CFBundleName>DockMvvmSample</CFBundleName>
  <CFBundleDisplayName>Dock Avalonia Demo</CFBundleDisplayName>
  <CFBundleIdentifier>com.avaloniaui.dockmvvmsample</CFBundleIdentifier>
  <NSHighResolutionCapable>true</NSHighResolutionCapable>
</PropertyGroup>
```

### 3. Info.plist Configuration

The app bundle includes automation support:

```xml
<!-- Accessibility and Automation Support -->
<key>NSAccessibilityUsageDescription</key>
<string>This application uses accessibility features for automated testing.</string>
<key>NSAppleEventsUsageDescription</key>
<string>This application allows automation for testing purposes.</string>
```

## Building the App Bundle

Use the provided script to build a proper macOS app bundle:

```bash
cd samples/DockMvvmSample
./build-app-bundle.sh
```

This creates: `samples/DockMvvmSample/bin/Debug/net9.0/DockMvvmSample.app`

The app bundle structure:
```
DockMvvmSample.app/
├── Contents/
│   ├── Info.plist
│   ├── MacOS/
│   │   ├── DockMvvmSample (executable)
│   │   ├── *.dll files
│   │   └── runtime dependencies
│   └── Resources/
│       └── DockMvvmSample.icns
```

## Running Tests

### Prerequisites
1. **macOS 10.15+** with accessibility permissions
2. **Appium Server** running on port 4723
3. **Built app bundle** using the script above

### Quick Test Run

```bash
cd tests/DockMvvmSample.AppiumTests
./Scripts/run-tests-macos.sh
```

### Manual Test Run

```bash
# Terminal 1: Start Appium
appium --base-path /wd/hub --port 4723

# Terminal 2: Run tests
cd tests/DockMvvmSample.AppiumTests
dotnet test
```

## Test Examples

### Finding Elements by Automation ID

```csharp
// Find main window using standard accessibility selectors
var mainWindow = Driver.FindElement(By.Id("MainWindow"));

// Find toolbar buttons
var backButton = Driver.FindElement(By.Id("BackButton"));
var dashboardButton = Driver.FindElement(By.Id("DashboardButton"));

// Find menu items
var fileMenu = Driver.FindElement(By.Id("FileMenu"));
var newLayoutMenuItem = Driver.FindElement(By.Id("NewLayoutMenuItem"));
```

### Interacting with Elements

```csharp
// Click buttons
dashboardButton.Click();

// Type in text boxes
var navigationTextBox = Driver.FindElement(By.XPath("//*[@AXIdentifier='NavigationTextBox']"));
navigationTextBox.SendKeys("TestPage");

// Open menus
fileMenu.Click();
```

## Available Test Classes

1. **BasicDockTests** - Debug and exploration tests
2. **AutomationPropertiesTests** - Demonstrates automation property usage:
   - `ApplicationStartsSuccessfully` - Verifies app launch and element discovery
   - `MainMenuIsAccessible` - Tests menu interactions
   - `ToolbarButtonsAreClickable` - Tests toolbar functionality

## Configuration Files

### macOS Test Configuration (`appsettings.macos.json`)

```json
{
  "DockMvvmSample": {
    "ExecutablePath": "/path/to/DockMvvmSample.app",
    "WindowTitle": "Dock Avalonia Demo"
  }
}
```

### Appium Capabilities

```json
{
  "platformName": "Mac",
  "automationName": "Mac2",
  "bundleId": "com.avaloniaui.dockmvvmsample"
}
```

## Troubleshooting

### App Bundle Issues
- Ensure executable permissions: `chmod +x DockMvvmSample.app/Contents/MacOS/DockMvvmSample`
- Verify Info.plist syntax with `plutil -lint Info.plist`
- Check bundle structure matches macOS requirements

### Automation Issues
- Grant accessibility permissions to Terminal and Appium
- Verify automation properties are set in XAML
- Check Appium server is running and accessible

### Element Not Found
- Use debug tests to explore available elements
- Verify automation ID values match XAML
- Check element visibility and timing

## Best Practices

1. **Use Descriptive Automation IDs** - Make them meaningful and unique
2. **Add Automation Names** - Provide human-readable descriptions
3. **Test on Real Devices** - Automation behavior can vary
4. **Use Explicit Waits** - Allow time for UI to load
5. **Verify App Bundle Structure** - Ensure proper macOS compliance

## Related Documentation

- [Avalonia IntegrationTestApp](https://github.com/AvaloniaUI/Avalonia/tree/master/samples/IntegrationTestApp)
- [macOS App Bundle Documentation](https://docs.avaloniaui.net/docs/deployment/macOS)
- [Appium macOS Testing Guide](http://appium.io/docs/en/drivers/mac/)
- [Accessibility Testing on macOS](https://developer.apple.com/accessibility/) 