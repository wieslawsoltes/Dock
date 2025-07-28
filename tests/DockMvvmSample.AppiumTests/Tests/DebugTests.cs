using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DockMvvmSample.AppiumTests.Infrastructure;
using DockMvvmSample.AppiumTests.PageObjects;
using OpenQA.Selenium;
using Xunit;
using Xunit.Abstractions;

namespace DockMvvmSample.AppiumTests.Tests;

[Collection("AppiumTests")]
public class DebugTests : BaseTest
{
    private readonly ITestOutputHelper _output;
    private readonly MainWindowPage _mainWindow;

    public DebugTests(ITestOutputHelper output)
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
            // Try to find app window using standard approach
            var appWindows = Driver.FindElements(By.XPath("//XCUIElementTypeWindow"));
            _output.WriteLine($"Found {appWindows.Count} app windows:");
            
            for (int i = 0; i < appWindows.Count; i++)
            {
                try
                {
                    var window = appWindows[i];
                    var name = window.GetAttribute("name") ?? window.GetAttribute("title") ?? "No Name";
                    var visible = window.GetAttribute("visible") ?? "unknown";
                    _output.WriteLine($"  Window {i}: Name='{name}', Visible='{visible}'");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"  Window {i}: Error getting info - {ex.Message}");
                }
            }
            
            // Try to find elements by automation ID using standard selectors
            try
            {
                var mainWindow = Driver.FindElement(By.Id("MainWindow"));
                _output.WriteLine("✓ Found MainWindow by Id");
                
                var mainMenu = Driver.FindElement(By.Id("MainMenu"));
                _output.WriteLine("✓ Found MainMenu by Id");
                
                var fileMenu = Driver.FindElement(By.Id("FileMenu"));
                _output.WriteLine("✓ Found FileMenu by Id");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Could not find elements by Id: {ex.Message}");
                
                // Fallback: try with XPath
                try
                {
                    var mainWindow = Driver.FindElement(By.XPath("//*[@identifier='MainWindow']"));
                    _output.WriteLine("✓ Found MainWindow by XPath identifier");
                }
                catch (Exception ex2)
                {
                    _output.WriteLine($"Could not find MainWindow by XPath: {ex2.Message}");
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
            appWindows = Driver.FindElements(By.XPath("//XCUIElementTypeWindow"));
            _output.WriteLine($"\nAfter 5 second wait, found {appWindows.Count} windows:");
            
            for (int i = 0; i < appWindows.Count; i++)
            {
                try
                {
                    var window = appWindows[i];
                    var name = window.GetAttribute("name") ?? window.GetAttribute("title") ?? "No Name";
                    var visible = window.GetAttribute("visible") ?? "unknown";
                    _output.WriteLine($"  Window {i}: Name='{name}', Visible='{visible}'");
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
            // Use the main driver for system-wide access
            var systemDriver = Driver;
            
            _output.WriteLine("Using main driver for system-wide access");
            
            // Try to find all applications
            var allApps = systemDriver.FindElements(By.XPath("//*[@AXRole='AXApplication']"));
            _output.WriteLine($"Found {allApps.Count} applications");
            
            // Try to find all windows
            var allWindows = systemDriver.FindElements(By.XPath("//*[@AXRole='AXWindow']"));
            _output.WriteLine($"Found {allWindows.Count} windows");
            
            // Try to find all elements
            var allElements = systemDriver.FindElements(By.XPath("//*"));
            _output.WriteLine($"Found {allElements.Count} total elements");
            
        }
        catch (Exception ex)
        {
            _output.WriteLine($"System-wide driver test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        _output.WriteLine("=== END SYSTEM WIDE DEBUG ===");
    }

    [Fact]
    public void DebugWithCorrectAttributes()
    {
        _output.WriteLine("=== DEBUG: Using correct Mac2/XCUITest attributes ===");
        
        try
        {
            // Use main driver for system access
            var systemDriver = Driver;
            
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
            
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Corrected attributes test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        _output.WriteLine("=== END CORRECTED ATTRIBUTES DEBUG ===");
    }

    [Fact]
    public void ExploreElementAttributes()
    {
        _output.WriteLine("=== DEBUG: Exploring actual element attributes ===");
        
        try
        {
            // Use main driver for system access
            var systemDriver = Driver;
            
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
            
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Element exploration failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        _output.WriteLine("=== END ELEMENT EXPLORATION ===");
    }

    [Fact]
    public void TestAppLaunchAndWindowDetection()
    {
        _output.WriteLine("=== DEBUG: Testing app launch and window detection ===");
        
        try
        {
            // Use main driver for system access
            var systemDriver = Driver;
            
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
                _output.WriteLine($"App process started with PID: {process.Id}");
                
                Thread.Sleep(8000);
                
                // Find all titled elements for debugging
                var allTitled = systemDriver.FindElements(By.XPath("//*[@title]"));
                _output.WriteLine($"Total titled elements: {allTitled.Count}");
                
                for (int i = 0; i < Math.Min(10, allTitled.Count); i++)
                {
                    try
                    {
                        var element = allTitled[i];
                        var title = element.GetAttribute("title") ?? "";
                        var elementType = element.GetAttribute("elementType") ?? "";
                        _output.WriteLine($"Titled element {i}: '{title}' (type: {elementType})");
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"Titled element {i}: Error - {ex.Message}");
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
            
        }
        catch (Exception ex)
        {
            _output.WriteLine($"App launch test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        _output.WriteLine("=== END APP LAUNCH DEBUG ===");
    }

    [Fact]
    public void FindActualWindowTitle()
    {
        _output.WriteLine("=== DEBUG: Finding actual window title ===");
        
        try
        {
            // Use main driver for system access
            var systemDriver = Driver;
            
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
            
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        _output.WriteLine("=== END WINDOW TITLE SEARCH ===");
    }
} 
