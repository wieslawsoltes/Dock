using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using DockMvvmSample.AppiumTests.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Appium.Windows;

namespace DockMvvmSample.AppiumTests.Infrastructure;

public record DriverResult(IWebDriver Driver, Process? AppProcess);

public static class AppiumDriverFactory
{
    public static DriverResult CreateDriver(TestConfiguration config)
    {
        var options = new AppiumOptions();
        
        // Set standard W3C capabilities
        if (config.AppiumSettings.DesiredCapabilities.ContainsKey("platformName"))
        {
            options.AddAdditionalCapability("platformName", config.AppiumSettings.DesiredCapabilities["platformName"]);
        }
        
        if (config.AppiumSettings.DesiredCapabilities.ContainsKey("automationName"))
        {
            options.AddAdditionalCapability("appium:automationName", config.AppiumSettings.DesiredCapabilities["automationName"]);
        }

        // Add other capabilities with appium: prefix for W3C compliance
        foreach (var capability in config.AppiumSettings.DesiredCapabilities)
        {
            if (capability.Key != "platformName" && capability.Key != "automationName")
            {
                options.AddAdditionalCapability($"appium:{capability.Key}", capability.Value);
            }
        }

        // Set timeouts
        options.AddAdditionalCapability("appium:newCommandTimeout", config.AppiumSettings.CommandTimeout);

        if (ConfigurationHelper.IsWindows)
        {
            return CreateWindowsDriver(config, options);
        }
        else if (ConfigurationHelper.IsMacOS)
        {
            return CreateMacDriver(config, options);
        }
        else
        {
            throw new PlatformNotSupportedException("This platform is not supported for Appium testing.");
        }
    }

    public static DriverResult CreateSystemWideMacDriver(TestConfiguration config)
    {
        var options = new AppiumOptions();
        
        // Set standard W3C capabilities
        options.AddAdditionalCapability("platformName", "Mac");
        options.AddAdditionalCapability("appium:automationName", "Mac2");
        
        // System-wide capabilities - don't launch any specific app
        options.AddAdditionalCapability("appium:systemWide", true);
        options.AddAdditionalCapability("appium:systemPort", 8200);
        options.AddAdditionalCapability("appium:skipLogCapture", true);
        options.AddAdditionalCapability("appium:shouldWaitForQuiescence", false);
        options.AddAdditionalCapability("appium:waitForQuiescence", false);
        options.AddAdditionalCapability("appium:launchTimeout", 30000);
        options.AddAdditionalCapability("appium:sessionStartupTimeout", 30000);
        options.AddAdditionalCapability("appium:newCommandTimeout", config.AppiumSettings.CommandTimeout);

        var serverUri = new Uri(config.AppiumSettings.ServerUrl);
        var driver = new MacDriver<AppiumWebElement>(serverUri, options);
        
        // Set implicit wait
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(config.AppiumSettings.ImplicitWait);
        
        return new DriverResult(driver, null);
    }

    private static DriverResult CreateWindowsDriver(TestConfiguration config, AppiumOptions options)
    {
        // Start the DockMvvmSample application
        var appProcess = StartApplication(config);
        
        // Wait a moment for the app to start
        Thread.Sleep(2000);

        // Create the Windows driver
        var serverUri = new Uri(config.AppiumSettings.ServerUrl);
        var driver = new WindowsDriver<WindowsElement>(serverUri, options);
        
        // Set implicit wait
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(config.AppiumSettings.ImplicitWait);
        
        return new DriverResult(driver, appProcess);
    }

    private static DriverResult CreateMacDriver(TestConfiguration config, AppiumOptions options)
    {
        // For macOS with Mac2 automation, launch the app externally first
        var appProcess = StartApplication(config);
        
        // Wait for the app to start and become ready - Avalonia apps need more time on macOS
        Thread.Sleep(5000);

        // Add capabilities for Mac2 automation
        options.AddAdditionalCapability("appium:shouldWaitForQuiescence", false);
        options.AddAdditionalCapability("appium:waitForQuiescence", false);
        options.AddAdditionalCapability("appium:launchTimeout", 30000);
        options.AddAdditionalCapability("appium:sessionStartupTimeout", 30000);
        
        // For Mac2, we can try to connect to system-wide accessibility
        // Instead of specific bundle, try to access all running applications
        options.AddAdditionalCapability("appium:systemWide", true);

        var serverUri = new Uri(config.AppiumSettings.ServerUrl);
        var driver = new MacDriver<AppiumWebElement>(serverUri, options);
        
        // Set implicit wait
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(config.AppiumSettings.ImplicitWait);
        
        return new DriverResult(driver, appProcess);
    }

    private static Process StartApplication(TestConfiguration config)
    {
        if (!File.Exists(config.DockMvvmSample.ExecutablePath))
        {
            throw new FileNotFoundException($"DockMvvmSample executable not found at: {config.DockMvvmSample.ExecutablePath}");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = config.DockMvvmSample.ExecutablePath,
            WorkingDirectory = config.DockMvvmSample.WorkingDirectory,
            UseShellExecute = false,
            CreateNoWindow = false
        };

        // On macOS, set environment variables for GUI applications
        if (ConfigurationHelper.IsMacOS)
        {
            startInfo.EnvironmentVariables["DISPLAY"] = ":0";
            startInfo.EnvironmentVariables["NSHighResolutionCapable"] = "YES";
            // Enable accessibility for Avalonia applications
            startInfo.EnvironmentVariables["AVALONIA_OSX_ENABLE_ACCESSIBILITY"] = "1";
            startInfo.EnvironmentVariables["NSApplication"] = "1";
            // Wait for app to fully initialize
        }

        var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start the DockMvvmSample application");
        }
        
        return process;
    }
} 