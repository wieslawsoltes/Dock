using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Themes;
using Dock.Avalonia.Controls;
using Dock.Settings;

namespace PackageConsumer;

internal static class FullDragPreviewProbe
{
    internal static void Verify()
    {
        ConsumerWindow resources = new();
        foreach (string key in new[] { "CaptionPreview", "LegacyCaptionPreview" })
            if (resources.Resources[key] is not DockingManager { ShowDockablePreviewOnDrag: false })
                throw new Exception("The full drag preview option did not compile in both XML namespaces.");

        bool oldManaged = DockSettings.UseManagedWindows;
        bool oldPreview = DockSettings.ShowDockablePreviewOnDrag;
        AvalonDockTheme theme = new();
        Application.Current!.Styles.Add(theme);
        try
        {
            DockSettings.ShowDockablePreviewOnDrag = false;
            foreach (bool managed in new[] { false, true })
                foreach (bool tool in new[] { false, true })
                    VerifyDrag(tool, managed);
        }
        finally
        {
            DockSettings.UseManagedWindows = oldManaged;
            DockSettings.ShowDockablePreviewOnDrag = oldPreview;
            Application.Current!.Styles.Remove(theme);
        }
    }

    private static void VerifyDrag(bool tool, bool managed)
    {
        DockSettings.UseManagedWindows = managed;
        TextBox editor = new() { Text = "Packaged full preview", AcceptsReturn = true };
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        content.Title = "Preview";
        content.Content = editor;
        ILayoutPanelElement pane = tool
            ? new LayoutAnchorablePane((LayoutAnchorable)content) { Children = { new LayoutAnchorable { Title = "Sibling" } } }
            : new LayoutDocumentPane(content) { Children = { new LayoutDocument { Title = "Sibling" } } };
        DockingManager manager = new() { Layout = new() { RootPanel = new() { Children = { pane } } } };
        Window owner = new() { Content = manager, Width = 800, Height = 500 };
        DragPreviewWindow? previewWindow = null;
        using IDisposable subscription = Window.WindowOpenedEvent.AddClassHandler<DragPreviewWindow>((window, _) => previewWindow = window);
        try
        {
            owner.Show();
            content.IsActive = true;
            Update();
            Control tab = manager.GetVisualDescendants().OfType<Control>().Single(control =>
                tool ? control is LayoutAnchorableTabItem anchor && ReferenceEquals(anchor.Model, content)
                     : control is LayoutDocumentTabItem document && ReferenceEquals(document.Model, content));
            Point start = tab.TranslatePoint(new Point(tab.Bounds.Width / 2, tab.Bounds.Height / 2), owner)!.Value;
            owner.MouseMove(start);
            owner.MouseDown(start, MouseButton.Left);
            owner.MouseMove(start + new Vector(12, 0), RawInputModifiers.LeftMouseButton);
            owner.MouseMove(start + new Vector(0, tool ? -80 : 80), RawInputModifiers.LeftMouseButton);
            Update();
            DragPreviewControl? preview = managed
                ? owner.GetVisualDescendants().OfType<DragPreviewControl>().SingleOrDefault()
                : previewWindow?.Content as DragPreviewControl;
            if (preview is not { ShowContent: true, PreviewContent: Image { Source: Bitmap bitmap } } ||
                bitmap.PixelSize.Width <= 100 || bitmap.PixelSize.Height <= 100 || preview.PreviewContentWidth <= 100 ||
                preview.PreviewContentHeight <= 100 || preview.Bounds.Height <= 100 || DockSettings.ShowDockablePreviewOnDrag)
                throw new Exception($"The packaged {(tool ? "tool" : "document")} preview did not show full content (managed={managed}).");
            owner.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
            owner.KeyRelease(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
            owner.MouseUp(start, MouseButton.Left);
            Update();
            if (!ReferenceEquals(content.Parent, pane) || !ReferenceEquals(TopLevel.GetTopLevel(editor), owner) ||
                manager.Layout.FloatingWindows.Count != 0 || manager.GetVisualDescendants().OfType<DockControl>().Single().IsDraggingDock)
                throw new Exception("Canceling the packaged full preview did not restore its content and layout.");
        }
        finally { owner.Close(); Dispatcher.UIThread.RunJobs(); }
        void Update() { Dispatcher.UIThread.RunJobs(); owner.UpdateLayout(); }
    }
}
