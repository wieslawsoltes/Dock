using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Themes;
using Dock.Model.Core;

namespace PackageConsumer;

internal static class FloatingModelProbe
{
    internal static void Verify()
    {
        foreach (bool tools in new[] { false, true })
        foreach (string suppression in new[] { "none", "child", "visibility", "all" })
            Verify(tools, suppression);
    }

    private static void Verify(bool tools, string suppression)
    {
        LayoutElement child = tools
            ? new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(new LayoutAnchorable { CanClose = true }))
            : new LayoutDocument();
        LayoutFloatingWindow model = tools
            ? new ToolWindow { RootPanel = (LayoutAnchorablePaneGroup)child, Suppression = suppression }
            : new DocumentWindow { RootDocument = (LayoutDocument)child, Suppression = suppression };
        DockingManager manager = new();
        manager.Layout.FloatingWindows.Add(model);
        Window owner = new() { Width = 640, Height = 400 };
        owner.Styles.Add(new AvalonDockTheme());
        owner.Content = manager;
        try
        {
            owner.Show();
            owner.UpdateLayout();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Pump();
            LayoutFloatingWindowControl host = manager.FloatingWindows.SingleOrDefault() ??
                throw new Exception($"Floating host missing after loading: tools={tools}, suppression={suppression}, loaded={manager.IsLoaded}.");
            if (model is LayoutAnchorableFloatingWindow tool) tool.RootPanel = null;
            else ((LayoutDocumentFloatingWindow)model).RootDocument = null;
            bool visible = !tools || suppression is "visibility" or "all";
            if (host.IsVisible != visible || ((IHostWindowCloseState)host).IsClosed ||
                !ReferenceEquals(child.Parent, model) || !ReferenceEquals(child.Root, manager.Layout) || child.DockModel.Owner is not null)
                throw new Exception("Floating child removal did not preserve notification overrides and public ownership.");
            Pump();
            bool closes = !tools || suppression is not ("child" or "all");
            if (((IHostWindowCloseState)host).IsClosed != closes || (child.Root is null) != closes)
                throw new Exception("Floating hosts did not observe their documented model close notifications.");

            LayoutElement destination = tools ? manager.Layout.RootPanel : manager.Layout.Descendents().OfType<LayoutDocumentPane>().Single();
            manager.DockFactory.AddDockable((IDock)destination.DockModel, child.DockModel);
            if (!ReferenceEquals(child.Parent, destination) || !ReferenceEquals(child.Root, manager.Layout) ||
                !ReferenceEquals(child.DockModel.Owner, destination.DockModel))
                throw new Exception("Native adoption did not replace the retained public floating parent.");
        }
        finally { manager.Layout.FloatingWindows.Clear(); owner.Close(); Pump(); }
    }

    private static bool Suppresses(string suppression, string? property, string child) => suppression == "all" ||
        (suppression == "child" && property == child) || (suppression == "visibility" && property == "IsVisible");
    private static void Pump() => Dispatcher.UIThread.RunJobs();

    private sealed class DocumentWindow : LayoutDocumentFloatingWindow
    {
        internal string Suppression { get; init; } = "none";
        protected override void RaisePropertyChanged(string? propertyName = null)
        {
            if (!Suppresses(Suppression, propertyName, nameof(RootDocument))) base.RaisePropertyChanged(propertyName);
        }
    }

    private sealed class ToolWindow : LayoutAnchorableFloatingWindow
    {
        internal string Suppression { get; init; } = "none";
        protected override void RaisePropertyChanged(string? propertyName = null)
        {
            if (!Suppresses(Suppression, propertyName, nameof(RootPanel))) base.RaisePropertyChanged(propertyName);
        }
    }
}
