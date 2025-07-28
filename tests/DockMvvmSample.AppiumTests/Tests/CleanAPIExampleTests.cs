using System.Linq;
using System.Threading;
using DockMvvmSample.AppiumTests.Infrastructure;
using DockMvvmSample.AppiumTests.PageObjects;
using OpenQA.Selenium;
using Xunit;
using Xunit.Abstractions;

namespace DockMvvmSample.AppiumTests.Tests;

[Collection("AppiumTests")]
public class CleanAPIExampleTests : BaseTest
{
    private readonly ITestOutputHelper _output;

    public CleanAPIExampleTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void DemonstrateBefore_VerboseDriverCasting()
    {
        _output.WriteLine("=== BEFORE: Verbose Driver Casting Approach ===");
        
        Thread.Sleep(3000);
        
        // OLD WAY - Verbose and repetitive
        var mainWindow = (Driver as OpenQA.Selenium.Appium.AppiumDriver<OpenQA.Selenium.Appium.AppiumWebElement>)?.FindElementByAccessibilityId("MainWindow") 
                        ?? Driver.FindElement(By.Id("MainWindow"));
        
        var fileMenu = (Driver as OpenQA.Selenium.Appium.AppiumDriver<OpenQA.Selenium.Appium.AppiumWebElement>)?.FindElementByAccessibilityId("FileMenu") 
                      ?? Driver.FindElement(By.Id("FileMenu"));
        
        // Complex interaction - lots of casting
        var navigationTextBox = (Driver as OpenQA.Selenium.Appium.AppiumDriver<OpenQA.Selenium.Appium.AppiumWebElement>)?.FindElementByAccessibilityId("NavigationTextBox") 
                               ?? Driver.FindElement(By.Id("NavigationTextBox"));
        navigationTextBox.Clear();
        navigationTextBox.SendKeys("Documents/Projects");
        
        var navigateButton = (Driver as OpenQA.Selenium.Appium.AppiumDriver<OpenQA.Selenium.Appium.AppiumWebElement>)?.FindElementByAccessibilityId("NavigateButton") 
                           ?? Driver.FindElement(By.Id("NavigateButton"));
        // navigateButton.Click(); // Commented out to avoid actual navigation
        
        Assert.NotNull(mainWindow);
        Assert.NotNull(fileMenu);
        Assert.NotNull(navigationTextBox);
        Assert.NotNull(navigateButton);
        
        _output.WriteLine("✓ Old approach works but is verbose and repetitive");
    }

    [Fact]
    public void DemonstrateAfter_CleanHelperMethods()
    {
        _output.WriteLine("=== AFTER: Clean Helper Methods Approach ===");
        
        Thread.Sleep(3000);
        
        // NEW WAY - Clean and simple
        var mainWindow = FindElementByAccessibilityId("MainWindow");
        var fileMenu = FindElementByAccessibilityId("FileMenu");
        
        // Complex interaction - much cleaner
        TypeInElement("NavigationTextBox", "Documents/Projects");
        var navigateButton = WaitForElementToBeClickable("NavigateButton");
        // navigateButton.Click(); // Commented out to avoid actual navigation
        
        // Get the entered text to verify
        var enteredText = GetElementText("NavigationTextBox");
        
        Assert.NotNull(mainWindow);
        Assert.NotNull(fileMenu);
        Assert.NotNull(navigateButton);
        Assert.Contains("Documents/Projects", enteredText);
        
        _output.WriteLine("✓ New approach is much cleaner and more readable");
        _output.WriteLine($"✓ Entered text verified: {enteredText}");
    }

    [Fact]
    public void DemonstratePageObjectWithCleanAPI()
    {
        _output.WriteLine("=== Using Clean API in Page Objects ===");
        
        Thread.Sleep(3000);
        
        var mainWindowPage = new MainWindowPage(Driver);
        
        // Page object now has clean, expressive methods
        Assert.True(mainWindowPage.VerifyMainElementsAccessible());
        _output.WriteLine("✓ All main elements are accessible");
        
        // Clean high-level actions
        mainWindowPage.ClickFileMenu();
        Thread.Sleep(500);
        _output.WriteLine("✓ File menu clicked using clean API");
        
        mainWindowPage.NavigateToPath("TestFolder/SubFolder");
        var currentPath = mainWindowPage.GetCurrentNavigationPath();
        _output.WriteLine($"✓ Navigation path set and retrieved: {currentPath}");
        
        // Easy element access
        var mainWindow = mainWindowPage.MainWindow;
        var dockControl = mainWindowPage.MainDockControl;
        
        Assert.NotNull(mainWindow);
        Assert.NotNull(dockControl);
        _output.WriteLine("✓ Page object elements accessed cleanly");
    }

    [Fact]
    public void DemonstrateMultipleElementValidation()
    {
        _output.WriteLine("=== Validating Multiple Elements at Once ===");
        
        Thread.Sleep(3000);
        
        // Validate multiple elements in one clean call
        var results = ValidateAccessibilityIds(
            "MainWindow", "FileMenu", "MainDockControl", 
            "BackButton", "ForwardButton", "DashboardButton",
            "NavigationTextBox", "NavigateButton"
        );
        
        var foundCount = results.Values.Count(found => found);
        var totalCount = results.Count;
        
        _output.WriteLine($"✓ Found {foundCount}/{totalCount} elements using clean validation");
        
        foreach (var result in results)
        {
            var status = result.Value ? "✓" : "✗";
            _output.WriteLine($"  {status} {result.Key}: {(result.Value ? "Found" : "Not Found")}");
        }
        
        // Assert that critical elements are found
        Assert.True(results["MainWindow"]);
        Assert.True(results["FileMenu"]);
        Assert.True(results["MainDockControl"]);
    }

    [Fact]
    public void DemonstrateErrorHandling()
    {
        _output.WriteLine("=== Clean Error Handling ===");
        
        Thread.Sleep(3000);
        
        // Try to find an element that doesn't exist
        bool found = TryFindElementByAccessibilityId("NonExistentElement", out var element);
        Assert.False(found);
        Assert.Null(element);
        _output.WriteLine("✓ TryFind method handles missing elements gracefully");
        
        // Validate mixed existing/non-existing elements
        var results = ValidateAccessibilityIds("MainWindow", "NonExistentElement", "FileMenu");
        Assert.True(results["MainWindow"]);
        Assert.False(results["NonExistentElement"]);
        Assert.True(results["FileMenu"]);
        _output.WriteLine("✓ Validation handles mixed results correctly");
        
        // Test timeout handling with a reasonable timeout
        var startTime = System.DateTime.Now;
        try
        {
            FindElementByAccessibilityIdWithWait("NonExistentElement", 2); // 2 second timeout
            Assert.Fail("Should have thrown an exception");
        }
        catch (OpenQA.Selenium.WebDriverTimeoutException)
        {
            var elapsed = System.DateTime.Now - startTime;
            _output.WriteLine($"✓ Timeout handled correctly after {elapsed.TotalSeconds:F1} seconds");
            Assert.True(elapsed.TotalSeconds >= 1.5 && elapsed.TotalSeconds <= 3.0);
        }
    }
} 