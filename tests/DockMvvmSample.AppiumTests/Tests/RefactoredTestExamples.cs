using System.Linq;
using System.Threading;
using DockMvvmSample.AppiumTests.Infrastructure;
using DockMvvmSample.AppiumTests.PageObjects;
using Xunit;
using Xunit.Abstractions;

namespace DockMvvmSample.AppiumTests.Tests;

[Collection("AppiumTests")]
public class RefactoredTestExamples : BaseTest
{
    private readonly ITestOutputHelper _output;
    private readonly MainWindowPage _mainWindow;

    public RefactoredTestExamples(ITestOutputHelper output)
    {
        _output = output;
        _mainWindow = new MainWindowPage(Driver);
    }

    [Fact]
    public void DemonstrateCleanPageObjectUsage()
    {
        _output.WriteLine("=== Demonstrating Clean Page Object Usage ===");
        
        // Wait for application to load using the new infrastructure
        _mainWindow.WaitForPageLoad();
        
        // Verify essential elements using new validation
        Assert.True(_mainWindow.VerifyMainElementsAccessible(), 
            "All essential elements should be accessible");
        _output.WriteLine("✓ Essential elements validation passed");
        
        // Use fluent API for complex interactions
        _mainWindow
            .ClickFileMenu()
            .ClickWindowMenu()
            .NavigateToPath("Documents/Projects")
            .ClickDashboard();
        
        _output.WriteLine("✓ Complex navigation completed using fluent API");
        
        // Get detailed status information
        var elementStatus = _mainWindow.GetDetailedElementStatus();
        var visibleCount = elementStatus.Values.Count(v => v);
        _output.WriteLine($"✓ Found {visibleCount}/{elementStatus.Count} visible elements");
    }

    [Fact]
    public void DemonstrateFluentElementActions()
    {
        _output.WriteLine("=== Demonstrating Fluent Element Actions ===");
        
        _mainWindow.WaitForPageLoad();
        
        // Use the fluent element actions API directly
        On.WaitForVisible("MainWindow")
          .Click("FileMenu")
          .Wait(500)
          .Type("NavigationTextBox", "TestPath/SubPath")
          .AssertVisible("NavigateButton")
          .Click("NavigateButton");
        
        _output.WriteLine("✓ Fluent element actions completed successfully");
        
        // Get navigation path and validate
        var currentPath = On.GetText("NavigationTextBox");
        Assert.Contains("TestPath", currentPath);
        _output.WriteLine($"✓ Navigation path validated: {currentPath}");
    }

    [Fact]
    public void DemonstrateElementSpecificFluentChains()
    {
        _output.WriteLine("=== Demonstrating Element-Specific Fluent Chains ===");
        
        _mainWindow.WaitForPageLoad();
        
        // Use element-specific fluent chains
        var navigationText = On.Element("NavigationTextBox")
            .WaitForVisible()
            .Type("Advanced/Test/Path", clearFirst: true)
            .GetText();
        
        Assert.Contains("Advanced", navigationText);
        _output.WriteLine($"✓ Element chain completed. Text: {navigationText}");
        
        // Demonstrate assertions within chains
        On.Element("MainWindow")
          .ShouldBeVisible("Main window should be visible")
          .ShouldContainText("Dock"); // Assuming main window has some text content
        
        _output.WriteLine("✓ Element assertions within chains completed");
    }

    [Fact]
    public void DemonstrateBulkElementValidation()
    {
        _output.WriteLine("=== Demonstrating Bulk Element Validation ===");
        
        _mainWindow.WaitForPageLoad();
        
        // Validate multiple elements at once
        var elementValidation = Elements.ValidateElements(
            "MainWindow", "FileMenu", "WindowMenu", "BackButton", 
            "ForwardButton", "DashboardButton", "NavigationTextBox", 
            "NavigateButton", "MainDockControl"
        );
        
        var foundCount = elementValidation.Values.Count(found => found);
        var totalCount = elementValidation.Count;
        
        _output.WriteLine($"Element Validation Results ({foundCount}/{totalCount} found):");
        foreach (var result in elementValidation)
        {
            var status = result.Value ? "✓" : "✗";
            _output.WriteLine($"  {status} {result.Key}");
        }
        
        // Assert that critical elements are found
        Assert.True(elementValidation["MainWindow"], "MainWindow should be found");
        Assert.True(elementValidation["MainDockControl"], "MainDockControl should be found");
        
        // Demonstrate visibility validation
        var visibilityResults = Elements.ValidateVisibility(
            "MainWindow", "FileMenu", "MainDockControl"
        );
        
        Assert.True(visibilityResults.Values.All(visible => visible), 
            "All critical elements should be visible");
        _output.WriteLine("✓ Visibility validation passed for all critical elements");
    }

    [Fact]
    public void DemonstrateAdvancedElementInformation()
    {
        _output.WriteLine("=== Demonstrating Advanced Element Information ===");
        
        _mainWindow.WaitForPageLoad();
        
        // Get detailed information about multiple elements
        var elementsInfo = Elements.GetElementsInfo(
            "MainWindow", "FileMenu", "NavigationTextBox", "MainDockControl"
        );
        
        _output.WriteLine("Detailed Element Information:");
        foreach (var info in elementsInfo)
        {
            var element = info.Value;
            var status = element.Exists ? "✓" : "✗";
            var visibility = element.IsVisible ? "Visible" : "Hidden";
            var enabled = element.IsEnabled ? "Enabled" : "Disabled";
            
            _output.WriteLine($"{status} {info.Key}:");
            _output.WriteLine($"    State: {visibility}, {enabled}");
            _output.WriteLine($"    Text: '{element.Text}'");
            _output.WriteLine($"    Tag: {element.TagName}");
        }
        
        // Assert based on detailed information
        Assert.True(elementsInfo["MainWindow"].Exists && elementsInfo["MainWindow"].IsVisible, 
            "MainWindow should exist and be visible");
    }

    [Fact]
    public void DemonstrateWaitingStrategies()
    {
        _output.WriteLine("=== Demonstrating Advanced Waiting Strategies ===");
        
        _mainWindow.WaitForPageLoad();
        
        // Wait for multiple elements to be available
        var waitResults = WaitForElements(10, 
            "MainWindow", "FileMenu", "MainDockControl", "NavigationTextBox");
        
        _output.WriteLine("Multi-element wait results:");
        foreach (var result in waitResults)
        {
            var status = result.Value ? "✓" : "✗";
            _output.WriteLine($"  {status} {result.Key}");
        }
        
        // Use custom condition waiting
        var navigationElement = Elements.WaitForCondition("NavigationTextBox", 
            element => element.Displayed && element.Enabled && !string.IsNullOrEmpty(element.GetAttribute("value")), 
            timeoutSeconds: 5);
        
        Assert.NotNull(navigationElement);
        _output.WriteLine("✓ Custom condition wait completed successfully");
        
        // Demonstrate the new application-specific wait
        var fullyLoaded = _mainWindow.WaitForApplicationToBeFullyLoaded();
        Assert.True(fullyLoaded, "Application should be fully loaded");
        _output.WriteLine("✓ Application fully loaded validation passed");
    }

    [Fact]
    public void DemonstrateErrorHandlingAndLogging()
    {
        _output.WriteLine("=== Demonstrating Error Handling and Logging ===");
        
        _mainWindow.WaitForPageLoad();
        
        // Try to find non-existent elements safely
        var foundNonExistent = Elements.TryFindByAccessibilityId("NonExistentElement", out var element);
        Assert.False(foundNonExistent);
        Assert.Null(element);
        _output.WriteLine("✓ Safe element finding handled missing element correctly");
        
        // Log current UI state for debugging
        _output.WriteLine("\nCurrent UI State:");
        LogUIState("MainWindow", "FileMenu", "NonExistentElement", "MainDockControl");
        
        // Use page object logging method
        _mainWindow.LogCurrentUIState();
        
        // Validate critical elements and log any issues
        var criticalElementsValid = _mainWindow.ValidateAllCriticalElements();
        Assert.True(criticalElementsValid, "All critical elements should be valid");
        _output.WriteLine("✓ Critical elements validation completed");
    }

    [Fact]
    public void DemonstrateComplexWorkflow()
    {
        _output.WriteLine("=== Demonstrating Complex Workflow with New Infrastructure ===");
        
        // Use the new page loading with validation
        _mainWindow.WaitForPageLoad();
        Assert.True(_mainWindow.ValidateAllCriticalElements(), "Critical elements must be available");
        
        // Perform complex navigation workflow
        var initialPath = _mainWindow.GetCurrentNavigationPath();
        _output.WriteLine($"Initial navigation path: '{initialPath}'");
        
        // Use the advanced navigation method
        _mainWindow.PerformComplexNavigation("TestWorkflow/ComplexPath");
        
        // Verify navigation occurred
        var newPath = _mainWindow.GetNavigationPathWithValidation();
        Assert.Contains("TestWorkflow", newPath);
        _output.WriteLine($"✓ Complex navigation completed. New path: '{newPath}'");
        
        // Navigate back using fluent API
        _mainWindow.NavigateBack();
        
        // Get tool and document information
        var documentTabs = _mainWindow.GetVisibleDocumentTabs();
        var toolWindows = _mainWindow.GetVisibleToolWindows();
        
        _output.WriteLine($"✓ Found {documentTabs.Count} document tabs and {toolWindows.Count} tool windows");
        
        // Take screenshot for documentation
        _mainWindow.TakeScreenshot("ComplexWorkflowCompleted");
        _output.WriteLine("✓ Screenshot taken for documentation");
    }

    [Fact]
    public void CompareOldVsNewApproach()
    {
        _output.WriteLine("=== Comparing Old vs New Approach ===");
        
        _mainWindow.WaitForPageLoad();
        
        // OLD WAY (what we used to do before refactoring)
        _output.WriteLine("OLD WAY - Using Elements helper directly:");
        var mainWindowOld = Elements.FindByAccessibilityIdWithWait("MainWindow", 10);
        var fileMenuOld = Elements.FindByAccessibilityIdWithWait("FileMenu", 10);
        Elements.Type("NavigationTextBox", "OldWayPath", true, 10);
        Elements.Click("NavigateButton", 10);
        var textOld = Elements.GetText("NavigationTextBox", 10);
        _output.WriteLine($"Direct Elements approach result: {textOld}");
        
        // NEW WAY - Clean fluent approach
        _output.WriteLine("NEW WAY - Clean fluent approach:");
        var result = On.Type("NavigationTextBox", "NewWayPath")
                       .Click("NavigateButton")
                       .GetText("NavigationTextBox");
        _output.WriteLine($"New way result: {result}");
        
        // Show the difference in readability and maintainability
        _output.WriteLine("✓ Fluent API approach is much cleaner and more maintainable");
        
        // Demonstrate advanced validation that wasn't easily possible before
        var detailedStatus = _mainWindow.GetDetailedElementStatus();
        var healthyElements = detailedStatus.Values.Count(v => v);
        _output.WriteLine($"✓ Advanced validation: {healthyElements}/{detailedStatus.Count} elements healthy");
    }
} 