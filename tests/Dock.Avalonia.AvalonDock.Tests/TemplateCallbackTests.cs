using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class TemplateCallbackTests
{
    public static IEnumerable<object[]> PresentationCases()
    {
        foreach (string name in new[] { "Content", "DocumentHeader", "DocumentTitle", "AnchorableHeader", "AnchorableTitle", "DocumentMenu" })
            foreach (bool selector in new[] { false, true })
                foreach (bool fail in new[] { false, true })
                    yield return new object[] { name, selector, fail };
    }

    [AvaloniaTheory]
    [MemberData(nameof(PresentationCases))]
    public void Existing_Presenters_Receive_Provider_Changes_After_The_Protected_Callback_Succeeds(string name, bool selector, bool fail)
    {
        using Workspace workspace = new(name, selector);
        Spec spec = GetSpec(name);
        AvaloniaProperty property = selector ? spec.Selector : spec.Template;
        IDataTemplate? previous = workspace.Presenter.ContentTemplate;
        workspace.Manager.Before = e =>
        {
            if (e.Property != property) return;
            Assert.Same(previous, workspace.Presenter.ContentTemplate);
            workspace.Presenter.UpdateChild();
            AssertMarker(workspace.Presenter, "first");
            if (fail) throw new InvalidOperationException("Provider callback failed.");
        };

        void Change()
        {
            if (selector) workspace.Manager.SetValue(spec.Selector, new Selector(workspace.Second));
            else workspace.Manager.SetValue(spec.Template, workspace.Second);
        }
        if (fail) Assert.Throws<InvalidOperationException>(Change);
        else Change();
        workspace.Manager.Before = null;
        workspace.Update();
        AssertMarker(workspace.Presenter, fail ? "first" : "second");
        Assert.Equal(fail, ReferenceEquals(previous, workspace.Presenter.ContentTemplate));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Reentrant_Content_Provider_Changes_Do_Not_Rebuild_The_Already_Delivered_Inner_Value(bool selector)
    {
        using Workspace workspace = new("Content", selector);
        Spec spec = GetSpec("Content");
        object requested = selector ? new Selector(workspace.Second) : workspace.Second;
        IDataTemplate final = Template("final");
        object redirected = selector ? new Selector(final) : final;
        AvaloniaProperty property = selector ? spec.Selector : spec.Template;
        Control? innerChild = null;
        workspace.Manager.Before = e =>
        {
            if (e.Property != property || !ReferenceEquals(e.NewValue, requested)) return;
            workspace.Manager.SetValue(property, redirected);
            workspace.Presenter.UpdateChild();
            AssertMarker(workspace.Presenter, "final");
            innerChild = workspace.Presenter.Child;
        };
        workspace.Manager.SetValue(property, requested);
        workspace.Update();
        Assert.Same(innerChild, workspace.Presenter.Child);
        AssertMarker(workspace.Presenter, "final");
    }

    [AvaloniaFact]
    public void A_Separate_Selector_Update_Does_Not_Import_A_Previously_Failed_Template_Value()
    {
        using Workspace workspace = new("Content", false);
        workspace.Manager.Before = e =>
        {
            if (e.Property == DockingManager.LayoutItemTemplateProperty) throw new InvalidOperationException("Template failed.");
        };
        Assert.Throws<InvalidOperationException>(() => workspace.Manager.LayoutItemTemplate = workspace.Second);
        workspace.Manager.Before = null;
        workspace.Manager.LayoutItemTemplateSelector = new Selector(Template("selected"));
        workspace.Update();
        AssertMarker(workspace.Presenter, "first");

        LayoutDocument added = new() { Content = new object() };
        ((LayoutDocumentPane)workspace.Document.Parent!).Children.Add(added);
        ContentPresenter newView = workspace.Manager.GetLayoutItemFromModel(added)!.View;
        newView.UpdateChild();
        AssertMarker(newView, "second");
    }

    [AvaloniaFact]
    public void Clearing_A_Later_Explicit_Template_Reveals_The_Last_Delivered_Selector()
    {
        using Workspace workspace = new("Content", true);
        workspace.Manager.Before = e =>
        {
            if (e.Property == DockingManager.LayoutItemTemplateSelectorProperty) throw new InvalidOperationException("Selector failed.");
        };
        Assert.Throws<InvalidOperationException>(() => workspace.Manager.LayoutItemTemplateSelector = new Selector(workspace.Second));
        workspace.Manager.Before = null;
        workspace.Manager.LayoutItemTemplate = Template("temporary");
        workspace.Update();
        AssertMarker(workspace.Presenter, "temporary");
        workspace.Manager.LayoutItemTemplate = null;
        workspace.Update();
        AssertMarker(workspace.Presenter, "first");
    }

    [AvaloniaTheory]
    [InlineData("DocumentHeader")]
    [InlineData("DocumentTitle")]
    [InlineData("AnchorableHeader")]
    [InlineData("AnchorableTitle")]
    [InlineData("DocumentMenu")]
    public void A_Nested_Template_Clear_Does_Not_Deliver_The_Outer_Selector_Before_Its_Callback_Returns(string name)
    {
        using Workspace workspace = new(name, false);
        Spec spec = GetSpec(name);
        Selector selector = new(workspace.Second);
        workspace.Manager.After = e =>
        {
            if (e.Property != spec.Selector) return;
            workspace.Presenter.UpdateChild();
            Assert.Equal(0, selector.CallsFor(workspace.Presenter));
        };
        workspace.Manager.SetValue(spec.Selector, selector);
        workspace.Manager.After = null;
        workspace.Update();
        Assert.True(selector.CallsFor(workspace.Presenter) > 0);
        AssertMarker(workspace.Presenter, "second");
    }

    [AvaloniaTheory]
    [InlineData("DocumentHeader")]
    [InlineData("DocumentTitle")]
    [InlineData("AnchorableHeader")]
    [InlineData("AnchorableTitle")]
    [InlineData("DocumentMenu")]
    public void Throwing_After_A_Selector_Base_Call_Retains_The_Completed_Template_Clear(string name)
    {
        using Workspace workspace = new(name, false);
        Spec spec = GetSpec(name);
        IDataTemplate? original = workspace.Presenter.ContentTemplate;
        Selector selector = new(workspace.Second);
        workspace.Manager.After = e =>
        {
            if (e.Property == spec.Selector) throw new InvalidOperationException("Selector failed after base.");
        };
        Assert.Throws<InvalidOperationException>(() => workspace.Manager.SetValue(spec.Selector, selector));
        workspace.Manager.After = null;
        workspace.Update();
        Assert.Null(workspace.Manager.GetValue(spec.Template));
        Assert.NotSame(original, workspace.Presenter.ContentTemplate);
        Assert.Equal(0, selector.CallsFor(workspace.Presenter));
        Assert.False(workspace.Presenter.Child is TextBlock text && (text.Classes.Contains("callback-first") || text.Classes.Contains("callback-second")));
    }

    [AvaloniaFact]
    public void Document_Header_Changes_Do_Not_Refresh_An_Unchanged_Coerced_Menu_Template()
    {
        using Workspace workspace = new("DocumentMenu", false);
        workspace.Manager.DocumentHeaderTemplate = workspace.First;
        workspace.Manager.DocumentPaneMenuItemHeaderTemplate = null;
        workspace.Update();
        IDataTemplate? previous = workspace.Presenter.ContentTemplate;
        Control? child = workspace.Presenter.Child;
        workspace.Manager.DocumentHeaderTemplate = workspace.Second;
        workspace.Update();
        Assert.Same(workspace.First, workspace.Manager.DocumentPaneMenuItemHeaderTemplate);
        Assert.Same(previous, workspace.Presenter.ContentTemplate);
        Assert.Same(child, workspace.Presenter.Child);
    }

    [AvaloniaTheory]
    [InlineData("DocumentHeader")]
    [InlineData("DocumentTitle")]
    [InlineData("AnchorableHeader")]
    [InlineData("AnchorableTitle")]
    [InlineData("DocumentMenu")]
    public void Replayed_NonNull_Selector_Arguments_Clear_A_Template_After_The_Current_Selector_Was_Cleared(string name)
    {
        Spec spec = GetSpec(name);
        Manager manager = new() { Suppress = true };
        manager.SetValue(spec.Selector, new Selector(Template("selected")));
        AvaloniaPropertyChangedEventArgs captured = manager.Changes[spec.Selector];
        manager.SetValue(spec.Selector, null);
        manager.SetValue(spec.Template, Template("current"));
        manager.ApplySelector(name, captured);
        Assert.Null(manager.GetValue(spec.Template));
        Assert.Null(manager.GetValue(spec.Selector));
    }

    [AvaloniaTheory]
    [InlineData("DocumentHeader")]
    [InlineData("DocumentTitle")]
    [InlineData("AnchorableHeader")]
    [InlineData("AnchorableTitle")]
    [InlineData("DocumentMenu")]
    public void Replayed_Null_Selector_Arguments_Preserve_An_Existing_Template_With_A_Current_Selector(string name)
    {
        Spec spec = GetSpec(name);
        Manager manager = new() { Suppress = true };
        IDataTemplate template = Template("current");
        manager.SetValue(spec.Template, template);
        manager.SetValue(spec.Selector, new Selector(template));
        manager.SetValue(spec.Selector, null);
        AvaloniaPropertyChangedEventArgs captured = manager.Changes[spec.Selector];
        Selector current = new(template);
        manager.SetValue(spec.Selector, current);
        manager.ApplySelector(name, captured);
        Assert.Same(template, manager.GetValue(spec.Template));
        Assert.Same(current, manager.GetValue(spec.Selector));
        if (name == "DocumentHeader") Assert.Same(current, manager.DocumentPaneMenuItemHeaderTemplateSelector);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Document_Menu_Selector_Fallback_Uses_The_Current_Value_At_The_Base_Call(bool beforeBase)
    {
        Manager manager = new();
        Selector requested = new(Template("requested"));
        Selector redirected = new(Template("redirected"));
        void Redirect(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == DockingManager.DocumentHeaderTemplateSelectorProperty && ReferenceEquals(e.NewValue, requested))
                manager.DocumentHeaderTemplateSelector = redirected;
        }
        if (beforeBase) manager.Before = Redirect;
        else manager.After = Redirect;
        manager.DocumentHeaderTemplateSelector = requested;
        Assert.Same(redirected, manager.DocumentHeaderTemplateSelector);
        Assert.Same(beforeBase ? redirected : requested, manager.DocumentPaneMenuItemHeaderTemplateSelector);
    }

    private static IDataTemplate Template(string marker) => new FuncDataTemplate<object>((_, _) =>
        new TextBlock { Text = marker, Classes = { "callback-" + marker } }, supportsRecycling: true);

    private static void AssertMarker(ContentPresenter presenter, string marker)
        => Assert.Contains("callback-" + marker, Assert.IsType<TextBlock>(presenter.Child).Classes);

    private static Spec GetSpec(string name) => name switch
    {
        "Content" => new(DockingManager.LayoutItemTemplateProperty, DockingManager.LayoutItemTemplateSelectorProperty),
        "DocumentHeader" => new(DockingManager.DocumentHeaderTemplateProperty, DockingManager.DocumentHeaderTemplateSelectorProperty),
        "DocumentTitle" => new(DockingManager.DocumentTitleTemplateProperty, DockingManager.DocumentTitleTemplateSelectorProperty),
        "AnchorableHeader" => new(DockingManager.AnchorableHeaderTemplateProperty, DockingManager.AnchorableHeaderTemplateSelectorProperty),
        "AnchorableTitle" => new(DockingManager.AnchorableTitleTemplateProperty, DockingManager.AnchorableTitleTemplateSelectorProperty),
        "DocumentMenu" => new(DockingManager.DocumentPaneMenuItemHeaderTemplateProperty, DockingManager.DocumentPaneMenuItemHeaderTemplateSelectorProperty),
        _ => throw new ArgumentOutOfRangeException(nameof(name))
    };

    private sealed record Spec(StyledProperty<IDataTemplate?> Template, StyledProperty<DataTemplateSelector?> Selector);

    private sealed class Selector(IDataTemplate template) : DataTemplateSelector
    {
        private readonly Dictionary<AvaloniaObject, int> _calls = new();
        internal int CallsFor(AvaloniaObject container) => _calls.GetValueOrDefault(container);
        public override IDataTemplate SelectTemplate(object? item, AvaloniaObject container)
        {
            _calls[container] = CallsFor(container) + 1;
            return template;
        }
    }

    private sealed class Workspace : IDisposable
    {
        internal Manager Manager { get; } = new();
        internal LayoutDocument Document { get; } = new() { Title = "Document", Content = new object() };
        internal LayoutAnchorable Tool { get; } = new() { Title = "Tool", Content = new object() };
        internal IDataTemplate First { get; } = Template("first");
        internal IDataTemplate Second { get; } = Template("second");
        internal ContentPresenter Presenter { get; }
        private readonly Window _window;
        private readonly Control _scope;
        private readonly MenuFlyout? _menu;

        internal Workspace(string name, bool selector)
        {
            Manager.Layout = new()
            {
                RootPanel = new()
                {
                    Children =
                    {
                        new LayoutDocumentPane(Document),
                        new LayoutAnchorablePane { Children = { Tool, new LayoutAnchorable { Title = "Other" } } }
                    }
                }
            };
            Spec spec = GetSpec(name);
            if (selector) Manager.SetValue(spec.Selector, new Selector(First));
            else Manager.SetValue(spec.Template, First);
            _window = new() { Content = Manager, Width = 800, Height = 500 };
            _window.Show();
            Dispatcher.UIThread.RunJobs();
            _scope = Manager;
            if (name == "DocumentTitle")
            {
                Document.Float();
                Dispatcher.UIThread.RunJobs();
                _scope = (Control)Document.FindParent<LayoutDocumentFloatingWindow>()!.DockWindow.Host!;
                ((Window)_scope).UpdateLayout();
            }
            else if (name == "DocumentMenu")
            {
                Button button = Manager.GetVisualDescendants().OfType<Button>().Single(value => value.Classes.Contains("avalondock-document-menu"));
                _menu = (MenuFlyout)button.Flyout!;
                _menu.ShowAt(button);
                Dispatcher.UIThread.RunJobs();
                _scope = _menu.Popup.Child!;
                TopLevel.GetTopLevel(_scope)!.UpdateLayout();
            }
            LayoutContent content = name.StartsWith("Anchorable", StringComparison.Ordinal) ? Tool : Document;
            Presenter = name == "Content" ? Manager.GetLayoutItemFromModel(Document)!.View
                : Assert.Single(_scope.GetVisualDescendants().OfType<ContentPresenter>(), value => ReferenceEquals(value.Content, content)
                    && value.Child is TextBlock text && text.Classes.Contains("callback-first"));
            AssertMarker(Presenter, "first");
        }

        internal void Update()
        {
            Dispatcher.UIThread.RunJobs();
            _window.UpdateLayout();
            TopLevel.GetTopLevel(_scope)?.UpdateLayout();
            Presenter.UpdateChild();
        }

        public void Dispose()
        {
            _menu?.Hide();
            Manager.Layout.FloatingWindows.Clear();
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private sealed class Manager : DockingManager
    {
        internal Action<AvaloniaPropertyChangedEventArgs>? Before { get; set; }
        internal Action<AvaloniaPropertyChangedEventArgs>? After { get; set; }
        internal bool Suppress { get; set; }
        internal Dictionary<AvaloniaProperty, AvaloniaPropertyChangedEventArgs> Changes { get; } = new();
        private void Change(AvaloniaPropertyChangedEventArgs e, Action apply)
        {
            Changes[e.Property] = e;
            Before?.Invoke(e);
            if (!Suppress) apply();
            After?.Invoke(e);
        }
        internal void ApplySelector(string name, AvaloniaPropertyChangedEventArgs e)
        {
            switch (name)
            {
                case "DocumentHeader": base.OnDocumentHeaderTemplateSelectorChanged(e); break;
                case "DocumentTitle": base.OnDocumentTitleTemplateSelectorChanged(e); break;
                case "AnchorableHeader": base.OnAnchorableHeaderTemplateSelectorChanged(e); break;
                case "AnchorableTitle": base.OnAnchorableTitleTemplateSelectorChanged(e); break;
                case "DocumentMenu": base.OnDocumentPaneMenuItemHeaderTemplateSelectorChanged(e); break;
                default: throw new ArgumentOutOfRangeException(nameof(name));
            }
        }
        protected override void OnLayoutItemTemplateChanged(AvaloniaPropertyChangedEventArgs e) => Change(e, () => base.OnLayoutItemTemplateChanged(e));
        protected override void OnLayoutItemTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e) => Change(e, () => base.OnLayoutItemTemplateSelectorChanged(e));
        protected override void OnDocumentHeaderTemplateChanged(AvaloniaPropertyChangedEventArgs e) => Change(e, () => base.OnDocumentHeaderTemplateChanged(e));
        protected override void OnDocumentHeaderTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e) => Change(e, () => base.OnDocumentHeaderTemplateSelectorChanged(e));
        protected override void OnDocumentTitleTemplateChanged(AvaloniaPropertyChangedEventArgs e) => Change(e, () => base.OnDocumentTitleTemplateChanged(e));
        protected override void OnDocumentTitleTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e) => Change(e, () => base.OnDocumentTitleTemplateSelectorChanged(e));
        protected override void OnAnchorableHeaderTemplateChanged(AvaloniaPropertyChangedEventArgs e) => Change(e, () => base.OnAnchorableHeaderTemplateChanged(e));
        protected override void OnAnchorableHeaderTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e) => Change(e, () => base.OnAnchorableHeaderTemplateSelectorChanged(e));
        protected override void OnAnchorableTitleTemplateChanged(AvaloniaPropertyChangedEventArgs e) => Change(e, () => base.OnAnchorableTitleTemplateChanged(e));
        protected override void OnAnchorableTitleTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e) => Change(e, () => base.OnAnchorableTitleTemplateSelectorChanged(e));
        protected override void OnDocumentPaneMenuItemHeaderTemplateChanged(AvaloniaPropertyChangedEventArgs e) => Change(e, () => base.OnDocumentPaneMenuItemHeaderTemplateChanged(e));
        protected override void OnDocumentPaneMenuItemHeaderTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e) => Change(e, () => base.OnDocumentPaneMenuItemHeaderTemplateSelectorChanged(e));
    }
}
