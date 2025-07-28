using System;
using OpenQA.Selenium;

namespace DockMvvmSample.AppiumTests.Infrastructure;

/// <summary>
/// Provides a fluent API for element interactions to make tests more readable
/// </summary>
public class FluentElementActions
{
    private readonly ElementHelper _elements;

    public FluentElementActions(ElementHelper elements)
    {
        _elements = elements ?? throw new ArgumentNullException(nameof(elements));
    }

    /// <summary>
    /// Starts a fluent action chain on a specific element
    /// </summary>
    public ElementActionChain Element(string accessibilityId)
    {
        return new ElementActionChain(_elements, accessibilityId);
    }

    /// <summary>
    /// Clicks an element and returns the fluent actions instance for chaining
    /// </summary>
    public FluentElementActions Click(string accessibilityId, int? timeoutSeconds = null)
    {
        _elements.Click(accessibilityId, timeoutSeconds);
        return this;
    }

    /// <summary>
    /// Types text into an element and returns the fluent actions instance for chaining
    /// </summary>
    public FluentElementActions Type(string accessibilityId, string text, bool clearFirst = true, int? timeoutSeconds = null)
    {
        _elements.Type(accessibilityId, text, clearFirst, timeoutSeconds);
        return this;
    }

    /// <summary>
    /// Waits for an element to be visible and returns the fluent actions instance for chaining
    /// </summary>
    public FluentElementActions WaitForVisible(string accessibilityId, int? timeoutSeconds = null)
    {
        _elements.WaitForVisible(accessibilityId, timeoutSeconds);
        return this;
    }

    /// <summary>
    /// Waits for an element to be clickable and returns the fluent actions instance for chaining
    /// </summary>
    public FluentElementActions WaitForClickable(string accessibilityId, int? timeoutSeconds = null)
    {
        _elements.WaitForClickable(accessibilityId, timeoutSeconds);
        return this;
    }

    /// <summary>
    /// Performs a navigation action using textbox + button pattern
    /// </summary>
    public FluentElementActions Navigate(string textBoxId, string buttonId, string path)
    {
        Type(textBoxId, path);
        Click(buttonId);
        return this;
    }

    /// <summary>
    /// Performs a menu click with optional wait
    /// </summary>
    public FluentElementActions ClickMenu(string menuAccessibilityId, int waitAfterClick = 500)
    {
        Click(menuAccessibilityId);
        if (waitAfterClick > 0)
        {
            System.Threading.Thread.Sleep(waitAfterClick);
        }
        return this;
    }

    /// <summary>
    /// Asserts that an element is visible
    /// </summary>
    public FluentElementActions AssertVisible(string accessibilityId, string? customMessage = null)
    {
        var isVisible = _elements.IsVisible(accessibilityId);
        if (!isVisible)
        {
            var message = customMessage ?? $"Element '{accessibilityId}' should be visible but is not";
            throw new AssertionException(message);
        }
        return this;
    }

    /// <summary>
    /// Asserts that an element is not visible
    /// </summary>
    public FluentElementActions AssertNotVisible(string accessibilityId, string? customMessage = null)
    {
        var isVisible = _elements.IsVisible(accessibilityId);
        if (isVisible)
        {
            var message = customMessage ?? $"Element '{accessibilityId}' should not be visible but is";
            throw new AssertionException(message);
        }
        return this;
    }

    /// <summary>
    /// Asserts that an element is enabled
    /// </summary>
    public FluentElementActions AssertEnabled(string accessibilityId, string? customMessage = null)
    {
        var isEnabled = _elements.IsEnabled(accessibilityId);
        if (!isEnabled)
        {
            var message = customMessage ?? $"Element '{accessibilityId}' should be enabled but is not";
            throw new AssertionException(message);
        }
        return this;
    }

    /// <summary>
    /// Asserts that an element contains specific text
    /// </summary>
    public FluentElementActions AssertTextContains(string accessibilityId, string expectedText, string? customMessage = null)
    {
        var actualText = _elements.GetText(accessibilityId);
        if (!actualText.Contains(expectedText))
        {
            var message = customMessage ?? $"Element '{accessibilityId}' should contain text '{expectedText}' but has '{actualText}'";
            throw new AssertionException(message);
        }
        return this;
    }

    /// <summary>
    /// Asserts that an element has exact text
    /// </summary>
    public FluentElementActions AssertTextEquals(string accessibilityId, string expectedText, string? customMessage = null)
    {
        var actualText = _elements.GetText(accessibilityId);
        if (actualText != expectedText)
        {
            var message = customMessage ?? $"Element '{accessibilityId}' should have text '{expectedText}' but has '{actualText}'";
            throw new AssertionException(message);
        }
        return this;
    }

    /// <summary>
    /// Executes a custom action and returns the fluent actions instance for chaining
    /// </summary>
    public FluentElementActions Do(Action action)
    {
        action?.Invoke();
        return this;
    }

    /// <summary>
    /// Adds a delay and returns the fluent actions instance for chaining
    /// </summary>
    public FluentElementActions Wait(int milliseconds)
    {
        System.Threading.Thread.Sleep(milliseconds);
        return this;
    }

    /// <summary>
    /// Gets text from an element (useful for storing values for later assertions)
    /// </summary>
    public string GetText(string accessibilityId, int? timeoutSeconds = null)
    {
        return _elements.GetText(accessibilityId, timeoutSeconds);
    }

    /// <summary>
    /// Gets an attribute from an element
    /// </summary>
    public string? GetAttribute(string accessibilityId, string attributeName, int? timeoutSeconds = null)
    {
        return _elements.GetAttribute(accessibilityId, attributeName, timeoutSeconds);
    }
}

/// <summary>
/// Provides a fluent API for actions on a specific element
/// </summary>
public class ElementActionChain
{
    private readonly ElementHelper _elements;
    private readonly string _accessibilityId;

    public ElementActionChain(ElementHelper elements, string accessibilityId)
    {
        _elements = elements ?? throw new ArgumentNullException(nameof(elements));
        _accessibilityId = accessibilityId ?? throw new ArgumentNullException(nameof(accessibilityId));
    }

    /// <summary>
    /// Clicks the element
    /// </summary>
    public ElementActionChain Click(int? timeoutSeconds = null)
    {
        _elements.Click(_accessibilityId, timeoutSeconds);
        return this;
    }

    /// <summary>
    /// Types text into the element
    /// </summary>
    public ElementActionChain Type(string text, bool clearFirst = true, int? timeoutSeconds = null)
    {
        _elements.Type(_accessibilityId, text, clearFirst, timeoutSeconds);
        return this;
    }

    /// <summary>
    /// Waits for the element to be visible
    /// </summary>
    public ElementActionChain WaitForVisible(int? timeoutSeconds = null)
    {
        _elements.WaitForVisible(_accessibilityId, timeoutSeconds);
        return this;
    }

    /// <summary>
    /// Waits for the element to be clickable
    /// </summary>
    public ElementActionChain WaitForClickable(int? timeoutSeconds = null)
    {
        _elements.WaitForClickable(_accessibilityId, timeoutSeconds);
        return this;
    }

    /// <summary>
    /// Performs a custom action on the element
    /// </summary>
    public ElementActionChain Do(Action<IWebElement> action, int? timeoutSeconds = null)
    {
        _elements.PerformElementAction(_accessibilityId, action, timeoutSeconds);
        return this;
    }

    /// <summary>
    /// Gets the text from the element
    /// </summary>
    public string GetText(int? timeoutSeconds = null)
    {
        return _elements.GetText(_accessibilityId, timeoutSeconds);
    }

    /// <summary>
    /// Gets an attribute from the element
    /// </summary>
    public string? GetAttribute(string attributeName, int? timeoutSeconds = null)
    {
        return _elements.GetAttribute(_accessibilityId, attributeName, timeoutSeconds);
    }

    /// <summary>
    /// Asserts that the element is visible
    /// </summary>
    public ElementActionChain ShouldBeVisible(string? customMessage = null)
    {
        var isVisible = _elements.IsVisible(_accessibilityId);
        if (!isVisible)
        {
            var message = customMessage ?? $"Element '{_accessibilityId}' should be visible but is not";
            throw new AssertionException(message);
        }
        return this;
    }

    /// <summary>
    /// Asserts that the element contains specific text
    /// </summary>
    public ElementActionChain ShouldContainText(string expectedText, string? customMessage = null)
    {
        var actualText = _elements.GetText(_accessibilityId);
        if (!actualText.Contains(expectedText))
        {
            var message = customMessage ?? $"Element '{_accessibilityId}' should contain text '{expectedText}' but has '{actualText}'";
            throw new AssertionException(message);
        }
        return this;
    }
}

/// <summary>
/// Custom exception for fluent assertion failures
/// </summary>
public class AssertionException : Exception
{
    public AssertionException(string message) : base(message) { }
    public AssertionException(string message, Exception innerException) : base(message, innerException) { }
} 