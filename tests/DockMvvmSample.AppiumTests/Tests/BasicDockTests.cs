using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DockMvvmSample.AppiumTests.Infrastructure;
using DockMvvmSample.AppiumTests.PageObjects;
using Xunit;
using Xunit.Abstractions;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace DockMvvmSample.AppiumTests.Tests;

[Collection("AppiumTests")]
public class BasicDockTests : BaseTest
{
    private readonly ITestOutputHelper _output;
    private readonly MainWindowPage _mainWindow;

    public BasicDockTests(ITestOutputHelper output)
    {
        _output = output;
        _mainWindow = new MainWindowPage(Driver);
    }



    [Fact]
    public void ApplicationStartsSuccessfully()
    {
        // Arrange & Act
        _mainWindow.WaitForApplicationToLoad();

        // Assert
        Assert.True(_mainWindow.IsMainWindowVisible(), "Main window should be visible after application starts");
        
        _output.WriteLine("✓ Application started successfully and main window is visible");
    }

    [Fact]
    public void MainMenuIsAccessible()
    {
        // Arrange
        _mainWindow.WaitForApplicationToLoad();

        // Act & Assert
        _mainWindow.ClickFileMenu();
        _output.WriteLine("✓ File menu is accessible");

        _mainWindow.ClickWindowMenu();
        _output.WriteLine("✓ Window menu is accessible");
    }

    [Fact]
    public void DocumentTabsAreVisible()
    {
        // Arrange
        _mainWindow.WaitForApplicationToLoad();

        // Act
        var documentTabs = _mainWindow.GetVisibleDocumentTabs();

        // Assert
        Assert.NotEmpty(documentTabs);
        _output.WriteLine($"✓ Found {documentTabs.Count} document tabs: {string.Join(", ", documentTabs)}");

        // Check for expected default documents
        Assert.Contains(documentTabs, tab => tab.Contains("Document"));
    }

    [Fact]
    public void ToolWindowsAreVisible()
    {
        // Arrange
        _mainWindow.WaitForApplicationToLoad();

        // Act
        var toolWindows = _mainWindow.GetVisibleToolWindows();

        // Assert
        Assert.NotEmpty(toolWindows);
        _output.WriteLine($"✓ Found {toolWindows.Count} tool windows: {string.Join(", ", toolWindows)}");

        // Check for expected default tools
        Assert.Contains(toolWindows, tool => tool.Contains("Tool"));
    }









    [Fact]
    public void CanDragTool1IntoTool5Dock()
    {
        _output.WriteLine("=== Testing Drag Tool1 to Tool5 Dock ===");
        
        // Wait for app to load
        Thread.Sleep(3000);
        
        try
        {
            // Find Tool1 tab and Tool5 tab using automation IDs
            var tool1Tab = Driver.FindElement(By.Id("Tool1"));
            var tool5Tab = Driver.FindElement(By.Id("Tool5"));
            
            _output.WriteLine("✓ Found Tool1 tab and Tool5 tab");
            
            // Get initial position
            var initialPosition = tool1Tab.Location;
            
            // Perform drag and drop
            var actions = new OpenQA.Selenium.Interactions.Actions(Driver);
            actions.DragAndDrop(tool1Tab, tool5Tab).Perform();
            
            _output.WriteLine("✓ Drag and drop operation completed");
            
            // Wait for UI to update
            Thread.Sleep(2000);
            
            // Verify Tool1 moved
            var tool1AfterDrop = Driver.FindElement(By.Id("Tool1"));
            var newPosition = tool1AfterDrop.Location;
            
            var distanceMoved = Math.Sqrt(Math.Pow(newPosition.X - initialPosition.X, 2) + 
                                        Math.Pow(newPosition.Y - initialPosition.Y, 2));
            
            Assert.True(distanceMoved > 50, $"Tool1 should have moved significantly (moved {distanceMoved} pixels)");
            
            _output.WriteLine($"✓ Tool1 successfully moved {distanceMoved} pixels");
            _output.WriteLine("=== Drag and drop test passed! ===");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"❌ Drag test failed: {ex.Message}");
            throw;
        }
    }

    public override void Dispose()
    {
        try
        {
            TakeScreenshot($"{GetType().Name}_Final");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Warning: Could not take final screenshot: {ex.Message}");
        }
        
        base.Dispose();
    }
} 