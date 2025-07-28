using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.UI;

namespace DockMvvmSample.AppiumTests.Infrastructure;

/// <summary>
/// Platform-specific element finding strategies to address Windows vs macOS differences
/// </summary>
public static class PlatformSpecificElementHelper
{
    /// <summary>
    /// Gets the optimal locator strategy for the current platform
    /// </summary>
    public static By GetOptimalLocator(IWebDriver driver, string accessibilityId)
    {
        var platformName = GetPlatformName(driver);
        
        return platformName?.ToLower() switch
        {
            "windows" => GetWindowsLocator(accessibilityId),
            "mac" => GetMacLocator(accessibilityId),
            _ => GetGenericLocator(accessibilityId)
        };
    }

    /// <summary>
    /// Gets Windows-specific locator with optimized attribute strategy
    /// </summary>
    private static By GetWindowsLocator(string accessibilityId)
    {
        // Windows prioritizes AutomationId (case-sensitive) then Name
        return By.XPath($"//*[@AutomationId='{accessibilityId}' or @Name='{accessibilityId}']");
    }

    /// <summary>
    /// Gets macOS-specific locator optimized for Mac2 driver
    /// </summary>
    private static By GetMacLocator(string accessibilityId)
    {
        // macOS uses identifier attribute primarily
        return By.XPath($"//*[@identifier='{accessibilityId}' or @name='{accessibilityId}']");
    }

    /// <summary>
    /// Gets generic cross-platform locator
    /// </summary>
    private static By GetGenericLocator(string accessibilityId)
    {
        return By.Id(accessibilityId);
    }

    /// <summary>
    /// Creates platform-optimized wait strategy
    /// </summary>
    public static WebDriverWait CreateOptimizedWait(IWebDriver driver, int timeoutSeconds)
    {
        var platformName = GetPlatformName(driver);
        
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
        
        // Windows requires longer polling intervals due to WinAppDriver behavior
        if (string.Equals(platformName, "Windows", StringComparison.OrdinalIgnoreCase))
        {
            wait.PollingInterval = TimeSpan.FromMilliseconds(500); // 500ms for Windows
        }
        else
        {
            wait.PollingInterval = TimeSpan.FromMilliseconds(250); // 250ms for other platforms
        }
        
        return wait;
    }

    /// <summary>
    /// Gets platform-specific element finding strategies in priority order
    /// </summary>
    public static IEnumerable<Func<IWebDriver, string, IWebElement?>> GetPlatformStrategies(IWebDriver driver)
    {
        var platformName = GetPlatformName(driver);
        
        if (string.Equals(platformName, "Windows", StringComparison.OrdinalIgnoreCase))
        {
            return GetWindowsStrategies();
        }
        else if (string.Equals(platformName, "Mac", StringComparison.OrdinalIgnoreCase))
        {
            return GetMacStrategies();
        }
        else
        {
            return GetGenericStrategies();
        }
    }

    private static IEnumerable<Func<IWebDriver, string, IWebElement?>> GetWindowsStrategies()
    {
        return new Func<IWebDriver, string, IWebElement?>[]
        {
            // Strategy 1: AutomationId (most reliable on Windows)
            (driver, id) => TryFindElement(driver, By.XPath($"//*[@AutomationId='{id}']")),
            
            // Strategy 2: Name attribute
            (driver, id) => TryFindElement(driver, By.XPath($"//*[@Name='{id}']")),
            
            // Strategy 3: Combined AutomationId or Name
            (driver, id) => TryFindElement(driver, By.XPath($"//*[@AutomationId='{id}' or @Name='{id}']")),
            
            // Strategy 4: Legacy approaches
            (driver, id) => TryFindElement(driver, By.Id(id)),
            
            // Strategy 5: AppiumBy if available
            (driver, id) => TryFindByAppiumAccessibilityId(driver, id)
        };
    }

    private static IEnumerable<Func<IWebDriver, string, IWebElement?>> GetMacStrategies()
    {
        return new Func<IWebDriver, string, IWebElement?>[]
        {
            // Strategy 1: identifier attribute (Mac2 driver)
            (driver, id) => TryFindElement(driver, By.XPath($"//*[@identifier='{id}']")),
            
            // Strategy 2: name attribute
            (driver, id) => TryFindElement(driver, By.XPath($"//*[@name='{id}']")),
            
            // Strategy 3: Combined approach
            (driver, id) => TryFindElement(driver, By.XPath($"//*[@identifier='{id}' or @name='{id}']")),
            
            // Strategy 4: AppiumBy if available
            (driver, id) => TryFindByAppiumAccessibilityId(driver, id)
        };
    }

    private static IEnumerable<Func<IWebDriver, string, IWebElement?>> GetGenericStrategies()
    {
        return new Func<IWebDriver, string, IWebElement?>[]
        {
            // Strategy 1: Generic ID
            (driver, id) => TryFindElement(driver, By.Id(id)),
            
            // Strategy 2: AppiumBy if available
            (driver, id) => TryFindByAppiumAccessibilityId(driver, id)
        };
    }

    private static IWebElement? TryFindElement(IWebDriver driver, By locator)
    {
        try
        {
            return driver.FindElement(locator);
        }
        catch (NoSuchElementException)
        {
            return null;
        }
    }

    private static IWebElement? TryFindByAppiumAccessibilityId(IWebDriver driver, string accessibilityId)
    {
        try
        {
            if (driver is AppiumDriver<AppiumWebElement> appiumDriver)
            {
                return appiumDriver.FindElementByAccessibilityId(accessibilityId);
            }
        }
        catch (NoSuchElementException)
        {
            // Fall through
        }
        return null;
    }

    private static string? GetPlatformName(IWebDriver driver)
    {
        try
        {
            if (driver is AppiumDriver<AppiumWebElement> appiumDriver)
            {
                return appiumDriver.Capabilities.GetCapability("platformName")?.ToString();
            }
        }
        catch
        {
            // Fall through
        }
        return null;
    }
}
