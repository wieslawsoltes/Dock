using System.Collections.Generic;

namespace DockMvvmSample.AppiumTests.Configuration;

public class AppiumSettings
{
    public string ServerUrl { get; set; } = "http://127.0.0.1:4723";
    public int CommandTimeout { get; set; } = 60;
    public int ImplicitWait { get; set; } = 10;
    public Dictionary<string, object> DesiredCapabilities { get; set; } = new();
}

public class TestSettings
{
    public int TestTimeout { get; set; } = 30;
    public bool ScreenshotOnFailure { get; set; } = true;
    public int RetryCount { get; set; } = 1;
}

public class DockMvvmSampleSettings
{
    public string ExecutablePath { get; set; } = string.Empty;
    public string WorkingDirectory { get; set; } = string.Empty;
    public string WindowTitle { get; set; } = "Dock Avalonia Demo";
}

public class TestConfiguration
{
    public AppiumSettings AppiumSettings { get; set; } = new();
    public TestSettings TestSettings { get; set; } = new();
    public DockMvvmSampleSettings DockMvvmSample { get; set; } = new();
} 