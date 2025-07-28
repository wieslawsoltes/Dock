using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.UI;

namespace DockMvvmSample.AppiumTests.Infrastructure;

/// <summary>
/// Extension methods for finding elements using AccessibilityId with improved error handling and waiting
/// </summary>
public static class AccessibilityExtensions
{
    /// <summary>
    /// Finds an element by AccessibilityId with explicit wait
    /// </summary>
    /// <param name="driver">WebDriver instance</param>
    /// <param name="accessibilityId">The accessibility ID to search for</param>
    /// <param name="timeoutInSeconds">Timeout in seconds (default: 10)</param>
    /// <returns>The found WebElement</returns>
    public static IWebElement FindElementByAccessibilityIdWithWait(this IWebDriver driver, string accessibilityId, int timeoutInSeconds = 10)
    {
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
        return wait.Until(d =>
        {
            try
            {
                // Cast to AppiumDriver to access FindElementByAccessibilityId
                if (d is AppiumDriver<AppiumWebElement> appiumDriver)
                {
                    return appiumDriver.FindElementByAccessibilityId(accessibilityId);
                }
                // Fallback to By.Id for non-Appium drivers
                return d.FindElement(By.Id(accessibilityId));
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
    /// <param name="driver">WebDriver instance</param>
    /// <param name="accessibilityId">The accessibility ID to search for</param>
    /// <param name="element">The found element (null if not found)</param>
    /// <returns>True if element was found, false otherwise</returns>
    public static bool TryFindElementByAccessibilityId(this IWebDriver driver, string accessibilityId, out IWebElement element)
    {
        try
        {
            if (driver is AppiumDriver<AppiumWebElement> appiumDriver)
            {
                element = appiumDriver.FindElementByAccessibilityId(accessibilityId);
            }
            else
            {
                element = driver.FindElement(By.Id(accessibilityId));
            }
            return true;
        }
        catch (NoSuchElementException)
        {
            element = null!;
            return false;
        }
    }

    /// <summary>
    /// Finds multiple elements by AccessibilityId
    /// </summary>
    /// <param name="driver">WebDriver instance</param>
    /// <param name="accessibilityId">The accessibility ID to search for</param>
    /// <returns>List of found WebElements</returns>
    public static IList<IWebElement> FindElementsByAccessibilityId(this IWebDriver driver, string accessibilityId)
    {
        try
        {
            IWebElement element;
            if (driver is AppiumDriver<AppiumWebElement> appiumDriver)
            {
                element = appiumDriver.FindElementByAccessibilityId(accessibilityId);
            }
            else
            {
                element = driver.FindElement(By.Id(accessibilityId));
            }
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
    /// <param name="driver">WebDriver instance</param>
    /// <param name="accessibilityId">The accessibility ID to search for</param>
    /// <param name="timeoutInSeconds">Timeout in seconds (default: 10)</param>
    /// <returns>The clickable WebElement</returns>
    public static IWebElement WaitForElementToBeClickableByAccessibilityId(this IWebDriver driver, string accessibilityId, int timeoutInSeconds = 10)
    {
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
        return wait.Until(d =>
        {
            try
            {
                IWebElement element;
                if (d is AppiumDriver<AppiumWebElement> appiumDriver)
                {
                    element = appiumDriver.FindElementByAccessibilityId(accessibilityId);
                }
                else
                {
                    element = d.FindElement(By.Id(accessibilityId));
                }
                return element.Enabled && element.Displayed ? element : null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        });
    }

    /// <summary>
    /// Compares the performance of By.Id vs FindElementByAccessibilityId
    /// </summary>
    /// <param name="driver">WebDriver instance</param>
    /// <param name="elementId">The element ID to test</param>
    /// <returns>Performance comparison result</returns>
    public static (TimeSpan ByIdTime, TimeSpan AccessibilityIdTime, bool SameElement) CompareElementFindingPerformance(
        this IWebDriver driver, string elementId)
    {
        // Test By.Id
        var startTime = DateTime.Now;
        IWebElement elementById;
        try
        {
            elementById = driver.FindElement(By.Id(elementId));
        }
        catch (NoSuchElementException)
        {
            return (TimeSpan.Zero, TimeSpan.Zero, false);
        }
        var byIdTime = DateTime.Now - startTime;

        // Test FindElementByAccessibilityId
        startTime = DateTime.Now;
        IWebElement elementByAccessibilityId;
        try
        {
            if (driver is AppiumDriver<AppiumWebElement> appiumDriver)
            {
                elementByAccessibilityId = appiumDriver.FindElementByAccessibilityId(elementId);
            }
            else
            {
                elementByAccessibilityId = driver.FindElement(By.Id(elementId));
            }
        }
        catch (NoSuchElementException)
        {
            return (byIdTime, TimeSpan.Zero, false);
        }
        var accessibilityIdTime = DateTime.Now - startTime;

        // Check if they found the same element
        bool sameElement;
        try
        {
            var id1 = elementById.GetAttribute("AutomationId") ?? elementById.GetAttribute("id");
            var id2 = elementByAccessibilityId.GetAttribute("AutomationId") ?? elementByAccessibilityId.GetAttribute("id");
            sameElement = string.Equals(id1, id2, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            sameElement = false;
        }

        return (byIdTime, accessibilityIdTime, sameElement);
    }

    /// <summary>
    /// Validates that all expected elements can be found using AccessibilityId
    /// </summary>
    /// <param name="driver">WebDriver instance</param>
    /// <param name="expectedElementIds">Array of accessibility IDs to validate</param>
    /// <returns>Dictionary with element ID as key and found status as value</returns>
    public static Dictionary<string, bool> ValidateAccessibilityIds(this IWebDriver driver, params string[] expectedElementIds)
    {
        var results = new Dictionary<string, bool>();
        
        foreach (var elementId in expectedElementIds)
        {
            results[elementId] = driver.TryFindElementByAccessibilityId(elementId, out _);
        }
        
        return results;
    }
} 