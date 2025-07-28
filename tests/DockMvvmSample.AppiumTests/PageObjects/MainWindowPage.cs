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

    public IWebElement Tool1Tab => FindTool("Tool1");
    public IWebElement Tool2Tab => FindTool("Tool2");
    public IWebElement Tool5Tab => FindTool("Tool5");
    
    public IWebElement LeftTopToolDock => FindDockContainer("LeftTopToolDock");
    public IWebElement LeftBottomToolDock => FindDockContainer("LeftBottomToolDock");
    public IWebElement RightTopToolDock => FindDockContainer("RightTopToolDock");
    public IWebElement RightBottomToolDock => FindDockContainer("RightBottomToolDock");

    public MainWindowPage ClickFileMenu()
    {
        ClickMenu("FileMenu");
        return this;
    }

    public MainWindowPage ClickWindowMenu()
    {
        ClickMenu("WindowMenu");
        return this;
    }

    public MainWindowPage NavigateToPath(string path)
    {
        base.NavigateToPath("NavigationTextBox", "NavigateButton", path);
        return this;
    }

    public MainWindowPage ClickDashboard()
    {
        ClickElement("DashboardButton");
        return this;
    }

    public MainWindowPage NavigateBack()
    {
        ClickElement("BackButton");
        return this;
    }

    public MainWindowPage NavigateForward()
    {
        ClickElement("ForwardButton");
        return this;
    }

    public string GetCurrentNavigationPath()
    {
        return GetElementAttribute("NavigationTextBox", "value") ?? GetElementText("NavigationTextBox");
    }

    public bool VerifyMainElementsAccessible()
    {
        return ValidatePageElements();
    }

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

    public Dictionary<string, bool> GetDetailedElementStatus()
    {
        var elementsToCheck = new[]
        {
            "MainWindow", "FileMenu", "WindowMenu", "BackButton", "ForwardButton",
            "DashboardButton", "NavigationTextBox", "NavigateButton", "MainDockControl"
        };

        return Elements.ValidateVisibility(elementsToCheck);
    }

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

    public void LogCurrentUIState()
    {
        LogElementsInfo(
            "MainWindow", "FileMenu", "WindowMenu", "BackButton", "ForwardButton",
            "DashboardButton", "NavigationTextBox", "NavigateButton", "MainDockControl"
        );
    }

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
} 
