using System;
using System.IO;
using System.Net.Sockets;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DockMvvmSample.AppiumTests;

public sealed class AppFixture : IDisposable
{
    public bool IsServerRunning { get; }
    public WindowsDriver? Session { get; }

    public AppFixture()
    {
        IsServerRunning = CheckServer();
        if (!IsServerRunning)
        {
            return;
        }

        var appPath = Path.GetFullPath(Path.Combine("..", "..", "samples", "DockMvvmSample", "bin", "Debug", "net9.0", "DockMvvmSample.exe"));
        var options = new AppiumOptions
        {
            App = appPath,
            DeviceName = "WindowsPC"
        };

        Session = new WindowsDriver(new Uri("http://127.0.0.1:4723"), options);
    }

    private static bool CheckServer()
    {
        try
        {
            using var client = new TcpClient();
            var task = client.ConnectAsync("127.0.0.1", 4723);
            task.Wait(TimeSpan.FromSeconds(1));
            return client.Connected;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        Session?.Quit();
    }
}

