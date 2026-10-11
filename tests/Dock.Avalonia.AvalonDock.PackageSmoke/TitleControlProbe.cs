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
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Themes;

namespace PackageConsumer;

public sealed class TitleControlProbe : AnchorablePaneTitle
{
    public int ModelChanges { get; private set; }
    public int Presses { get; private set; }
    public int Releases { get; private set; }
    public int Moves { get; private set; }
    public int Exits { get; private set; }

    protected override void OnModelChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnModelChanged(e);
        ModelChanges++;
    }

    protected override void OnMouseLeftButtonDown(PointerPressedEventArgs e) { Presses++; base.OnMouseLeftButtonDown(e); }
    protected override void OnMouseLeftButtonUp(PointerReleasedEventArgs e) { Releases++; base.OnMouseLeftButtonUp(e); }
    protected override void OnMouseMove(PointerEventArgs e) { Moves++; base.OnMouseMove(e); }
    protected override void OnMouseLeave(PointerEventArgs e) { Exits++; base.OnMouseLeave(e); }

    internal static void Verify()
    {
        LayoutAnchorable first = new() { Title = "First", CanClose = true };
        LayoutAnchorable second = new() { Title = "Second", CanClose = true };
        LayoutAnchorablePane pane = new(first);
        pane.Children.Add(second);
        DockingManager manager = new() { Layout = new() { RootPanel = new LayoutPanel(pane) } };
        TitleControlProbe title = new() { Model = first, Width = 200, Height = 40, HorizontalAlignment = HorizontalAlignment.Left };
        if (title.ModelChanges != 1 || !ReferenceEquals(title.LayoutItem, manager.GetLayoutItemFromModel(first)))
            throw new Exception("An external title override did not resolve the existing item container.");
        title.SetLayoutItem(null);
        if (title.LayoutItem is not null) throw new Exception("The protected title container setter did not clear the read-only property.");
        title.Model = null;
        title.Model = first;
        ConsumerWindow resources = new();
        foreach (string key in new[] { "AnchorableTitleControl", "LegacyAnchorableTitleControl" })
        {
            AnchorablePaneTitle compiled = (AnchorablePaneTitle)((IDataTemplate)resources.Resources[key]!).Build(first)!;
            compiled.DataContext = first;
            if (!ReferenceEquals(compiled.Model, first) || !ReferenceEquals(compiled.LayoutItem, title.LayoutItem))
                throw new Exception("The packaged title could not resolve its model/container through both XML namespaces.");
            compiled.DataContext = null;
        }
        DockPanel panel = new();
        DockPanel.SetDock(title, global::Avalonia.Controls.Dock.Top);
        panel.Children.Add(title);
        panel.Children.Add(manager);
        Window window = new() { Width = 700, Height = 400 };
        window.Styles.Add(new AvalonDockTheme());
        window.Content = panel;
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            if (!window.Styles.TryGetResource(typeof(AnchorablePaneTitle), null, out _))
                throw new Exception("The packaged scoped theme did not contain the public tool title theme.");
            Point start = title.TranslatePoint(new Point(title.Bounds.Width / 2, title.Bounds.Height / 2), window)!.Value;
            window.MouseMove(start);
            window.MouseDown(start, MouseButton.Left);
            window.MouseUp(start, MouseButton.Left);
            if (title.Presses != 1 || title.Releases != 1 || !ReferenceEquals(manager.Layout.ActiveContent, first))
                throw new Exception($"The public title hooks did not activate the represented tool: presses={title.Presses}, releases={title.Releases}, active={manager.Layout.ActiveContent?.Title}.");
            int began = 0;
            manager.DockFactory.WindowMoveDragBegin += (_, args) => { began++; args.Cancel = true; };
            window.MouseDown(start, MouseButton.Left);
            window.MouseMove(start + new Vector(300, 0), RawInputModifiers.LeftMouseButton);
            Dispatcher.UIThread.RunJobs();
            LayoutAnchorableFloatingWindow floating = manager.Layout.FloatingWindows.OfType<LayoutAnchorableFloatingWindow>().Single();
            if (!floating.Descendents().OfType<LayoutAnchorable>().SequenceEqual(new[] { first, second }) ||
                began != 1 || manager.FloatingWindows.Single().IsDragging || title.Exits == 0 || title.Moves == 0)
                throw new Exception("The packaged custom title did not float its pane through native drag cancellation.");
            window.MouseUp(start + new Vector(300, 0), MouseButton.Left);
        }
        finally
        {
            manager.Layout.FloatingWindows.Clear();
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
