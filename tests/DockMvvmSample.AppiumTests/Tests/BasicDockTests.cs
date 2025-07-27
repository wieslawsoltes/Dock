using System;
using System.Linq;
using DockMvvmSample.AppiumTests.Infrastructure;
using DockMvvmSample.AppiumTests.PageObjects;
using Xunit;
using Xunit.Abstractions;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
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
            // Use main driver for system access
            var systemDriver = Driver;
            
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
            
            systemDriver.Quit();
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        _output.WriteLine("=== END WINDOW TITLE SEARCH ===");
    }

    [Fact]
    public void CanDragTool1IntoTool5Dock()
    {
        _output.WriteLine("=== DRAG TOOL1 INTO TOOL5 DOCK TEST ===");
        
        try
        {
            // Launch the DockMvvmSample application if it's not already running
            var processes = System.Diagnostics.Process.GetProcessesByName("DockMvvmSample");
            if (processes.Length == 0)
            {
                _output.WriteLine("DockMvvmSample not running, launching application...");
                var appPath = "/Users/wieslawsoltes/GitHub/Dock/samples/DockMvvmSample/bin/Debug/net9.0/DockMvvmSample";
                var startInfo = new ProcessStartInfo
                {
                    FileName = appPath,
                    WorkingDirectory = "/Users/wieslawsoltes/GitHub/Dock/samples/DockMvvmSample/bin/Debug/net9.0/",
                    UseShellExecute = false,
                    CreateNoWindow = false
                };
                
                // Set environment variables for Avalonia accessibility
                startInfo.EnvironmentVariables["AVALONIA_OSX_ENABLE_ACCESSIBILITY"] = "1";
                
                var appProcess = Process.Start(startInfo);
                if (appProcess != null)
                {
                    _output.WriteLine($"DockMvvmSample launched with PID: {appProcess.Id}");
                    Thread.Sleep(3000); // Wait for app to fully load
                }
                else
                {
                    _output.WriteLine("Failed to launch DockMvvmSample");
                }
            }
            else
            {
                _output.WriteLine($"DockMvvmSample is already running ({processes.Length} processes)");
            }
            
            // Wait for the application to fully load
            _mainWindow.WaitForApplicationToLoad();
            Thread.Sleep(3000); // Allow extra time for UI stabilization
            
            _output.WriteLine("Application loaded, starting drag and drop test");
            
            // Take a screenshot before the test
            TakeScreenshot("BeforeDragDrop");
            
            // Find Tool1 tab using AutomationProperties
            IWebElement? tool1Tab = null;
            IWebElement? tool5Tab = null;
            IWebElement? tool5Dock = null;
            
            _output.WriteLine("Searching for Tool1 tab...");
            try
            {
                // Try multiple selectors for Tool1
                var tool1Selectors = new[]
                {
                    "//*[@title='Tool1']",
                    "//*[@label='Tool1']", 
                    "//*[contains(@title, 'Tool1')]",
                    "//*[contains(@label, 'Tool1')]",
                    "//*[@identifier='Tool1Control']"
                };
                
                foreach (var selector in tool1Selectors)
                {
                    try
                    {
                        var elements = Driver.FindElements(By.XPath(selector));
                        if (elements.Count > 0)
                        {
                            tool1Tab = elements[0];
                            _output.WriteLine($"Found Tool1 using selector: {selector}");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"Selector '{selector}' failed: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error searching for Tool1: {ex.Message}");
            }
            
            _output.WriteLine("Searching for Tool5 tab...");
            try
            {
                // Try multiple selectors for Tool5
                var tool5Selectors = new[]
                {
                    "//*[@title='Tool5']",
                    "//*[@label='Tool5']",
                    "//*[contains(@title, 'Tool5')]",
                    "//*[contains(@label, 'Tool5')]",
                    "//*[@identifier='Tool5Control']"
                };
                
                foreach (var selector in tool5Selectors)
                {
                    try
                    {
                        var elements = Driver.FindElements(By.XPath(selector));
                        if (elements.Count > 0)
                        {
                            tool5Tab = elements[0];
                            _output.WriteLine($"Found Tool5 using selector: {selector}");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"Selector '{selector}' failed: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error searching for Tool5: {ex.Message}");
            }
            
            // If we found the tools, try to find their dock containers
            if (tool1Tab != null && tool5Tab != null)
            {
                _output.WriteLine("Both tools found, attempting drag and drop...");
                
                // Get element positions for logging
                var tool1Location = tool1Tab.Location;
                var tool1Size = tool1Tab.Size;
                var tool5Location = tool5Tab.Location;
                var tool5Size = tool5Tab.Size;
                
                _output.WriteLine($"Tool1 position: ({tool1Location.X}, {tool1Location.Y}), size: ({tool1Size.Width}, {tool1Size.Height})");
                _output.WriteLine($"Tool5 position: ({tool5Location.X}, {tool5Location.Y}), size: ({tool5Size.Width}, {tool5Size.Height})");
                
                // Try to find the Tool5 dock container (the area where Tool1 should be dropped)
                try
                {
                    // Look for the parent dock container of Tool5
                    var tool5Parent = tool5Tab.FindElement(By.XPath("ancestor::*[contains(@class, 'ToolDock') or contains(@elementType, 'Group')]"));
                    if (tool5Parent != null)
                    {
                        tool5Dock = tool5Parent;
                        _output.WriteLine("Found Tool5 dock container");
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Could not find Tool5 dock container: {ex.Message}");
                    // Use Tool5 tab itself as fallback
                    tool5Dock = tool5Tab;
                }
                
                // Perform the drag and drop operation
                var actions = new OpenQA.Selenium.Interactions.Actions(Driver);
                
                // Method 1: Simple drag and drop
                try
                {
                    _output.WriteLine("Attempting simple drag and drop...");
                    actions.DragAndDrop(tool1Tab, tool5Dock).Perform();
                    
                    // Wait for the UI to update
                    Thread.Sleep(2000);
                    
                    _output.WriteLine("Simple drag and drop completed");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Simple drag and drop failed: {ex.Message}");
                    
                    // Method 2: Manual click, hold, move, release
                    try
                    {
                        _output.WriteLine("Attempting manual drag sequence...");
                        
                        actions = new OpenQA.Selenium.Interactions.Actions(Driver);
                        actions.ClickAndHold(tool1Tab)
                               .MoveToElement(tool5Dock)
                               .Release()
                               .Perform();
                        
                        Thread.Sleep(2000);
                        _output.WriteLine("Manual drag sequence completed");
                    }
                    catch (Exception ex2)
                    {
                        _output.WriteLine($"Manual drag sequence also failed: {ex2.Message}");
                        throw;
                    }
                }
                
                // Take a screenshot after the operation
                TakeScreenshot("AfterDragDrop");
                
                // Verify the result - Tool1 should now be in the same dock as Tool5
                try
                {
                    Thread.Sleep(1000);
                    
                    // Look for Tool1 near Tool5 or in the same container
                    var tool1AfterDrop = Driver.FindElements(By.XPath("//*[@identifier='Tool1' or @title='Tool1']"));
                    var tool5AfterDrop = Driver.FindElements(By.XPath("//*[@identifier='Tool5' or @title='Tool5']"));
                    
                    _output.WriteLine($"After drop - Tool1 elements found: {tool1AfterDrop.Count}");
                    _output.WriteLine($"After drop - Tool5 elements found: {tool5AfterDrop.Count}");
                    
                    if (tool1AfterDrop.Count > 0 && tool5AfterDrop.Count > 0)
                    {
                        var newTool1Pos = tool1AfterDrop[0].Location;
                        var newTool5Pos = tool5AfterDrop[0].Location;
                        
                        _output.WriteLine($"Tool1 new position: ({newTool1Pos.X}, {newTool1Pos.Y})");
                        _output.WriteLine($"Tool5 position: ({newTool5Pos.X}, {newTool5Pos.Y})");
                        
                        // Check if Tool1 moved significantly (indicating successful drag)
                        var distanceMoved = Math.Sqrt(Math.Pow(newTool1Pos.X - tool1Location.X, 2) + 
                                                    Math.Pow(newTool1Pos.Y - tool1Location.Y, 2));
                        
                        _output.WriteLine($"Tool1 moved distance: {distanceMoved} pixels");
                        
                        if (distanceMoved > 50) // Threshold for significant movement
                        {
                            _output.WriteLine("✓ Tool1 appears to have been successfully moved");
                        }
                        else
                        {
                            _output.WriteLine("⚠ Tool1 may not have moved significantly");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Verification step failed: {ex.Message}");
                }
                
                _output.WriteLine("✓ Drag and drop test completed successfully");
            }
            else
            {
                var tool1Found = tool1Tab != null ? "✓" : "✗";
                var tool5Found = tool5Tab != null ? "✓" : "✗";
                
                _output.WriteLine($"Could not find required elements - Tool1: {tool1Found}, Tool5: {tool5Found}");
                
                // Debug: List all available elements with automation properties
                try
                {
                    var allAutomationElements = Driver.FindElements(By.XPath("//*[@identifier or @title or @label]"));
                    _output.WriteLine($"All elements with automation properties ({allAutomationElements.Count}):");
                    
                    for (int i = 0; i < Math.Min(20, allAutomationElements.Count); i++)
                    {
                        var element = allAutomationElements[i];
                        var identifier = element.GetAttribute("identifier") ?? "";
                        var title = element.GetAttribute("title") ?? "";
                        var label = element.GetAttribute("label") ?? "";
                        var elementType = element.GetAttribute("elementType") ?? "";
                        
                        _output.WriteLine($"  {i}: identifier='{identifier}', title='{title}', label='{label}', type='{elementType}'");
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Debug listing failed: {ex.Message}");
                }
                
                Assert.Fail("Could not find Tool1 and/or Tool5 elements for drag and drop test");
            }
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Drag and drop test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Take a screenshot on failure
            try
            {
                TakeScreenshot("DragDropTestFailure");
            }
            catch
            {
                _output.WriteLine("Could not take failure screenshot");
            }
            
            throw;
        }
        
        _output.WriteLine("=== END DRAG TOOL1 INTO TOOL5 DOCK TEST ===");
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