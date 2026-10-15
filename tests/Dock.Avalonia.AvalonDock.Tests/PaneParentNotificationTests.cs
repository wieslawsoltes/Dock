using System.Collections.Generic;
using Avalonia.Headless.XUnit;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class PaneParentNotificationTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Tool_Parent_Changes_Notify_Direct_Hosting_Without_An_Extra_Hosted_Property_Event(bool native)
    {
        LayoutAnchorablePane pane = new(new LayoutAnchorable());
        LayoutAnchorablePaneGroup previous = new(pane);
        LayoutAnchorablePaneGroup next = new();
        LayoutAnchorableFloatingWindow floating = new() { RootPanel = next };
        List<string> notifications = new();
        pane.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is "IsHostedInFloatingWindow" or "IsDirectlyHostedInFloatingWindow" or "Parent")
                notifications.Add(args.PropertyName);
        };

        if (native) pane.DockModel.Owner = next.DockModel;
        else pane.Parent = next;

        Assert.Equal(new[] { "IsDirectlyHostedInFloatingWindow", "Parent" }, notifications);
        Assert.True(pane.IsHostedInFloatingWindow);
        Assert.False(pane.IsDirectlyHostedInFloatingWindow);
        notifications.Clear();
        previous.Children.Add(new LayoutAnchorablePane());
        Assert.Empty(notifications);
        next.Children.Add(new LayoutAnchorablePane());
        Assert.Equal(new[] { "IsDirectlyHostedInFloatingWindow" }, notifications);
        Assert.Same(floating, pane.FindParent<LayoutAnchorableFloatingWindow>());
    }
}
