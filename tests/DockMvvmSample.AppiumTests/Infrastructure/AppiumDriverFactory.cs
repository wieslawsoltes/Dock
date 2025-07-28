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

public static class AppiumDriverFactory
{
    public static IWebDriver CreateDriver(TestConfiguration config, out Process? applicationProcess)
    {
        applicationProcess = null;
        
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
            return CreateWindowsDriver(config, options, out applicationProcess);
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

    private static IWebDriver CreateWindowsDriver(TestConfiguration config, AppiumOptions options, out Process? applicationProcess)
    {
        applicationProcess = null;

        // Check if WinAppDriver should handle app lifecycle (when app capability is set to executable)
        if (options.ToDictionary().ContainsKey("appium:app") && 
            options.ToDictionary()["appium:app"].ToString() != "Root")
        {
            // Let WinAppDriver handle the application lifecycle
            // Convert relative path to absolute path for WinAppDriver
            var appPath = config.DockMvvmSample.ExecutablePath;
            if (!Path.IsPathRooted(appPath))
            {
                appPath = Path.GetFullPath(appPath);
            }

            if (!File.Exists(appPath))
            {
                throw new FileNotFoundException($"DockMvvmSample executable not found at: {appPath}");
            }

            // Update the app capability with absolute path
            options.AddAdditionalCapability("appium:app", appPath);
        }
        else
        {
            // Manual app lifecycle management (legacy approach)
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
            
            applicationProcess = Process.Start(startInfo);
            if (applicationProcess == null)
            {
                throw new InvalidOperationException("Failed to start the DockMvvmSample application");
            }
            
            // Wait for the app to start
            Thread.Sleep(2000);
        }

        // Create the Windows driver
        var serverUri = new Uri(config.AppiumSettings.ServerUrl);
        var driver = new WindowsDriver<WindowsElement>(serverUri, options);
        
        // Set implicit wait
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(config.AppiumSettings.ImplicitWait);
        
        return driver;
    }

    private static IWebDriver CreateMacDriver(TestConfiguration config, AppiumOptions options)
    {
        var appBundlePath = config.DockMvvmSample.ExecutablePath;
        
        // Validate app bundle exists
        if (!Directory.Exists(appBundlePath))
        {
            throw new DirectoryNotFoundException($"DockMvvmSample app bundle not found at: {appBundlePath}");
        }
        
        // Configure app bundle for Appium Mac2 driver - let Appium handle app lifecycle
        if (appBundlePath.EndsWith(".app"))
        {
            options.AddAdditionalCapability("appium:bundleId", "com.avaloniaui.dockmvvmsample");
            options.AddAdditionalCapability("appium:app", appBundlePath);
        }

        // Standard Mac2 automation capabilities for better stability
        options.AddAdditionalCapability("appium:shouldWaitForQuiescence", false);
        options.AddAdditionalCapability("appium:waitForQuiescence", false);
        
        // Let Appium handle app launching and management
        options.AddAdditionalCapability("appium:noReset", false);

        var serverUri = new Uri(config.AppiumSettings.ServerUrl);
        var driver = new MacDriver<AppiumWebElement>(serverUri, options);
        
        // Set implicit wait
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(config.AppiumSettings.ImplicitWait);
        
        return driver;
    }
} 