using System;
using System.Diagnostics;
using System.IO;
using DockMvvmSample.AppiumTests.Configuration;
using OpenQA.Selenium;

namespace DockMvvmSample.AppiumTests.Infrastructure;

public abstract class BaseTest : IDisposable
{
    protected IWebDriver Driver { get; private set; }
    protected TestConfiguration Configuration { get; private set; }
    protected Process? ApplicationProcess { get; set; }

    protected BaseTest()
    {
        Configuration = ConfigurationHelper.LoadConfiguration();
        Driver = AppiumDriverFactory.CreateDriver(Configuration, out var appProcess);
        ApplicationProcess = appProcess;
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
            // Quit the Appium driver first
            Driver?.Quit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error disposing driver: {ex.Message}");
        }

        // Kill the application process if it's still running (Windows only)
        try
        {
            if (ApplicationProcess != null && !ApplicationProcess.HasExited)
            {
                Console.WriteLine($"Killing application process with PID: {ApplicationProcess.Id}");
                ApplicationProcess.Kill();
                ApplicationProcess.WaitForExit(5000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error killing application process: {ex.Message}");
        }
        finally
        {
            ApplicationProcess?.Dispose();
        }
    }
} 