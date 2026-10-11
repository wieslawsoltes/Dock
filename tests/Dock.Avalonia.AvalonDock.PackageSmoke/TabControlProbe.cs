using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Themes;
using Dock.Model.Core;
using Orientation = Avalonia.Layout.Orientation;

namespace PackageConsumer;

public sealed class DocumentTabProbe : LayoutDocumentTabItem
{
    public int ModelChanges { get; private set; }
    public int Presses { get; private set; }
    public int Releases { get; private set; }
    public int Moves { get; private set; }
    public int Enters { get; private set; }
    public int Exits { get; private set; }
    public int Downs { get; private set; }
    public void AssignItem(LayoutItem? item) => SetLayoutItem(item);
    protected override void OnModelChanged(AvaloniaPropertyChangedEventArgs e) { ModelChanges++; base.OnModelChanged(e); }
    protected override void OnMouseLeftButtonDown(PointerPressedEventArgs e) { Presses++; base.OnMouseLeftButtonDown(e); }
    protected override void OnMouseLeftButtonUp(PointerReleasedEventArgs e) { Releases++; base.OnMouseLeftButtonUp(e); }
    protected override void OnMouseMove(PointerEventArgs e) { Moves++; base.OnMouseMove(e); }
    protected override void OnMouseEnter(PointerEventArgs e) { Enters++; base.OnMouseEnter(e); }
    protected override void OnMouseLeave(PointerEventArgs e) { Exits++; base.OnMouseLeave(e); }
    protected override void OnMouseDown(PointerPressedEventArgs e) { Downs++; base.OnMouseDown(e); }
}

public sealed class ToolTabProbe : LayoutAnchorableTabItem
{
    public int ModelChanges { get; private set; }
    public int Presses { get; private set; }
    public int Releases { get; private set; }
    public int Moves { get; private set; }
    public int Enters { get; private set; }
    public int Exits { get; private set; }
    public void AssignItem(LayoutItem? item) => SetLayoutItem(item);
    protected override void OnModelChanged(AvaloniaPropertyChangedEventArgs e) { ModelChanges++; base.OnModelChanged(e); }
    protected override void OnMouseLeftButtonDown(PointerPressedEventArgs e) { Presses++; base.OnMouseLeftButtonDown(e); }
    protected override void OnMouseLeftButtonUp(PointerReleasedEventArgs e) { Releases++; base.OnMouseLeftButtonUp(e); }
    protected override void OnMouseMove(PointerEventArgs e) { Moves++; base.OnMouseMove(e); }
    protected override void OnMouseEnter(PointerEventArgs e) { Enters++; base.OnMouseEnter(e); }
    protected override void OnMouseLeave(PointerEventArgs e) { Exits++; base.OnMouseLeave(e); }
}

internal static class TabControlProbe
{
    internal static void Verify()
    {
        Verify(false);
        Verify(true);
    }

    private static void Verify(bool tool)
    {
        LayoutContent first = tool ? new LayoutAnchorable { CanClose = true } : new LayoutDocument();
        LayoutContent second = tool ? new LayoutAnchorable { CanClose = true } : new LayoutDocument();
        first.Title = "First tab";
        second.Title = "Second tab";
        ILayoutPane pane = tool ? new LayoutAnchorablePane((LayoutAnchorable)first) : new LayoutDocumentPane(first);
        if (pane is LayoutAnchorablePane tools) tools.Children.Add((LayoutAnchorable)second);
        else ((LayoutDocumentPane)pane).Children.Add(second);
        LayoutPanel root = new();
        root.Children.Add((ILayoutPanelElement)pane);
        if (tool) root.Children.Add(new LayoutDocumentPane(new LayoutDocument { Title = "Document" }));
        DockingManager manager = new() { Layout = new() { RootPanel = root } };
        Control tab = tool ? new ToolTabProbe { Model = first } : new DocumentTabProbe { Model = first };
        Control other = tool ? new ToolTabProbe { Model = second } : new DocumentTabProbe { Model = second };
        LayoutItem item = manager.GetLayoutItemFromModel(first)!;
        if (!ReferenceEquals(Item(tab), item)) throw new Exception("A packaged public tab did not resolve its existing item.");
        if (tab is DocumentTabProbe document)
        {
            document.AssignItem(null);
            if (document.LayoutItem is not null) throw new Exception("The document tab's protected setter did not clear its container.");
            document.Model = null;
            document.Model = first;
        }
        else
        {
            ToolTabProbe anchorable = (ToolTabProbe)tab;
            anchorable.AssignItem(null);
            if (anchorable.LayoutItem is not null) throw new Exception("The tool tab's protected setter did not clear its container.");
            anchorable.Model = null;
            anchorable.Model = first;
        }
        ConsumerWindow resources = new();
        foreach (string key in tool ? new[] { "ToolTabControl", "LegacyToolTabControl" } : new[] { "DocumentTabControl", "LegacyDocumentTabControl" })
        {
            Control compiled = ((IDataTemplate)resources.Resources[key]!).Build(first)!;
            compiled.DataContext = first;
            if (!ReferenceEquals(Model(compiled), first) || !ReferenceEquals(Item(compiled), item))
                throw new Exception("Both packaged XML namespaces must resolve the same tab model and item.");
            compiled.DataContext = null;
        }
        tab.Width = other.Width = 180;
        tab.Height = other.Height = 40;
        StackPanel row = new() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Left };
        row.Children.Add(tab);
        row.Children.Add(other);
        DockPanel panel = new();
        DockPanel.SetDock(row, global::Avalonia.Controls.Dock.Top);
        panel.Children.Add(row);
        panel.Children.Add(manager);
        Window window = new() { Width = 800, Height = 400 };
        window.Styles.Add(new AvalonDockTheme());
        window.Content = panel;
        try
        {
            window.Show();
            Update();
            CheckNativeHeader(manager);
            IDockItemDragPolicy policy = (IDockItemDragPolicy)manager.DockFactory;
            SetReposition(false);
            if (policy.CanReorderItem(first.DockModel) || !policy.CanDragItem(first.DockModel))
                throw new Exception("The packaged factory did not distinguish reordering from dragging.");
            DragAcross(true);
            DragAcross(false);
            if (!pane.Children.SequenceEqual(new[] { first, second }))
                throw new Exception("A restricted packaged tab row reordered its pane.");
            SetReposition(true);
            DragAcross(false);
            if (!pane.Children.SequenceEqual(new[] { second, first }))
                throw new Exception("The custom packaged tab row did not reorder the live pane.");
            int began = 0;
            manager.DockFactory.WindowMoveDragBegin += (_, args) => { began++; args.Cancel = true; };
            SeparateGesture();
            Point start = Center(tab);
            Point end = start + new Vector(0, 90);
            window.MouseMove(start);
            window.MouseDown(start, MouseButton.Left);
            window.MouseMove(start + new Vector(12, 0), RawInputModifiers.LeftMouseButton);
            window.MouseMove(end, RawInputModifiers.LeftMouseButton);
            Update();
            if (!first.IsFloating || second.IsFloating || began != 1 || manager.FloatingWindows.Single().IsDragging)
                throw new Exception("A custom packaged tab did not float one item through native drag cancellation.");
            CheckNativeHeader(manager.FloatingWindows.Single());
            window.MouseUp(end, MouseButton.Left);
            if (tab is DocumentTabProbe { ModelChanges: > 0, Presses: > 0, Releases: > 0, Moves: > 0, Enters: > 0, Exits: > 0, Downs: > 0 }) return;
            if (tab is ToolTabProbe { ModelChanges: > 0, Presses: > 0, Releases: > 0, Moves: > 0, Enters: > 0, Exits: > 0 }) return;
            throw new Exception("The external public-tab model and input overrides were not invoked.");
        }
        finally
        {
            manager.Layout.FloatingWindows.Clear();
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }

        void Update() { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); }
        Point Center(Control control) => control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), window)!.Value;
        void SeparateGesture()
        {
            Point background = new(799, 399);
            window.MouseMove(background);
            window.MouseDown(background, MouseButton.Left);
            window.MouseUp(background, MouseButton.Left);
        }
        void SetReposition(bool allowed)
        {
            if (pane is LayoutAnchorablePane tools) tools.CanRepositionItems = allowed;
            else ((LayoutDocumentPane)pane).CanRepositionItems = allowed;
        }
        void DragAcross(bool native)
        {
            SeparateGesture();
            Control source = native ? FindTab(manager, first) : tab;
            Control target = native ? FindTab(manager, second) : other;
            Point start = Center(source);
            Point end = Center(target);
            window.MouseMove(start);
            window.MouseDown(start, MouseButton.Left);
            window.MouseMove(start + new Vector(12, 0), RawInputModifiers.LeftMouseButton);
            window.MouseMove(end, RawInputModifiers.LeftMouseButton);
            window.MouseUp(end, MouseButton.Left);
            Update();
        }
        Control FindTab(Control scope, LayoutContent model) => scope.GetVisualDescendants().OfType<Control>()
            .Single(control => (tool ? control is LayoutAnchorableTabItem : control is LayoutDocumentTabItem) && ReferenceEquals(Model(control), model));
        void CheckNativeHeader(Control scope)
        {
            Control header = FindTab(scope, first);
            if (!ReferenceEquals(Item(header), item)) throw new Exception("The native packaged tab did not retain its existing item.");
        }
    }

    private static LayoutContent? Model(Control tab) => tab is LayoutDocumentTabItem document ? document.Model : ((LayoutAnchorableTabItem)tab).Model;
    private static LayoutItem? Item(Control tab) => tab is LayoutDocumentTabItem document ? document.LayoutItem : ((LayoutAnchorableTabItem)tab).LayoutItem;
}
