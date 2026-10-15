using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class TemplateCallbackProbe
{
    internal static void Verify()
    {
        IDataTemplate first = Template("first"), second = Template("second"), final = Template("final");
        LayoutDocument document = new() { Content = new object() };
        Manager manager = new()
        {
            Layout = new() { RootPanel = new(new LayoutDocumentPane(document)) },
            LayoutItemTemplate = first
        };
        ContentPresenter view = manager.GetLayoutItemFromModel(document)!.View;
        view.UpdateChild();
        IDataTemplate? before = view.ContentTemplate;
        manager.BeforeTemplate = _ =>
        {
            if (!ReferenceEquals(before, view.ContentTemplate))
                throw new Exception("A packaged template callback observed premature presenter delivery.");
            view.UpdateChild();
            VerifyMarker(view, "first");
            throw new InvalidOperationException("Expected callback failure.");
        };
        bool failed = false;
        try { manager.LayoutItemTemplate = second; }
        catch (InvalidOperationException e) when (e.Message == "Expected callback failure.") { failed = true; }
        if (!failed) throw new Exception("The packaged template callback exception did not propagate.");
        manager.BeforeTemplate = null;
        manager.LayoutItemTemplateSelector = new Selector(final);
        view.UpdateChild();
        VerifyMarker(view, "first");
        LayoutDocument added = new() { Content = new object() };
        ((LayoutDocumentPane)document.Parent!).Children.Add(added);
        ContentPresenter addedView = manager.GetLayoutItemFromModel(added)!.View;
        addedView.UpdateChild();
        VerifyMarker(addedView, "second");

        manager.LayoutItemTemplate = first;
        Control? inner = null;
        manager.BeforeTemplate = e =>
        {
            if (!ReferenceEquals(e.NewValue, second)) return;
            manager.LayoutItemTemplate = final;
            view.UpdateChild();
            inner = view.Child;
        };
        manager.LayoutItemTemplate = second;
        view.UpdateChild();
        if (!ReferenceEquals(inner, view.Child))
            throw new Exception("A packaged reentrant template update rebuilt the delivered inner value.");
        VerifyMarker(view, "final");

        manager = new() { SuppressHeader = true, DocumentHeaderTemplate = first };
        manager.DocumentHeaderTemplateSelector = new Selector(first);
        manager.DocumentHeaderTemplateSelector = null;
        AvaloniaPropertyChangedEventArgs captured = manager.HeaderChange!;
        Selector current = new(second);
        manager.DocumentHeaderTemplateSelector = current;
        manager.ReplayHeader(captured);
        if (!ReferenceEquals(first, manager.DocumentHeaderTemplate) ||
            !ReferenceEquals(current, manager.DocumentPaneMenuItemHeaderTemplateSelector))
            throw new Exception("A packaged selector replay confused its supplied argument with current menu fallback.");
    }

    private static IDataTemplate Template(string text)
        => new FuncDataTemplate<object>((_, _) => new TextBlock { Text = text }, supportsRecycling: true);
    private static void VerifyMarker(ContentPresenter view, string text)
    {
        if (view.Child is not TextBlock child || child.Text != text)
            throw new Exception("A packaged presenter did not retain the last delivered template settings.");
    }

    private sealed class Selector(IDataTemplate template) : DataTemplateSelector
    {
        public override IDataTemplate SelectTemplate(object? item, AvaloniaObject container) => template;
    }

    private sealed class Manager : DockingManager
    {
        internal Action<AvaloniaPropertyChangedEventArgs>? BeforeTemplate { get; set; }
        internal bool SuppressHeader { get; set; }
        internal AvaloniaPropertyChangedEventArgs? HeaderChange { get; private set; }
        internal void ReplayHeader(AvaloniaPropertyChangedEventArgs e) => base.OnDocumentHeaderTemplateSelectorChanged(e);
        protected override void OnLayoutItemTemplateChanged(AvaloniaPropertyChangedEventArgs e)
        {
            BeforeTemplate?.Invoke(e);
            base.OnLayoutItemTemplateChanged(e);
        }
        protected override void OnDocumentHeaderTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e)
        {
            HeaderChange = e;
            if (!SuppressHeader) base.OnDocumentHeaderTemplateSelectorChanged(e);
        }
    }
}
