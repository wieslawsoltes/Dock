using System;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Themes;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace PackageConsumer;

internal static class NativeWindowProbe
{
    internal static void Verify()
    {
        LayoutDocument document = new() { Content = "Editor" };
        LayoutAnchorable tool = new() { Content = "Tools" };
        DockingManager manager = new() { DocumentContextMenu = null, AnchorableContextMenu = null,
            Layout = new() { RootPanel = new() { Children =
            { new LayoutDocumentPane(document), new LayoutDocumentPane(new LayoutDocument()), new LayoutAnchorablePane(tool) } } } };
        Window owner = new() { Width = 800, Height = 600 };
        owner.Styles.Add(new AvalonDockTheme());
        owner.Content = manager;
        ClosingOverlay? overlay = null;
        try
        {
            owner.Show(); owner.UpdateLayout(); Dispatcher.UIThread.RunJobs();
            manager.DockFactory.HostWindowLocator![nameof(IDockWindow)] = () =>
            {
                LayoutFloatingWindow model = manager.Layout.FloatingWindows.Single(item => item.DockWindow.Host is null);
                return model is LayoutDocumentFloatingWindow doc ? new DocumentWindow(doc) : new ToolWindow((LayoutAnchorableFloatingWindow)model);
            };
            foreach (LayoutContent content in new LayoutContent[] { document, tool })
            {
                content.Float(); Dispatcher.UIThread.RunJobs();
                LayoutFloatingWindowControl host = manager.FloatingWindows.Last();
                ICaptionProbe probe = (ICaptionProbe)host;
                manager.ShowSystemMenu = true;
                if (probe.RightClick()) throw new Exception("The packaged native caption consumed an enabled system fallback.");
                manager.ShowSystemMenu = false;
                if (!probe.RightClick()) throw new Exception("The packaged native caption ignored the disabled system fallback.");
                ContextMenu menu = new() { Items = { new MenuItem { Header = "Custom caption menu" } } };
                if (content is LayoutDocument) manager.DocumentContextMenu = menu;
                else manager.AnchorableContextMenu = menu;
                if (!probe.RightClick() || !menu.IsOpen || !ReferenceEquals(menu.DataContext, manager.GetLayoutItemFromModel(content)))
                    throw new Exception("The packaged typed host did not route its caption to the custom layout-item menu.");
                menu.Close();
            }
            DockTarget target = new();
            overlay = new(manager, manager, target) { Content = target, Width = 150, Height = 150, Veto = true };
            overlay.Show(owner);
            overlay.Close();
            if (!overlay.IsVisible || !overlay.HookCalled || !ReferenceEquals(target, overlay.Content))
                throw new Exception("The packaged overlay BCL closing hook did not preserve a canceled overlay.");
            overlay.Veto = false;
            overlay.Close();
            if (overlay.Content is not null) throw new Exception("The packaged overlay did not release content after close.");
        }
        finally
        {
            if (overlay is not null) { overlay.Veto = false; overlay.Close(); }
            manager.Layout.FloatingWindows.Clear(); owner.Close(); Dispatcher.UIThread.RunJobs();
        }
    }

    private interface ICaptionProbe { bool RightClick(); }
    private sealed class DocumentWindow(LayoutDocumentFloatingWindow model) : LayoutDocumentFloatingWindowControl(model), ICaptionProbe
    {
        public bool RightClick() { bool handled = false; FilterMessage(IntPtr.Zero, 0xA5, (IntPtr)2, IntPtr.Zero, ref handled); return handled; }
    }
    private sealed class ToolWindow(LayoutAnchorableFloatingWindow model) : LayoutAnchorableFloatingWindowControl(model), ICaptionProbe
    {
        public bool RightClick() { bool handled = false; FilterMessage(IntPtr.Zero, 0xA5, (IntPtr)2, IntPtr.Zero, ref handled); return handled; }
    }
    private sealed class ClosingOverlay(DockingManager manager, Control surface, Control adorner) : OverlayWindow(manager, surface, adorner)
    {
        internal bool Veto { get; set; }
        internal bool HookCalled { get; private set; }
        protected override void OnClosing(CancelEventArgs e) { HookCalled = true; e.Cancel |= Veto; base.OnClosing(e); }
    }
}
