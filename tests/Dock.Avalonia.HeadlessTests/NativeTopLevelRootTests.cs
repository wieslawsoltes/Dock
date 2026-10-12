using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class NativeTopLevelRootTests
{
    [AvaloniaFact]
    public void Content_Resolves_Its_Owning_Window_Across_Detachment_And_Transfer()
    {
        Border content = new();
        Window first = new() { Content = content };
        Window second = new();
        try
        {
            first.Show();
            second.Show();
            Assert.Same(first, content.GetVisualRoot());
            Assert.Same(first, first.GetVisualRoot());
            first.Content = null;
            first.UpdateLayout();
            Assert.Null(content.GetVisualRoot());
            second.Content = content;
            second.UpdateLayout();
            Assert.Same(second, content.GetVisualRoot());
        }
        finally { second.Close(); first.Close(); }
    }

    [AvaloniaFact]
    public void Detached_And_Null_Visuals_Have_No_Root()
    {
        Assert.Null(new Border().GetVisualRoot());
        Assert.Null(VisualRootCompatExtensions.GetVisualRoot(null));
    }

    [AvaloniaTheory]
    [InlineData(Alignment.Left)]
    [InlineData(Alignment.Right)]
    [InlineData(Alignment.Top)]
    [InlineData(Alignment.Bottom)]
    public void Native_Pinned_Window_Uses_The_Owner_And_Tracks_The_Preview_Size(Alignment alignment)
    {
        bool previous = DockSettings.UsePinnedDockWindow;
        DockSettings.UsePinnedDockWindow = true;
        Window? owner = null;
        try
        {
            Factory factory = new();
            Tool tool = new();
            tool.SetPinnedBounds(0, 0, 220, 190);
            ToolDock preview = new()
            {
                Alignment = alignment, IsEmpty = false,
                VisibleDockables = factory.CreateList<IDockable>(tool), ActiveDockable = tool
            };
            RootDock root = new() { Factory = factory, PinnedDock = preview };
            PinnedDockControl control = new() { DataContext = root, PinnedDockAlignment = alignment };
            owner = new Window { Width = 800, Height = 600, Content = control };
            owner.Show();
            owner.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            PinnedDockWindow window = Assert.Single(owner.OwnedWindows.OfType<PinnedDockWindow>());
            Assert.True(window.IsVisible);
            Assert.Same(owner, window.Owner);
            ContentControl inline = control.GetVisualDescendants().OfType<ContentControl>().Single(child => child.Name == "PART_PinnedDock");
            Assert.Equal(0, inline.Opacity);
            Assert.False(inline.IsHitTestVisible);
            Assert.Equal(inline.Bounds.Width, window.Width, 1);
            Assert.Equal(inline.Bounds.Height, window.Height, 1);

            tool.SetPinnedBounds(0, 0, 300, 270);
            control.InvalidateMeasure();
            owner.UpdateLayout();
            Assert.Equal(inline.Bounds.Width, window.Width, 1);
            Assert.Equal(inline.Bounds.Height, window.Height, 1);
            Assert.Equal(alignment is Alignment.Left or Alignment.Right ? 300 : 270,
                alignment is Alignment.Left or Alignment.Right ? window.Width : window.Height, 1);

            root.PinnedDock = null;
            owner.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            Assert.Empty(owner.OwnedWindows);
            Assert.False(window.IsVisible);
            Assert.Equal(1, inline.Opacity);
            Assert.True(inline.IsHitTestVisible);
        }
        finally { owner?.Close(); DockSettings.UsePinnedDockWindow = previous; }
    }
}
