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
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
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

    public bool IsMainWindowVisible()
    {
        try
        {
            // Use automation ID to check if main window is visible
            var mainWindow = _driver.FindElement(By.Id("MainWindow"));
            return mainWindow.Displayed;
        }
        catch (NoSuchElementException)
        {
            // Fallback to flexible search patterns if automation ID doesn't work
            try
            {
                var searchPatterns = new[]
                {
                    "//*[@identifier='MainWindow']",
                    "//*[contains(@title, 'Dock Avalonia Demo') or contains(@label, 'Dock Avalonia Demo')]",
                    "//*[contains(@title, 'DockMvvm') or contains(@label, 'DockMvvm')]",
                    "//*[contains(@title, 'Sample') or contains(@label, 'Sample')]",
                    "//*[@elementType='55' and @enabled='true']" // Look for any enabled window
                };

                foreach (var pattern in searchPatterns)
                {
                    var elements = _driver.FindElements(By.XPath(pattern));
                    if (elements.Count > 0)
                    {
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception)
            {
                return false;
            }
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

    public void WaitForApplicationToLoad()
    {
        _wait.Until(d => IsMainWindowVisible());
        // Additional wait for UI to stabilize
        System.Threading.Thread.Sleep(1000);
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
} 