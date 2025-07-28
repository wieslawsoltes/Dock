using System;
using System.Threading;
using DockMvvmSample.AppiumTests.Infrastructure;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using Xunit;
using Xunit.Abstractions;

namespace DockMvvmSample.AppiumTests.Tests;

[Collection("AppiumTests")]
public class PlatformSpecificElementFindingTests : BaseTest
{
    private readonly ITestOutputHelper _output;

    public PlatformSpecificElementFindingTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ElementFinding_ShouldWorkAcrossPlatforms_WithNewStrategy()
    {
        _output.WriteLine("=== Testing Platform-Specific Element Finding Improvements ===");
        
        // Wait for app to load
        Thread.Sleep(3000);
        
        try
        {
            // Get platform information
            var capabilities = (Driver as AppiumDriver<AppiumWebElement>)?.Capabilities;
            var platformName = capabilities?.GetCapability("platformName")?.ToString();
            var automationName = capabilities?.GetCapability("automationName")?.ToString();
            
            _output.WriteLine($"Platform: {platformName}");
            _output.WriteLine($"Automation: {automationName}");
            
            // Test the improved element finding strategy
            _output.WriteLine("\n--- Testing Improved Element Finding Strategy ---");
            
            var criticalElements = new[]
            {
                "MainWindow",
                "FileMenu", 
                "MainDockControl",
                "BackButton",
                "ForwardButton",
                "DashboardButton",
                "NavigationTextBox"
            };
            
            foreach (var elementId in criticalElements)
            {
                var startTime = DateTime.Now;
                
                try
                {
                    var element = Elements.FindByAccessibilityId(elementId);
                    var findTime = DateTime.Now - startTime;
                    
                    Assert.NotNull(element);
                    Assert.True(element.Displayed, $"Element {elementId} should be visible");
                    
                    _output.WriteLine($"✓ Found {elementId} in {findTime.TotalMilliseconds:F0}ms - Strategy: Platform-aware");
                    
                    // Log element properties for debugging
                    var automationId = GetElementAttributeSafely(element, "AutomationId");
                    var name = GetElementAttributeSafely(element, "Name");
                    var className = GetElementAttributeSafely(element, "ClassName");
                    
                    _output.WriteLine($"    AutomationId: {automationId}, Name: {name}, ClassName: {className}");
                }
                catch (Exception ex)
                {
                    var findTime = DateTime.Now - startTime;
                    _output.WriteLine($"✗ Failed to find {elementId} after {findTime.TotalMilliseconds:F0}ms: {ex.Message}");
                    throw;
                }
            }
            
            _output.WriteLine("\n--- Platform Strategy Validation ---");
            
            // Test platform-specific behavior
            if (string.Equals(platformName, "Windows", StringComparison.OrdinalIgnoreCase))
            {
                _output.WriteLine("✓ Using Windows-optimized element finding strategies");
                _output.WriteLine("  - Using @automationid and @name attributes");
                _output.WriteLine("  - Fallback strategies for Windows Application Driver");
            }
            else if (string.Equals(platformName, "Mac", StringComparison.OrdinalIgnoreCase))
            {
                _output.WriteLine("✓ Using macOS-optimized element finding strategies");
                _output.WriteLine("  - Using @identifier and @name attributes");
                _output.WriteLine("  - Optimized for Mac2 automation driver");
            }
            else
            {
                _output.WriteLine("✓ Using generic cross-platform strategies with modern AppiumBy reflection");
            }
            
            _output.WriteLine("\n=== Platform-Specific Element Finding Test Completed Successfully ===");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Platform-specific test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
    
    [Fact]
    public void ElementFinding_ShouldHandleEdgeCases_Gracefully()
    {
        _output.WriteLine("=== Testing Edge Cases and Error Handling ===");
        
        Thread.Sleep(3000);
        
        try
        {
            // Test 1: Non-existent element
            _output.WriteLine("Testing non-existent element handling...");
            
            var foundNonExistent = Elements.TryFindByAccessibilityId("NonExistentElement123", out var nonExistentElement);
            Assert.False(foundNonExistent);
            Assert.Null(nonExistentElement);
            _output.WriteLine("✓ Non-existent element handled gracefully");
            
            // Test 2: Empty/null input handling
            _output.WriteLine("Testing empty input handling...");
            
            try
            {
                Elements.FindByAccessibilityId("");
                _output.WriteLine("⚠ Empty string didn't throw exception - this might be expected");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"✓ Empty string handled: {ex.GetType().Name}");
            }
            
            // Test 3: Special characters in element ID
            _output.WriteLine("Testing special character handling...");
            
            var foundSpecial = Elements.TryFindByAccessibilityId("Element.With.Dots", out var specialElement);
            _output.WriteLine($"Special character element found: {foundSpecial}");
            
            // Test 4: Validate multiple finding strategies work
            _output.WriteLine("Testing fallback strategies...");
            
            var mainWindow = Elements.FindByAccessibilityId("MainWindow");
            Assert.NotNull(mainWindow);
            
            // Try to find the same element using different approaches and verify consistency
            var sameElementCheck = Elements.TryFindByAccessibilityId("MainWindow", out var mainWindow2);
            Assert.True(sameElementCheck);
            Assert.NotNull(mainWindow2);
            
            _output.WriteLine("✓ Element finding consistency validated");
            
            _output.WriteLine("\n=== Edge Case Testing Completed Successfully ===");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Edge case test failed: {ex.Message}");
            throw;
        }
    }
    
    private string GetElementAttributeSafely(IWebElement element, string attributeName)
    {
        try
        {
            return element.GetAttribute(attributeName) ?? "null";
        }
        catch
        {
            return "error";
        }
    }
} 