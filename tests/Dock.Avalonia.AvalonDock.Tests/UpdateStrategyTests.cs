using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Headless.XUnit;
using AvalonDock;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class UpdateStrategyTests
{
    public static IEnumerable<object[]> InsertionCases()
    {
        foreach (string operation in new[] { "initial-document", "initial-tool", "live-document", "live-tool", "dock-document", "dock-tool", "show" })
            foreach (bool replace in new[] { false, true })
                foreach (bool handled in new[] { false, true })
                    yield return new object[] { operation, replace, handled };
    }

    [AvaloniaTheory]
    [MemberData(nameof(InsertionCases))]
    public void After_Insert_Uses_The_Strategy_Current_After_Before_Insert(string operation, bool replace, bool handled)
    {
        DockingManager manager = CreateManager();
        LayoutRoot root = manager.Layout;
        LayoutContent? inserted = null;
        ILayoutContainer? destination = null;
        List<string> calls = new();
        Strategy replacement = new()
        {
            After = (layout, content) =>
            {
                Assert.Same(root, layout);
                Assert.Same(inserted, content);
                if (!handled) Assert.Same(destination, content.Parent);
                calls.Add("replacement:after");
            }
        };
        Strategy original = new()
        {
            Before = (layout, content, target) =>
            {
                Assert.Same(root, layout);
                inserted = content;
                destination = target;
                manager.LayoutUpdateStrategy = replace ? replacement : null;
                calls.Add("original:before");
                return handled;
            },
            After = (_, _) => calls.Add("original:after")
        };

        if (operation.StartsWith("dock", StringComparison.Ordinal))
        {
            LayoutContent content = operation.EndsWith("document", StringComparison.Ordinal)
                ? new LayoutDocument { Content = new object() } : new LayoutAnchorable { Content = new object() };
            ILayoutGroup pane = content is LayoutDocument
                ? root.Descendents().OfType<LayoutDocumentPane>().Single()
                : root.Descendents().OfType<LayoutAnchorablePane>().Single();
            pane.InsertChildAt(pane.ChildrenCount, content);
            content.Float();
            Assert.True(content.IsFloating);
            content.PreviousContainer = null;
            manager.LayoutUpdateStrategy = original;
            content.Dock();
            Assert.Equal(handled, content.IsFloating);
        }
        else if (operation == "show")
        {
            LayoutAnchorable content = root.Descendents().OfType<LayoutAnchorable>().Single();
            content.Hide();
            manager.LayoutUpdateStrategy = original;
            content.Show();
            Assert.Equal(handled, content.IsHidden);
        }
        else
        {
            bool documents = operation.EndsWith("document", StringComparison.Ordinal);
            ObservableCollection<object> source = new();
            if (operation.StartsWith("live", StringComparison.Ordinal)) SetSource(manager, source, documents);
            manager.LayoutUpdateStrategy = original;
            source.Add(new object());
            if (operation.StartsWith("initial", StringComparison.Ordinal)) SetSource(manager, source, documents);
            Assert.Equal(handled, inserted!.Parent is null);
        }

        Assert.NotNull(inserted);
        Assert.Equal(replace ? new[] { "original:before", "replacement:after" } : new[] { "original:before" }, calls);
    }

    [AvaloniaTheory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void Layout_Replacement_Keeps_The_Initial_Root_But_Live_Callbacks_Read_The_Current_Root(bool documents, bool initialImport)
    {
        DockingManager manager = CreateManager();
        LayoutRoot oldRoot = manager.Layout;
        LayoutRoot newRoot = CreateRoot();
        RangeSource source = new();
        object first = new();
        object second = new();
        List<(string Phase, LayoutRoot Root, LayoutContent Content)> calls = new();
        bool replaced = false;
        manager.LayoutUpdateStrategy = new Strategy
        {
            Before = (root, content, _) =>
            {
                calls.Add(("before", root, content));
                if (!replaced)
                {
                    replaced = true;
                    manager.Layout = newRoot;
                }
                return false;
            },
            After = (root, content) => calls.Add(("after", root, content))
        };
        if (initialImport)
        {
            source.AddRange(first, second);
            SetSource(manager, source, documents);
        }
        else
        {
            SetSource(manager, source, documents);
            source.AddRange(first, second);
        }

        Assert.Equal(8, calls.Count); // Two nested imports plus the original pair.
        LayoutContent firstOuter = calls[0].Content;
        Assert.Same(oldRoot, calls[0].Root);
        Assert.Equal("before", calls[0].Phase);
        Assert.All(calls.Skip(1).Take(4), call => Assert.Same(newRoot, call.Root));
        Assert.Same(firstOuter, calls[5].Content);
        Assert.Equal("after", calls[5].Phase);
        Assert.All(calls.Skip(5), call => Assert.Same(initialImport ? oldRoot : newRoot, call.Root));
        Assert.Same(first, firstOuter.Content);
        Assert.Same(second, calls[6].Content.Content);
        Assert.Same(calls[6].Content, calls[7].Content);
        // Destination selection remains per batch even when callback arguments
        // follow a replacement layout, matching the upstream collection API.
        Assert.Same(oldRoot, firstOuter.Root);
        Assert.Same(oldRoot, calls[6].Content.Root);
        Assert.Equal(2, newRoot.Descendents().OfType<LayoutContent>().Count(content => ReferenceEquals(content.Content, first) || ReferenceEquals(content.Content, second)));
    }

    [AvaloniaTheory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void Replacing_A_Strategy_During_A_Batch_Applies_To_The_Following_Items(bool documents, bool initialImport)
    {
        DockingManager manager = CreateManager();
        object first = new();
        object second = new();
        RangeSource source = new();
        List<(string Phase, object? Content)> calls = new();
        Strategy replacement = new()
        {
            Before = (_, content, _) => { calls.Add(("replacement:before", content.Content)); return false; },
            After = (_, content) => calls.Add(("replacement:after", content.Content))
        };
        manager.LayoutUpdateStrategy = new Strategy
        {
            Before = (_, content, _) =>
            {
                calls.Add(("original:before", content.Content));
                manager.LayoutUpdateStrategy = replacement;
                return false;
            },
            After = (_, content) => calls.Add(("original:after", content.Content))
        };
        if (initialImport)
        {
            source.AddRange(first, second);
            SetSource(manager, source, documents);
        }
        else
        {
            SetSource(manager, source, documents);
            source.AddRange(first, second);
        }

        Assert.Equal(new (string, object?)[]
        {
            ("original:before", first), ("replacement:after", first),
            ("replacement:before", second), ("replacement:after", second)
        }, calls);
    }

    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void Show_Uses_The_Return_Pane_And_Index_Changed_By_The_Strategy(int index)
    {
        DockingManager manager = CreateManager();
        LayoutAnchorable tool = manager.Layout.Descendents().OfType<LayoutAnchorable>().Single();
        LayoutAnchorablePane original = (LayoutAnchorablePane)tool.Parent!;
        LayoutAnchorable first = new();
        LayoutAnchorable second = new();
        LayoutAnchorablePane target = new() { Children = { first, second } };
        manager.Layout.RootPanel.Children.Add(target);
        tool.Hide();
        int oldPaneAdditions = 0;
        original.Children.CollectionChanged += (_, args) =>
        {
            if (args.Action == NotifyCollectionChangedAction.Add) oldPaneAdditions++;
        };
        int after = 0;
        manager.LayoutUpdateStrategy = new Strategy
        {
            Before = (_, content, destination) =>
            {
                Assert.Same(tool, content);
                Assert.Same(original, destination);
                content.PreviousContainer = target;
                content.PreviousContainerIndex = index;
                return false;
            },
            After = (_, content) =>
            {
                Assert.Same(target, content.Parent);
                Assert.Same(target, content.PreviousContainer);
                Assert.Same(tool, target.Children[Math.Min(index, 2)]);
                Assert.Same(target.DockModel, tool.DockModel.Owner);
                Assert.DoesNotContain(tool, manager.Layout.Hidden);
                after++;
            }
        };

        tool.Show();

        Assert.Equal(1, after);
        Assert.Equal(0, oldPaneAdditions);
        Assert.Same(tool, target.SelectedContent);
        Assert.Same(tool, manager.Layout.ActiveContent);
        Assert.Null(tool.PreviousContainer);
        Assert.Equal(-1, tool.PreviousContainerIndex);
    }

    [AvaloniaFact]
    public void Clearing_The_Return_Pane_In_Before_Show_Leaves_The_Tool_Hidden_And_Still_Calls_After()
    {
        DockingManager manager = CreateManager();
        LayoutAnchorable tool = manager.Layout.Descendents().OfType<LayoutAnchorable>().Single();
        tool.Hide();
        int after = 0;
        manager.LayoutUpdateStrategy = new Strategy
        {
            Before = (_, content, _) => { content.PreviousContainer = null; return false; },
            After = (_, content) => { Assert.True(((LayoutAnchorable)content).IsHidden); after++; }
        };

        tool.Show();

        Assert.Equal(1, after);
        Assert.Same(tool, Assert.Single(manager.Layout.Hidden));
        Assert.Same(manager.Layout.DockModel, tool.DockModel.Owner);
    }

    [AvaloniaFact]
    public void Docking_The_Last_Focused_Document_Chooses_The_First_Pane_Instead_Of_Its_Own_Pane()
    {
        LayoutDocument first = new();
        LayoutDocument moving = new();
        LayoutDocumentPane firstPane = new(first);
        LayoutDocumentPane original = new(moving);
        DockingManager manager = new()
        {
            Layout = new() { RootPanel = new LayoutPanel { Children = { firstPane, original } } }
        };
        moving.IsActive = true;
        Assert.Same(moving, manager.Layout.LastFocusedDocument);
        manager.LayoutUpdateStrategy = new Strategy
        {
            Before = (_, _, destination) => { Assert.Same(firstPane, destination); return false; }
        };

        moving.Dock();

        Assert.Equal(new[] { first, moving }, firstPane.Children);
        Assert.Same(firstPane.DockModel, moving.DockModel.Owner);
        Assert.Same(moving, manager.Layout.ActiveContent);
    }

    private static DockingManager CreateManager() => new() { Layout = CreateRoot() };

    private static LayoutRoot CreateRoot() => new()
    {
        RootPanel = new LayoutPanel
        {
            Children = { new LayoutDocumentPane(new LayoutDocument()), new LayoutAnchorablePane(new LayoutAnchorable()) }
        }
    };

    private static void SetSource(DockingManager manager, IEnumerable source, bool documents)
    {
        if (documents) manager.DocumentsSource = source;
        else manager.AnchorablesSource = source;
    }

    private sealed class RangeSource : ObservableCollection<object>
    {
        internal void AddRange(params object[] values)
        {
            int index = Count;
            foreach (object value in values) Items.Add(value);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, values, index));
        }
    }

    private sealed class Strategy : ILayoutUpdateStrategy
    {
        internal Func<LayoutRoot, LayoutContent, ILayoutContainer?, bool>? Before { get; init; }
        internal Action<LayoutRoot, LayoutContent>? After { get; init; }
        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument document, ILayoutContainer? destination) => Before?.Invoke(layout, document, destination) ?? false;
        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable tool, ILayoutContainer? destination) => Before?.Invoke(layout, tool, destination) ?? false;
        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument document) => After?.Invoke(layout, document);
        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable tool) => After?.Invoke(layout, tool);
    }
}
