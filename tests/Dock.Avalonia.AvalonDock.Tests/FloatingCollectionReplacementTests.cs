using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class FloatingCollectionReplacementTests
{
    [AvaloniaFact]
    public void Replacing_A_Retained_Closed_Model_With_Itself_Does_Not_Reopen_Its_Host()
    {
        DockingManager manager = new();
        LayoutFloatingWindow model = Create(false);
        Window view = new() { Content = manager, Width = 800, Height = 500 };
        view.Show();
        manager.Layout.FloatingWindows.Add(model);
        Dispatcher.UIThread.RunJobs();
        try
        {
            ((LayoutFloatingWindowControl)model.DockWindow.Host!).Close();
            Dispatcher.UIThread.RunJobs();
            Assert.Same(model, Assert.Single(manager.Layout.FloatingWindows));
            Assert.Null(model.DockWindow.Host);

            manager.Layout.FloatingWindows[0] = model;

            Assert.Null(model.DockWindow.Host);
            Assert.Empty(manager.FloatingWindows);
        }
        finally { view.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Canceled_Replacement_Still_Publishes_Windows_Added_By_The_Closing_Callback(bool tools)
    {
        DockingManager manager = new();
        LayoutFloatingWindow previous = Create(tools);
        LayoutFloatingWindow incoming = Create(!tools);
        LayoutFloatingWindow sibling = Create(false);
        Window view = new() { Content = manager, Width = 800, Height = 500 };
        view.Show();
        manager.Layout.FloatingWindows.Add(previous);
        Dispatcher.UIThread.RunJobs();
        bool cancel = true;
        manager.DockFactory.WindowClosing += (_, args) =>
        {
            if (!cancel) return;
            manager.Layout.FloatingWindows.Add(sibling);
            args.Cancel = true;
        };
        try
        {
            manager.Layout.FloatingWindows[0] = incoming;

            Assert.Equal(new[] { previous, sibling }, manager.Layout.FloatingWindows);
            Assert.Equal(new[] { previous.DockWindow, sibling.DockWindow }, ((RootDock)manager.Layout.DockModel).Windows!);
            Assert.Same(manager.Layout, previous.Parent);
            Assert.Same(manager.Layout, sibling.Parent);
            Assert.Null(incoming.Parent);
        }
        finally { cancel = false; view.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [AvaloniaFact]
    public void Retrying_A_Failed_Parent_Callback_Presents_The_Replacement_Host()
    {
        DockingManager manager = new();
        LayoutFloatingWindow previous = Create(false);
        LayoutFloatingWindow incoming = Create(true);
        Window view = new() { Content = manager, Width = 800, Height = 500 };
        view.Show();
        manager.Layout.FloatingWindows.Add(previous);
        Dispatcher.UIThread.RunJobs();
        bool fail = true;
        incoming.PropertyChanged += (_, args) =>
        {
            if (fail && args.PropertyName == "Parent") throw new InvalidOperationException("parent callback");
        };
        try
        {
            Assert.Throws<InvalidOperationException>(() => manager.Layout.FloatingWindows[0] = incoming);
            fail = false;
            manager.Layout.FloatingWindows[0] = incoming;
            Dispatcher.UIThread.RunJobs();

            Assert.True(((LayoutFloatingWindowControl)incoming.DockWindow.Host!).IsVisible);
            Assert.Same(incoming, Assert.Single(manager.Layout.FloatingWindows));
        }
        finally { fail = false; view.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [AvaloniaTheory]
    [InlineData("indexer")]
    [InlineData("outgoing")]
    [InlineData("incoming")]
    public void A_Failing_Replacement_Callback_Leaves_Completed_Phases_And_Can_Be_Retried(string phase)
    {
        LayoutRoot root = new();
        LayoutFloatingWindow previous = Create(false);
        LayoutFloatingWindow incoming = Create(true);
        root.FloatingWindows.Add(previous);
        InvalidOperationException error = new("replacement callback");
        bool fail = true;
        int published = 0;
        ((INotifyPropertyChanged)root.FloatingWindows).PropertyChanged += (_, args) =>
        {
            if (fail && phase == "indexer" && args.PropertyName == "Item[]") throw error;
        };
        previous.PropertyChanged += (_, args) =>
        {
            if (fail && phase == "outgoing" && args.PropertyName == "Parent") throw error;
        };
        incoming.PropertyChanged += (_, args) =>
        {
            if (fail && phase == "incoming" && args.PropertyName == "Parent") throw error;
        };
        root.FloatingWindows.CollectionChanged += (_, _) => published++;

        Assert.Same(error, Assert.Throws<InvalidOperationException>(() => root.FloatingWindows[0] = incoming));

        Assert.Same(incoming, Assert.Single(root.FloatingWindows));
        Assert.Equal(0, published);
        Assert.Same(phase == "indexer" ? root : null, previous.Parent);
        Assert.Same(phase == "incoming" ? root : null, incoming.Parent);
        Assert.Same(incoming.DockWindow, Assert.Single(((RootDock)root.DockModel).Windows!));
        fail = false;
        root.FloatingWindows[0] = incoming;
        Assert.Same(root, incoming.Parent);
        Assert.Equal(1, published);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Parent_Callback_Can_Add_Another_Floating_Model(bool outgoing)
    {
        LayoutRoot root = new();
        LayoutFloatingWindow previous = Create(false);
        LayoutFloatingWindow incoming = Create(true);
        LayoutFloatingWindow sibling = Create(false);
        root.FloatingWindows.Add(previous);
        bool added = false;
        (outgoing ? previous : incoming).PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != "Parent" || added) return;
            added = true;
            root.FloatingWindows.Add(sibling);
            if (outgoing) Assert.Null(incoming.Parent);
        };

        root.FloatingWindows[0] = incoming;

        Assert.True(added);
        Assert.Equal(new[] { incoming, sibling }, root.FloatingWindows);
        Assert.Same(root, incoming.Parent);
        Assert.Same(root, sibling.Parent);
        Assert.Equal(new[] { incoming.DockWindow, sibling.DockWindow }, ((RootDock)root.DockModel).Windows!);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Replacing_A_Floating_Model_Publishes_One_Replacement_After_Parent_Hooks(bool tools, bool shown)
    {
        DockingManager manager = new();
        LayoutRoot root = manager.Layout;
        LayoutFloatingWindow first = Create(false);
        LayoutFloatingWindow previous = Create(tools);
        LayoutFloatingWindow last = Create(true);
        LayoutFloatingWindow incoming = Create(!tools);
        Window? view = shown ? new() { Content = manager, Width = 800, Height = 500 } : null;
        view?.Show();
        root.FloatingWindows.Add(first);
        root.FloatingWindows.Add(previous);
        root.FloatingWindows.Add(last);
        Dispatcher.UIThread.RunJobs();
        LayoutFloatingWindowControl? oldHost = previous.DockWindow.Host as LayoutFloatingWindowControl;
        List<string> phases = new();
        List<NotifyCollectionChangedEventArgs> changes = new();
        ((INotifyPropertyChanged)root.FloatingWindows).PropertyChanged += (_, args) => phases.Add(args.PropertyName!);
        previous.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != "Parent") return;
            Assert.Same(incoming, root.FloatingWindows[1]);
            Assert.Null(previous.Parent);
            phases.Add("detached");
        };
        incoming.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != "Parent") return;
            Assert.Same(incoming, root.FloatingWindows[1]);
            Assert.Same(root, incoming.Parent);
            phases.Add("attached");
        };
        root.FloatingWindows.CollectionChanged += (_, args) => { changes.Add(args); phases.Add("collection"); };
        try
        {
            root.FloatingWindows[1] = incoming;

            Assert.Equal(new[] { "Item[]", "detached", "attached", "collection" }, phases);
            NotifyCollectionChangedEventArgs change = Assert.Single(changes);
            Assert.Equal(NotifyCollectionChangedAction.Replace, change.Action);
            Assert.Same(previous, Assert.Single(change.OldItems!));
            Assert.Same(incoming, Assert.Single(change.NewItems!));
            Assert.Equal(1, change.OldStartingIndex);
            Assert.Equal(1, change.NewStartingIndex);
            Assert.Equal(new[] { first, incoming, last }, root.FloatingWindows);
            Assert.Same(incoming.DockWindow, ((RootDock)root.DockModel).Windows![1]);
            Assert.Null(previous.DockWindow.Owner);
            Assert.Same(root.DockModel, incoming.DockWindow.Owner);
            if (shown)
            {
                Assert.False(oldHost!.IsVisible);
                Assert.NotSame(oldHost, incoming.DockWindow.Host);
                Assert.True(((LayoutFloatingWindowControl)incoming.DockWindow.Host!).IsVisible);
            }
        }
        finally
        {
            // The assertions above apply to replacement, not later cleanup.
            view?.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static LayoutFloatingWindow Create(bool tools) => tools
        ? new LayoutAnchorableFloatingWindow { RootPanel = new(new LayoutAnchorablePane(new LayoutAnchorable { Content = new object() })) }
        : new LayoutDocumentFloatingWindow { RootDocument = new() { Content = new object() } };
}
