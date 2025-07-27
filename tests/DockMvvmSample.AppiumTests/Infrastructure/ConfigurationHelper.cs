using System;
using System.IO;
using System.Runtime.InteropServices;
using DockMvvmSample.AppiumTests.Configuration;
using Microsoft.Extensions.Configuration;

namespace DockMvvmSample.AppiumTests.Infrastructure;

public static class ConfigurationHelper
{
    public static TestConfiguration LoadConfiguration()
    {
        var basePath = GetBasePath();
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        // Add platform-specific configuration
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            configBuilder.AddJsonFile("appsettings.windows.json", optional: true, reloadOnChange: true);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            configBuilder.AddJsonFile("appsettings.macos.json", optional: true, reloadOnChange: true);
        }

        var configuration = configBuilder.Build();
        var testConfig = new TestConfiguration();
        
        ConfigurationBinder.Bind(configuration, testConfig);
        
        // Convert relative paths to absolute paths
        if (!string.IsNullOrEmpty(testConfig.DockMvvmSample.ExecutablePath) && 
            !Path.IsPathRooted(testConfig.DockMvvmSample.ExecutablePath))
        {
            testConfig.DockMvvmSample.ExecutablePath = Path.GetFullPath(
                Path.Combine(basePath, testConfig.DockMvvmSample.ExecutablePath));
        }
        
        if (!string.IsNullOrEmpty(testConfig.DockMvvmSample.WorkingDirectory) && 
            !Path.IsPathRooted(testConfig.DockMvvmSample.WorkingDirectory))
        {
            testConfig.DockMvvmSample.WorkingDirectory = Path.GetFullPath(
                Path.Combine(basePath, testConfig.DockMvvmSample.WorkingDirectory));
        }

        return testConfig;
    }

    private static string GetBasePath()
    {
        // Return the project directory, not the bin directory
        // AppContext.BaseDirectory points to bin/Debug/net9.0/, but we want the project root
        var baseDir = AppContext.BaseDirectory;
        
        // Navigate up from bin/Debug/net9.0/ to the project root (3 levels up)
        var projectDir = Directory.GetParent(baseDir)?.Parent?.Parent?.Parent?.FullName;
        return projectDir ?? baseDir;
    }

    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
} 