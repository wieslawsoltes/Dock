using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DockMvvmSample.AppiumTests.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.UI;

namespace DockMvvmSample.AppiumTests.Infrastructure;

public abstract class BaseTest : IDisposable
{
    protected IWebDriver Driver { get; private set; }
    protected TestConfiguration Configuration { get; private set; }
    protected Process? ApplicationProcess { get; set; }
    
    /// <summary>
    /// Unified element helper for all element interactions
    /// </summary>
    protected ElementHelper Elements { get; private set; }

    protected BaseTest()
    {
        Configuration = ConfigurationHelper.LoadConfiguration();
        Driver = AppiumDriverFactory.CreateDriver(Configuration, out var appProcess);
        ApplicationProcess = appProcess;
        Elements = new ElementHelper(Driver);
    }

    /// <summary>
    /// Fluent API for element interactions
    /// </summary>
    protected FluentElementActions On => new FluentElementActions(Elements);

    /// <summary>
    /// Waits for multiple elements to be available
    /// </summary>
    protected Dictionary<string, bool> WaitForElements(int timeoutSeconds = 10, params string[] accessibilityIds)
    {
        var results = new Dictionary<string, bool>();
        var startTime = DateTime.Now;
        var maxWaitTime = TimeSpan.FromSeconds(timeoutSeconds);
        
        foreach (var id in accessibilityIds)
        {
            var remainingTime = maxWaitTime - (DateTime.Now - startTime);
            if (remainingTime.TotalSeconds <= 0)
            {
                results[id] = false;
                continue;
            }
            
            try
            {
                Elements.FindByAccessibilityIdWithWait(id, (int)remainingTime.TotalSeconds);
                results[id] = true;
            }
            catch (WebDriverTimeoutException)
            {
                results[id] = false;
            }
        }
        
        return results;
    }

    /// <summary>
    /// Gets detailed information about the current UI state for debugging
    /// </summary>
    protected void LogUIState(params string[] elementIds)
    {
        var info = Elements.GetElementsInfo(elementIds);
        
        Console.WriteLine("=== UI State Debug Information ===");
        foreach (var item in info)
        {
            var status = item.Value.Exists ? "✓" : "✗";
            var visibility = item.Value.IsVisible ? "Visible" : "Hidden";
            var enabled = item.Value.IsEnabled ? "Enabled" : "Disabled";
            
            Console.WriteLine($"{status} {item.Key}: {visibility}, {enabled}, Text: '{item.Value.Text}'");
        }
        Console.WriteLine("=================================");
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