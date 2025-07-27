using System;
using System.IO;
using DockMvvmSample.AppiumTests.Configuration;
using OpenQA.Selenium;

namespace DockMvvmSample.AppiumTests.Infrastructure;

public abstract class BaseTest : IDisposable
{
    protected IWebDriver Driver { get; private set; }
    protected TestConfiguration Configuration { get; private set; }

    protected BaseTest()
    {
        Configuration = ConfigurationHelper.LoadConfiguration();
        Driver = AppiumDriverFactory.CreateDriver(Configuration);
    }

    protected void TakeScreenshot(string testName)
    {
        if (!Configuration.TestSettings.ScreenshotOnFailure)
            return;

        try
        {
            var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
            var screenshotPath = Path.Combine(
                AppContext.BaseDirectory, 
                "Screenshots", 
                $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
            
            Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath)!);
            screenshot.SaveAsFile(screenshotPath);
        }
        catch (Exception ex)
        {
            // Don't fail the test because of screenshot issues
            Console.WriteLine($"Failed to take screenshot: {ex.Message}");
        }
    }

    public virtual void Dispose()
    {
        try
        {
            // Appium handles app lifecycle management properly now
            Driver?.Quit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error disposing driver: {ex.Message}");
        }
    }
} 