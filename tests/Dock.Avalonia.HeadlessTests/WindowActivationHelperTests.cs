using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model.Avalonia;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class WindowActivationHelperTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Bringing_Dock_Windows_Forward_Skips_Hidden_Windows(bool hiddenInitiator)
    {
        Factory factory = new();
        HostWindow hidden = new();
        Window visible = new();
        DockControl control = new();
        hidden.Content = control;
        factory.HostWindows.Add(hidden);
        factory.DockControls.Add(control);
        try
        {
            visible.Show();
            hidden.Show();
            hidden.Hide();
            Dispatcher.UIThread.RunJobs();
            int hiddenActivations = 0;
            hidden.Activated += (_, _) => hiddenActivations++;

            WindowActivationHelper.ActivateAllWindows(factory, hiddenInitiator ? hidden : visible);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(0, hiddenActivations);
            Assert.False(hidden.IsVisible);

            hidden.Show();
            Dispatcher.UIThread.RunJobs();
            hiddenActivations = 0;
            WindowActivationHelper.ActivateAllWindows(factory, hiddenInitiator ? hidden : visible);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, hiddenActivations);
        }
        finally
        {
            hidden.Close();
            visible.Close();
        }
    }
}
