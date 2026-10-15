using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Headless.XUnit;
using AvalonDock;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class SourceRootPlacementTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Tool_Source_Fallback_Publishes_The_Outer_Panel_With_The_Original_Already_Inside(bool initial)
    {
        DockingManager manager = new();
        LayoutRoot root = manager.Layout;
        LayoutPanel original = root.RootPanel;
        List<ILayoutPanelElement[]> children = new();
        root.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == "RootPanel") children.Add(root.RootPanel.Children.ToArray());
        };

        Import(manager, initial, new object());

        Assert.Equal(2, children.Count);
        Assert.IsType<LayoutDocumentPane>(Assert.Single(children[0]));
        Assert.Same(original, Assert.Single(children[1]));
        Assert.Same(original, root.RootPanel.Children[0]);
        Assert.IsType<LayoutAnchorablePane>(root.RootPanel.Children[1]);
        Assert.Same(root.RootPanel, original.Parent);
        Assert.Same(root.RootPanel.DockModel, original.DockModel.Owner);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Failing_Outer_Panel_Notification_Retains_The_Original_Subtree_Before_Tool_Insertion(bool initial)
    {
        DockingManager manager = new();
        LayoutRoot root = manager.Layout;
        LayoutPanel original = root.RootPanel;
        InvalidOperationException failure = new("outer panel observer");
        int changes = 0;
        root.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == "RootPanel" && ++changes == 2) throw failure;
        };

        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => Import(manager, initial, new object())));

        Assert.Same(original, Assert.Single(root.RootPanel.Children));
        Assert.Same(root.RootPanel, original.Parent);
        Assert.Empty(root.Descendents().OfType<LayoutAnchorable>());
        Assert.Same(root.RootPanel.DockModel, original.DockModel.Owner);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Redirection_While_Wrapping_The_Original_Panel_Uses_The_Current_Root_Only_For_Live_Imports(bool initial)
    {
        RedirectManager manager = new();
        LayoutRoot previous = manager.Layout;
        LayoutRoot redirected = new();
        LayoutPanel original = previous.RootPanel;
        bool changed = false;
        previous.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != "RootPanel" || changed) return;
            changed = true;
            manager.SuppressLayout = true;
            manager.Layout = redirected;
        };
        object content = new();

        Import(manager, initial, content);

        Assert.True(changed);
        Assert.Same(redirected, manager.Layout);
        LayoutRoot destination = initial ? previous : redirected;
        LayoutRoot untouched = initial ? redirected : previous;
        Assert.Same(original, destination.RootPanel.Children[0]);
        LayoutAnchorable imported = Assert.Single(destination.Descendents().OfType<LayoutAnchorable>());
        Assert.Same(content, imported.Content);
        Assert.Same(destination, imported.Root);
        Assert.Empty(untouched.Descendents().OfType<LayoutAnchorable>());
        Assert.Same(destination.RootPanel.DockModel, original.DockModel.Owner);
    }

    private static void Import(DockingManager manager, bool initial, object content)
    {
        ObservableCollection<object> source = new();
        if (initial)
        {
            source.Add(content);
            manager.AnchorablesSource = source;
        }
        else
        {
            manager.AnchorablesSource = source;
            source.Add(content);
        }
    }

    private sealed class RedirectManager : DockingManager
    {
        internal bool SuppressLayout { get; set; }
        protected override void OnLayoutChanged(LayoutRoot oldLayout, LayoutRoot newLayout)
        {
            if (!SuppressLayout) base.OnLayoutChanged(oldLayout, newLayout);
        }
    }
}
