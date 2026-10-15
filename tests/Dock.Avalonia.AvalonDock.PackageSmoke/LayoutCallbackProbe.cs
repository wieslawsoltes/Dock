using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using AvalonDock;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class LayoutCallbackProbe
{
    internal static void Verify()
    {
        Manager manager = new();
        LayoutRoot supplied = Root("supplied");
        LayoutRoot current = Root("current");
        manager.Before = next =>
        {
            if (!ReferenceEquals(next, supplied)) return;
            manager.Suppress = true;
            manager.Layout = current;
            manager.Suppress = false;
        };
        manager.Layout = supplied;
        if (!ReferenceEquals(manager, current.Manager) || supplied.Manager is not null ||
            manager.GetLayoutItemFromModel(current.Descendents().OfType<LayoutDocument>().Single()) is null)
            throw new Exception("Packaged layout redirection did not use current ownership and containers.");

        DockingManager other = new() { Layout = Root("foreign") };
        manager.Apply(null, other.Layout);
        if (!ReferenceEquals(other, other.Layout.Manager) || !ReferenceEquals(manager, current.Manager))
            throw new Exception("A packaged replay transferred ownership of its foreign argument.");

        manager = new();
        ObservableCollection<object> source = new() { new object() };
        manager.DocumentsSource = source;
        LayoutRoot previous = manager.Layout;
        current = Root("current");
        supplied = Root("supplied");
        manager.Suppress = true;
        manager.Layout = current;
        manager.Apply(previous, supplied);
        if (!supplied.Descendents().OfType<LayoutDocument>().Any(item => ReferenceEquals(item.Content, source[0])) ||
            current.Descendents().OfType<LayoutDocument>().Any(item => ReferenceEquals(item.Content, source[0])))
            throw new Exception("A packaged layout replay did not import into its supplied root.");
        source.Add(new object());
        if (!current.Descendents().OfType<LayoutDocument>().Any(item => ReferenceEquals(item.Content, source[1])))
            throw new Exception("A packaged live source did not follow the current layout after replay.");

        manager = new();
        manager.Initialize();
        previous = manager.Layout;
        current = Root("current");
        manager.Suppress = true;
        manager.Layout = current;
        NotificationRoot notifications = new();
        manager.Apply(previous, notifications);
        manager.Apply(null, notifications);
        int changes = 0;
        manager.PanelChanged = () => current.RootPanel = Root("callback " + ++changes).RootPanel;
        current.RootPanel = Root("pending").RootPanel;
        notifications.NotifyPanel();
        if (changes != 2)
            throw new Exception("Packaged replay did not preserve distinct supplied-root registrations.");
        manager.PanelChanged = null;
        manager.Apply(manager.Layout, null);
        if (!ReferenceEquals(manager, manager.Layout.Manager))
            throw new Exception("A null packaged callback argument did not retain current layout ownership.");
    }

    private static LayoutRoot Root(string value) => new()
    {
        RootPanel = new LayoutPanel(new LayoutDocumentPane(new LayoutDocument { Content = value }))
    };

    private sealed class Manager : DockingManager
    {
        internal Action<LayoutRoot>? Before { get; set; }
        internal bool Suppress { get; set; }
        internal Action? PanelChanged { get; set; }
        internal void Initialize() => InitializeIfNeeded();
        internal void Apply(LayoutRoot? previous, LayoutRoot? next) => base.OnLayoutChanged(previous!, next!);
        protected override void OnLayoutChanged(LayoutRoot oldLayout, LayoutRoot newLayout)
        {
            Before?.Invoke(newLayout);
            if (!Suppress) base.OnLayoutChanged(oldLayout, newLayout);
        }
        protected override void OnLayoutRootPanelChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.OnLayoutRootPanelChanged(e);
            PanelChanged?.Invoke();
        }
    }

    private sealed class NotificationRoot : LayoutRoot
    {
        internal void NotifyPanel() => RaisePropertyChanged(nameof(RootPanel));
    }
}
