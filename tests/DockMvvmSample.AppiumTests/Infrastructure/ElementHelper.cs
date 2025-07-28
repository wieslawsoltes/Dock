using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.UI;

namespace DockMvvmSample.AppiumTests.Infrastructure;

/// <summary>
/// Unified helper class for element finding and interaction with cross-platform compatibility
/// </summary>
public class ElementHelper
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _defaultWait;
    private readonly int _defaultTimeoutSeconds;

    public ElementHelper(IWebDriver driver, int defaultTimeoutSeconds = 10)
    {
        _driver = driver ?? throw new ArgumentNullException(nameof(driver));
        _defaultTimeoutSeconds = defaultTimeoutSeconds;
        _defaultWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(defaultTimeoutSeconds));
    }

    #region Platform-Specific Helpers

    /// <summary>
    /// Gets the appropriate AccessibilityId locator based on the platform
    /// </summary>
    private By GetAccessibilityIdLocator(string accessibilityId)
    {
        // Check if we can determine platform from capabilities
        var capabilities = (_driver as AppiumDriver<AppiumWebElement>)?.Capabilities;
        var platformName = capabilities?.GetCapability("platformName")?.ToString();
        
        if (string.Equals(platformName, "Windows", StringComparison.OrdinalIgnoreCase))
        {
            // Windows Application Driver - try accessibility id and name attributes
            return By.XPath($"//*[@automationid='{accessibilityId}' or @name='{accessibilityId}']");
        }
        else if (string.Equals(platformName, "Mac", StringComparison.OrdinalIgnoreCase))
        {
            // Mac2 driver - use accessibility identifier
            return By.XPath($"//*[@identifier='{accessibilityId}' or @name='{accessibilityId}']");
        }
        else
        {
            // Generic fallback - try multiple approaches
            try 
            {
                // Try to use reflection to access AppiumBy if available
                var appiumByType = System.Type.GetType("OpenQA.Selenium.Appium.AppiumBy, Appium.WebDriver");
                if (appiumByType != null)
                {
                    var method = appiumByType.GetMethod("AccessibilityId", new[] { typeof(string) });
                    if (method != null)
                    {
                        return (By)method.Invoke(null, new object[] { accessibilityId });
                    }
                }
            }
            catch
            {
                // Ignore reflection errors
            }
            
            // Ultimate fallback
            return By.Id(accessibilityId);
        }
    }

    #endregion

    #region Core Element Finding Methods

    /// <summary>
    /// Finds an element by AccessibilityId with platform-specific strategies
    /// </summary>
    public IWebElement FindByAccessibilityId(string accessibilityId)
    {
        try
        {
            // Try modern AppiumBy approach first (works with newer versions)
            return _driver.FindElement(GetAccessibilityIdLocator(accessibilityId));
        }
        catch (NoSuchElementException)
        {
            // Fallback for older versions or different platform behaviors
            try
            {
                return (_driver as AppiumDriver<AppiumWebElement>)?.FindElementByAccessibilityId(accessibilityId) 
                       ?? _driver.FindElement(By.Id(accessibilityId));
            }
            catch (NoSuchElementException)
            {
                // Final fallback - try by name attribute for Windows
                return _driver.FindElement(By.XPath($"//*[@name='{accessibilityId}' or @automationid='{accessibilityId}']"));
            }
        }
    }

    /// <summary>
    /// Finds an element by AccessibilityId with explicit wait and platform-specific strategies
    /// </summary>
    public IWebElement FindByAccessibilityIdWithWait(string accessibilityId, int? timeoutSeconds = null)
    {
        var timeout = timeoutSeconds ?? _defaultTimeoutSeconds;
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeout));
        
        return wait.Until(driver =>
        {
            try
            {
                // Use the new platform-aware finding method
                return FindByAccessibilityId(accessibilityId);
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        })!;
    }

    /// <summary>
    /// Tries to find an element by AccessibilityId without throwing exceptions
    /// </summary>
    public bool TryFindByAccessibilityId(string accessibilityId, out IWebElement? element)
    {
        try
        {
            element = FindByAccessibilityId(accessibilityId);
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
    public IList<IWebElement> FindAllByAccessibilityId(string accessibilityId)
    {
        try
        {
            var element = FindByAccessibilityId(accessibilityId);
            return new List<IWebElement> { element };
        }
        catch (NoSuchElementException)
        {
            return new List<IWebElement>();
        }
    }

    /// <summary>
    /// Waits for an element to be clickable using AccessibilityId with platform-specific strategies
    /// </summary>
    public IWebElement WaitForClickable(string accessibilityId, int? timeoutSeconds = null)
    {
        var timeout = timeoutSeconds ?? _defaultTimeoutSeconds;
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeout));
        
        return wait.Until(driver =>
        {
            try
            {
                var element = FindByAccessibilityId(accessibilityId);
                return element.Enabled && element.Displayed ? element : null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        })!;
    }

    /// <summary>
    /// Waits for an element to be visible using AccessibilityId with platform-specific strategies
    /// </summary>
    public IWebElement WaitForVisible(string accessibilityId, int? timeoutSeconds = null)
    {
        var timeout = timeoutSeconds ?? _defaultTimeoutSeconds;
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeout));
        
        return wait.Until(driver =>
        {
            try
            {
                var element = FindByAccessibilityId(accessibilityId);
                return element.Displayed ? element : null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        })!;
    }

    #endregion

    #region Element Interaction Methods

    /// <summary>
    /// Clicks an element by AccessibilityId with wait
    /// </summary>
    public ElementHelper Click(string accessibilityId, int? timeoutSeconds = null)
    {
        var element = WaitForClickable(accessibilityId, timeoutSeconds);
        element.Click();
        return this; // Fluent API
    }

    /// <summary>
    /// Types text into an element by AccessibilityId with wait
    /// </summary>
    public ElementHelper Type(string accessibilityId, string text, bool clearFirst = true, int? timeoutSeconds = null)
    {
        var element = FindByAccessibilityIdWithWait(accessibilityId, timeoutSeconds);
        if (clearFirst) element.Clear();
        element.SendKeys(text);
        return this; // Fluent API
    }

    /// <summary>
    /// Gets the text content of an element by AccessibilityId
    /// </summary>
    public string GetText(string accessibilityId, int? timeoutSeconds = null)
    {
        var element = FindByAccessibilityIdWithWait(accessibilityId, timeoutSeconds);
        return element.Text;
    }

    /// <summary>
    /// Gets an attribute value from an element by AccessibilityId
    /// </summary>
    public string? GetAttribute(string accessibilityId, string attributeName, int? timeoutSeconds = null)
    {
        var element = FindByAccessibilityIdWithWait(accessibilityId, timeoutSeconds);
        return element.GetAttribute(attributeName);
    }

    /// <summary>
    /// Checks if an element is visible by AccessibilityId
    /// </summary>
    public bool IsVisible(string accessibilityId)
    {
        try
        {
            var element = FindByAccessibilityId(accessibilityId);
            return element.Displayed;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if an element is enabled by AccessibilityId
    /// </summary>
    public bool IsEnabled(string accessibilityId)
    {
        try
        {
            var element = FindByAccessibilityId(accessibilityId);
            return element.Enabled;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    #endregion

    #region Validation and Bulk Operations

    /// <summary>
    /// Validates that all expected elements can be found using AccessibilityId
    /// </summary>
    public Dictionary<string, bool> ValidateElements(params string[] accessibilityIds)
    {
        var results = new Dictionary<string, bool>();
        
        foreach (var id in accessibilityIds)
        {
            results[id] = TryFindByAccessibilityId(id, out _);
        }
        
        return results;
    }

    /// <summary>
    /// Validates that all expected elements are visible
    /// </summary>
    public Dictionary<string, bool> ValidateVisibility(params string[] accessibilityIds)
    {
        var results = new Dictionary<string, bool>();
        
        foreach (var id in accessibilityIds)
        {
            results[id] = IsVisible(id);
        }
        
        return results;
    }

    /// <summary>
    /// Gets information about multiple elements at once
    /// </summary>
    public Dictionary<string, ElementInfo> GetElementsInfo(params string[] accessibilityIds)
    {
        var results = new Dictionary<string, ElementInfo>();
        
        foreach (var id in accessibilityIds)
        {
            if (TryFindByAccessibilityId(id, out var element) && element != null)
            {
                results[id] = new ElementInfo
                {
                    Exists = true,
                    IsVisible = element.Displayed,
                    IsEnabled = element.Enabled,
                    Text = element.Text,
                    TagName = element.TagName
                };
            }
            else
            {
                results[id] = new ElementInfo { Exists = false };
            }
        }
        
        return results;
    }

    #endregion

    #region Advanced Element Finding

    /// <summary>
    /// Performs a custom action on an element with automatic wait and error handling
    /// </summary>
    public T PerformElementAction<T>(string accessibilityId, Func<IWebElement, T> action, int? timeoutSeconds = null)
    {
        var element = FindByAccessibilityIdWithWait(accessibilityId, timeoutSeconds);
        return action(element);
    }

    /// <summary>
    /// Performs a custom action on an element with automatic wait and error handling (void return)
    /// </summary>
    public ElementHelper PerformElementAction(string accessibilityId, Action<IWebElement> action, int? timeoutSeconds = null)
    {
        var element = FindByAccessibilityIdWithWait(accessibilityId, timeoutSeconds);
        action(element);
        return this; // Fluent API
    }

    /// <summary>
    /// Waits for a custom condition on an element with platform-specific strategies
    /// </summary>
    public IWebElement WaitForCondition(string accessibilityId, Func<IWebElement, bool> condition, int? timeoutSeconds = null)
    {
        var timeout = timeoutSeconds ?? _defaultTimeoutSeconds;
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeout));
        
        return wait.Until(driver =>
        {
            try
            {
                var element = FindByAccessibilityId(accessibilityId);
                return condition(element) ? element : null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        })!;
    }

    #endregion
}

/// <summary>
/// Information about an element's state
/// </summary>
public class ElementInfo
{
    public bool Exists { get; set; }
    public bool IsVisible { get; set; }
    public bool IsEnabled { get; set; }
    public string Text { get; set; } = string.Empty;
    public string TagName { get; set; } = string.Empty;
} 