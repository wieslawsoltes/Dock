using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Appium.Windows;

namespace DockMvvmSample.AppiumTests.Infrastructure;

/// <summary>
/// Unified helper class for element finding and interaction with cross-platform compatibility
/// Enhanced with Windows-specific workarounds for Appium 2.0 issues
/// </summary>
public class ElementHelper
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _defaultWait;
    private readonly int _defaultTimeoutSeconds;
    private readonly bool _isWindows;

    public ElementHelper(IWebDriver driver, int defaultTimeoutSeconds = 10)
    {
        _driver = driver ?? throw new ArgumentNullException(nameof(driver));
        _defaultTimeoutSeconds = defaultTimeoutSeconds;
        _defaultWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(defaultTimeoutSeconds));
        
        // Detect Windows platform
        _isWindows = IsWindowsPlatform();
    }

    #region Platform Detection

    /// <summary>
    /// Detects if we're running on Windows platform
    /// </summary>
    private bool IsWindowsPlatform()
    {
        try
        {
            // Check if driver is WindowsDriver
            if (_driver is WindowsDriver<WindowsElement>)
                return true;

            // Check capabilities
            var capabilities = (_driver as AppiumDriver<AppiumWebElement>)?.Capabilities;
            var platformName = capabilities?.GetCapability("platformName")?.ToString();
            return string.Equals(platformName, "Windows", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Platform-Specific Helpers

    /// <summary>
    /// Gets the appropriate AccessibilityId locator based on the platform
    /// Enhanced with Windows-specific workarounds
    /// </summary>
    private By GetAccessibilityIdLocator(string accessibilityId)
    {
        if (_isWindows)
        {
            // Windows-specific locator strategies with multiple fallbacks
            // Windows Application Driver has issues with certain locators, so we try multiple approaches
            return By.XPath($"//*[@automationid='{accessibilityId}' or @name='{accessibilityId}' or @id='{accessibilityId}']");
        }
        else if (string.Equals(GetPlatformName(), "Mac", StringComparison.OrdinalIgnoreCase))
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

    /// <summary>
    /// Gets platform name from driver capabilities
    /// </summary>
    private string GetPlatformName()
    {
        try
        {
            var capabilities = (_driver as AppiumDriver<AppiumWebElement>)?.Capabilities;
            return capabilities?.GetCapability("platformName")?.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    #endregion

    #region Enhanced Element Finding Methods with Windows Workarounds

    /// <summary>
    /// Finds an element by AccessibilityId with enhanced Windows workarounds
    /// </summary>
    public IWebElement FindByAccessibilityId(string accessibilityId)
    {
        if (_isWindows)
        {
            // Windows-specific enhanced finding with multiple strategies
            return FindByAccessibilityIdWindows(accessibilityId);
        }

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
    /// Windows-specific element finding with multiple fallback strategies
    /// </summary>
    private IWebElement FindByAccessibilityIdWindows(string accessibilityId)
    {
        var strategies = new[]
        {
            // Strategy 1: Direct automationid
            () => _driver.FindElement(By.XPath($"//*[@automationid='{accessibilityId}']")),
            
            // Strategy 2: Name attribute
            () => _driver.FindElement(By.XPath($"//*[@name='{accessibilityId}']")),
            
            // Strategy 3: ID attribute
            () => _driver.FindElement(By.XPath($"//*[@id='{accessibilityId}']")),
            
            // Strategy 4: Legacy AppiumBy approach
            () => (_driver as AppiumDriver<AppiumWebElement>)?.FindElementByAccessibilityId(accessibilityId),
            
            // Strategy 5: Generic ID
            () => _driver.FindElement(By.Id(accessibilityId)),
            
            // Strategy 6: Case-insensitive search
            () => _driver.FindElement(By.XPath($"//*[translate(@automationid,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='{accessibilityId.ToLower()}']")),
            
            // Strategy 7: Contains search
            () => _driver.FindElement(By.XPath($"//*[contains(@automationid,'{accessibilityId}') or contains(@name,'{accessibilityId}')]"))
        };

        Exception lastException = null;
        
        foreach (var strategy in strategies)
        {
            try
            {
                var element = strategy();
                if (element != null)
                {
                    return element;
                }
            }
            catch (NoSuchElementException ex)
            {
                lastException = ex;
                continue;
            }
            catch (Exception ex)
            {
                lastException = ex;
                continue;
            }
        }

        throw new NoSuchElementException($"Element with accessibility ID '{accessibilityId}' not found using any strategy", lastException);
    }

    /// <summary>
    /// Finds an element by AccessibilityId with explicit wait (fixes implicit wait issues)
    /// </summary>
    public IWebElement FindByAccessibilityIdWithWait(string accessibilityId, int? timeoutSeconds = null)
    {
        var timeout = timeoutSeconds ?? _defaultTimeoutSeconds;
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeout));
        
        if (_isWindows)
        {
            // Windows-specific wait with multiple strategies
            return wait.Until(driver =>
            {
                try
                {
                    return FindByAccessibilityIdWindows(accessibilityId);
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
            });
        }

        return wait.Until(driver =>
        {
            try
            {
                return FindByAccessibilityId(accessibilityId);
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        });
    }

    /// <summary>
    /// Enhanced try-find method with Windows workarounds
    /// </summary>
    public bool TryFindByAccessibilityId(string accessibilityId, out IWebElement? element)
    {
        try
        {
            element = FindByAccessibilityId(accessibilityId);
            return element != null;
        }
        catch (NoSuchElementException)
        {
            element = null;
            return false;
        }
    }

    /// <summary>
    /// Enhanced find all method with Windows workarounds
    /// </summary>
    public IList<IWebElement> FindAllByAccessibilityId(string accessibilityId)
    {
        if (_isWindows)
        {
            // Windows-specific multiple strategy approach
            var results = new List<IWebElement>();
            
            try { results.AddRange(_driver.FindElements(By.XPath($"//*[@automationid='{accessibilityId}']"))); } catch { }
            try { results.AddRange(_driver.FindElements(By.XPath($"//*[@name='{accessibilityId}']"))); } catch { }
            try { results.AddRange(_driver.FindElements(By.XPath($"//*[@id='{accessibilityId}']"))); } catch { }
            
            return results.Distinct().ToList();
        }

        try
        {
            return _driver.FindElements(GetAccessibilityIdLocator(accessibilityId));
        }
        catch
        {
            return new List<IWebElement>();
        }
    }

    #endregion

    #region Enhanced Wait Methods

    /// <summary>
    /// Enhanced wait for clickable with Windows workarounds
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
                return element != null && element.Displayed && element.Enabled ? element : null;
            }
            catch
            {
                return null;
            }
        });
    }

    /// <summary>
    /// Enhanced wait for visible with Windows workarounds
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
                return element != null && element.Displayed ? element : null;
            }
            catch
            {
                return null;
            }
        });
    }

    #endregion

    #region Action Methods

    /// <summary>
    /// Enhanced click with Windows workarounds
    /// </summary>
    public ElementHelper Click(string accessibilityId, int? timeoutSeconds = null)
    {
        var element = WaitForClickable(accessibilityId, timeoutSeconds);
        element.Click();
        return this;
    }

    /// <summary>
    /// Enhanced type with Windows workarounds
    /// </summary>
    public ElementHelper Type(string accessibilityId, string text, bool clearFirst = true, int? timeoutSeconds = null)
    {
        var element = WaitForClickable(accessibilityId, timeoutSeconds);
        if (clearFirst)
        {
            element.Clear();
        }
        element.SendKeys(text);
        return this;
    }

    /// <summary>
    /// Enhanced get text with Windows workarounds
    /// </summary>
    public string GetText(string accessibilityId, int? timeoutSeconds = null)
    {
        var element = WaitForVisible(accessibilityId, timeoutSeconds);
        return element.Text;
    }

    /// <summary>
    /// Enhanced get attribute with Windows workarounds
    /// </summary>
    public string? GetAttribute(string accessibilityId, string attributeName, int? timeoutSeconds = null)
    {
        var element = WaitForVisible(accessibilityId, timeoutSeconds);
        return element.GetAttribute(attributeName);
    }

    #endregion

    #region Validation Methods

    /// <summary>
    /// Enhanced visibility check with Windows workarounds
    /// </summary>
    public bool IsVisible(string accessibilityId)
    {
        try
        {
            var element = FindByAccessibilityId(accessibilityId);
            return element != null && element.Displayed;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Enhanced enabled check with Windows workarounds
    /// </summary>
    public bool IsEnabled(string accessibilityId)
    {
        try
        {
            var element = FindByAccessibilityId(accessibilityId);
            return element != null && element.Enabled;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Enhanced element validation with Windows workarounds
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
    /// Enhanced visibility validation with Windows workarounds
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
    /// Enhanced element info gathering with Windows workarounds
    /// </summary>
    public Dictionary<string, ElementInfo> GetElementsInfo(params string[] accessibilityIds)
    {
        var results = new Dictionary<string, ElementInfo>();
        foreach (var id in accessibilityIds)
        {
            var info = new ElementInfo();
            if (TryFindByAccessibilityId(id, out var element))
            {
                info.Exists = true;
                info.IsVisible = element.Displayed;
                info.IsEnabled = element.Enabled;
                info.Text = element.Text;
                info.TagName = element.TagName;
            }
            results[id] = info;
        }
        return results;
    }

    #endregion

    #region Action Delegation Methods

    /// <summary>
    /// Enhanced element action with Windows workarounds
    /// </summary>
    public T PerformElementAction<T>(string accessibilityId, Func<IWebElement, T> action, int? timeoutSeconds = null)
    {
        var element = WaitForVisible(accessibilityId, timeoutSeconds);
        return action(element);
    }

    /// <summary>
    /// Enhanced element action with Windows workarounds
    /// </summary>
    public ElementHelper PerformElementAction(string accessibilityId, Action<IWebElement> action, int? timeoutSeconds = null)
    {
        var element = WaitForVisible(accessibilityId, timeoutSeconds);
        action(element);
        return this;
    }

    /// <summary>
    /// Enhanced condition wait with Windows workarounds
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
                return element != null && condition(element) ? element : null;
            }
            catch
            {
                return null;
            }
        });
    }

    #endregion
}

/// <summary>
/// Element information structure
/// </summary>
public class ElementInfo
{
    public bool Exists { get; set; }
    public bool IsVisible { get; set; }
    public bool IsEnabled { get; set; }
    public string Text { get; set; } = string.Empty;
    public string TagName { get; set; } = string.Empty;
} 