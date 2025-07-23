using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Appium.Windows;
using Xunit;

namespace DockMvvmSample.AppiumTests;

public class MainWindowTests : IClassFixture<AppFixture>
{
    private readonly AppFixture _fixture;
    private readonly WindowsDriver? _session;

    public MainWindowTests(AppFixture fixture)
    {
        _fixture = fixture;
        _session = fixture.Session;
    }

    [Fact]
    public void MainWindow_Title_Is_Correct()
    {
        if (!_fixture.IsServerRunning || _session is null)
        {
            return;
        }

        Assert.Equal("Dock Avalonia Demo", _session.Title);
    }

    [Fact]
    public void Can_Drag_Dockable()
    {
        if (!_fixture.IsServerRunning || _session is null)
        {
            return;
        }

        var source = _session.FindElement(By.Name("Tool1"));
        var target = _session.FindElement(By.Name("Tool2"));
        new Actions(_session).DragAndDrop(source, target).Perform();
    }

    [Fact]
    public void Can_Float_Tool_Window()
    {
        if (!_fixture.IsServerRunning || _session is null)
        {
            return;
        }

        var tool = _session.FindElement(By.Name("Tool1"));
        new Actions(_session).DoubleClick(tool).Perform();
    }

    [Fact]
    public void Can_Dock_Floating_Window_Back()
    {
        if (!_fixture.IsServerRunning || _session is null)
        {
            return;
        }

        var tool = _session.FindElement(By.Name("Tool1"));
        var dock = _session.FindElement(By.Id("DockControl"));
        new Actions(_session).DragAndDrop(tool, dock).Perform();
    }

    [Fact]
    public void Can_Move_Splitter()
    {
        if (!_fixture.IsServerRunning || _session is null)
        {
            return;
        }

        var splitter = _session.FindElement(By.ClassName("GridSplitter"));
        new Actions(_session).ClickAndHold(splitter).MoveByOffset(50, 0).Release().Perform();
    }

    [Fact]
    public void Can_Close_Dockable()
    {
        if (!_fixture.IsServerRunning || _session is null)
        {
            return;
        }

        var close = _session.FindElement(By.Name("Close"));
        close.Click();
    }
}

