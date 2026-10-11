using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Themes;

namespace PackageConsumer;

public sealed class CachePaneProbe : LayoutCachePaneControl
{
    public int Selections { get; private set; }
    protected override void OnSelectionChanged(SelectionChangedEventArgs e) { Selections++; base.OnSelectionChanged(e); }
}

public sealed class ToolPaneProbe(LayoutAnchorablePane model) : LayoutAnchorablePaneControl(model)
{
    protected override void OnGotKeyboardFocus(FocusChangedEventArgs e) => base.OnGotKeyboardFocus(e);
    protected override void OnMouseLeftButtonDown(PointerPressedEventArgs e) => base.OnMouseLeftButtonDown(e);
    protected override void OnMouseRightButtonDown(PointerPressedEventArgs e) => base.OnMouseRightButtonDown(e);
}

public sealed class PaneManagerProbe : DockingManager
{
    public int Documents { get; private set; }
    public int Tools { get; private set; }
    protected override void OnDocumentPaneControlStyleChanged(AvaloniaPropertyChangedEventArgs e) { Documents++; base.OnDocumentPaneControlStyleChanged(e); }
    protected override void OnAnchorablePaneControlStyleChanged(AvaloniaPropertyChangedEventArgs e) { Tools++; base.OnAnchorablePaneControlStyleChanged(e); }
}

internal static class PaneControlProbe
{
    internal static void Verify()
    {
        ConsumerWindow resources = new();
        LayoutDocument first = new() { Title = "First", Content = new TextBox { Text = "Editor" } };
        LayoutDocumentPane documents = new(first);
        documents.Children.Add(new LayoutDocument { Title = "Second" });
        LayoutAnchorablePane tools = new(new LayoutAnchorable { Title = "Tool" });
        tools.Children.Add(new() { Title = "Other tool" });
        PaneManagerProbe manager = new()
        {
            Layout = new() { RootPanel = new LayoutPanel { Children = { documents, tools } } },
            DocumentPaneControlStyle = (ControlTheme)resources.Resources["DocumentPaneStyle"]!,
            AnchorablePaneControlStyle = (ControlTheme)resources.Resources["ToolPaneStyle"]!
        };
        ToolPaneProbe custom = new(tools);
        if (!ReferenceEquals(custom.Model, tools) || !ReferenceEquals(custom.ItemsSource, tools.Children))
            throw new Exception("The external tool pane constructor did not retain its live public pane.");
        CachePaneProbe cache = new() { ItemsSource = new[] { "First", "Second" } };
        cache.SelectedIndex = 1; cache.SelectedIndex = -1;
        if (cache.Selections != 2) throw new Exception("The external pane selection hook was not invoked.");
        foreach (string key in new[] { "CachePane", "LegacyCachePane" })
        {
            LayoutCachePaneControl pane = (LayoutCachePaneControl)((IDataTemplate)resources.Resources[key]!).Build(documents)!;
            pane.DataContext = documents;
            if (!ReferenceEquals(pane.ItemsSource, documents.Children)) throw new Exception("The cache pane did not compile in both XML namespaces.");
        }
        Window window = new() { Width = 1000, Height = 650 };
        window.Styles.Add(new AvalonDockTheme());
        window.Content = manager;
        try
        {
            window.Show(); Update();
            LayoutDocumentPaneControl docControl = manager.GetVisualDescendants().OfType<LayoutDocumentPaneControl>().Single();
            LayoutAnchorablePaneControl toolControl = manager.GetVisualDescendants().OfType<LayoutAnchorablePaneControl>().Single();
            if (!ReferenceEquals(docControl.Model, documents) || !ReferenceEquals(toolControl.Model, tools) ||
                !ReferenceEquals(docControl.Tag, documents) || !ReferenceEquals(toolControl.Tag, tools))
                throw new Exception("Packaged pane themes did not bind the existing public models.");
            first.IsActive = true;
            documents.SelectedContentIndex = 1;
            if (docControl.SelectedIndex != 1 || !ReferenceEquals(manager.Layout.ActiveContent, first))
                throw new Exception("Model selection unexpectedly activated a document through its pane control.");
            documents.MoveChild(1, 0);
            Update();
            if (docControl.SelectedIndex != 0 || !ReferenceEquals(docControl.SelectedItem, documents.Children[0]))
                throw new Exception("Reordering lost the selected document in its native pane.");
            docControl.SelectedIndex = -1;
            if (documents.SelectedContentIndex != -1) throw new Exception("The packaged pane did not clear selection.");
            manager.DocumentPaneControlStyle = (ControlTheme)resources.Resources["LegacyDocumentPaneStyle"]!;
            manager.AnchorablePaneControlStyle = (ControlTheme)resources.Resources["LegacyToolPaneStyle"]!;
            Update();
            if (!ReferenceEquals(docControl.Theme, manager.DocumentPaneControlStyle) ||
                !ReferenceEquals(toolControl.Theme, manager.AnchorablePaneControlStyle) || manager.Documents != 2 || manager.Tools != 2)
                throw new Exception("Runtime pane themes or external manager hooks did not update.");
            first.IsActive = true;
            Control view = manager.GetLayoutItemFromModel(first)!.View;
            first.Float(); Update();
            LayoutFloatingWindowControl floating = manager.FloatingWindows.Single();
            LayoutDocumentPaneControl floatingPane = floating.GetVisualDescendants().OfType<LayoutDocumentPaneControl>().Single();
            if (!ReferenceEquals(floatingPane.Theme, manager.DocumentPaneControlStyle) ||
                !floating.GetVisualDescendants().Contains(view))
                throw new Exception("The packaged floating pane lost its manager theme or cached content view.");
            first.Dock(); Update();
            if (!ReferenceEquals(view, manager.GetLayoutItemFromModel(first)!.View))
                throw new Exception("Docking recreated the application content view.");
        }
        finally
        {
            foreach (LayoutFloatingWindowControl floating in manager.FloatingWindows.ToArray()) floating.Close();
            window.Close(); Dispatcher.UIThread.RunJobs();
        }
        void Update()
        {
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            foreach (LayoutFloatingWindowControl floating in manager.FloatingWindows) floating.UpdateLayout();
        }
    }
}
