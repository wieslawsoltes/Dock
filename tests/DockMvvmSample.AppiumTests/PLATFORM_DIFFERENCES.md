# Windows vs macOS Appium Implementation Differences

This document explains the key differences between Windows and macOS Appium implementations and how our test infrastructure handles them.

## Architecture Differences

### Windows Architecture
```
Tests → Appium Server (port 4723) → WinAppDriver (port 4724) → Application
```
- **Two separate processes required**
- **Appium 2.x** acts as a proxy to **WinAppDriver 1.x**
- WinAppDriver must run as Administrator
- More complex setup and configuration

### macOS Architecture
```
Tests → Appium Server (port 4723) → Application
```
- **Single unified process**
- **Mac2 driver** built into Appium 2.x
- Direct automation without proxy layer
- Simpler setup and configuration

## Element Finding Differences

### Windows Element Finding
- **AutomationId attribute**: `@AutomationId` (case-sensitive)
- **Name attribute**: `@Name` 
- **Multiple fallback strategies** required due to WinAppDriver inconsistencies
- **Explicit waits only** - implicit waits don't work
- **500ms polling intervals** for stability

### macOS Element Finding  
- **Identifier attribute**: `@identifier` (primary)
- **Name attribute**: `@name`
- **Consistent behavior** with Mac2 driver
- **Implicit waits work** properly
- **250ms polling intervals** for responsiveness

## Configuration Differences

### Windows Configuration (`appsettings.windows.json`)
```json
{
  "AppiumSettings": {
    "ServerUrl": "http://127.0.0.1:4723",  // Appium server, NOT WinAppDriver
    "ImplicitWait": 0,                     // Disabled - doesn't work
    "DesiredCapabilities": {
      "platformName": "Windows",
      "automationName": "Windows",
      "shouldWaitForQuiescence": false,    // Windows-specific
      "ms:experimental-webdriver": true,   // WinAppDriver enhancement
      "ms:waitForAppLaunch": 10           // Windows timing
    }
  }
}
```

### macOS Configuration (`appsettings.macos.json`)
```json
{
  "AppiumSettings": {
    "ServerUrl": "http://127.0.0.1:4723",  // Direct to Appium
    "ImplicitWait": 10,                    // Works properly
    "DesiredCapabilities": {
      "platformName": "Mac",
      "automationName": "Mac2",
      "bundleId": "com.avaloniaui.dockmvvmsample"  // macOS app identification
    }
  }
}
```

## Driver Creation Differences

### Windows Driver Creation
```csharp
// Windows requires WindowsDriver with specific capabilities
var driver = new WindowsDriver<WindowsElement>(serverUri, options);

// NO implicit wait (doesn't work)
// Enhanced validation for WinAppDriver connectivity
// Longer stabilization wait (5 seconds)
```

### macOS Driver Creation
```csharp
// macOS uses MacDriver with AppiumWebElement
var driver = new MacDriver<AppiumWebElement>(serverUri, options);

// Implicit wait works properly
driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
// Simpler validation and faster startup
```

## Wait Strategy Differences

### Windows Wait Strategies
```csharp
// All waits must be explicit
var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
wait.PollingInterval = TimeSpan.FromMilliseconds(500); // Slower for stability

// Multiple element finding strategies with fallbacks
var element = wait.Until(driver => {
    // Try multiple XPath strategies
    // Handle WinAppDriver inconsistencies
});
```

### macOS Wait Strategies
```csharp
// Can use implicit waits
driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

// Faster polling for responsive UI
var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
wait.PollingInterval = TimeSpan.FromMilliseconds(250);

// Simpler, more consistent element finding
var element = driver.FindElement(By.XPath("//*[@identifier='elementId']"));
```

## Error Handling Differences

### Windows Error Patterns
- **Connection errors**: WinAppDriver not running or wrong permissions
- **Element not found**: Multiple strategies needed, timing issues
- **Session errors**: Server startup order problems
- **Permission errors**: Administrator rights required

### macOS Error Patterns
- **Permission errors**: Accessibility permissions needed
- **Bundle errors**: App bundle path or bundle ID issues
- **Driver errors**: Mac2 driver installation problems
- **Element errors**: Usually consistent, easier to debug

## Platform Detection in Code

Our infrastructure automatically detects the platform and applies appropriate strategies:

```csharp
public class ElementHelper
{
    private readonly bool _isWindows;
    
    public ElementHelper(IWebDriver driver)
    {
        _isWindows = IsWindowsPlatform(driver);
        // Apply platform-specific initialization
    }
    
    public IWebElement FindByAccessibilityId(string accessibilityId)
    {
        if (_isWindows)
        {
            return FindByAccessibilityIdWindows(accessibilityId); // Multiple strategies
        }
        else
        {
            return FindByAccessibilityIdMac(accessibilityId);     // Optimized for Mac2
        }
    }
}
```

## Setup Requirements Summary

### Windows Setup
1. **Enable Developer Mode** in Windows Settings
2. **Install WinAppDriver** (requires Administrator)
3. **Install Appium 2.x** with Windows driver
4. **Run WinAppDriver as Administrator** on port 4724
5. **Run Appium server** on port 4723
6. **Configure tests** to connect to port 4723

### macOS Setup
1. **Install Xcode Command Line Tools**
2. **Install Appium 2.x** with Mac2 driver
3. **Grant Accessibility permissions**
4. **Build app as .app bundle**
5. **Run Appium server** on port 4723
6. **Configure tests** with correct bundle ID

## Performance Characteristics

### Windows Performance
- **Slower element finding** due to multiple fallback strategies
- **Higher latency** due to proxy architecture (Appium → WinAppDriver)
- **More memory usage** from two server processes
- **Longer stabilization times** needed

### macOS Performance  
- **Faster element finding** with consistent driver behavior
- **Lower latency** with direct driver communication
- **Lower resource usage** from single process
- **Quicker test execution** overall

## Troubleshooting Quick Reference

### Windows: "Elements not found"
1. Check server URLs: Tests → 4723, WinAppDriver → 4724
2. Verify both servers are running
3. Check Developer Mode is enabled
4. Ensure WinAppDriver runs as Administrator
5. Use diagnostic script: `.\Scripts\diagnose-windows.ps1`

### macOS: "App not found"
1. Check app bundle path in configuration
2. Verify bundle ID matches Info.plist
3. Ensure accessibility permissions granted
4. Rebuild app bundle if needed
5. Check Mac2 driver installation

This architecture difference is the primary reason why the tests work well on macOS but have historically had issues on Windows. Our enhanced infrastructure now handles these differences automatically.
