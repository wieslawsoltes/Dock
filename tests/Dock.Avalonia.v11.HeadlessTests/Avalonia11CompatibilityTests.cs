using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Diagnostics.Controls;
using Dock.Avalonia.Themes.Browser;
using Dock.Avalonia.Themes.Fluent;
using Dock.Avalonia.Themes.Simple;
using DockReactiveUISample.ViewModels;
using DockReactiveUISample.Views;
using Xunit;

namespace Dock.Avalonia.v11.HeadlessTests;

public sealed class Avalonia11CompatibilityTests
{
    [Fact]
    public void ReferencesAvalonia11()
    {
        Assert.Equal(11, typeof(AvaloniaObject).Assembly.GetName().Version?.Major);
    }

    [AvaloniaFact]
    public void V11ThemeResourcesLoad()
    {
        Assert.NotNull(new DockFluentTheme());
        Assert.NotNull(new DockSimpleTheme());
        Assert.NotNull(new BrowserTabTheme());
        Assert.NotNull(new DockDebugView());
    }

    [AvaloniaFact]
    public void HostWindowUsesAvalonia11TitleBarContract()
    {
        Application app = Application.Current ?? throw new InvalidOperationException("Application is not initialized.");
        IStyle[] originalStyles = app.Styles.ToArray();
        var window = new HostWindow();

        try
        {
            app.Styles.Clear();
            app.Styles.Add(new FluentTheme());
            app.Styles.Add(new DockFluentTheme());

            window.Show();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            HostWindowTitleBar? titleBar = window.GetVisualDescendants().OfType<HostWindowTitleBar>().FirstOrDefault();
            Assert.NotNull(titleBar);
            Assert.IsAssignableFrom<TitleBar>(titleBar);
            Assert.Equal(SystemDecorations.Full, window.SystemDecorations);
        }
        finally
        {
            window.Close();
            app.Styles.Clear();
            foreach (IStyle style in originalStyles)
            {
                app.Styles.Add(style);
            }
        }
    }

    [AvaloniaFact]
    public void ReactiveUISampleUsesAvalonia11XamlContract()
    {
        var viewModel = new MainWindowViewModel();

        try
        {
            var view = new MainView
            {
                DataContext = viewModel
            };

            TextBox? id = view.FindControl<TextBox>("Id");
            Assert.NotNull(id);
            Assert.Equal("Dashboard", id.Watermark);
            Assert.NotNull(viewModel.Layout);
        }
        finally
        {
            viewModel.CloseLayout();
        }
    }
}
