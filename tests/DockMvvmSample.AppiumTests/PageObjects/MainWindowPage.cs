using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    #region Clean AccessibilityId Helper Methods

    /// <summary>
    /// Finds an element by AccessibilityId with automatic fallback to By.Id
    /// </summary>
    private IWebElement FindElementByAccessibilityId(string accessibilityId)
    {
        return (_driver as AppiumDriver<AppiumWebElement>)?.FindElementByAccessibilityId(accessibilityId) 
               ?? _driver.FindElement(By.Id(accessibilityId));
    }

    /// <summary>
    /// Finds an element by AccessibilityId with explicit wait and automatic fallback
    /// </summary>
    private IWebElement FindElementByAccessibilityIdWithWait(string accessibilityId, int timeoutInSeconds = 10)
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutInSeconds));
        return wait.Until(driver =>
        {
            try
            {
                return (driver as AppiumDriver<AppiumWebElement>)?.FindElementByAccessibilityId(accessibilityId) 
                       ?? driver.FindElement(By.Id(accessibilityId));
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        });
    }

    /// <summary>
    /// Clicks an element by AccessibilityId with wait
    /// </summary>
    private void ClickElement(string accessibilityId, int timeoutInSeconds = 10)
    {
        var element = FindElementByAccessibilityIdWithWait(accessibilityId, timeoutInSeconds);
        element.Click();
    }

    /// <summary>
    /// Types text into an element by AccessibilityId with wait
    /// </summary>
    private void TypeInElement(string accessibilityId, string text, bool clearFirst = true, int timeoutInSeconds = 10)
    {
        var element = FindElementByAccessibilityIdWithWait(accessibilityId, timeoutInSeconds);
        if (clearFirst) element.Clear();
        element.SendKeys(text);
    }

    #endregion

    #region Page Elements - Clean API

    // Main window elements using clean AccessibilityId methods
    public IWebElement MainWindow => FindElementByAccessibilityIdWithWait("MainWindow");
    public IWebElement FileMenu => FindElementByAccessibilityIdWithWait("FileMenu");
    public IWebElement WindowMenu => FindElementByAccessibilityIdWithWait("WindowMenu");

    // Navigation elements
    public IWebElement BackButton => FindElementByAccessibilityIdWithWait("BackButton");
    public IWebElement ForwardButton => FindElementByAccessibilityIdWithWait("ForwardButton");
    public IWebElement DashboardButton => FindElementByAccessibilityIdWithWait("DashboardButton");
    public IWebElement HomeButton => FindElementByAccessibilityIdWithWait("HomeButton");
    public IWebElement NavigationTextBox => FindElementByAccessibilityIdWithWait("NavigationTextBox");
    public IWebElement NavigateButton => FindElementByAccessibilityIdWithWait("NavigateButton");
    public IWebElement ThemeButton => FindElementByAccessibilityIdWithWait("ThemeButton");

    // Main dock control
    public IWebElement MainDockControl => FindElementByAccessibilityIdWithWait("MainDockControl");

    #endregion

    #region Legacy Elements (for backward compatibility)

    public IWebElement MainWindowLegacy => _wait.Until(d => d.FindElement(By.Id("MainWindow")));
    public IWebElement FileMenuLegacy => _wait.Until(d => d.FindElement(By.Id("FileMenu")));
    public IWebElement WindowMenuLegacy => _wait.Until(d => d.FindElement(By.Id("WindowMenu")));

    #endregion

    #region High-Level Actions using Clean Methods

    /// <summary>
    /// Clicks the File menu using clean AccessibilityId method
    /// </summary>
    public void ClickFileMenu()
    {
        ClickElement("FileMenu");
    }

    /// <summary>
    /// Navigates to a specific path using clean AccessibilityId methods
    /// </summary>
    public void NavigateToPath(string path)
    {
        TypeInElement("NavigationTextBox", path);
        ClickElement("NavigateButton");
    }

    /// <summary>
    /// Clicks the dashboard button using clean AccessibilityId method
    /// </summary>
    public void ClickDashboard()
    {
        ClickElement("DashboardButton");
    }

    /// <summary>
    /// Navigates back using clean AccessibilityId method
    /// </summary>
    public void NavigateBack()
    {
        ClickElement("BackButton");
    }

    /// <summary>
    /// Navigates forward using clean AccessibilityId method
    /// </summary>
    public void NavigateForward()
    {
        ClickElement("ForwardButton");
    }

    /// <summary>
    /// Gets the current navigation path
    /// </summary>
    public string GetCurrentNavigationPath()
    {
        var textBox = FindElementByAccessibilityIdWithWait("NavigationTextBox");
        return textBox.GetAttribute("value") ?? textBox.Text;
    }

    /// <summary>
    /// Verifies that all main UI elements are accessible
    /// </summary>
    public bool VerifyMainElementsAccessible()
    {
        var essentialElements = new[]
        {
            "MainWindow", "FileMenu", "MainDockControl", 
            "BackButton", "ForwardButton", "DashboardButton"
        };

        foreach (var elementId in essentialElements)
        {
            try
            {
                var element = FindElementByAccessibilityIdWithWait(elementId, 5);
                if (element == null) return false;
            }
            catch
            {
                return false;
            }
        }
        return true;
    }

    #endregion

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

    #region Missing Methods (for backward compatibility with existing tests)

    public void ClickWindowMenu()
    {
        ClickElement("WindowMenu");
    }

    public bool IsMainWindowVisible()
    {
        try
        {
            var mainWindow = FindElementByAccessibilityId("MainWindow");
            return mainWindow.Displayed;
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

    #endregion

    public void WaitForApplicationToLoad()
    {
        var maxWaitTime = TimeSpan.FromSeconds(30);
        var startTime = DateTime.Now;
        
        while (DateTime.Now - startTime < maxWaitTime)
        {
            try
            {
                var mainWindow = _driver.FindElement(By.Id("MainWindow"));
                if (mainWindow.Displayed)
                {
                    // Additional wait for UI elements to be ready
                    Thread.Sleep(2000);
                    return;
                }
            }
            catch (NoSuchElementException)
            {
                // Window not ready yet, continue waiting
            }
            
            Thread.Sleep(500);
        }
        
        throw new TimeoutException("Application did not load within the expected time frame");
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

    // Alternative methods using FindElementByAccessibilityId
    public IWebElement MainWindowByAccessibilityId => _wait.Until(d => 
        (d as AppiumDriver<AppiumWebElement>)?.FindElementByAccessibilityId("MainWindow") ?? d.FindElement(By.Id("MainWindow")));
    public IWebElement FileMenuByAccessibilityId => _wait.Until(d => 
        (d as AppiumDriver<AppiumWebElement>)?.FindElementByAccessibilityId("FileMenu") ?? d.FindElement(By.Id("FileMenu")));
    public IWebElement WindowMenuByAccessibilityId => _wait.Until(d => 
        (d as AppiumDriver<AppiumWebElement>)?.FindElementByAccessibilityId("WindowMenu") ?? d.FindElement(By.Id("WindowMenu")));
    
    // Method to find any element by accessibility ID with wait
    public IWebElement FindByAccessibilityId(string accessibilityId, int timeoutInSeconds = 10)
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutInSeconds));
        return wait.Until(driver => 
            (driver as AppiumDriver<AppiumWebElement>)?.FindElementByAccessibilityId(accessibilityId) 
            ?? driver.FindElement(By.Id(accessibilityId)));
    }
} 