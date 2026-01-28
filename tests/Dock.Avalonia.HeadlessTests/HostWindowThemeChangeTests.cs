using System;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class HostWindowThemeChangeTests
{
    private static IRootDock CreateLayout(Factory factory)
    {
        var root = factory.CreateRootDock();
        root.Factory = factory;
        root.VisibleDockables = factory.CreateList<IDockable>();
        return root;
    }

    [AvaloniaFact]
    public void HostWindow_ThemeChange_DoesNotDuplicate_DockControls()
    {
        var app = Application.Current ?? throw new InvalidOperationException("Application is not initialized.");
        var factory = new Factory();
        var layout = CreateLayout(factory);
        var window = new HostWindow();
        var originalVariant = app.RequestedThemeVariant;

        try
        {
            window.SetLayout(layout);
            window.Show();
            window.UpdateLayout();

            Assert.Single(factory.DockControls);
            Assert.Same(layout, window.Content);

            app.RequestedThemeVariant = ThemeVariant.Dark;
            window.UpdateLayout();
            Assert.Single(factory.DockControls);

            app.RequestedThemeVariant = ThemeVariant.Light;
            window.UpdateLayout();
            Assert.Single(factory.DockControls);
        }
        finally
        {
            window.Close();
            app.RequestedThemeVariant = originalVariant;
        }
    }
}
