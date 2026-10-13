using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class ItemMetadataProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        int checks = 0;
        foreach (bool tool in new[] { false, true })
        {
            foreach (bool clear in new[] { false, true })
                Check($"null title {tool}/{clear}", () =>
                {
                    var (_, content, item) = Create(tool);
                    item.Title = "previous";
                    if (clear) item.ClearValue(LayoutItem.TitleProperty);
                    else item.Title = null;
                    Require(content.Title is null && content.DockModel.Title is null && item.Title is null);
                    item.Title = "";
                    Require(content.Title == "");
                });
            foreach (bool title in new[] { false, true })
            foreach (bool twoWay in new[] { false, true })
                Check($"binding lifetime {tool}/{title}/{twoWay}", () =>
                {
                    LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
                    AvaloniaProperty property = title ? (AvaloniaProperty)LayoutContent.TitleProperty : LayoutContent.ContentIdProperty;
                    ValueSource source = new() { Value = "source" };
                    CompiledBindingPath path = new CompiledBindingPathBuilder().Property(ValueSource.ValueProperty,
                        PropertyInfoAccessorFactory.CreateAvaloniaPropertyAccessor).Build();
                    using IDisposable binding = content.Bind(property, new CompiledBindingExtension(path)
                        { Source = source, Mode = twoWay ? BindingMode.TwoWay : BindingMode.OneWay });
                    content.SetValue(property, "explicit");
                    Require(Equals(content.GetValue(property), "explicit") && source.Value == (twoWay ? "explicit" : "source"));
                    if (title) content.ClearValue(LayoutContent.TitleProperty);
                    else content.ClearValue(LayoutContent.ContentIdProperty);
                    Require(content.GetValue(property) is null && source.Value == (twoWay ? null : "source"));
                    source.Value = "replacement";
                    Require(Equals(content.GetValue(property), "replacement"));
                    Require(BindingOperations.GetBindingExpressionBase(content, property) is not null);
                    binding.Dispose();
                    content.SetValue(property, "local");
                    source.Value = "detached";
                    Require(Equals(content.GetValue(property), "local"));
                });
        }
        foreach (bool grouped in new[] { false, true })
            Check($"canceled document visibility {grouped}", () =>
            {
                LayoutDocument document = new() { Content = new object(), CanClose = false };
                LayoutDocumentPane pane = new(document);
                DockingManager manager = new() { Layout = new() { RootPanel = new(grouped ? new LayoutDocumentPaneGroup(pane) : pane) } };
                LayoutItem item = manager.GetLayoutItemFromModel(document)!;
                EventHandler<CancelEventArgs> cancel = (_, args) =>
                {
                    Require(!document.IsVisible && !item.IsVisible && pane.IsVisible == !grouped);
                    args.Cancel = true;
                };
                document.Closing += cancel;
                item.IsVisible = false;
                Require(!document.IsVisible && !item.IsVisible && ReferenceEquals(document.Parent, pane));
                document.Closing -= cancel;
                item.IsVisible = true;
                Require(document.IsVisible && pane.IsVisible);
                item.IsVisible = false;
                Require(document.Parent is null && !document.IsVisible);
            });
        foreach (bool canHide in new[] { false, true })
            Check($"tool visibility bypasses hide command {canHide}", () =>
            {
                var (_, content, item) = Create(true);
                LayoutAnchorable tool = (LayoutAnchorable)content;
                tool.CanHide = canHide;
                int hiding = 0, hidden = 0, closing = 0;
                tool.Hiding += (_, args) => { hiding++; args.Cancel = true; };
                tool.Hidden += (_, _) => hidden++;
                tool.Closing += (_, _) => closing++;
                item.IsVisible = false;
                Require(tool.IsHidden && !item.IsVisible && hiding == 0 && hidden == 1 && closing == 0);
                item.IsVisible = true;
                Require(tool.IsVisible);
            });
        foreach (bool tool in new[] { false, true })
            Check($"visibility failure and retry {tool}", () =>
            {
                var (_, content, item) = Create(tool);
                EventHandler failure = (_, _) => throw new InvalidOperationException("expected callback failure");
                EventHandler<CancelEventArgs> closeFailure = (_, args) => failure(null, args);
                if (content is LayoutAnchorable anchorable) anchorable.Hidden += failure;
                else content.Closing += closeFailure;
                try { item.IsVisible = false; throw new Exception("Expected callback failure was not raised."); }
                catch (InvalidOperationException e) when (e.Message == "expected callback failure") { }
                Require(!item.IsVisible);
                if (content is LayoutAnchorable hiddenTool)
                {
                    Require(hiddenTool.IsHidden);
                    hiddenTool.Hidden -= failure;
                }
                else
                {
                    Require(!((LayoutDocument)content).IsVisible);
                    content.Closing -= closeFailure;
                }
                item.IsVisible = true;
                item.IsVisible = false;
                Require(content is LayoutAnchorable next ? next.IsHidden : content.Parent is null);
            });
        if (failures.Count != 0) throw new InvalidOperationException($"Packaged item metadata: {failures.Count}/{checks} failed: " + string.Join("; ", failures));
        Console.WriteLine($"Item metadata: {checks} packaged checks passed.");

        void Check(string name, Action action)
        {
            checks++;
            try { action(); }
            catch (Exception error) { failures.Add(name + ": " + error.Message); }
        }
    }

    private static (DockingManager, LayoutContent, LayoutItem) Create(bool tool)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        content.Content = new object();
        ILayoutPanelElement pane = content is LayoutAnchorable anchorable ? new LayoutAnchorablePane(anchorable) : new LayoutDocumentPane(content);
        DockingManager manager = new() { Layout = new() { RootPanel = new(pane) } };
        return (manager, content, manager.GetLayoutItemFromModel(content)!);
    }

    private static void Require(bool condition)
    {
        if (!condition) throw new InvalidOperationException("Item metadata callback contract changed.");
    }

    private sealed class ValueSource : AvaloniaObject
    {
        public static readonly StyledProperty<string?> ValueProperty = AvaloniaProperty.Register<ValueSource, string?>(nameof(Value));
        public string? Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    }
}
