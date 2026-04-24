using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class AdornerHelperIntegrationTests
{
    [AvaloniaFact]
    public void Floating_Adorner_Should_Create_Window_For_Shown_DockControl()
    {
        var hostWindow = new Window
        {
            Width = 400,
            Height = 300
        };
        var dockControl = new DockControl
        {
            Width = 240,
            Height = 180
        };
        var helper = new AdornerHelper<DockTarget>(true);

        hostWindow.Content = dockControl;
        hostWindow.Show();
        hostWindow.UpdateLayout();
        dockControl.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        try
        {
            helper.AddAdorner(dockControl, indicatorsOnly: false);

            var windowField = typeof(AdornerHelper<DockTarget>).GetField("_window", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(windowField);

            var adornerWindow = Assert.IsType<DockAdornerWindow>(windowField!.GetValue(helper));
            Assert.True(adornerWindow.IsVisible);
            Assert.Same(helper.Adorner, adornerWindow.Content);
        }
        finally
        {
            helper.RemoveAdorner(dockControl);
            hostWindow.Close();
        }
    }
}
