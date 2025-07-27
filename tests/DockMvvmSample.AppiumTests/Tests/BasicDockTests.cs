using System;
using System.Linq;
using DockMvvmSample.AppiumTests.Infrastructure;
using DockMvvmSample.AppiumTests.PageObjects;
using Xunit;
using Xunit.Abstractions;
using OpenQA.Selenium;
using System.Threading;
using System.Diagnostics;

namespace DockMvvmSample.AppiumTests.Tests;

[Collection("AppiumTests")]
public class BasicDockTests : BaseTest
{
    private readonly ITestOutputHelper _output;
    private readonly MainWindowPage _mainWindow;

    public BasicDockTests(ITestOutputHelper output)
    {
        _output = output;
        _mainWindow = new MainWindowPage(Driver);
    }

    [Fact]
    public void DebugAvailableElements()
    {
        _output.WriteLine("=== DEBUG: Listing all available elements ===");
        
        try
        {
            // Try to find all windows
            var allWindows = Driver.FindElements(By.XPath("//*[@AXRole='AXWindow']"));
            _output.WriteLine($"Found {allWindows.Count} windows:");
            
            for (int i = 0; i < allWindows.Count; i++)
            {
                try
                {
                    var window = allWindows[i];
                    var name = window.GetAttribute("AXTitle") ?? window.GetAttribute("name") ?? "No Name";
                    var role = window.GetAttribute("AXRole") ?? "No Role";
                    _output.WriteLine($"  Window {i}: Name='{name}', Role='{role}'");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"  Window {i}: Error getting info - {ex.Message}");
                }
            }
            
            // Try to find all elements with names
            var allNamedElements = Driver.FindElements(By.XPath("//*[@AXTitle]"));
            _output.WriteLine($"\nFound {allNamedElements.Count} elements with titles:");
            
            for (int i = 0; i < Math.Min(10, allNamedElements.Count); i++) // Limit to first 10
            {
                try
                {
                    var element = allNamedElements[i];
                    var title = element.GetAttribute("AXTitle");
                    var role = element.GetAttribute("AXRole") ?? "No Role";
                    _output.WriteLine($"  Element {i}: Title='{title}', Role='{role}'");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"  Element {i}: Error getting info - {ex.Message}");
                }
            }

            // Wait a bit to let app fully load
            Thread.Sleep(5000);
            
            // Try again after waiting
            allWindows = Driver.FindElements(By.XPath("//*[@AXRole='AXWindow']"));
            _output.WriteLine($"\nAfter 5 second wait, found {allWindows.Count} windows:");
            
            for (int i = 0; i < allWindows.Count; i++)
            {
                try
                {
                    var window = allWindows[i];
                    var name = window.GetAttribute("AXTitle") ?? window.GetAttribute("name") ?? "No Name";
                    var role = window.GetAttribute("AXRole") ?? "No Role";
                    _output.WriteLine($"  Window {i}: Name='{name}', Role='{role}'");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"  Window {i}: Error getting info - {ex.Message}");
                }
            }
            
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Debug test failed with error: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        _output.WriteLine("=== END DEBUG ===");
    }

    [Fact]
    public void DebugSystemWideWithoutApp()
    {
        _output.WriteLine("=== DEBUG: Creating system-wide Mac2 driver ===");
        
        try
        {
            // Create a new system-wide driver
            var systemDriver = AppiumDriverFactory.CreateSystemWideMacDriver(Configuration).Driver;
            
            _output.WriteLine("System-wide driver created successfully");
            
            // Try to find all applications
            var allApps = systemDriver.FindElements(By.XPath("//*[@AXRole='AXApplication']"));
            _output.WriteLine($"Found {allApps.Count} applications");
            
            // Try to find all windows
            var allWindows = systemDriver.FindElements(By.XPath("//*[@AXRole='AXWindow']"));
            _output.WriteLine($"Found {allWindows.Count} windows");
            
            // Try to find all elements
            var allElements = systemDriver.FindElements(By.XPath("//*"));
            _output.WriteLine($"Found {allElements.Count} total elements");
            
            systemDriver.Quit();
        }
        catch (Exception ex)
        {
            _output.WriteLine($"System-wide driver test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        _output.WriteLine("=== END SYSTEM WIDE DEBUG ===");
    }

    [Fact]
    public void ApplicationStartsSuccessfully()
    {
        // Arrange & Act
        _mainWindow.WaitForApplicationToLoad();

        // Assert
        Assert.True(_mainWindow.IsMainWindowVisible(), "Main window should be visible after application starts");
        
        _output.WriteLine("✓ Application started successfully and main window is visible");
    }

    [Fact]
    public void MainMenuIsAccessible()
    {
        // Arrange
        _mainWindow.WaitForApplicationToLoad();

        // Act & Assert
        _mainWindow.ClickFileMenu();
        _output.WriteLine("✓ File menu is accessible");

        _mainWindow.ClickWindowMenu();
        _output.WriteLine("✓ Window menu is accessible");
    }

    [Fact]
    public void DocumentTabsAreVisible()
    {
        // Arrange
        _mainWindow.WaitForApplicationToLoad();

        // Act
        var documentTabs = _mainWindow.GetVisibleDocumentTabs();

        // Assert
        Assert.NotEmpty(documentTabs);
        _output.WriteLine($"✓ Found {documentTabs.Count} document tabs: {string.Join(", ", documentTabs)}");

        // Check for expected default documents
        Assert.Contains(documentTabs, tab => tab.Contains("Document"));
    }

    [Fact]
    public void ToolWindowsAreVisible()
    {
        // Arrange
        _mainWindow.WaitForApplicationToLoad();

        // Act
        var toolWindows = _mainWindow.GetVisibleToolWindows();

        // Assert
        Assert.NotEmpty(toolWindows);
        _output.WriteLine($"✓ Found {toolWindows.Count} tool windows: {string.Join(", ", toolWindows)}");

        // Check for expected default tools
        Assert.Contains(toolWindows, tool => tool.Contains("Tool"));
    }



    [Fact]
    public void ExploreElementAttributes()
    {
        _output.WriteLine("=== DEBUG: Exploring actual element attributes ===");
        
        try
        {
            // Create a new system-wide driver
            var systemDriver = AppiumDriverFactory.CreateSystemWideMacDriver(Configuration).Driver;
            
            // Get all elements
            var allElements = systemDriver.FindElements(By.XPath("//*"));
            _output.WriteLine($"Found {allElements.Count} total elements");
            
            // Explore the first 10 elements to see their attributes
            for (int i = 0; i < Math.Min(10, allElements.Count); i++)
            {
                try
                {
                    var element = allElements[i];
                    _output.WriteLine($"\n--- Element {i} ---");
                    
                    // Try common accessibility attributes
                    var role = element.GetAttribute("AXRole") ?? element.GetAttribute("role") ?? "null";
                    var name = element.GetAttribute("AXName") ?? element.GetAttribute("name") ?? "null";
                    var title = element.GetAttribute("AXTitle") ?? element.GetAttribute("title") ?? "null";
                    var value = element.GetAttribute("AXValue") ?? element.GetAttribute("value") ?? "null";
                    var tagName = element.TagName ?? "null";
                    
                    _output.WriteLine($"  TagName: {tagName}");
                    _output.WriteLine($"  AXRole: {role}");
                    _output.WriteLine($"  AXName: {name}");
                    _output.WriteLine($"  AXTitle: {title}");
                    _output.WriteLine($"  AXValue: {value}");
                    
                    // Try to get all available attributes
                    try
                    {
                        var text = element.Text ?? "null";
                        _output.WriteLine($"  Text: {text}");
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"  Text: Error - {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"  Element {i}: Error getting attributes - {ex.Message}");
                }
            }
            
            // Try different role searches
            _output.WriteLine("\n--- Role Search Results ---");
            
            var windowElements = systemDriver.FindElements(By.XPath("//XCUIElementTypeWindow"));
            _output.WriteLine($"XCUIElementTypeWindow: {windowElements.Count}");
            
            var appElements = systemDriver.FindElements(By.XPath("//XCUIElementTypeApplication"));
            _output.WriteLine($"XCUIElementTypeApplication: {appElements.Count}");
            
            // Try searching by tag name instead of XPath attributes
            try
            {
                var windowsByTag = systemDriver.FindElements(By.TagName("XCUIElementTypeWindow"));
                _output.WriteLine($"TagName=XCUIElementTypeWindow: {windowsByTag.Count}");
                
                var appsByTag = systemDriver.FindElements(By.TagName("XCUIElementTypeApplication"));
                _output.WriteLine($"TagName=XCUIElementTypeApplication: {appsByTag.Count}");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"TagName search error: {ex.Message}");
            }
            
            systemDriver.Quit();
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Element exploration failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        _output.WriteLine("=== END ELEMENT EXPLORATION ===");
    }

    [Fact]
    public void DebugWithCorrectAttributes()
    {
        _output.WriteLine("=== DEBUG: Using correct Mac2/XCUITest attributes ===");
        
        try
        {
            // Create a new system-wide driver
            var systemDriver = AppiumDriverFactory.CreateSystemWideMacDriver(Configuration).Driver;
            
            // Get all elements
            var allElements = systemDriver.FindElements(By.XPath("//*"));
            _output.WriteLine($"Found {allElements.Count} total elements");
            
            // Explore the first 10 elements with correct attributes
            for (int i = 0; i < Math.Min(10, allElements.Count); i++)
            {
                try
                {
                    var element = allElements[i];
                    _output.WriteLine($"\n--- Element {i} ---");
                    
                    // Use correct Mac2/XCUITest attributes
                    var elementType = element.GetAttribute("elementType") ?? "null";
                    var label = element.GetAttribute("label") ?? "null";
                    var title = element.GetAttribute("title") ?? "null";
                    var value = element.GetAttribute("value") ?? "null";
                    var identifier = element.GetAttribute("identifier") ?? "null";
                    var enabled = element.GetAttribute("enabled") ?? "null";
                    
                    _output.WriteLine($"  elementType: {elementType}");
                    _output.WriteLine($"  label: {label}");
                    _output.WriteLine($"  title: {title}");
                    _output.WriteLine($"  value: {value}");
                    _output.WriteLine($"  identifier: {identifier}");
                    _output.WriteLine($"  enabled: {enabled}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"  Element {i}: Error - {ex.Message}");
                }
            }
            
            // Search for applications and windows using correct attributes
            _output.WriteLine("\n--- Searching with correct elementType ---");
            
            var applications = systemDriver.FindElements(By.XPath("//*[@elementType='XCUIElementTypeApplication']"));
            _output.WriteLine($"Applications: {applications.Count}");
            
            if (applications.Count > 0)
            {
                for (int i = 0; i < Math.Min(5, applications.Count); i++)
                {
                    var app = applications[i];
                    var label = app.GetAttribute("label") ?? "null";
                    var title = app.GetAttribute("title") ?? "null";
                    _output.WriteLine($"  App {i}: label='{label}', title='{title}'");
                }
            }
            
            var windows = systemDriver.FindElements(By.XPath("//*[@elementType='XCUIElementTypeWindow']"));
            _output.WriteLine($"Windows: {windows.Count}");
            
            if (windows.Count > 0)
            {
                for (int i = 0; i < Math.Min(5, windows.Count); i++)
                {
                    var window = windows[i];
                    var label = window.GetAttribute("label") ?? "null";
                    var title = window.GetAttribute("title") ?? "null";
                    _output.WriteLine($"  Window {i}: label='{label}', title='{title}'");
                }
            }
            
            // Look for anything with "Dock" in the title or label
            var dockElements = systemDriver.FindElements(By.XPath("//*[contains(@title, 'Dock') or contains(@label, 'Dock')]"));
            _output.WriteLine($"Elements containing 'Dock': {dockElements.Count}");
            
            systemDriver.Quit();
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Corrected attributes test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        _output.WriteLine("=== END CORRECTED ATTRIBUTES DEBUG ===");
    }

    [Fact]
    public void TestAppLaunchAndWindowDetection()
    {
        _output.WriteLine("=== DEBUG: Testing app launch and window detection ===");
        
        try
        {
            // Create a system-wide driver
            var systemDriver = AppiumDriverFactory.CreateSystemWideMacDriver(Configuration).Driver;
            
            _output.WriteLine("System-wide driver created successfully");
            
            // Check initial state
            var initialElements = systemDriver.FindElements(By.XPath("//*[contains(@title, 'Dock') or contains(@label, 'Dock')]"));
            _output.WriteLine($"Initial elements containing 'Dock': {initialElements.Count}");
            
            // Try to manually launch the app
            var appPath = "/Users/wieslawsoltes/GitHub/Dock/samples/DockMvvmSample/bin/Debug/net9.0/DockMvvmSample";
            _output.WriteLine($"Launching app at: {appPath}");
            
            var startInfo = new ProcessStartInfo
            {
                FileName = appPath,
                WorkingDirectory = "/Users/wieslawsoltes/GitHub/Dock/samples/DockMvvmSample/bin/Debug/net9.0/",
                UseShellExecute = false,
                CreateNoWindow = false
            };
            
            // Set environment variables for Avalonia
            startInfo.EnvironmentVariables["DISPLAY"] = ":0";
            startInfo.EnvironmentVariables["NSHighResolutionCapable"] = "YES";
            startInfo.EnvironmentVariables["AVALONIA_OSX_ENABLE_ACCESSIBILITY"] = "1";
            
            var process = Process.Start(startInfo);
            if (process != null)
            {
                _output.WriteLine($"App process started with PID: {process.Id}");
                
                // Wait for app to fully load
                Thread.Sleep(8000);
                
                // Check all windows now
                var allWindows = systemDriver.FindElements(By.XPath("//*[@elementType='55']")); // 55 is window type
                _output.WriteLine($"Found {allWindows.Count} windows after app launch");
                
                // Check all elements with any title
                var allTitled = systemDriver.FindElements(By.XPath("//*[@title!='']"));
                _output.WriteLine($"Found {allTitled.Count} elements with non-empty titles");
                
                // Show first 10 titled elements
                for (int i = 0; i < Math.Min(10, allTitled.Count); i++)
                {
                    try
                    {
                        var element = allTitled[i];
                        var title = element.GetAttribute("title") ?? "";
                        var elementType = element.GetAttribute("elementType") ?? "";
                        _output.WriteLine($"  Element {i}: title='{title}', type='{elementType}'");
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"  Element {i}: Error - {ex.Message}");
                    }
                }
                
                // Look for Dock or Avalonia specifically
                var dockElements = systemDriver.FindElements(By.XPath("//*[contains(@title, 'Dock') or contains(@label, 'Dock') or contains(@title, 'Avalonia') or contains(@label, 'Avalonia')]"));
                _output.WriteLine($"Elements containing 'Dock' or 'Avalonia': {dockElements.Count}");
                
                // Kill the process
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                        process.WaitForExit(5000);
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Error killing process: {ex.Message}");
                }
            }
            else
            {
                _output.WriteLine("Failed to start app process");
            }
            
            systemDriver.Quit();
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        _output.WriteLine("=== END APP LAUNCH TEST ===");
    }

    [Fact]
    public void FindActualWindowTitle()
    {
        _output.WriteLine("=== DEBUG: Finding actual window title ===");
        
        try
        {
            // Create a system-wide driver
            var systemDriver = AppiumDriverFactory.CreateSystemWideMacDriver(Configuration).Driver;
            
            // Get initial window count
            var initialWindows = systemDriver.FindElements(By.XPath("//*[@elementType='55']"));
            _output.WriteLine($"Initial windows: {initialWindows.Count}");
            
            // Launch the app
            var appPath = "/Users/wieslawsoltes/GitHub/Dock/samples/DockMvvmSample/bin/Debug/net9.0/DockMvvmSample";
            var startInfo = new ProcessStartInfo
            {
                FileName = appPath,
                WorkingDirectory = "/Users/wieslawsoltes/GitHub/Dock/samples/DockMvvmSample/bin/Debug/net9.0/",
                UseShellExecute = false,
                CreateNoWindow = false
            };
            
            startInfo.EnvironmentVariables["DISPLAY"] = ":0";
            startInfo.EnvironmentVariables["NSHighResolutionCapable"] = "YES";
            startInfo.EnvironmentVariables["AVALONIA_OSX_ENABLE_ACCESSIBILITY"] = "1";
            
            var process = Process.Start(startInfo);
            if (process != null)
            {
                _output.WriteLine($"App started with PID: {process.Id}");
                
                // Wait for app to load
                Thread.Sleep(8000);
                
                // Get all windows now
                var allWindows = systemDriver.FindElements(By.XPath("//*[@elementType='55']"));
                _output.WriteLine($"Windows after launch: {allWindows.Count}");
                
                // Show details of ALL windows
                for (int i = 0; i < allWindows.Count; i++)
                {
                    try
                    {
                        var window = allWindows[i];
                        var title = window.GetAttribute("title") ?? "";
                        var label = window.GetAttribute("label") ?? "";
                        var identifier = window.GetAttribute("identifier") ?? "";
                        var enabled = window.GetAttribute("enabled") ?? "";
                        
                        _output.WriteLine($"Window {i}:");
                        _output.WriteLine($"  title='{title}'");
                        _output.WriteLine($"  label='{label}'");
                        _output.WriteLine($"  identifier='{identifier}'");
                        _output.WriteLine($"  enabled='{enabled}'");
                        _output.WriteLine("");
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"Window {i}: Error - {ex.Message}");
                    }
                }
                
                // Also look for any elements that might be our app
                var possibleAppElements = systemDriver.FindElements(By.XPath("//*[contains(@title, 'DockMvvm') or contains(@label, 'DockMvvm') or contains(@title, 'Sample') or contains(@label, 'Sample')]"));
                _output.WriteLine($"Elements containing 'DockMvvm' or 'Sample': {possibleAppElements.Count}");
                
                for (int i = 0; i < possibleAppElements.Count; i++)
                {
                    try
                    {
                        var element = possibleAppElements[i];
                        var title = element.GetAttribute("title") ?? "";
                        var label = element.GetAttribute("label") ?? "";
                        var elementType = element.GetAttribute("elementType") ?? "";
                        _output.WriteLine($"Possible app element {i}: title='{title}', label='{label}', type='{elementType}'");
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"App element {i}: Error - {ex.Message}");
                    }
                }
                
                // Kill the process
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                        process.WaitForExit(5000);
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Error killing process: {ex.Message}");
                }
            }
            
            systemDriver.Quit();
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        _output.WriteLine("=== END WINDOW TITLE SEARCH ===");
    }

    public override void Dispose()
    {
        try
        {
            TakeScreenshot($"{GetType().Name}_Final");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Warning: Could not take final screenshot: {ex.Message}");
        }
        
        base.Dispose();
    }
} 