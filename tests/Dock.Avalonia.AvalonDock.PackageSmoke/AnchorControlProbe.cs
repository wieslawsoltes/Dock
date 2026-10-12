using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Model.Controls;

namespace PackageConsumer;

public sealed class AnchorManagerProbe : DockingManager
{
    public int[] SideChanges { get; } = new int[4];
    public int PreviewChanges { get; private set; }
    public int ClosingTimerChanges { get; private set; }
    public bool SuppressTimerCallback { get; set; }
    public AvaloniaPropertyChangedEventArgs? LastTimerChange { get; private set; }
    public void ApplyTimerChange(AvaloniaPropertyChangedEventArgs args) => base.OnAutoHideWindowClosingTimerChanged(args);
    public void ReplacePreview(LayoutAutoHideWindowControl? value) => SetAutoHideWindow(value);
    protected override void OnAutoHideWindowChanged(AvaloniaPropertyChangedEventArgs e) { PreviewChanges++; base.OnAutoHideWindowChanged(e); }
    protected override void OnAutoHideWindowClosingTimerChanged(AvaloniaPropertyChangedEventArgs e)
    {
        ClosingTimerChanges++;
        LastTimerChange = e;
        if (!SuppressTimerCallback) base.OnAutoHideWindowClosingTimerChanged(e);
    }
    protected override void OnLeftSidePanelChanged(AvaloniaPropertyChangedEventArgs e) { SideChanges[0]++; base.OnLeftSidePanelChanged(e); }
    protected override void OnTopSidePanelChanged(AvaloniaPropertyChangedEventArgs e) { SideChanges[1]++; base.OnTopSidePanelChanged(e); }
    protected override void OnRightSidePanelChanged(AvaloniaPropertyChangedEventArgs e) { SideChanges[2]++; base.OnRightSidePanelChanged(e); }
    protected override void OnBottomSidePanelChanged(AvaloniaPropertyChangedEventArgs e) { SideChanges[3]++; base.OnBottomSidePanelChanged(e); }
}

internal static class AnchorControlProbe
{
    internal static void Verify()
    {
        AnchorControlWindow window = new();
        AnchorManagerProbe manager = window.FindControl<AnchorManagerProbe>("Manager")!;
        try
        {
            window.Show(); Update();
            LayoutAnchorSideControl[] sides = { manager.LeftSidePanel!, manager.TopSidePanel!, manager.RightSidePanel!, manager.BottomSidePanel! };
            if (manager.SideChanges.Any(count => count != 1)) throw new Exception("Packaged side creation did not invoke the external manager hooks.");
            foreach (LayoutAnchorSideControl side in sides)
            {
                LayoutAnchorSide model = (LayoutAnchorSide)side.Model;
                if (!ReferenceEquals(side.Tag, model) || side.IsLeftSide != (model.Side == AnchorSide.Left) ||
                    side.IsTopSide != (model.Side == AnchorSide.Top) || side.IsRightSide != (model.Side == AnchorSide.Right) ||
                    side.IsBottomSide != (model.Side == AnchorSide.Bottom))
                    throw new Exception("The packaged side flags or compiled public styles did not match the native edge.");
                foreach (LayoutAnchorGroupControl group in side.Children)
                foreach (LayoutAnchorControl anchor in group.Children)
                {
                    if (!ReferenceEquals(group.Model, group.Tag) || !ReferenceEquals(anchor.Model, anchor.Tag) ||
                        !ReferenceEquals(((LayoutAnchorable)anchor.Model).DockModel, anchor.DataContext) || anchor.Side != model.Side ||
                        !anchor.GetVisualAncestors().Contains(side))
                        throw new Exception("The packaged group and anchor controls did not present the existing model hierarchy.");
                }
            }
            LayoutAnchorGroupControl left = sides[0].Children[0];
            LayoutAnchorControl first = left.Children[0], second = left.Children[1];
            LayoutAnchorable tool = (LayoutAnchorable)first.Model;
            LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            document.IsActive = true;
            tool.IsSelected = true; Update();
            if (tool.IsSelected || !ReferenceEquals(manager.Layout.ActiveContent, document) ||
                !ReferenceEquals(((IRootDock)manager.DockLayout).PinnedDock?.ActiveDockable, tool.DockModel))
                throw new Exception("The packaged anchor did not open a passive native preview for selection.");
            Control view = manager.GetLayoutItemFromModel(tool)!.View;
            LayoutAutoHideWindowControl preview = manager.AutoHideWindow!;
            if (!ReferenceEquals(view.FindVisualAncestor<LayoutAutoHideWindowControl>(), preview))
                throw new Exception("The packaged visual ancestor helper did not find the existing preview.");
            if (!ReferenceEquals(view.FindLogicalAncestor<DockingManager>(), manager))
                throw new Exception("The packaged logical ancestor helper did not reach the manager through template parents.");
            if (!ReferenceEquals(view.FindVisualTreeRoot(), window.GetVisualParent() ?? window))
                throw new Exception("The packaged visual root helper did not reach the window's host.");
            if (!manager.FindVisualChildren<LayoutAutoHideWindowControl>().Contains(preview))
                throw new Exception("The packaged visual descendant helper did not find the displayed preview.");
            if (preview.Content is not Control body || !preview.FindLogicalChildren<Control>().Contains(body))
                throw new Exception("The packaged logical descendant helper did not find the preview's existing content.");
            if (!view.FindLogicalAncestorsAndSelf().Contains(manager))
                throw new Exception("The packaged logical ancestor sequence did not reach the manager.");
            if (manager.PreviewChanges != 1 || !ReferenceEquals(preview.Model, tool) || !ReferenceEquals(preview.Tag, tool) ||
                !preview.GetVisualDescendants().Contains(view) || ((ISolidColorBrush)preview.Presenter!.Background!).Color != Colors.Coral ||
                preview.GetVisualDescendants().OfType<LayoutAnchorableControl>().Single().Tag is not "Packaged preview")
                throw new Exception("The packaged public auto-hide window did not wrap the native preview or apply its compiled styles.");
            tool.AutoHideMinWidth = 180;
            tool.AutoHideWidth = 230;
            manager.GridSplitterWidth = 14;
            Update();
            LayoutGridResizerControl resizer = manager.GetVisualDescendants().OfType<LayoutGridResizerControl>()
                .Single(control => control.Name == "PART_PinnedDockSplitter" && control.IsEffectivelyVisible);
            resizer.BackgroundWhileDragging = Brushes.Teal;
            resizer.OpacityWhileDragging = 0.4;
            if (resizer.Bounds.Width != 14) throw new Exception("The packaged auto-hide resizer ignored the manager's width.");
            Point resizeStart = Center(resizer);
            window.MouseMove(resizeStart); window.MouseDown(resizeStart, MouseButton.Left);
            window.MouseMove(resizeStart + new Vector(40, 0), RawInputModifiers.LeftMouseButton); Update();
            Border ghost = manager.GetVisualDescendants().OfType<Border>().Single(control => control.Name == "PART_PinnedDockResizePreview");
            if (!ghost.IsVisible || ghost.Background != Brushes.Teal || ghost.Opacity != 0.4 || tool.AutoHideWidth != 230)
                throw new Exception("The packaged auto-hide preview did not retain its brush, opacity, and uncommitted size.");
            window.MouseUp(resizeStart + new Vector(40, 0), MouseButton.Left); Update();
            if (ghost.IsVisible || tool.AutoHideWidth != 270)
                throw new Exception("The packaged auto-hide resizer did not commit the native pinned width on release.");
            manager.ReplacePreview(null); Update(); manager.ReplacePreview(preview); Update();
            if (manager.PreviewChanges != 3 || !ReferenceEquals(preview.Model, tool) || !preview.GetVisualDescendants().Contains(view))
                throw new Exception("An external manager could not clear and restore its packaged preview presentation.");
            Point start = Center(first), target = Center(second);
            window.MouseMove(start); window.MouseDown(start, MouseButton.Left);
            window.MouseMove(target, RawInputModifiers.LeftMouseButton); window.MouseUp(target, MouseButton.Left); Update();
            if (!ReferenceEquals(left.Children[0], second) || !ReferenceEquals(left.Children[1], first) ||
                !ReferenceEquals(manager.DockLayout.LeftPinnedDockables![0], ((LayoutAnchorable)second.Model).DockModel) || !tool.IsActive)
                throw new Exception("The packaged anchor lost native pinned reordering or activation.");
            if (!manager.GetVisualDescendants().OfType<LayoutAnchorableControl>().Single(control => ReferenceEquals(control.Model, tool))
                .GetVisualDescendants().Contains(view))
                throw new Exception("The native preview replaced the packaged tool's existing content view.");

            manager.AnchorTemplate = (IControlTemplate)window.Resources["CustomAnchor"]!;
            manager.AnchorGroupTemplate = (IControlTemplate)window.Resources["CustomGroup"]!;
            manager.AnchorSideTemplate = (IControlTemplate)window.Resources["CustomSide"]!;
            tool.Title = "Changed"; Update();
            if (!first.GetVisualDescendants().OfType<TextBlock>().Any(text => text.Name == "CustomAnchorHeader" && text.Text == "Changed") ||
                !left.GetVisualDescendants().OfType<ItemsControl>().Any(items => items.Name == "CustomGroupItems") ||
                !sides[0].GetVisualDescendants().OfType<ItemsControl>().Any(items => items.Name == "CustomSideItems"))
                throw new Exception("The packaged manager templates did not update existing controls through compiled bindings.");
            manager.ClearValue(DockingManager.AnchorSideTemplateProperty);
            manager.ClearValue(DockingManager.AnchorGroupTemplateProperty);
            manager.ClearValue(DockingManager.AnchorTemplateProperty); Update();
            int changes = manager.SideChanges[0];
            manager.LeftSidePanel = null; Update();
            if (manager.GetVisualDescendants().Contains(sides[0])) throw new Exception("The packaged side property did not remove its presentation.");
            manager.LeftSidePanel = sides[0]; Update();
            if (manager.SideChanges[0] != changes + 2 || !manager.GetVisualDescendants().Contains(first))
                throw new Exception("Restoring the packaged side replaced the public anchor or missed the external override.");
            object? content = tool.Content;
            using StringWriter xml = new();
            XmlLayoutSerializer serializer = new(manager);
            serializer.Serialize(xml); serializer.Deserialize(new StringReader(xml.ToString())); Update();
            if (preview.Model is not null) throw new Exception("XML replacement retained the previous packaged auto-hide model.");
            LayoutAnchorControl restored = manager.LeftSidePanel!.Children.SelectMany(group => group.Children)
                .Single(anchor => ((LayoutAnchorable)anchor.Model).ContentId == "left-a");
            if (ReferenceEquals(manager.LeftSidePanel, sides[0]) || !ReferenceEquals(((LayoutAnchorable)restored.Model).Content, content))
                throw new Exception("Packaged XML replacement did not update public controls and preserve application content.");
            window.Content = null; Update(); window.Content = manager; Update();
            if (!manager.GetVisualDescendants().Contains(restored)) throw new Exception("The packaged anchor did not survive visual reattachment.");
            manager.AutoHideWindowClosingTimer = 0;
            ((LayoutAnchorable)restored.Model).IsSelected = true; Update();
            window.MouseMove(new Point(895, 645));
            System.Threading.Thread.Sleep(80); Update();
            System.Threading.Thread.Sleep(20); Update();
            if (preview.Model is not null || manager.DockLayout.PinnedDock is not null || manager.ClosingTimerChanges != 1)
                throw new Exception("The packaged idle closing timer did not close the native preview.");
            manager.AutoHideWindowClosingTimer = -1;
            if (manager.AutoHideWindowClosingTimer != 1500 || manager.ClosingTimerChanges != 2)
                throw new Exception("The packaged closing timer did not preserve its default coercion and external hook.");
            manager.AutoHideWindowClosingTimer = int.MaxValue;
            manager.SuppressTimerCallback = true;
            manager.AutoHideWindowClosingTimer = 0;
            AvaloniaPropertyChangedEventArgs zeroDelay = manager.LastTimerChange!;
            ((LayoutAnchorable)restored.Model).IsSelected = false;
            ((LayoutAnchorable)restored.Model).IsSelected = true; Update();
            window.MouseMove(new Point(895, 645));
            System.Threading.Thread.Sleep(80); Update();
            System.Threading.Thread.Sleep(20); Update();
            if (preview.Model is null)
                throw new Exception("A suppressed packaged delay callback changed the initialized timer.");
            manager.AutoHideWindowClosingTimer = int.MaxValue;
            manager.ApplyTimerChange(zeroDelay);
            DispatcherFrame closingFrame = new();
            void PreviewChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
            {
                if (preview.Model is null) closingFrame.Continue = false;
            }
            preview.PropertyChanged += PreviewChanged;
            try
            {
                using IDisposable deadline = DispatcherTimer.RunOnce(() => closingFrame.Continue = false, TimeSpan.FromSeconds(5));
                if (preview.Model is not null) Dispatcher.UIThread.PushFrame(closingFrame);
            }
            finally { preview.PropertyChanged -= PreviewChanged; Update(); }
            if (preview.Model is not null || manager.AutoHideWindowClosingTimer != int.MaxValue)
                throw new Exception($"A replayed packaged delay callback did not close: argument={zeroDelay.NewValue}, current={manager.AutoHideWindowClosingTimer}, active={(preview.Model as LayoutAnchorable)?.IsActive}, pointer={preview.IsPointerOver}, keep={(preview.Model as LayoutAnchorable)?.DockModel.KeepPinnedDockableVisible}.");
            manager.SuppressTimerCallback = false;
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }

        Point Center(Control control) => control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), window)!.Value;
        void Update() { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); }
    }
}
