using System.Linq;
using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class NativeWindowPresentationTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Preview_Allows_Focus_Transfers_And_Closes_When_Both_Windows_Lose_Activation(bool keepVisible)
    {
        bool previous = DockSettings.UsePinnedDockWindow;
        DockSettings.UsePinnedDockWindow = true;
        Window? owner = null;
        PinnedDockWindow? window = null;
        try
        {
            Factory factory = new();
            Tool tool = new() { KeepPinnedDockableVisible = keepVisible };
            tool.SetPinnedBounds(0, 0, 220, 190);
            RootDock root = new()
            {
                LeftPinnedDockables = factory.CreateList<IDockable>(tool),
                VisibleDockables = factory.CreateList<IDockable>()
            };
            factory.InitLayout(root);
            owner = new Window { Width = 800, Height = 600, Content = new PinnedDockControl { DataContext = root } };
            owner.Show();
            factory.PreviewPinnedDockable(tool);
            owner.UpdateLayout();
            window = Assert.Single(owner.OwnedWindows.OfType<PinnedDockWindow>());
            bool closed = false;
            window.Closed += (_, _) => closed = true;
            // Headless exposes deactivation through Hide. Temporarily unown the
            // preview so hiding the owner does not also hide the target window.
            window.Hide();
            owner.Hide();
            window.Show();
            window.Activate();
            Dispatcher.UIThread.RunJobs();
            Assert.True(window.IsVisible);
            Assert.True(window.IsActive);

            window.Hide();
            owner.Show();
            owner.Activate();
            Dispatcher.UIThread.RunJobs();
            Assert.False(closed);
            window.Show(owner);

            window.Hide();
            owner.Hide();
            Dispatcher.UIThread.RunJobs();
            owner.UpdateLayout();
            Assert.Equal(!keepVisible, closed);
            Assert.Equal(keepVisible, root.PinnedDock is not null);
        }
        finally { window?.Close(); owner?.Close(); DockSettings.UsePinnedDockWindow = previous; }
    }

    [AvaloniaTheory]
    [InlineData(BindingPriority.LocalValue)]
    [InlineData(BindingPriority.Style)]
    public void Resolving_The_Default_Owner_Preserves_The_Floating_Title_Binding(BindingPriority priority)
    {
        bool previous = DockSettings.UseOwnerForFloatingWindows;
        DockSettings.UseOwnerForFloatingWindows = true;
        Factory factory = new();
        DockControl control = new();
        factory.DockControls.Add(control);
        Window owner = new() { Title = "Owner title", Content = control, Width = 800, Height = 600 };
        HostWindow host = new() { Window = new DockWindow { Layout = new RootDock { Factory = factory } } };
        using WriteableBitmap bitmap = new(new PixelSize(1, 1), new Vector(96, 96));
        WindowIcon icon = new(bitmap);
        owner.Icon = icon;
        using BehaviorSubject<string> title = new("Child title");
        using BehaviorSubject<WindowIcon?> icons = new(null);
        using var binding = host.Bind(Window.TitleProperty, title, priority);
        using var iconBinding = host.Bind(Window.IconProperty, icons, priority);
        try
        {
            owner.Show();
            host.Present(false);
            Assert.Same(owner, host.Owner);
            Assert.Equal("Child title", host.Title);
            Assert.Null(host.Icon);
            icons.OnNext(icon);
            Assert.Same(icon, host.Icon);
            title.OnNext("Changed child title");
            Assert.Equal("Changed child title", host.Title);
        }
        finally { host.Close(); owner.Close(); DockSettings.UseOwnerForFloatingWindows = previous; }
    }
}
