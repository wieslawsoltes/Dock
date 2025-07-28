using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DockMvvmSample.AppiumTests.Infrastructure;
using OpenQA.Selenium;

namespace DockMvvmSample.AppiumTests.PageObjects;

/// <summary>
/// Main window page object using the new BasePage infrastructure
/// Clean, maintainable implementation with fluent API and comprehensive validation
/// </summary>
public class MainWindowPage : BasePage
{
    public MainWindowPage(IWebDriver driver) : base(driver)
    {
    }

    #region Essential Elements Definition

    protected override bool IsPageLoaded()
    {
        return TryFindElement("MainWindow", out var mainWindow) && 
               mainWindow != null && mainWindow.Displayed &&
               TryFindElement("MainDockControl", out var dockControl) &&
               dockControl != null && dockControl.Displayed;
    }

    protected override string[] GetEssentialElementIds()
    {
        return new[] 
        { 
            "MainWindow", 
            "FileMenu", 
            "MainDockControl", 
            "BackButton", 
            "ForwardButton", 
            "DashboardButton" 
        };
    }

    #endregion

    #region Page Elements (Clean Property Access)

    public IWebElement MainWindow => Elements.FindByAccessibilityIdWithWait("MainWindow");
    public IWebElement FileMenu => Elements.FindByAccessibilityIdWithWait("FileMenu");
    public IWebElement WindowMenu => Elements.FindByAccessibilityIdWithWait("WindowMenu");
    public IWebElement BackButton => Elements.FindByAccessibilityIdWithWait("BackButton");
    public IWebElement ForwardButton => Elements.FindByAccessibilityIdWithWait("ForwardButton");
    public IWebElement DashboardButton => Elements.FindByAccessibilityIdWithWait("DashboardButton");
    public IWebElement HomeButton => Elements.FindByAccessibilityIdWithWait("HomeButton");
    public IWebElement NavigationTextBox => Elements.FindByAccessibilityIdWithWait("NavigationTextBox");
    public IWebElement NavigateButton => Elements.FindByAccessibilityIdWithWait("NavigateButton");
    public IWebElement ThemeButton => Elements.FindByAccessibilityIdWithWait("ThemeButton");
    public IWebElement MainDockControl => Elements.FindByAccessibilityIdWithWait("MainDockControl");

    // Tool elements - using advanced finding strategies for backward compatibility
    public IWebElement Tool1Tab => FindTool("Tool1");
    public IWebElement Tool2Tab => FindTool("Tool2");
    public IWebElement Tool5Tab => FindTool("Tool5");
    
    // ToolDock containers - using element finding with fallback strategies
    public IWebElement LeftTopToolDock => FindDockContainer("LeftTopToolDock");
    public IWebElement LeftBottomToolDock => FindDockContainer("LeftBottomToolDock");
    public IWebElement RightTopToolDock => FindDockContainer("RightTopToolDock");
    public IWebElement RightBottomToolDock => FindDockContainer("RightBottomToolDock");

    #endregion

    #region High-Level Page Actions (Fluent API)

    /// <summary>
    /// Clicks the File menu using the new fluent API
    /// </summary>
    public MainWindowPage ClickFileMenu()
    {
        ClickMenu("FileMenu");
        return this;
    }

    /// <summary>
    /// Clicks the Window menu using the new fluent API
    /// </summary>
    public MainWindowPage ClickWindowMenu()
    {
        ClickMenu("WindowMenu");
        return this;
    }

    /// <summary>
    /// Navigates to a specific path using the navigation controls
    /// </summary>
    public MainWindowPage NavigateToPath(string path)
    {
        base.NavigateToPath("NavigationTextBox", "NavigateButton", path);
        return this;
    }

    /// <summary>
    /// Clicks the dashboard button
    /// </summary>
    public MainWindowPage ClickDashboard()
    {
        ClickElement("DashboardButton");
        return this;
    }

    /// <summary>
    /// Navigates back using the back button
    /// </summary>
    public MainWindowPage NavigateBack()
    {
        ClickElement("BackButton");
        return this;
    }

    /// <summary>
    /// Navigates forward using the forward button
    /// </summary>
    public MainWindowPage NavigateForward()
    {
        ClickElement("ForwardButton");
        return this;
    }

    /// <summary>
    /// Gets the current navigation path
    /// </summary>
    public string GetCurrentNavigationPath()
    {
        return GetElementAttribute("NavigationTextBox", "value") ?? GetElementText("NavigationTextBox");
    }

    /// <summary>
    /// Verifies that all main UI elements are accessible using the new validation methods
    /// </summary>
    public bool VerifyMainElementsAccessible()
    {
        return ValidatePageElements();
    }

    #endregion

    #region Advanced Interactions (Showing Fluent API Capabilities)

    /// <summary>
    /// Demonstrates fluent API for complex workflows
    /// </summary>
    public MainWindowPage PerformComplexNavigation(string targetPath)
    {
        // Chain multiple operations using fluent API
        WaitForVisible("NavigationTextBox");
        TypeInElement("NavigationTextBox", targetPath);
        WaitForClickable("NavigateButton");
        ClickElement("NavigateButton");
        WaitForVisible("MainDockControl");
        return this;
    }

    /// <summary>
    /// Demonstrates element-specific fluent chains
    /// </summary>
    public string GetNavigationPathWithValidation()
    {
        // Using the fluent element actions
        var pathText = Elements.PerformElementAction("NavigationTextBox", element =>
        {
            if (!element.Displayed)
                throw new InvalidOperationException("Navigation text box is not visible");
            return element.GetAttribute("value") ?? element.Text;
        });

        return pathText;
    }

    /// <summary>
    /// Shows how to use the new validation methods
    /// </summary>
    public Dictionary<string, bool> GetDetailedElementStatus()
    {
        var elementsToCheck = new[]
        {
            "MainWindow", "FileMenu", "WindowMenu", "BackButton", "ForwardButton",
            "DashboardButton", "NavigationTextBox", "NavigateButton", "MainDockControl"
        };

        return Elements.ValidateVisibility(elementsToCheck);
    }

    #endregion

    #region Tool and Dock Management (Simplified with New Infrastructure)

    /// <summary>
    /// Finds tool elements using the advanced element finding with fallback strategies
    /// </summary>
    public IWebElement FindTool(string toolName)
    {
        // Try direct accessibility ID first
        if (TryFindElement(toolName, out var tool) && tool != null)
        {
            return tool;
        }

        // Use advanced finding with custom condition
        var dockControl = Elements.FindByAccessibilityIdWithWait("MainDockControl");
        var toolElements = dockControl.FindElements(By.XPath($".//*[contains(@name, '{toolName}') or contains(text(), '{toolName}')]"));
        var foundTool = toolElements.FirstOrDefault();
        
        if (foundTool == null)
            throw new NoSuchElementException($"Could not find tool '{toolName}' using any available strategy");
            
        return foundTool;
    }

    /// <summary>
    /// Finds dock container elements using advanced element finding with fallback strategies
    /// </summary>
    private IWebElement FindDockContainer(string containerName)
    {
        // Try direct accessibility ID first
        if (TryFindElement(containerName, out var container) && container != null)
        {
            return container;
        }

        // Use advanced finding with dock control
        var dockControl = Elements.FindByAccessibilityIdWithWait("MainDockControl");
        var containerElements = dockControl.FindElements(By.XPath($".//*[contains(@name, '{containerName}') or contains(@automationid, '{containerName}')]"));
        var foundContainer = containerElements.FirstOrDefault();
        
        if (foundContainer == null)
            throw new NoSuchElementException($"Could not find dock container '{containerName}' using any available strategy");
            
        return foundContainer;
    }

    /// <summary>
    /// Gets visible document tabs using the new element helpers
    /// </summary>
    public IList<string> GetVisibleDocumentTabs()
    {
        try
        {
            var mainDock = MainDockControl;
            var tabElements = mainDock.FindElements(By.XPath(".//*[contains(@class, 'Tab') or contains(@role, 'tab')]"));
            
            return tabElements
                .Where(tab => tab.Displayed)
                .Select(tab => tab.GetAttribute("title") ?? tab.GetAttribute("name") ?? tab.Text ?? "Unknown Tab")
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();
        }
        catch (NoSuchElementException)
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Gets visible tool windows using the new element helpers
    /// </summary>
    public IList<string> GetVisibleToolWindows()
    {
        try
        {
            var mainDock = MainDockControl;
            var toolElements = mainDock.FindElements(By.XPath(".//*[contains(@class, 'Tool') or contains(@role, 'tool')]"));
            
            return toolElements
                .Where(tool => tool.Displayed)
                .Select(tool => tool.GetAttribute("title") ?? tool.GetAttribute("name") ?? tool.Text ?? "Unknown Tool")
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();
        }
        catch (NoSuchElementException)
        {
            return new List<string>();
        }
    }

    #endregion

    #region Demonstration of New Helper Features

    /// <summary>
    /// Shows how to use the advanced element information gathering
    /// </summary>
    public void LogCurrentUIState()
    {
        LogElementsInfo(
            "MainWindow", "FileMenu", "WindowMenu", "BackButton", "ForwardButton",
            "DashboardButton", "NavigationTextBox", "NavigateButton", "MainDockControl"
        );
    }

    /// <summary>
    /// Demonstrates bulk element validation
    /// </summary>
    public bool ValidateAllCriticalElements()
    {
        var criticalElements = new[]
        {
            "MainWindow", "FileMenu", "MainDockControl"
        };

        var results = Elements.ValidateElements(criticalElements);
        var allFound = results.Values.All(found => found);
        
        if (!allFound)
        {
            var missing = results.Where(r => !r.Value).Select(r => r.Key);
            Console.WriteLine($"Missing critical elements: {string.Join(", ", missing)}");
        }

        return allFound;
    }

    /// <summary>
    /// Shows how to use custom element conditions
    /// </summary>
    public bool WaitForApplicationToBeFullyLoaded(int timeoutSeconds = 30)
    {
        try
        {
            // Wait for main window
            WaitForVisible("MainWindow", timeoutSeconds);
            
            // Wait for dock control to be interactive
            Elements.WaitForCondition("MainDockControl", element => 
                element.Displayed && element.Enabled, timeoutSeconds);
            
            // Additional stability wait
            Thread.Sleep(1000);
            
            return true;
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    #endregion

    #region Backward Compatibility Methods

    /// <summary>
    /// Legacy method for backward compatibility - now much simpler
    /// </summary>
    public void WaitForApplicationToLoad()
    {
        WaitForPageLoad();
    }

    /// <summary>
    /// Legacy method for backward compatibility
    /// </summary>
    public bool IsMainWindowVisible()
    {
        return IsElementVisible("MainWindow");
    }

    #endregion
} 