using System;
using System.Threading;
using DockMvvmSample.AppiumTests.Infrastructure;
using OpenQA.Selenium;
using Xunit;
using Xunit.Abstractions;

namespace DockMvvmSample.AppiumTests.Tests;

[Collection("AppiumTests")]
public class AutomationPropertiesTests : BaseTest
{
    private readonly ITestOutputHelper _output;

    public AutomationPropertiesTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ApplicationStartsSuccessfully()
    {
        _output.WriteLine("=== Testing Application Startup with Automation Properties ===");
        
        // Wait for app to load
        Thread.Sleep(3000);
        
        try
        {
            // Test 1: Find main window using standard automation approach
            var mainWindow = Driver.FindElement(By.Id("MainWindow"));
            Assert.NotNull(mainWindow);
            _output.WriteLine("✓ Found MainWindow by Id");
            
            // Verify window title
            var windowTitle = mainWindow.GetAttribute("name") ?? mainWindow.GetAttribute("title");
            Assert.Contains("Dock Avalonia Demo", windowTitle ?? "");
            _output.WriteLine($"✓ Window title: {windowTitle}");
            
            // Test 2: Find and interact with menu
            var mainMenu = Driver.FindElement(By.Id("MainMenu"));
            Assert.NotNull(mainMenu);
            _output.WriteLine("✓ Found MainMenu by Id");
            
            // Test 3: Find File menu
            var fileMenu = Driver.FindElement(By.Id("FileMenu"));
            Assert.NotNull(fileMenu);
            _output.WriteLine("✓ Found FileMenu by Id");
            
            // Test 4: Find toolbar elements
            var backButton = Driver.FindElement(By.Id("BackButton"));
            Assert.NotNull(backButton);
            _output.WriteLine("✓ Found BackButton by Id");
            
            var forwardButton = Driver.FindElement(By.Id("ForwardButton"));
            Assert.NotNull(forwardButton);
            _output.WriteLine("✓ Found ForwardButton by Id");
            
            var dashboardButton = Driver.FindElement(By.Id("DashboardButton"));
            Assert.NotNull(dashboardButton);
            _output.WriteLine("✓ Found DashboardButton by Id");
            
            // Test 5: Find navigation text box
            var navigationTextBox = Driver.FindElement(By.Id("NavigationTextBox"));
            Assert.NotNull(navigationTextBox);
            _output.WriteLine("✓ Found NavigationTextBox by Id");
            
            // Test 6: Find main dock control
            var mainDockControl = Driver.FindElement(By.Id("MainDockControl"));
            Assert.NotNull(mainDockControl);
            _output.WriteLine("✓ Found MainDockControl by Id");
            
            _output.WriteLine("=== All automation property tests passed! ===");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"❌ Test failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public void MainMenuIsAccessible()
    {
        _output.WriteLine("=== Testing Main Menu Accessibility ===");
        
        // Wait for app to load
        Thread.Sleep(3000);
        
        try
        {
            // Find and click File menu
            var fileMenu = Driver.FindElement(By.Id("FileMenu"));
            fileMenu.Click();
            
            // Wait for menu to open
            Thread.Sleep(1000);
            
            // Find menu items
            var newLayoutMenuItem = Driver.FindElement(By.Id("NewLayoutMenuItem"));
            Assert.NotNull(newLayoutMenuItem);
            _output.WriteLine("✓ Found NewLayoutMenuItem by Id");
            
            var openLayoutMenuItem = Driver.FindElement(By.Id("OpenLayoutMenuItem"));
            Assert.NotNull(openLayoutMenuItem);
            _output.WriteLine("✓ Found OpenLayoutMenuItem by Id");
            
            var saveLayoutMenuItem = Driver.FindElement(By.Id("SaveLayoutMenuItem"));
            Assert.NotNull(saveLayoutMenuItem);
            _output.WriteLine("✓ Found SaveLayoutMenuItem by Id");
            
            // Click somewhere else to close menu
            var mainWindow = Driver.FindElement(By.Id("MainWindow"));
            mainWindow.Click();
            
            _output.WriteLine("=== Menu accessibility tests passed! ===");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"❌ Menu test failed: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public void ToolbarButtonsAreClickable()
    {
        _output.WriteLine("=== Testing Toolbar Button Interactions ===");
        
        // Wait for app to load
        Thread.Sleep(3000);
        
        try
        {
            // Test clicking Dashboard button
            var dashboardButton = Driver.FindElement(By.Id("DashboardButton"));
            Assert.True(dashboardButton.Enabled);
            dashboardButton.Click();
            _output.WriteLine("✓ Dashboard button clicked successfully");
            
            // Wait a moment
            Thread.Sleep(1000);
            
            // Test Home button
            var homeButton = Driver.FindElement(By.Id("HomeButton"));
            Assert.True(homeButton.Enabled);
            homeButton.Click();
            _output.WriteLine("✓ Home button clicked successfully");
            
            // Test navigation text box interaction
            var navigationTextBox = Driver.FindElement(By.Id("NavigationTextBox"));
            navigationTextBox.Clear();
            navigationTextBox.SendKeys("TestPage");
            _output.WriteLine("✓ Navigation text box interaction successful");
            
            // Test Navigate button
            var navigateButton = Driver.FindElement(By.Id("NavigateButton"));
            Assert.True(navigateButton.Enabled);
            navigateButton.Click();
            _output.WriteLine("✓ Navigate button clicked successfully");
            
            _output.WriteLine("=== Toolbar interaction tests passed! ===");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"❌ Toolbar test failed: {ex.Message}");
            throw;
        }
    }
} 