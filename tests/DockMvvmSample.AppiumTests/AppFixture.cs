using System;
using System.IO;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DockMvvmSample.AppiumTests;

public sealed class AppFixture : IDisposable
{
    public WindowsDriver Session { get; }

    public AppFixture()
    {
        var appPath = Path.GetFullPath(Path.Combine("..", "..", "samples", "DockMvvmSample", "bin", "Debug", "net9.0", "DockMvvmSample.exe"));
        var options = new AppiumOptions();
        options.App = appPath;
        options.DeviceName = "WindowsPC";

        Session = new WindowsDriver(new Uri("http://127.0.0.1:4723"), options);
    }

    public void Dispose()
    {
        Session?.Quit();
    }
}

