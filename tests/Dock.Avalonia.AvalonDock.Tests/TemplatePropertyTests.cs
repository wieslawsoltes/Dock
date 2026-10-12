using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class TemplatePropertyTests
{
    public static IEnumerable<object[]> Cases()
    {
        foreach (string name in new[] { "DocumentHeader", "DocumentTitle", "AnchorableHeader", "AnchorableTitle", "DocumentPaneMenuItemHeader" })
            foreach (bool suppress in new[] { false, true })
                foreach (string source in new[] { "local", "binding", "style" })
                    yield return new object[] { name, suppress, source };
    }

    [AvaloniaTheory]
    [MemberData(nameof(Cases))]
    public void Template_Properties_Match_Original_WPF_Observations(string name, bool suppress, string source)
    {
        Spec spec = GetSpec(name);
        TraceManager manager = new() { Suppress = suppress };
        IDataTemplate first = new FuncDataTemplate<object>(static (_, _) => new Border());
        IDataTemplate second = new FuncDataTemplate<object>(static (_, _) => new Border());
        DataTemplateSelector selector = new(); DataTemplateSelector replacement = new();
        string scenario = source + (suppress ? "-override" : "-base");
        List<string> rows = new();
        void Take(string stage)
        {
            object? template = manager.GetValue(spec.Template);
            object? selected = manager.GetValue(spec.Selector);
            string t = template is null ? "null" : ReferenceEquals(template, first) ? "first" : ReferenceEquals(template, second) ? "second" : "other";
            string s = selected is null ? "null" : ReferenceEquals(selected, selector) ? "first" : ReferenceEquals(selected, replacement) ? "second" : "other";
            bool bound = BindingOperations.GetBindingExpressionBase(manager, spec.Template) is not null;
            rows.Add(string.Join('\t', name, scenario, stage, t, s, bound, string.Join('>', manager.Events)));
            manager.Events.Clear();
        }
        Take("initial");
        using IDisposable? binding = source == "binding"
            ? manager.Bind(spec.Template, new CompiledBindingExtension { Source = first }) : null;
        if (source == "style")
        {
            manager.Styles.Add(new Style { Setters = { new Setter(spec.Template, first) } });
            manager.ApplyStyling();
        }
        else if (source == "local") manager.SetValue(spec.Template, first);
        Take("first-template");
        manager.SetValue(spec.Selector, selector); Take("set-selector");
        // WPF SetValue replaces a binding; Avalonia SetValue keeps it. The
        // external assignment uses explicit disposal for the equivalent input.
        // The preceding selector stage verifies the adapter's own disposal.
        binding?.Dispose();
        manager.SetValue(spec.Template, second); Take("second-template");
        manager.SetValue(spec.Selector, replacement); Take("replace-selector");
        manager.SetValue(spec.Selector, null); Take("clear-selector");
        manager.CoerceValue(spec.Template); Take("explicit-coerce");
        manager.SetValue(spec.Template, first); Take("reassign-first");
        Assert.Equal(Observations(name, scenario), rows);
    }

    [AvaloniaFact]
    public void Menu_Fallback_Is_Updated_By_Assignment_Or_Explicit_Coercion()
    {
        TraceManager menu = new();
        IDataTemplate headerOne = new FuncDataTemplate<object>(static (_, _) => new Border());
        IDataTemplate headerTwo = new FuncDataTemplate<object>(static (_, _) => new Border());
        List<string> rows = new();
        void Take(string stage) => rows.Add(string.Join('\t', "MenuFallback", "base", stage,
            menu.DocumentPaneMenuItemHeaderTemplate is null ? "null" : ReferenceEquals(menu.DocumentPaneMenuItemHeaderTemplate, headerOne) ? "first" : ReferenceEquals(menu.DocumentPaneMenuItemHeaderTemplate, headerTwo) ? "second" : "other", "-", false, string.Join('>', menu.Events)));
        Take("initial");
        menu.DocumentHeaderTemplate = headerOne; Take("set-header");
        menu.DocumentPaneMenuItemHeaderTemplate = null; Take("set-menu-null");
        menu.DocumentHeaderTemplate = headerTwo; Take("replace-header");
        menu.DocumentPaneMenuItemHeaderTemplateSelector = new DataTemplateSelector(); Take("set-menu-selector");
        menu.DocumentHeaderTemplate = null; Take("clear-header");
        menu.CoerceValue(DockingManager.DocumentPaneMenuItemHeaderTemplateProperty); Take("coerce-menu");
        Assert.Equal(Observations("MenuFallback", "base"), rows);
    }

    [AvaloniaTheory]
    [InlineData("DocumentHeader")]
    [InlineData("DocumentTitle")]
    [InlineData("AnchorableHeader")]
    [InlineData("AnchorableTitle")]
    [InlineData("DocumentPaneMenuItemHeader")]
    public void Suppressing_The_Base_Callback_Preserves_The_Rendered_Explicit_Template(string name)
    {
        Spec spec = GetSpec(name);
        DocumentMenuFixture resources = new();
        TraceManager manager = new() { Suppress = true };
        LayoutDocument document = new() { Title = "Document" };
        LayoutAnchorable tool = new() { Title = "Tool" };
        manager.Layout.RootPanel = new LayoutPanel
        {
            Children =
            {
                new LayoutAnchorablePane { Children = { tool, new LayoutAnchorable { Title = "Other tool" } } },
                new LayoutDocumentPane(document)
            }
        };
        IDataTemplate template = (IDataTemplate)resources.Resources["MenuHeader"]!;
        manager.SetValue(spec.Template, template);
        CountingSelector selector = new() { Template = (IDataTemplate)resources.Resources["AlternateHeader"]! };
        manager.SetValue(spec.Selector, selector);
        Window window = new() { Width = 850, Height = 550, Content = manager };
        MenuFlyout? menu = null;
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            Control scope = manager;
            if (name == "DocumentTitle")
            {
                document.Float();
                Dispatcher.UIThread.RunJobs();
                scope = (Control)document.FindParent<LayoutDocumentFloatingWindow>()!.DockWindow.Host!;
                ((Window)scope).UpdateLayout();
            }
            else if (name == "DocumentPaneMenuItemHeader")
            {
                Button button = manager.GetVisualDescendants().OfType<Button>()
                    .Single(value => value.Classes.Contains("avalondock-document-menu"));
                menu = (MenuFlyout)button.Flyout!;
                menu.ShowAt(button);
                Dispatcher.UIThread.RunJobs();
                scope = menu.Popup.Child!;
                TopLevel.GetTopLevel(scope)!.UpdateLayout();
            }
            Assert.Same(template, manager.GetValue(spec.Template));
            Assert.Contains(scope.GetVisualDescendants().OfType<TextBlock>(), text => text.Classes.Contains("menu-header"));
            Assert.Equal(0, selector.Calls);
        }
        finally
        {
            menu?.Hide();
            manager.Layout.FloatingWindows.Clear();
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaTheory]
    [InlineData("DocumentHeader")]
    [InlineData("DocumentTitle")]
    [InlineData("AnchorableHeader")]
    [InlineData("AnchorableTitle")]
    [InlineData("DocumentPaneMenuItemHeader")]
    public void Selector_Callbacks_Preserve_Style_Bindings_Below_The_Local_Override(string name)
    {
        Spec spec = GetSpec(name);
        DockingManager manager = new();
        IDataTemplate template = new FuncDataTemplate<object>(static (_, _) => new Border());
        manager.Styles.Add(new Style
        {
            Setters = { new Setter(spec.Template, new CompiledBindingExtension { Source = template }) }
        });
        manager.ApplyStyling();
        Assert.Same(template, manager.GetValue(spec.Template));
        manager.SetValue(spec.Selector, new DataTemplateSelector());
        Assert.Null(manager.GetValue(spec.Template));
        manager.SetValue(spec.Selector, null);
        manager.ClearValue(spec.Template);
        Assert.Same(template, manager.GetValue(spec.Template));
        Assert.NotNull(BindingOperations.GetBindingExpressionBase(manager, spec.Template));
    }

    private sealed class CountingSelector : DataTemplateSelector
    {
        public IDataTemplate? Template { get; init; }
        public int Calls { get; private set; }
        public override IDataTemplate? SelectTemplate(object? item, AvaloniaObject container)
        {
            Calls++;
            return Template;
        }
    }

    private static IEnumerable<string> Observations(string name, string scenario) =>
        File.ReadLines(Path.Combine(AppContext.BaseDirectory, "Assets", "WpfTemplateProperties.tsv"))
            .Where(row => row.StartsWith(name + "\t" + scenario + "\t", StringComparison.Ordinal));

    private static Spec GetSpec(string name) => name switch
    {
        "DocumentHeader" => new(DockingManager.DocumentHeaderTemplateProperty, DockingManager.DocumentHeaderTemplateSelectorProperty),
        "DocumentTitle" => new(DockingManager.DocumentTitleTemplateProperty, DockingManager.DocumentTitleTemplateSelectorProperty),
        "AnchorableHeader" => new(DockingManager.AnchorableHeaderTemplateProperty, DockingManager.AnchorableHeaderTemplateSelectorProperty),
        "AnchorableTitle" => new(DockingManager.AnchorableTitleTemplateProperty, DockingManager.AnchorableTitleTemplateSelectorProperty),
        "DocumentPaneMenuItemHeader" => new(DockingManager.DocumentPaneMenuItemHeaderTemplateProperty, DockingManager.DocumentPaneMenuItemHeaderTemplateSelectorProperty),
        _ => throw new ArgumentOutOfRangeException(nameof(name))
    };

    private sealed record Spec(StyledProperty<IDataTemplate?> Template, StyledProperty<DataTemplateSelector?> Selector);

    private sealed class TraceManager : DockingManager
    {
        internal bool Suppress { get; init; }
        internal List<string> Events { get; } = new();
        protected override void OnDocumentHeaderTemplateChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Events.Add("DocumentHeader:template");
            base.OnDocumentHeaderTemplateChanged(e);
        }
        protected override void OnDocumentHeaderTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Events.Add("DocumentHeader:selector-enter");
            if (!Suppress) base.OnDocumentHeaderTemplateSelectorChanged(e);
            Events.Add("DocumentHeader:selector-exit");
        }
        protected override void OnDocumentTitleTemplateChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Events.Add("DocumentTitle:template");
            base.OnDocumentTitleTemplateChanged(e);
        }
        protected override void OnDocumentTitleTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Events.Add("DocumentTitle:selector-enter");
            if (!Suppress) base.OnDocumentTitleTemplateSelectorChanged(e);
            Events.Add("DocumentTitle:selector-exit");
        }
        protected override void OnAnchorableHeaderTemplateChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Events.Add("AnchorableHeader:template");
            base.OnAnchorableHeaderTemplateChanged(e);
        }
        protected override void OnAnchorableHeaderTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Events.Add("AnchorableHeader:selector-enter");
            if (!Suppress) base.OnAnchorableHeaderTemplateSelectorChanged(e);
            Events.Add("AnchorableHeader:selector-exit");
        }
        protected override void OnAnchorableTitleTemplateChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Events.Add("AnchorableTitle:template");
            base.OnAnchorableTitleTemplateChanged(e);
        }
        protected override void OnAnchorableTitleTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Events.Add("AnchorableTitle:selector-enter");
            if (!Suppress) base.OnAnchorableTitleTemplateSelectorChanged(e);
            Events.Add("AnchorableTitle:selector-exit");
        }
        protected override void OnDocumentPaneMenuItemHeaderTemplateChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Events.Add("DocumentPaneMenuItemHeader:template");
            base.OnDocumentPaneMenuItemHeaderTemplateChanged(e);
        }
        protected override void OnDocumentPaneMenuItemHeaderTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Events.Add("DocumentPaneMenuItemHeader:selector-enter");
            if (!Suppress) base.OnDocumentPaneMenuItemHeaderTemplateSelectorChanged(e);
            Events.Add("DocumentPaneMenuItemHeader:selector-exit");
        }
    }
}
