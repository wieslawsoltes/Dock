using OpenQA.Selenium.Appium.Windows;
using Xunit;

namespace DockMvvmSample.AppiumTests;

public class MainWindowTests : IClassFixture<AppFixture>
{
    private readonly WindowsDriver _session;

    public MainWindowTests(AppFixture fixture)
    {
        _session = fixture.Session;
    }

    [Fact(Skip = "Requires Appium server")]
    public void MainWindow_Title_Is_Correct()
    {
        Assert.Equal("Dock Avalonia Demo", _session.Title);
    }
}

