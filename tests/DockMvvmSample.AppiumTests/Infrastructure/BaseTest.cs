using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DockMvvmSample.AppiumTests.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using Xunit;

namespace DockMvvmSample.AppiumTests.Infrastructure;

public abstract class BaseTest : IDisposable
{
    protected IWebDriver Driver { get; private set; }
    protected TestConfiguration Configuration { get; private set; }
    private Process? _appProcess;

    protected BaseTest()
    {
        Configuration = ConfigurationHelper.LoadConfiguration();
        var result = AppiumDriverFactory.CreateDriver(Configuration);
        Driver = result.Driver;
        _appProcess = result.AppProcess;
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
            Driver?.Quit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error disposing driver: {ex.Message}");
        }

        // Terminate the app process if it's still running
        if (_appProcess != null && !_appProcess.HasExited)
        {
            try
            {
                // On macOS, try to gracefully terminate first
                if (ConfigurationHelper.IsMacOS)
                {
                    // Try to find and terminate DockMvvmSample processes
                    var dockProcesses = Process.GetProcessesByName("DockMvvmSample");
                    foreach (var process in dockProcesses)
                    {
                        try
                        {
                            if (!process.HasExited)
                            {
                                process.Kill();
                                process.WaitForExit(5000);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error killing DockMvvmSample process {process.Id}: {ex.Message}");
                        }
                    }
                }
                else
                {
                    _appProcess.Kill();
                    _appProcess.WaitForExit(5000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error terminating app process: {ex.Message}");
            }
        }
    }
} 