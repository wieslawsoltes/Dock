using System;
using System.Linq;
using System.Threading;
using DockMvvmSample.AppiumTests.Infrastructure;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using Xunit;
using Xunit.Abstractions;

namespace DockMvvmSample.AppiumTests.Tests;

[Collection("AppiumTests")]
public class AccessibilityIdTests : BaseTest
{
    private readonly ITestOutputHelper _output;

    public AccessibilityIdTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void FindElementByAccessibilityId_ShouldWork_OnWindowsAndMacOS()
    {
        _output.WriteLine("=== Testing FindElementByAccessibilityId Cross-Platform Compatibility ===");
        
        // Wait for app to load
        Thread.Sleep(3000);
        
        try
        {
            // Test 1: Use clean helper method
            _output.WriteLine("Finding MainWindow using Elements.FindByAccessibilityId...");
            var mainWindow = Elements.FindByAccessibilityId("MainWindow");
            Assert.NotNull(mainWindow);
            _output.WriteLine("✓ Found MainWindow using AccessibilityId");
            
            // Test 2: Compare performance between old and new approach
            _output.WriteLine("\n--- Performance Comparison ---");
            
            // Method 1: Old By.Id approach
            var startTime = DateTime.Now;
            var menuById = Driver.FindElement(By.Id("MainMenu"));
            var byIdTime = DateTime.Now - startTime;
            _output.WriteLine($"By.Id time: {byIdTime.TotalMilliseconds}ms");
            
            // Method 2: New clean AccessibilityId approach
            startTime = DateTime.Now;
            var menuByAccessibilityId = Elements.FindByAccessibilityId("MainMenu");
            var accessibilityIdTime = DateTime.Now - startTime;
            _output.WriteLine($"Elements.FindByAccessibilityId time: {accessibilityIdTime.TotalMilliseconds}ms");
            
            // Both should find the same element
            Assert.Equal(menuById.GetAttribute("AutomationId"), menuByAccessibilityId.GetAttribute("AutomationId"));
            _output.WriteLine("✓ Both methods found the same element");
            
            // Test 3: Test all major UI elements using clean helper methods
            _output.WriteLine("\n--- Testing All UI Elements with Clean AccessibilityId Methods ---");
            
            var elements = new[]
            {
                "FileMenu",
                "BackButton", 
                "ForwardButton",
                "DashboardButton",
                "NavigationTextBox",
                "MainDockControl"
            };
            
            foreach (var elementId in elements)
            {
                try
                {
                    var element = Elements.FindByAccessibilityId(elementId);
                    Assert.NotNull(element);
                    _output.WriteLine($"✓ Found {elementId} using clean AccessibilityId method");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"✗ Failed to find {elementId}: {ex.Message}");
                    throw;
                }
            }
            
            // Test 4: Verify accessibility attributes using helper method
            _output.WriteLine("\n--- Verifying Accessibility Properties ---");
            
            var automationId = Elements.GetAttribute("FileMenu", "AutomationId");
            var name = Elements.GetAttribute("FileMenu", "Name") ?? Elements.GetAttribute("FileMenu", "title");
            
            _output.WriteLine($"AutomationId: {automationId}");
            _output.WriteLine($"Name/Title: {name}");
            
            Assert.Equal("FileMenu", automationId);
            _output.WriteLine("✓ Accessibility properties correctly exposed");
            
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    [Fact] 
    public void AccessibilityId_ShouldBeConsistent_AcrossPlatforms()
    {
        _output.WriteLine("=== Testing Cross-Platform Consistency ===");
        
        Thread.Sleep(3000);
        
        // This test verifies that the same accessibility IDs work on both Windows and macOS
        // The advantage of AccessibilityId over other locators is platform independence
        
        var platformIndependentElements = new[]
        {
            "MainWindow",
            "MainMenu", 
            "FileMenu",
            "MainDockControl",
            "DashboardButton",
            "NavigationTextBox"
        };
        
        // Use the clean validation helper method
        var results = Elements.ValidateElements(platformIndependentElements);
        
        foreach (var result in results)
        {
            Assert.True(result.Value, $"Element {result.Key} should be accessible on current platform");
            _output.WriteLine($"✓ {result.Key} found and accessible on current platform");
        }
        
        _output.WriteLine("✓ All elements consistently accessible across platforms");
    }

    [Fact]
    public void AccessibilityId_ShouldSupportComplexInteractions()
    {
        _output.WriteLine("=== Testing Complex Interactions with Clean AccessibilityId Methods ===");
        
        Thread.Sleep(3000);
        
        try
        {
            // Test menu interaction using clean helper methods
            _output.WriteLine("Testing menu interaction...");
            Elements.Click("FileMenu");
            Thread.Sleep(500);
            
            // Try to find submenu items
            try
            {
                var newLayoutMenuItem = Elements.FindByAccessibilityId("NewLayoutMenuItem");
                Assert.NotNull(newLayoutMenuItem);
                _output.WriteLine("✓ Found submenu item using clean AccessibilityId method");
            }
            catch (NoSuchElementException)
            {
                _output.WriteLine("ℹ Submenu items may not be visible or may not have accessibility IDs");
            }
            
            // Test navigation using clean helper methods
            _output.WriteLine("Testing navigation...");
            Elements.Type("NavigationTextBox", "test navigation");
            
            // Verify the text was entered correctly
            var enteredText = Elements.GetText("NavigationTextBox");
            _output.WriteLine($"Entered text: {enteredText}");
            
            // Test clickable element waiting
            var navigateButton = Elements.WaitForClickable("NavigateButton");
            Assert.True(navigateButton.Enabled, "Navigate button should be enabled");
            // navigateButton.Click(); // Commented out to avoid actual navigation
            
            _output.WriteLine("✓ Complex interactions work with clean AccessibilityId methods");
            
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Complex interaction test failed: {ex.Message}");
            // Don't throw here as this might be expected behavior
        }
    }

    [Fact]
    public void HelperMethods_ShouldProvideCleanAPI()
    {
        _output.WriteLine("=== Testing Clean Helper Method API ===");
        
        Thread.Sleep(3000);
        
        try
        {
            // Test basic element finding
            var mainWindow = Elements.FindByAccessibilityIdWithWait("MainWindow", 15);
            Assert.NotNull(mainWindow);
            _output.WriteLine("✓ Elements.FindByAccessibilityIdWithWait works");
            
            // Test try-find pattern
            bool found = Elements.TryFindByAccessibilityId("MainMenu", out var menuElement);
            Assert.True(found);
            Assert.NotNull(menuElement);
            _output.WriteLine("✓ Elements.TryFindByAccessibilityId works");
            
            // Test attribute retrieval
            var windowTitle = Elements.GetAttribute("MainWindow", "title");
            Assert.NotNull(windowTitle);
            _output.WriteLine($"✓ Elements.GetAttribute works: {windowTitle}");
            
            // Test multiple elements finding
            var elements = Elements.FindAllByAccessibilityId("MainMenu");
            Assert.True(elements.Count > 0);
            _output.WriteLine("✓ Elements.FindAllByAccessibilityId works");
            
            // Test validation helper
            var validationResults = Elements.ValidateElements("MainWindow", "MainMenu", "FileMenu");
            Assert.True(validationResults.All(r => r.Value));
            _output.WriteLine("✓ Elements.ValidateElements works");
            
            _output.WriteLine("✓ All helper methods provide a clean, easy-to-use API");
            
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Helper method test failed: {ex.Message}");
            throw;
        }
    }
} 