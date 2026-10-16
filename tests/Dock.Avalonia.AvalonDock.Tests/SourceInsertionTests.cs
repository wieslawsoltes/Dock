using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using AvalonDock;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class SourceInsertionTests
{
    [AvaloniaTheory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void A_Source_Batch_Keeps_Its_Destination_When_A_Strategy_Changes_Focus(bool documents, bool initialImport)
    {
        LayoutContent first = CreateContent(documents, "first");
        LayoutContent second = CreateContent(documents, "second");
        ILayoutGroup firstPane = CreatePane(first);
        ILayoutGroup secondPane = CreatePane(second);
        LayoutPanel panel = new();
        panel.Children.Add((ILayoutPanelElement)firstPane);
        panel.Children.Add((ILayoutPanelElement)secondPane);
        RecordingStrategy strategy = new() { After = _ => second.IsActive = true };
        DockingManager manager = new() { Layout = new() { RootPanel = panel }, LayoutUpdateStrategy = strategy };
        first.IsActive = true;
        RangeSource source = new();
        object[] batch = { new object(), new object() };
        if (initialImport)
        {
            source.AddRange(batch);
            SetSource(manager, source, documents);
        }
        else
        {
            SetSource(manager, source, documents);
            source.AddRange(batch);
        }

        Assert.Equal(2, strategy.Destinations.Count);
        Assert.All(strategy.Destinations, destination => Assert.Same(firstPane, destination));
        Assert.Equal(batch, firstPane.Children.OfType<LayoutContent>().Skip(1).Select(content => content.Content));
        Assert.Single(secondPane.Children);

        object next = new();
        source.Add(next);
        Assert.Same(secondPane, strategy.Destinations[2]);
        Assert.Same(next, secondPane.Children.OfType<LayoutContent>().Last().Content);
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void A_New_Tool_Pane_Is_Reused_For_The_Rest_Of_The_Batch(bool initialImport)
    {
        LayoutAnchorable other = new() { Content = "other" };
        LayoutAnchorablePane otherPane = new(other);
        RecordingStrategy strategy = new();
        DockingManager manager = new() { LayoutUpdateStrategy = strategy };
        strategy.After = _ =>
        {
            if (otherPane.Parent is null) manager.Layout.RootPanel.Children.Add(otherPane);
            other.IsActive = true;
        };
        RangeSource source = new();
        object[] batch = { new object(), new object() };
        if (initialImport)
        {
            source.AddRange(batch);
            manager.AnchorablesSource = source;
        }
        else
        {
            manager.AnchorablesSource = source;
            source.AddRange(batch);
        }

        Assert.Null(strategy.Destinations[0]);
        LayoutAnchorablePane created = Assert.IsType<LayoutAnchorablePane>(strategy.Destinations[1]);
        Assert.Equal(new GridLength(200), created.DockWidth);
        Assert.Equal(batch, created.Children.Select(content => content.Content));
        Assert.Single(otherPane.Children);
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Initial_Imports_Are_Snapshotted_Before_Strategies_Edit_The_Source(bool documents)
    {
        object first = new();
        object second = new();
        RangeSource source = new() { first, second };
        RecordingStrategy strategy = new();
        strategy.After = _ =>
        {
            strategy.After = null;
            source.Add(new object());
        };
        DockingManager manager = new() { LayoutUpdateStrategy = strategy };
        SetSource(manager, source, documents);
        Assert.Equal(3, source.Count);
        Assert.Equal(new[] { first, second }, manager.Layout.Descendents().OfType<LayoutContent>().Select(content => content.Content));

        object subsequent = new();
        source.Add(subsequent);
        Assert.Equal(new[] { first, second, subsequent }, manager.Layout.Descendents().OfType<LayoutContent>().Select(content => content.Content));
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Initial_Imports_Skip_Existing_Values_And_Nulls_But_Preserve_Source_Duplicates(bool documents)
    {
        SourceValue existing = new("existing");
        SourceValue repeated = new("repeated");
        SourceValue equalRepeated = new("repeated");
        LayoutContent manual = CreateContent(documents, existing);
        LayoutPanel panel = new();
        panel.Children.Add((ILayoutPanelElement)CreatePane(manual));
        DockingManager manager = new() { Layout = new() { RootPanel = panel } };
        RangeSource source = new() { new SourceValue("existing"), repeated, equalRepeated, null };
        SetSource(manager, source, documents);

        LayoutContent[] imported = manager.Layout.Descendents().OfType<LayoutContent>().ToArray();
        Assert.Equal(3, imported.Length);
        Assert.Same(manual, imported[0]);
        Assert.Same(repeated, imported[1].Content);
        Assert.Same(equalRepeated, imported[2].Content);
        Assert.NotEqual(imported[1].ContentId, imported[2].ContentId);

        // Collection notifications preserve every new item, including null and
        // values equal to content already in the layout.
        source.AddRange(new object?[] { existing, new SourceValue("existing"), null });
        Assert.Equal(6, manager.Layout.Descendents().OfType<LayoutContent>().Count());
        Assert.Null(manager.Layout.Descendents().OfType<LayoutContent>().Last().Content);
    }

    private static LayoutContent CreateContent(bool documents, object content) => documents
        ? new LayoutDocument { Content = content }
        : new LayoutAnchorable { Content = content };

    private static ILayoutGroup CreatePane(LayoutContent content) => content is LayoutDocument document
        ? new LayoutDocumentPane(document)
        : new LayoutAnchorablePane((LayoutAnchorable)content);

    private static void SetSource(DockingManager manager, IEnumerable source, bool documents)
    {
        if (documents) manager.DocumentsSource = source;
        else manager.AnchorablesSource = source;
    }

    private sealed class RangeSource : ObservableCollection<object?>
    {
        internal void AddRange(object?[] items)
        {
            int index = Count;
            foreach (object? item in items) Items.Add(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items, index));
        }
    }

    private sealed class RecordingStrategy : ILayoutUpdateStrategy
    {
        internal List<ILayoutContainer?> Destinations { get; } = new();
        internal Action<LayoutContent>? After { get; set; }
        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument document, ILayoutContainer? destination)
        {
            Destinations.Add(destination);
            return false;
        }
        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable tool, ILayoutContainer? destination)
        {
            Destinations.Add(destination);
            return false;
        }
        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument document) => After?.Invoke(document);
        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable tool) => After?.Invoke(tool);
    }

    private sealed record SourceValue(string Key);
}
