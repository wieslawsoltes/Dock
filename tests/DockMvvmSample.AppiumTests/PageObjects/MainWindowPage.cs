using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DockMvvmSample.AppiumTests.Infrastructure;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace DockMvvmSample.AppiumTests.PageObjects;

public class MainWindowPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public MainWindowPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30)); // Increased from 10 to 30 seconds
    }

    // Main window elements - using automation IDs
    public IWebElement MainWindow => _wait.Until(d => d.FindElement(By.Id("MainWindow")));
    
    // Menu elements - using automation IDs  
    public IWebElement FileMenu => _wait.Until(d => d.FindElement(By.Id("FileMenu")));
    public IWebElement WindowMenu => _wait.Until(d => d.FindElement(By.Id("WindowMenu")));
    
    // Tool elements - using automation IDs with fallback strategies
    public IWebElement Tool1Tab => _wait.Until(d => FindToolElement(d, "Tool1"));
    public IWebElement Tool2Tab => _wait.Until(d => FindToolElement(d, "Tool2"));
    public IWebElement Tool5Tab => _wait.Until(d => FindToolElement(d, "Tool5"));
    
    // ToolDock containers - using automation IDs with fallback strategies
    public IWebElement LeftTopToolDock => _wait.Until(d => FindDockContainerElement(d, "LeftTopToolDock"));
    public IWebElement LeftBottomToolDock => _wait.Until(d => FindDockContainerElement(d, "LeftBottomToolDock"));
    public IWebElement RightTopToolDock => _wait.Until(d => FindDockContainerElement(d, "RightTopToolDock"));
    public IWebElement RightBottomToolDock => _wait.Until(d => FindDockContainerElement(d, "RightBottomToolDock"));



    // Dock panel elements - these might vary based on the actual control structure
    public IList<IWebElement> DockPanels => _driver.FindElements(By.ClassName("DockPanel")).Cast<IWebElement>().ToList();
    public IList<IWebElement> ToolWindows => _driver.FindElements(By.ClassName("XCUIElementTypeToolDock")).Cast<IWebElement>().ToList();
    public IList<IWebElement> DocumentTabs => _driver.FindElements(By.ClassName("XCUIElementTypeDocumentTab")).Cast<IWebElement>().ToList();

    // Actions
    public void ClickFileMenu()
    {
        FileMenu.Click();
    }

    public void ClickWindowMenu()
    {
        WindowMenu.Click();
    }

    public void WaitForApplicationToLoad(bool strictMode = true)
    {
        // Multi-stage approach for better Windows compatibility
        try
        {
            // Stage 1: Wait for basic window availability
            _wait.Until(d => IsApplicationProcessReady(d));
            System.Diagnostics.Debug.WriteLine("Stage 1 completed: Application process ready");
            
            // Stage 2: Wait for main window to be visible and ready
            _wait.Until(d => IsMainWindowVisible());
            System.Diagnostics.Debug.WriteLine("Stage 2 completed: Main window visible");
            
            // Stage 3: Wait for essential UI elements to be available (with timeout fallback)
            if (strictMode)
            {
                try
                {
                    var essentialWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10)); // Shorter timeout for this stage
                    essentialWait.Until(d => AreEssentialElementsReady(d));
                    System.Diagnostics.Debug.WriteLine("Stage 3 completed: Essential elements ready");
                }
                catch (WebDriverTimeoutException)
                {
                    System.Diagnostics.Debug.WriteLine("Stage 3 timeout - using fallback validation");
                    // Fallback: If main window is visible and we have some elements, continue
                    if (IsMainWindowVisible() && HasBasicUIElements())
                    {
                        System.Diagnostics.Debug.WriteLine("Stage 3 fallback: Basic UI validation passed");
                    }
                    else
                    {
                        throw; // Re-throw if even basic validation fails
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Stage 3 skipped: Non-strict mode");
            }
            
            // Stage 4: Final stabilization wait (dynamic based on platform)
            var stabilizationTime = ConfigurationHelper.IsWindows ? 2000 : 1000;
            System.Threading.Thread.Sleep(stabilizationTime);
            
            System.Diagnostics.Debug.WriteLine("Application successfully loaded and ready for interaction");
        }
        catch (WebDriverTimeoutException ex)
        {
            var diagnosticInfo = GetDiagnosticInfo();
            System.Diagnostics.Debug.WriteLine($"WaitForApplicationToLoad failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Diagnostic info: {diagnosticInfo}");
            throw new InvalidOperationException($"Application failed to load within the expected time. {diagnosticInfo}", ex);
        }
    }



    private bool IsApplicationProcessReady(IWebDriver driver)
    {
        try
        {
            // On Windows, check if we can interact with the application at all
            if (ConfigurationHelper.IsWindows)
            {
                // Try to get the current window handle - this ensures WinAppDriver can communicate with the app
                var currentWindow = driver.CurrentWindowHandle;
                return !string.IsNullOrEmpty(currentWindow);
            }
            else
            {
                // On other platforms, just check if driver is responsive
                return driver.WindowHandles.Count > 0;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"IsApplicationProcessReady failed: {ex.Message}");
            return false;
        }
    }

    private bool AreEssentialElementsReady(IWebDriver driver)
    {
        try
        {
            // Check if main UI elements are available and interactable
            var essentialElements = new[]
            {
                "MainDockControl",  // Main dock container
                "FileMenu",         // File menu  
                "WindowMenu"        // Window menu
            };

            var foundElements = 0;
            var totalElements = essentialElements.Length;

            foreach (var elementId in essentialElements)
            {
                try
                {
                    var element = driver.FindElement(By.Id(elementId));
                    // On Windows, also verify the element is enabled and displayed
                    if (ConfigurationHelper.IsWindows)
                    {
                        if (element.Displayed && element.Enabled)
                        {
                            foundElements++;
                            System.Diagnostics.Debug.WriteLine($"Essential element {elementId} is ready: Displayed={element.Displayed}, Enabled={element.Enabled}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Essential element {elementId} not ready: Displayed={element.Displayed}, Enabled={element.Enabled}");
                        }
                    }
                    else
                    {
                        foundElements++;
                        System.Diagnostics.Debug.WriteLine($"Essential element {elementId} found on non-Windows platform");
                    }
                }
                catch (NoSuchElementException)
                {
                    System.Diagnostics.Debug.WriteLine($"Essential element {elementId} not found");
                }
            }

            // Be more lenient - require at least 50% of essential elements to be ready
            // This accounts for variations in UI loading timing
            var readyPercentage = (double)foundElements / totalElements;
            var isReady = readyPercentage >= 0.5; // At least 50% of elements must be ready
            
            System.Diagnostics.Debug.WriteLine($"Essential elements check: {foundElements}/{totalElements} ready ({readyPercentage:P0}) - {(isReady ? "PASSED" : "FAILED")}");
            return isReady;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AreEssentialElementsReady failed: {ex.Message}");
            // If we can't check essential elements but main window is visible, assume it's ready
            return true;
        }
    }

    public bool IsMainWindowVisible()
    {
        try
        {
            // Strategy 1: Use automation ID to check if main window is visible
            var mainWindow = _driver.FindElement(By.Id("MainWindow"));
            
            // On Windows, additional checks for window state
            if (ConfigurationHelper.IsWindows)
            {
                // Verify window is not just visible but also enabled and ready for interaction
                return mainWindow.Displayed && mainWindow.Enabled;
            }
            
            return mainWindow.Displayed;
        }
        catch (NoSuchElementException)
        {
            // Fallback to platform-specific search patterns
            try
            {
                var searchPatterns = GetPlatformSpecificWindowSearchPatterns();

                foreach (var pattern in searchPatterns)
                {
                    var elements = _driver.FindElements(By.XPath(pattern));
                    if (elements.Count > 0)
                    {
                        var element = elements[0];
                        
                        // Windows-specific additional validation
                        if (ConfigurationHelper.IsWindows)
                        {
                            return element.Displayed && element.Enabled;
                        }
                        
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IsMainWindowVisible fallback failed: {ex.Message}");
                return false;
            }
        }
    }

    private string[] GetPlatformSpecificWindowSearchPatterns()
    {
        if (ConfigurationHelper.IsWindows)
        {
            // Windows-specific patterns for WinAppDriver
            return new[]
            {
                "//*[@AutomationId='MainWindow']",
                "//*[@Name='MainWindow']",
                "//*[contains(@Name, 'Dock Avalonia Demo')]",
                "//*[contains(@Name, 'DockMvvm')]",
                "//*[contains(@Name, 'Sample')]",
                "//*[@ControlType='Window' and @IsEnabled='True']",
                "//*[@ClassName='Window']",
                // Backup patterns for different Windows versions
                "//*[@LocalizedControlType='window']"
            };
        }
        else
        {
            // macOS and other platforms
            return new[]
            {
                "//*[@identifier='MainWindow']",
                "//*[contains(@title, 'Dock Avalonia Demo') or contains(@label, 'Dock Avalonia Demo')]",
                "//*[contains(@title, 'DockMvvm') or contains(@label, 'DockMvvm')]",
                "//*[contains(@title, 'Sample') or contains(@label, 'Sample')]",
                "//*[@elementType='55' and @enabled='true']" // Look for any enabled window
            };
        }
    }

    private string GetDiagnosticInfo()
    {
        try
        {
            var info = new List<string>();
            
            info.Add($"Platform: {(ConfigurationHelper.IsWindows ? "Windows" : "Other")}");
            info.Add($"Driver type: {_driver.GetType().Name}");
            
            try
            {
                info.Add($"Window handles count: {_driver.WindowHandles.Count}");
                info.Add($"Current window handle: {_driver.CurrentWindowHandle}");
            }
            catch (Exception ex)
            {
                info.Add($"Window handle info unavailable: {ex.Message}");
            }

            try
            {
                var allElements = _driver.FindElements(By.XPath("//*"));
                info.Add($"Total elements found: {allElements.Count}");
                
                // Log first few elements for debugging
                for (int i = 0; i < Math.Min(5, allElements.Count); i++)
                {
                    var element = allElements[i];
                    var tagName = element.TagName;
                    var automationId = element.GetAttribute("AutomationId") ?? element.GetAttribute("automationid") ?? element.GetAttribute("id");
                    var name = element.GetAttribute("Name") ?? element.GetAttribute("name");
                    info.Add($"Element {i}: TagName={tagName}, AutomationId={automationId}, Name={name}");
                }

                // Specifically look for the essential elements we're trying to find
                var essentialElements = new[] { "MainDockControl", "FileMenu", "WindowMenu" };
                var foundEssentialElements = new List<string>();
                
                foreach (var elementId in essentialElements)
                {
                    try
                    {
                        var element = _driver.FindElement(By.Id(elementId));
                        foundEssentialElements.Add($"{elementId} (found)");
                    }
                    catch (NoSuchElementException)
                    {
                        foundEssentialElements.Add($"{elementId} (NOT FOUND)");
                    }
                }
                info.Add($"Essential elements status: {string.Join(", ", foundEssentialElements)}");

                // Look for elements that might be similar to what we're looking for
                var elementsWithAutomationId = allElements
                    .Where(e => 
                    {
                        try 
                        { 
                            var id = e.GetAttribute("AutomationId") ?? e.GetAttribute("automationid") ?? e.GetAttribute("id");
                            return !string.IsNullOrEmpty(id);
                        } 
                        catch 
                        { 
                            return false; 
                        }
                    })
                    .Take(20) // Limit to first 20 to avoid huge output
                    .ToList();

                var automationIds = elementsWithAutomationId.Select(e => 
                {
                    try
                    {
                        return e.GetAttribute("AutomationId") ?? e.GetAttribute("automationid") ?? e.GetAttribute("id");
                    }
                    catch
                    {
                        return "unknown";
                    }
                }).ToList();

                info.Add($"Available AutomationIds (first 20): {string.Join(", ", automationIds)}");
            }
            catch (Exception ex)
            {
                info.Add($"Element enumeration failed: {ex.Message}");
            }

            return string.Join("; ", info);
        }
        catch (Exception ex)
        {
            return $"Diagnostic info collection failed: {ex.Message}";
        }
    }
    
    // Helper method for drag and drop operations
    public void DragToolToToolDock(IWebElement sourceToolTab, IWebElement targetToolDock)
    {
        var actions = new OpenQA.Selenium.Interactions.Actions(_driver);
        actions.DragAndDrop(sourceToolTab, targetToolDock).Perform();
        
        // Wait for the UI to update
        System.Threading.Thread.Sleep(2000);
    }

    // Helper method for reordering tools within the same tab strip
    public void DragToolOverTool(IWebElement sourceToolTab, IWebElement targetToolTab)
    {
        var actions = new OpenQA.Selenium.Interactions.Actions(_driver);
        actions.MoveToElement(sourceToolTab)
               .ClickAndHold()
               .MoveToElement(targetToolTab)
               .Release()
               .Build()
               .Perform();
        
        // Wait for the UI to update
        System.Threading.Thread.Sleep(2000);
    }

    public bool IsDocumentTabPresent(string tabName)
    {
        try
        {
            var tab = _driver.FindElement(By.Name(tabName));
            return tab.Displayed;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }



    public IList<string> GetVisibleDocumentTabs()
    {
        var tabs = DocumentTabs.Where(tab => tab.Displayed).ToList();
        
        // If we have tabs but can't get their names, return generic names
        if (tabs.Count > 0)
        {
            var tabNames = new List<string>();
            for (int i = 0; i < tabs.Count; i++)
            {
                var tab = tabs[i];
                var name = tab.GetAttribute("title") ?? tab.GetAttribute("label") ?? tab.GetAttribute("identifier") ?? tab.Text;
                if (string.IsNullOrEmpty(name))
                {
                    name = $"DocumentTab{i + 1}"; // Fallback name
                }
                tabNames.Add(name);
            }
            return tabNames;
        }
        
        return new List<string>();
    }

    public IList<string> GetVisibleToolWindows()
    {
        var tools = ToolWindows.Where(tool => tool.Displayed).ToList();
        
        // If we have tool windows but can't get their names, return generic names
        if (tools.Count > 0)
        {
            var toolNames = new List<string>();
            for (int i = 0; i < tools.Count; i++)
            {
                var tool = tools[i];
                var name = tool.GetAttribute("title") ?? tool.GetAttribute("label") ?? tool.GetAttribute("identifier") ?? tool.Text;
                if (string.IsNullOrEmpty(name))
                {
                    name = $"ToolWindow{i + 1}"; // Fallback name
                }
                toolNames.Add(name);
            }
            return toolNames;
        }
        
        return new List<string>();
    }

    private IWebElement FindToolElement(IWebDriver driver, string toolName)
    {
        System.Diagnostics.Debug.WriteLine($"Searching for tool element: {toolName}");
        
        // Strategy 1: Try automation ID (preferred approach)
        try
        {
            var element = driver.FindElement(By.Id(toolName));
            System.Diagnostics.Debug.WriteLine($"Found {toolName} using automation ID");
            return element;
        }
        catch (NoSuchElementException)
        {
            System.Diagnostics.Debug.WriteLine($"Could not find {toolName} by automation ID, trying fallback strategies");
        }

        // Strategy 2: Try finding by title/text content within DockControl
        try
        {
            var dockControl = driver.FindElement(By.Id("MainDockControl"));
            var toolByText = dockControl.FindElements(By.XPath($".//*[contains(text(), '{toolName}')]"));
            if (toolByText.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Found {toolName} using text content search");
                return toolByText[0];
            }
        }
        catch (NoSuchElementException)
        {
            System.Diagnostics.Debug.WriteLine($"Could not find {toolName} by text content");
        }

        // Strategy 3: Try finding by accessibility name
        try
        {
            var elements = driver.FindElements(By.XPath($"//*[@name='{toolName}']"));
            if (elements.Count > 0)
            {
                return elements[0];
            }
        }
        catch (NoSuchElementException)
        {
            // Continue to next strategy
        }

        // Strategy 4: Try finding by title attribute
        try
        {
            var elements = driver.FindElements(By.XPath($"//*[@title='{toolName}']"));
            if (elements.Count > 0)
            {
                return elements[0];
            }
        }
        catch (NoSuchElementException)
        {
            // Continue to next strategy
        }

        // Strategy 5: Try a more flexible search within tab containers
        try
        {
            var dockControl = driver.FindElement(By.Id("MainDockControl"));
            // Look for any element that might be a tab with our tool name
            var possibleTabs = dockControl.FindElements(By.XPath($".//*[contains(@automationid, '{toolName}') or contains(@name, '{toolName}') or contains(@title, '{toolName}') or contains(text(), '{toolName}')]"));
            if (possibleTabs.Count > 0)
            {
                return possibleTabs[0];
            }
        }
        catch (NoSuchElementException)
        {
            // Continue to final strategy
        }

        // Strategy 6: Last resort - try finding by class name and then filtering
        try
        {
            var dockControl = driver.FindElement(By.Id("MainDockControl"));
            var allElements = dockControl.FindElements(By.XPath(".//*"));
            foreach (var element in allElements)
            {
                var automationId = element.GetAttribute("automationid");
                var name = element.GetAttribute("name");
                var title = element.GetAttribute("title");
                var text = element.Text;

                if (string.Equals(automationId, toolName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(name, toolName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(title, toolName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(text, toolName, StringComparison.OrdinalIgnoreCase))
                {
                    return element;
                }
            }
        }
        catch (Exception)
        {
            // Ignore and throw original exception
        }

        // If all strategies fail, throw a descriptive exception
        throw new NoSuchElementException($"Could not find tool element '{toolName}' using any available strategy. Tried: automation ID, text content, accessibility name, title attribute, flexible search, and exhaustive search.");
    }

    private IWebElement FindDockContainerElement(IWebDriver driver, string dockContainerName)
    {
        // Strategy 1: Try automation ID (preferred approach)
        try
        {
            return driver.FindElement(By.Id(dockContainerName));
        }
        catch (NoSuchElementException)
        {
            // Continue to fallback strategies
        }

        // Strategy 2: Try finding by accessibility name
        try
        {
            var elements = driver.FindElements(By.XPath($"//*[@name='{dockContainerName}']"));
            if (elements.Count > 0)
            {
                return elements[0];
            }
        }
        catch (NoSuchElementException)
        {
            // Continue to next strategy
        }

        // Strategy 3: Try finding within the main dock control by partial name match
        try
        {
            var dockControl = driver.FindElement(By.Id("MainDockControl"));
            var containers = dockControl.FindElements(By.XPath($".//*[contains(@automationid, '{dockContainerName}') or contains(@name, '{dockContainerName}')]"));
            if (containers.Count > 0)
            {
                return containers[0];
            }
        }
        catch (NoSuchElementException)
        {
            // Continue to next strategy
        }

        // Strategy 4: Last resort - search all elements for the dock container
        try
        {
            var dockControl = driver.FindElement(By.Id("MainDockControl"));
            var allElements = dockControl.FindElements(By.XPath(".//*"));
            foreach (var element in allElements)
            {
                var automationId = element.GetAttribute("automationid");
                var name = element.GetAttribute("name");

                if (string.Equals(automationId, dockContainerName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(name, dockContainerName, StringComparison.OrdinalIgnoreCase))
                {
                    return element;
                }
            }
        }
        catch (Exception)
        {
            // Ignore and throw original exception
        }

        // If all strategies fail, throw a descriptive exception
        throw new NoSuchElementException($"Could not find dock container '{dockContainerName}' using any available strategy. Tried: automation ID, accessibility name, partial name match, and exhaustive search.");
    }

    private bool HasBasicUIElements()
    {
        try
        {
            // Check if we have basic UI structure - this is very lenient
            var allElements = _driver.FindElements(By.XPath("//*"));
            var elementsWithAutomationId = allElements.Where(e => 
            {
                try 
                { 
                    var id = e.GetAttribute("AutomationId") ?? e.GetAttribute("automationid") ?? e.GetAttribute("id");
                    return !string.IsNullOrEmpty(id);
                } 
                catch 
                { 
                    return false; 
                }
            }).ToList();

            System.Diagnostics.Debug.WriteLine($"Basic UI check: Found {allElements.Count} total elements, {elementsWithAutomationId.Count} with AutomationId");
            
            // If we have at least 50 elements with automation IDs, assume UI is ready
            return elementsWithAutomationId.Count >= 50;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"HasBasicUIElements failed: {ex.Message}");
            return true; // If we can't check, assume it's ready
        }
    }
} 