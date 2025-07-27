using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
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

    // Main window elements - using Mac2/XCUITest attributes and application name
    public IWebElement MainWindow => _wait.Until(d => d.FindElement(By.XPath("//*[@title='Dock Avalonia Demo' or @label='Dock Avalonia Demo' or contains(@title, 'DockMvvmSample') or contains(@label, 'DockMvvmSample')]")));
    
    // Menu elements - using Mac2/XCUITest attributes  
    public IWebElement FileMenu => _wait.Until(d => d.FindElement(By.XPath("//*[@title='File']")));
    public IWebElement WindowMenu => _wait.Until(d => d.FindElement(By.XPath("//*[@title='Window']")));



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
            // Look for elements that could be our app - more flexible search
            var searchPatterns = new[]
            {
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
        catch (NoSuchElementException)
        {
            return false;
        }
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
} 