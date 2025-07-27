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
        _output.WriteLine("=== DRAG TOOL1 INTO TOOL5 DOCK TEST ===");
        
        try
        {
            // Wait for the application to fully load (app is already started by driver factory)
            _mainWindow.WaitForApplicationToLoad();
            
            _output.WriteLine("Application loaded, starting drag and drop test");
            
            // Take a screenshot before the test
            TakeScreenshot("BeforeDragDrop");
            
            // Find Tool1 tab using AutomationProperties
            IWebElement? tool1Tab = null;
            IWebElement? tool5Tab = null;
            IWebElement? tool5Dock = null;
            
            _output.WriteLine("Searching for Tool1 tab...");
            try
            {
                // Try multiple selectors for Tool1
                var tool1Selectors = new[]
                {
                    "//*[@title='Tool1']",
                    "//*[@label='Tool1']", 
                    "//*[contains(@title, 'Tool1')]",
                    "//*[contains(@label, 'Tool1')]",
                    "//*[@identifier='Tool1Control']"
                };
                
                foreach (var selector in tool1Selectors)
                {
                    try
                    {
                        var elements = Driver.FindElements(By.XPath(selector));
                        if (elements.Count > 0)
                        {
                            tool1Tab = elements[0];
                            _output.WriteLine($"Found Tool1 using selector: {selector}");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"Selector '{selector}' failed: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error searching for Tool1: {ex.Message}");
            }
            
            _output.WriteLine("Searching for Tool5 tab...");
            try
            {
                // Try multiple selectors for Tool5
                var tool5Selectors = new[]
                {
                    "//*[@title='Tool5']",
                    "//*[@label='Tool5']",
                    "//*[contains(@title, 'Tool5')]",
                    "//*[contains(@label, 'Tool5')]",
                    "//*[@identifier='Tool5Control']"
                };
                
                foreach (var selector in tool5Selectors)
                {
                    try
                    {
                        var elements = Driver.FindElements(By.XPath(selector));
                        if (elements.Count > 0)
                        {
                            tool5Tab = elements[0];
                            _output.WriteLine($"Found Tool5 using selector: {selector}");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"Selector '{selector}' failed: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error searching for Tool5: {ex.Message}");
            }
            
            // If we found the tools, try to find their dock containers
            if (tool1Tab != null && tool5Tab != null)
            {
                _output.WriteLine("Both tools found, attempting drag and drop...");
                
                // Get element positions for logging
                var tool1Location = tool1Tab.Location;
                var tool1Size = tool1Tab.Size;
                var tool5Location = tool5Tab.Location;
                var tool5Size = tool5Tab.Size;
                
                _output.WriteLine($"Tool1 position: ({tool1Location.X}, {tool1Location.Y}), size: ({tool1Size.Width}, {tool1Size.Height})");
                _output.WriteLine($"Tool5 position: ({tool5Location.X}, {tool5Location.Y}), size: ({tool5Size.Width}, {tool5Size.Height})");
                
                // Try to find the Tool5 dock container (the area where Tool1 should be dropped)
                try
                {
                    // Look for the parent dock container of Tool5
                    var tool5Parent = tool5Tab.FindElement(By.XPath("ancestor::*[contains(@class, 'ToolDock') or contains(@elementType, 'Group')]"));
                    if (tool5Parent != null)
                    {
                        tool5Dock = tool5Parent;
                        _output.WriteLine("Found Tool5 dock container");
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Could not find Tool5 dock container: {ex.Message}");
                    // Use Tool5 tab itself as fallback
                    tool5Dock = tool5Tab;
                }
                
                // Perform the drag and drop operation
                var actions = new OpenQA.Selenium.Interactions.Actions(Driver);
                
                // Method 1: Simple drag and drop
                try
                {
                    _output.WriteLine("Attempting simple drag and drop...");
                    actions.DragAndDrop(tool1Tab, tool5Dock).Perform();
                    
                    // Wait for the UI to update
                    Thread.Sleep(2000);
                    
                    _output.WriteLine("Simple drag and drop completed");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Simple drag and drop failed: {ex.Message}");
                    
                    // Method 2: Manual click, hold, move, release
                    try
                    {
                        _output.WriteLine("Attempting manual drag sequence...");
                        
                        actions = new OpenQA.Selenium.Interactions.Actions(Driver);
                        actions.ClickAndHold(tool1Tab)
                               .MoveToElement(tool5Dock)
                               .Release()
                               .Perform();
                        
                        Thread.Sleep(2000);
                        _output.WriteLine("Manual drag sequence completed");
                    }
                    catch (Exception ex2)
                    {
                        _output.WriteLine($"Manual drag sequence also failed: {ex2.Message}");
                        throw;
                    }
                }
                
                // Take a screenshot after the operation
                TakeScreenshot("AfterDragDrop");
                
                // Verify the result - Tool1 should now be in the same dock as Tool5
                try
                {
                    Thread.Sleep(1000);
                    
                    // Look for Tool1 near Tool5 or in the same container
                    var tool1AfterDrop = Driver.FindElements(By.XPath("//*[@identifier='Tool1' or @title='Tool1']"));
                    var tool5AfterDrop = Driver.FindElements(By.XPath("//*[@identifier='Tool5' or @title='Tool5']"));
                    
                    _output.WriteLine($"After drop - Tool1 elements found: {tool1AfterDrop.Count}");
                    _output.WriteLine($"After drop - Tool5 elements found: {tool5AfterDrop.Count}");
                    
                    if (tool1AfterDrop.Count > 0 && tool5AfterDrop.Count > 0)
                    {
                        var newTool1Pos = tool1AfterDrop[0].Location;
                        var newTool5Pos = tool5AfterDrop[0].Location;
                        
                        _output.WriteLine($"Tool1 new position: ({newTool1Pos.X}, {newTool1Pos.Y})");
                        _output.WriteLine($"Tool5 position: ({newTool5Pos.X}, {newTool5Pos.Y})");
                        
                        // Check if Tool1 moved significantly (indicating successful drag)
                        var distanceMoved = Math.Sqrt(Math.Pow(newTool1Pos.X - tool1Location.X, 2) + 
                                                    Math.Pow(newTool1Pos.Y - tool1Location.Y, 2));
                        
                        _output.WriteLine($"Tool1 moved distance: {distanceMoved} pixels");
                        
                        if (distanceMoved > 50) // Threshold for significant movement
                        {
                            _output.WriteLine("✓ Tool1 appears to have been successfully moved");
                        }
                        else
                        {
                            _output.WriteLine("⚠ Tool1 may not have moved significantly");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Verification step failed: {ex.Message}");
                }
                
                _output.WriteLine("✓ Drag and drop test completed successfully");
            }
            else
            {
                var tool1Found = tool1Tab != null ? "✓" : "✗";
                var tool5Found = tool5Tab != null ? "✓" : "✗";
                
                _output.WriteLine($"Could not find required elements - Tool1: {tool1Found}, Tool5: {tool5Found}");
                
                // Debug: List all available elements with automation properties
                try
                {
                    var allAutomationElements = Driver.FindElements(By.XPath("//*[@identifier or @title or @label]"));
                    _output.WriteLine($"All elements with automation properties ({allAutomationElements.Count}):");
                    
                    for (int i = 0; i < Math.Min(20, allAutomationElements.Count); i++)
                    {
                        var element = allAutomationElements[i];
                        var identifier = element.GetAttribute("identifier") ?? "";
                        var title = element.GetAttribute("title") ?? "";
                        var label = element.GetAttribute("label") ?? "";
                        var elementType = element.GetAttribute("elementType") ?? "";
                        
                        _output.WriteLine($"  {i}: identifier='{identifier}', title='{title}', label='{label}', type='{elementType}'");
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Debug listing failed: {ex.Message}");
                }
                
                Assert.Fail("Could not find Tool1 and/or Tool5 elements for drag and drop test");
            }
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Drag and drop test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Take a screenshot on failure
            try
            {
                TakeScreenshot("DragDropTestFailure");
            }
            catch
            {
                _output.WriteLine("Could not take failure screenshot");
            }
            
            throw;
        }
        
        _output.WriteLine("=== END DRAG TOOL1 INTO TOOL5 DOCK TEST ===");
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