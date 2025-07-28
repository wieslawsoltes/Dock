using System;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;

namespace DockMvvmSample.AppiumTests.Infrastructure;

/// <summary>
/// Base class for all page objects, providing common functionality and standardized patterns
/// </summary>
public abstract class BasePage
{
    protected IWebDriver Driver { get; }
    protected ElementHelper Elements { get; }

    protected BasePage(IWebDriver driver)
    {
        Driver = driver ?? throw new ArgumentNullException(nameof(driver));
        Elements = new ElementHelper(driver);
    }

    #region Common Page Actions

    /// <summary>
    /// Waits for the page to load by verifying key elements are present
    /// Override this method in derived classes to specify page-specific elements
    /// </summary>
    public virtual void WaitForPageLoad(int timeoutSeconds = 30)
    {
        var startTime = DateTime.Now;
        var maxWaitTime = TimeSpan.FromSeconds(timeoutSeconds);
        
        while (DateTime.Now - startTime < maxWaitTime)
        {
            if (IsPageLoaded())
            {
                // Additional small wait for UI stability
                Thread.Sleep(1000);
                return;
            }
            
            Thread.Sleep(500);
        }
        
        throw new TimeoutException($"Page {GetType().Name} did not load within {timeoutSeconds} seconds");
    }

    /// <summary>
    /// Override this method to define page-specific loading criteria
    /// </summary>
    protected abstract bool IsPageLoaded();

    /// <summary>
    /// Validates that all essential page elements are accessible
    /// Override this method to specify page-specific essential elements
    /// </summary>
    public virtual bool ValidatePageElements()
    {
        var essentialElements = GetEssentialElementIds();
        if (essentialElements.Length == 0)
            return true; // No validation needed if no essential elements defined
            
        var results = Elements.ValidateElements(essentialElements);
        return results.Values.All(found => found);
    }

    /// <summary>
    /// Override this method to specify essential elements that must be present for the page to be considered loaded
    /// </summary>
    protected virtual string[] GetEssentialElementIds()
    {
        return Array.Empty<string>();
    }

    #endregion

    #region Fluent Element Interaction API

    /// <summary>
    /// Clicks an element and returns this page for method chaining
    /// </summary>
    protected BasePage ClickElement(string accessibilityId, int? timeoutSeconds = null)
    {
        Elements.Click(accessibilityId, timeoutSeconds);
        return this;
    }

    /// <summary>
    /// Types text into an element and returns this page for method chaining
    /// </summary>
    protected BasePage TypeInElement(string accessibilityId, string text, bool clearFirst = true, int? timeoutSeconds = null)
    {
        Elements.Type(accessibilityId, text, clearFirst, timeoutSeconds);
        return this;
    }

    /// <summary>
    /// Waits for an element to be clickable and returns this page for method chaining
    /// </summary>
    protected BasePage WaitForClickable(string accessibilityId, int? timeoutSeconds = null)
    {
        Elements.WaitForClickable(accessibilityId, timeoutSeconds);
        return this;
    }

    /// <summary>
    /// Waits for an element to be visible and returns this page for method chaining
    /// </summary>
    protected BasePage WaitForVisible(string accessibilityId, int? timeoutSeconds = null)
    {
        Elements.WaitForVisible(accessibilityId, timeoutSeconds);
        return this;
    }

    #endregion

    #region Element Property Access

    /// <summary>
    /// Gets text from an element
    /// </summary>
    protected string GetElementText(string accessibilityId, int? timeoutSeconds = null)
    {
        return Elements.GetText(accessibilityId, timeoutSeconds);
    }

    /// <summary>
    /// Gets an attribute value from an element
    /// </summary>
    protected string? GetElementAttribute(string accessibilityId, string attributeName, int? timeoutSeconds = null)
    {
        return Elements.GetAttribute(accessibilityId, attributeName, timeoutSeconds);
    }

    /// <summary>
    /// Checks if an element is visible
    /// </summary>
    public bool IsElementVisible(string accessibilityId)
    {
        return Elements.IsVisible(accessibilityId);
    }

    /// <summary>
    /// Checks if an element is enabled
    /// </summary>
    protected bool IsElementEnabled(string accessibilityId)
    {
        return Elements.IsEnabled(accessibilityId);
    }

    /// <summary>
    /// Tries to find an element without throwing exceptions
    /// </summary>
    protected bool TryFindElement(string accessibilityId, out IWebElement? element)
    {
        return Elements.TryFindByAccessibilityId(accessibilityId, out element);
    }

    #endregion

    #region Advanced Operations

    /// <summary>
    /// Performs a custom action on an element with error handling
    /// </summary>
    protected T PerformElementAction<T>(string accessibilityId, Func<IWebElement, T> action, int? timeoutSeconds = null)
    {
        return Elements.PerformElementAction(accessibilityId, action, timeoutSeconds);
    }

    /// <summary>
    /// Performs a custom action on an element with error handling (void return)
    /// </summary>
    protected BasePage PerformElementAction(string accessibilityId, Action<IWebElement> action, int? timeoutSeconds = null)
    {
        Elements.PerformElementAction(accessibilityId, action, timeoutSeconds);
        return this;
    }

    /// <summary>
    /// Waits for a custom condition on an element
    /// </summary>
    protected IWebElement WaitForElementCondition(string accessibilityId, Func<IWebElement, bool> condition, int? timeoutSeconds = null)
    {
        return Elements.WaitForCondition(accessibilityId, condition, timeoutSeconds);
    }

    #endregion

    #region Common UI Patterns

    /// <summary>
    /// Clicks a menu item and waits for it to open
    /// </summary>
    protected BasePage ClickMenu(string menuAccessibilityId, int waitAfterClick = 500)
    {
        ClickElement(menuAccessibilityId);
        Thread.Sleep(waitAfterClick); // Wait for menu animation
        return this;
    }

    /// <summary>
    /// Navigates using a navigation control (text box + button pattern)
    /// </summary>
    protected BasePage NavigateToPath(string textBoxAccessibilityId, string buttonAccessibilityId, string path)
    {
        TypeInElement(textBoxAccessibilityId, path);
        ClickElement(buttonAccessibilityId);
        return this;
    }

    /// <summary>
    /// Selects an item from a dropdown or list
    /// </summary>
    protected BasePage SelectFromDropdown(string dropdownAccessibilityId, string optionText)
    {
        ClickElement(dropdownAccessibilityId);
        Thread.Sleep(300); // Wait for dropdown to open
        
        // Find and click the option (this might need to be customized based on the actual dropdown implementation)
        var optionElement = Driver.FindElement(By.XPath($"//*[contains(text(), '{optionText}')]"));
        optionElement.Click();
        
        return this;
    }

    #endregion

    #region Screenshot and Debugging

    /// <summary>
    /// Takes a screenshot for debugging purposes
    /// </summary>
    public void TakeScreenshot(string description)
    {
        if (Driver is ITakesScreenshot screenshotDriver)
        {
            try
            {
                var screenshot = screenshotDriver.GetScreenshot();
                var fileName = $"{GetType().Name}_{description}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                var filePath = System.IO.Path.Combine(AppContext.BaseDirectory, "Screenshots", fileName);
                
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath)!);
                screenshot.SaveAsFile(filePath);
            }
            catch (Exception ex)
            {
                // Don't fail the test because of screenshot issues
                Console.WriteLine($"Failed to take screenshot: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets detailed information about multiple elements for debugging
    /// </summary>
    protected void LogElementsInfo(params string[] accessibilityIds)
    {
        var elementsInfo = Elements.GetElementsInfo(accessibilityIds);
        
        foreach (var info in elementsInfo)
        {
            var status = info.Value.Exists ? "✓" : "✗";
            var visibility = info.Value.IsVisible ? "Visible" : "Hidden";
            var enabled = info.Value.IsEnabled ? "Enabled" : "Disabled";
            
            Console.WriteLine($"{status} {info.Key}: {visibility}, {enabled}, Text: '{info.Value.Text}'");
        }
    }

    #endregion
} 