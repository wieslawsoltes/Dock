using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.AvalonDock;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        if (args is ["--closing", string output])
        {
            FloatingCloseProbe.Run(output);
            return;
        }
        if (args is ["--floating-models", string modelOutput])
        {
            FloatingModelProbe.Run(modelOutput);
            return;
        }
        List<string> rows = new() { "property\tscenario\tstage\ttemplate\tselector\tbound\tevents" };
        Spec[] specs =
        {
            new("DocumentHeader", DockingManager.DocumentHeaderTemplateProperty, DockingManager.DocumentHeaderTemplateSelectorProperty),
            new("DocumentTitle", DockingManager.DocumentTitleTemplateProperty, DockingManager.DocumentTitleTemplateSelectorProperty),
            new("AnchorableHeader", DockingManager.AnchorableHeaderTemplateProperty, DockingManager.AnchorableHeaderTemplateSelectorProperty),
            new("AnchorableTitle", DockingManager.AnchorableTitleTemplateProperty, DockingManager.AnchorableTitleTemplateSelectorProperty),
            new("DocumentPaneMenuItemHeader", DockingManager.DocumentPaneMenuItemHeaderTemplateProperty, DockingManager.DocumentPaneMenuItemHeaderTemplateSelectorProperty),
        };
        foreach (Spec spec in specs)
        {
            foreach (bool suppress in new[] { false, true })
            {
                foreach (string source in new[] { "local", "binding", "style" })
                {
                    TraceManager manager = new() { Suppress = suppress };
                    DataTemplate first = new(); DataTemplate second = new();
                    DataTemplateSelector selector = new(); DataTemplateSelector replacement = new();
                    string scenario = source + (suppress ? "-override" : "-base");
                    void Take(string stage)
                    {
                        object template = manager.GetValue(spec.Template);
                        object selected = manager.GetValue(spec.Selector);
                        string t = template is null ? "null" : ReferenceEquals(template, first) ? "first" : ReferenceEquals(template, second) ? "second" : "other";
                        string s = selected is null ? "null" : ReferenceEquals(selected, selector) ? "first" : ReferenceEquals(selected, replacement) ? "second" : "other";
                        rows.Add(string.Join('\t', spec.Name, scenario, stage, t, s, BindingOperations.IsDataBound(manager, spec.Template), string.Join('>', manager.Events)));
                        manager.Events.Clear();
                    }
                    Take("initial");
                    if (source == "binding") BindingOperations.SetBinding(manager, spec.Template, new Binding { Source = first });
                    else if (source == "style") manager.Style = new Style(typeof(DockingManager)) { Setters = { new Setter(spec.Template, first) } };
                    else manager.SetValue(spec.Template, first);
                    Take("first-template");
                    manager.SetValue(spec.Selector, selector); Take("set-selector");
                    manager.SetValue(spec.Template, second); Take("second-template");
                    manager.SetValue(spec.Selector, replacement); Take("replace-selector");
                    manager.SetValue(spec.Selector, null); Take("clear-selector");
                    manager.CoerceValue(spec.Template); Take("explicit-coerce");
                    manager.SetValue(spec.Template, first); Take("reassign-first");
                }
            }
        }
        TraceManager menu = new();
        DataTemplate headerOne = new(); DataTemplate headerTwo = new();
        void Menu(string stage) => rows.Add(string.Join('\t', "MenuFallback", "base", stage,
            menu.DocumentPaneMenuItemHeaderTemplate is null ? "null" : ReferenceEquals(menu.DocumentPaneMenuItemHeaderTemplate, headerOne) ? "first" : ReferenceEquals(menu.DocumentPaneMenuItemHeaderTemplate, headerTwo) ? "second" : "other", "-", false, string.Join('>', menu.Events)));
        Menu("initial");
        menu.DocumentHeaderTemplate = headerOne; Menu("set-header");
        menu.DocumentPaneMenuItemHeaderTemplate = null; Menu("set-menu-null");
        menu.DocumentHeaderTemplate = headerTwo; Menu("replace-header");
        menu.DocumentPaneMenuItemHeaderTemplateSelector = new DataTemplateSelector(); Menu("set-menu-selector");
        menu.DocumentHeaderTemplate = null; Menu("clear-header");
        menu.CoerceValue(DockingManager.DocumentPaneMenuItemHeaderTemplateProperty); Menu("coerce-menu");
        File.WriteAllLines(args[0], rows);
        Console.WriteLine($"Recorded {rows.Count - 1} WPF template-property observations.");
    }
    private sealed record Spec(string Name, DependencyProperty Template, DependencyProperty Selector);
}

internal sealed class TraceManager : DockingManager
{
    internal bool Suppress { get; init; }
    internal List<string> Events { get; } = new();
    protected override void OnDocumentHeaderTemplateChanged(DependencyPropertyChangedEventArgs e)
    {
        Events.Add("DocumentHeader:template");
        base.OnDocumentHeaderTemplateChanged(e);
    }
    protected override void OnDocumentHeaderTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
    {
        Events.Add("DocumentHeader:selector-enter");
        if (!Suppress) base.OnDocumentHeaderTemplateSelectorChanged(e);
        Events.Add("DocumentHeader:selector-exit");
    }
    protected override void OnDocumentTitleTemplateChanged(DependencyPropertyChangedEventArgs e)
    {
        Events.Add("DocumentTitle:template");
        base.OnDocumentTitleTemplateChanged(e);
    }
    protected override void OnDocumentTitleTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
    {
        Events.Add("DocumentTitle:selector-enter");
        if (!Suppress) base.OnDocumentTitleTemplateSelectorChanged(e);
        Events.Add("DocumentTitle:selector-exit");
    }
    protected override void OnAnchorableHeaderTemplateChanged(DependencyPropertyChangedEventArgs e)
    {
        Events.Add("AnchorableHeader:template");
        base.OnAnchorableHeaderTemplateChanged(e);
    }
    protected override void OnAnchorableHeaderTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
    {
        Events.Add("AnchorableHeader:selector-enter");
        if (!Suppress) base.OnAnchorableHeaderTemplateSelectorChanged(e);
        Events.Add("AnchorableHeader:selector-exit");
    }
    protected override void OnAnchorableTitleTemplateChanged(DependencyPropertyChangedEventArgs e)
    {
        Events.Add("AnchorableTitle:template");
        base.OnAnchorableTitleTemplateChanged(e);
    }
    protected override void OnAnchorableTitleTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
    {
        Events.Add("AnchorableTitle:selector-enter");
        if (!Suppress) base.OnAnchorableTitleTemplateSelectorChanged(e);
        Events.Add("AnchorableTitle:selector-exit");
    }
    protected override void OnDocumentPaneMenuItemHeaderTemplateChanged(DependencyPropertyChangedEventArgs e)
    {
        Events.Add("DocumentPaneMenuItemHeader:template");
        base.OnDocumentPaneMenuItemHeaderTemplateChanged(e);
    }
    protected override void OnDocumentPaneMenuItemHeaderTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
    {
        Events.Add("DocumentPaneMenuItemHeader:selector-enter");
        if (!Suppress) base.OnDocumentPaneMenuItemHeaderTemplateSelectorChanged(e);
        Events.Add("DocumentPaneMenuItemHeader:selector-exit");
    }
}
