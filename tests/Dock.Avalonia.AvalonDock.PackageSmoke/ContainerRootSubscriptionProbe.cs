using System;
using Avalonia;
using Avalonia.Styling;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class ContainerRootSubscriptionProbe
{
    internal static void Verify()
    {
        LayoutRoot oldRoot = Root("old");
        Manager manager = new() { Layout = oldRoot };
        Selector selector = new();
        manager.LayoutItemContainerStyleSelector = selector;
        manager.Replay();
        manager.Replay();
        selector.Calls = 0;
        int notifications = AddEmptyPane(oldRoot);
        if (selector.Calls != notifications * 4)
            throw new Exception("Packaged style callbacks did not preserve independent root registrations.");

        manager.Layout = Root("current");
        selector.Calls = 0;
        notifications = AddEmptyPane(oldRoot);
        if (selector.Calls != notifications * 4 || selector.LastModel as string != "current")
            throw new Exception("Packaged old-root notifications did not refresh the current layout.");
        LayoutDocument foreign = new() { Content = "foreign" };
        oldRoot.RootPanel.Children.Add(new LayoutDocumentPane(foreign));
        if (manager.GetLayoutItemFromModel(foreign) is not null)
            throw new Exception("A packaged old-root notification imported foreign content.");

        manager.Layout = oldRoot;
        // Two contents now cause two complete passes through both containers.
        selector.Calls = 0;
        notifications = AddEmptyPane(oldRoot);
        if (selector.Calls != notifications * 4 * 4)
            throw new Exception("Packaged root replacement did not detach one registration before reattaching.");
    }

    private static LayoutRoot Root(string content)
        => new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(new LayoutDocument { Content = content })) };

    private static int AddEmptyPane(LayoutRoot root)
    {
        int count = 0;
        void Added(object? sender, LayoutElementEventArgs args) => count++;
        root.ElementAdded += Added;
        try { root.RootPanel.Children.Add(new LayoutAnchorablePane()); }
        finally { root.ElementAdded -= Added; }
        if (count == 0) throw new Exception("The packaged root did not publish its native addition.");
        return count;
    }

    private sealed class Selector : StyleSelector
    {
        internal int Calls { get; set; }
        internal object? LastModel { get; private set; }
        public override ControlTheme? SelectStyle(object? item, AvaloniaObject container)
        {
            Calls++;
            LastModel = item;
            return null;
        }
    }

    private sealed class Manager : DockingManager
    {
        private AvaloniaPropertyChangedEventArgs? _change;
        internal void Replay() => base.OnLayoutItemContainerStyleSelectorChanged(_change!);
        protected override void OnLayoutItemContainerStyleSelectorChanged(AvaloniaPropertyChangedEventArgs e)
        {
            _change = e;
            base.OnLayoutItemContainerStyleSelectorChanged(e);
        }
    }
}
