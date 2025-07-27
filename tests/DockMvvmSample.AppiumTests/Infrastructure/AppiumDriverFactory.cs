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
        // Launch the app bundle using standard macOS approach
        var appProcess = StartApplication(config);
        
        // Standard wait for app initialization
        Thread.Sleep(3000);

        // Set app bundle path for proper app targeting
        var appBundlePath = config.DockMvvmSample.ExecutablePath;
        if (appBundlePath.EndsWith(".app"))
        {
            options.AddAdditionalCapability("appium:bundleId", "com.avaloniaui.dockmvvmsample");
            options.AddAdditionalCapability("appium:app", appBundlePath);
        }

        // Standard Mac2 automation capabilities
        options.AddAdditionalCapability("appium:shouldWaitForQuiescence", false);
        options.AddAdditionalCapability("appium:waitForQuiescence", false);

        var serverUri = new Uri(config.AppiumSettings.ServerUrl);
        var driver = new MacDriver<AppiumWebElement>(serverUri, options);
        
        // Set implicit wait
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(config.AppiumSettings.ImplicitWait);
        
        return new DriverResult(driver, appProcess);
    }

    private static Process StartApplication(TestConfiguration config)
    {
        var executablePath = config.DockMvvmSample.ExecutablePath;
        
        // Handle macOS app bundles
        if (executablePath.EndsWith(".app"))
        {
            if (!Directory.Exists(executablePath))
            {
                throw new DirectoryNotFoundException($"DockMvvmSample app bundle not found at: {executablePath}");
            }
            
            // Use 'open' command to launch macOS app bundle
            var startInfo = new ProcessStartInfo
            {
                FileName = "open",
                Arguments = $"\"{executablePath}\"",
                WorkingDirectory = config.DockMvvmSample.WorkingDirectory,
                UseShellExecute = false,
                CreateNoWindow = false
            };
            
            SetMacOSEnvironment(startInfo);
            var process = Process.Start(startInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start the DockMvvmSample application");
            }
            return process;
        }
        else
        {
            // Regular executable file
            if (!File.Exists(executablePath))
            {
                throw new FileNotFoundException($"DockMvvmSample executable not found at: {executablePath}");
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                WorkingDirectory = config.DockMvvmSample.WorkingDirectory,
                UseShellExecute = false,
                CreateNoWindow = false
            };
            
            SetMacOSEnvironment(startInfo);
            var process = Process.Start(startInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start the DockMvvmSample application");
            }
            return process;
        }
    }

    private static void SetMacOSEnvironment(ProcessStartInfo startInfo)
    {
        // On macOS, set environment variables for GUI applications
        if (ConfigurationHelper.IsMacOS)
        {
            startInfo.EnvironmentVariables["DISPLAY"] = ":0";
            startInfo.EnvironmentVariables["NSHighResolutionCapable"] = "YES";
            // Enable accessibility for Avalonia applications
            startInfo.EnvironmentVariables["AVALONIA_OSX_ENABLE_ACCESSIBILITY"] = "1";
            startInfo.EnvironmentVariables["NSApplication"] = "1";
        }
    }
} 