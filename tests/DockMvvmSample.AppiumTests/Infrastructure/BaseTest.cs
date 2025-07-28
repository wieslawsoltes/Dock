using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DockMvvmSample.AppiumTests.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.UI;

namespace DockMvvmSample.AppiumTests.Infrastructure;

public abstract class BaseTest : IDisposable
{
    protected IWebDriver Driver { get; private set; }
    protected TestConfiguration Configuration { get; private set; }
    protected Process? ApplicationProcess { get; set; }

    protected BaseTest()
    {
        Configuration = ConfigurationHelper.LoadConfiguration();
        Driver = AppiumDriverFactory.CreateDriver(Configuration, out var appProcess);
        ApplicationProcess = appProcess;
    }

    #region Element Finding Helper Methods

    /// <summary>
    /// Finds an element by AccessibilityId with automatic fallback to By.Id
    /// </summary>
    /// <param name="accessibilityId">The accessibility ID to search for</param>
    /// <returns>The found WebElement</returns>
    protected IWebElement FindElementByAccessibilityId(string accessibilityId)
    {
        return (Driver as AppiumDriver<AppiumWebElement>)?.FindElementByAccessibilityId(accessibilityId) 
               ?? Driver.FindElement(By.Id(accessibilityId));
    }

    /// <summary>
    /// Finds an element by AccessibilityId with explicit wait and automatic fallback
    /// </summary>
    /// <param name="accessibilityId">The accessibility ID to search for</param>
    /// <param name="timeoutInSeconds">Timeout in seconds (default: 10)</param>
    /// <returns>The found WebElement</returns>
    protected IWebElement FindElementByAccessibilityIdWithWait(string accessibilityId, int timeoutInSeconds = 10)
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutInSeconds));
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
    /// Tries to find an element by AccessibilityId without throwing exceptions
    /// </summary>
    /// <param name="accessibilityId">The accessibility ID to search for</param>
    /// <param name="element">The found element (null if not found)</param>
    /// <returns>True if element was found, false otherwise</returns>
    protected bool TryFindElementByAccessibilityId(string accessibilityId, out IWebElement? element)
    {
        try
        {
            element = FindElementByAccessibilityId(accessibilityId);
            return true;
        }
        catch (NoSuchElementException)
        {
            element = null;
            return false;
        }
    }

    /// <summary>
    /// Finds multiple elements by AccessibilityId with automatic fallback
    /// </summary>
    /// <param name="accessibilityId">The accessibility ID to search for</param>
    /// <returns>List of found WebElements</returns>
    protected IList<IWebElement> FindElementsByAccessibilityId(string accessibilityId)
    {
        try
        {
            var element = FindElementByAccessibilityId(accessibilityId);
            return new List<IWebElement> { element };
        }
        catch (NoSuchElementException)
        {
            return new List<IWebElement>();
        }
    }

    /// <summary>
    /// Waits for an element to be clickable using AccessibilityId
    /// </summary>
    /// <param name="accessibilityId">The accessibility ID to search for</param>
    /// <param name="timeoutInSeconds">Timeout in seconds (default: 10)</param>
    /// <returns>The clickable WebElement</returns>
    protected IWebElement WaitForElementToBeClickable(string accessibilityId, int timeoutInSeconds = 10)
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutInSeconds));
        return wait.Until(driver =>
        {
            try
            {
                var element = (driver as AppiumDriver<AppiumWebElement>)?.FindElementByAccessibilityId(accessibilityId) 
                             ?? driver.FindElement(By.Id(accessibilityId));
                return element.Enabled && element.Displayed ? element : null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        });
    }

    /// <summary>
    /// Validates that all expected elements can be found using AccessibilityId
    /// </summary>
    /// <param name="expectedElementIds">Array of accessibility IDs to validate</param>
    /// <returns>Dictionary with element ID as key and found status as value</returns>
    protected Dictionary<string, bool> ValidateAccessibilityIds(params string[] expectedElementIds)
    {
        var results = new Dictionary<string, bool>();
        
        foreach (var elementId in expectedElementIds)
        {
            results[elementId] = TryFindElementByAccessibilityId(elementId, out _);
        }
        
        return results;
    }

    /// <summary>
    /// Performs common element interactions with better error handling
    /// </summary>
    /// <param name="accessibilityId">The accessibility ID of the element</param>
    /// <param name="action">The action to perform (click, clear, etc.)</param>
    /// <param name="timeoutInSeconds">Timeout for finding the element</param>
    protected void PerformElementAction(string accessibilityId, Action<IWebElement> action, int timeoutInSeconds = 10)
    {
        var element = FindElementByAccessibilityIdWithWait(accessibilityId, timeoutInSeconds);
        action(element);
    }

    /// <summary>
    /// Clicks an element by AccessibilityId with wait
    /// </summary>
    /// <param name="accessibilityId">The accessibility ID of the element to click</param>
    /// <param name="timeoutInSeconds">Timeout for finding the element</param>
    protected void ClickElement(string accessibilityId, int timeoutInSeconds = 10)
    {
        PerformElementAction(accessibilityId, element => element.Click(), timeoutInSeconds);
    }

    /// <summary>
    /// Types text into an element by AccessibilityId with wait
    /// </summary>
    /// <param name="accessibilityId">The accessibility ID of the element</param>
    /// <param name="text">The text to type</param>
    /// <param name="clearFirst">Whether to clear the field first (default: true)</param>
    /// <param name="timeoutInSeconds">Timeout for finding the element</param>
    protected void TypeInElement(string accessibilityId, string text, bool clearFirst = true, int timeoutInSeconds = 10)
    {
        PerformElementAction(accessibilityId, element =>
        {
            if (clearFirst) element.Clear();
            element.SendKeys(text);
        }, timeoutInSeconds);
    }

    /// <summary>
    /// Gets the text content of an element by AccessibilityId
    /// </summary>
    /// <param name="accessibilityId">The accessibility ID of the element</param>
    /// <param name="timeoutInSeconds">Timeout for finding the element</param>
    /// <returns>The text content of the element</returns>
    protected string GetElementText(string accessibilityId, int timeoutInSeconds = 10)
    {
        var element = FindElementByAccessibilityIdWithWait(accessibilityId, timeoutInSeconds);
        return element.Text;
    }

    /// <summary>
    /// Gets an attribute value from an element by AccessibilityId
    /// </summary>
    /// <param name="accessibilityId">The accessibility ID of the element</param>
    /// <param name="attributeName">The name of the attribute to retrieve</param>
    /// <param name="timeoutInSeconds">Timeout for finding the element</param>
    /// <returns>The attribute value</returns>
    protected string? GetElementAttribute(string accessibilityId, string attributeName, int timeoutInSeconds = 10)
    {
        var element = FindElementByAccessibilityIdWithWait(accessibilityId, timeoutInSeconds);
        return element.GetAttribute(attributeName);
    }

    #endregion

    protected void TakeScreenshot(string testName)
    {
        if (!Configuration.TestSettings.ScreenshotOnFailure)
            return;

        try
        {
            var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
            var screenshotPath = Path.Combine(
                AppContext.BaseDirectory, 
                "Screenshots", 
                $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
            
            Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath)!);
            screenshot.SaveAsFile(screenshotPath);
        }
        catch (Exception ex)
        {
            // Don't fail the test because of screenshot issues
            Console.WriteLine($"Failed to take screenshot: {ex.Message}");
        }
    }

    public virtual void Dispose()
    {
        try
        {
            // Quit the Appium driver first
            Driver?.Quit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error disposing driver: {ex.Message}");
        }

        // Kill the application process if it's still running (Windows only)
        try
        {
            if (ApplicationProcess != null && !ApplicationProcess.HasExited)
            {
                Console.WriteLine($"Killing application process with PID: {ApplicationProcess.Id}");
                ApplicationProcess.Kill();
                ApplicationProcess.WaitForExit(5000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error killing application process: {ex.Message}");
        }
        finally
        {
            ApplicationProcess?.Dispose();
        }
    }
} 