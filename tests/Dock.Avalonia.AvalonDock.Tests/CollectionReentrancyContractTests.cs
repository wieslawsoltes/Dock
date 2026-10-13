using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class CollectionReentrancyContractTests
{
    [AvaloniaTheory]
    [InlineData("Count")]
    [InlineData("Item[]")]
    [InlineData("Parent")]
    public void Retrying_A_Failed_Window_Insertion_Completes_Ownership_And_Presentation(string phase)
    {
        DockingManager manager = new();
        LayoutDocumentFloatingWindow incoming = new() { RootDocument = new() { Content = new object() } };
        Window view = new() { Content = manager, Width = 800, Height = 500 };
        view.Show();
        bool fail = true;
        InvalidOperationException error = new("insertion callback");
        ((INotifyPropertyChanged)manager.Layout.FloatingWindows).PropertyChanged += (_, args) =>
        {
            if (fail && args.PropertyName == phase) throw error;
        };
        incoming.PropertyChanged += (_, args) =>
        {
            if (fail && phase == "Parent" && args.PropertyName == phase) throw error;
        };
        try
        {
            Assert.Same(error, Assert.Throws<InvalidOperationException>(() => manager.Layout.FloatingWindows.Add(incoming)));
            Assert.Same(incoming, Assert.Single(manager.Layout.FloatingWindows));
            Assert.Same(incoming.DockWindow, Assert.Single(((RootDock)manager.Layout.DockModel).Windows!));
            Assert.Same(phase == "Parent" ? manager.Layout : null, incoming.Parent);
            fail = false;

            manager.Layout.FloatingWindows[0] = incoming;
            Dispatcher.UIThread.RunJobs();

            Assert.Same(manager.Layout, incoming.Parent);
            Assert.Same(manager.Layout.DockModel, incoming.DockWindow.Owner);
            Assert.Same(manager.DockFactory, incoming.DockWindow.Factory);
            Assert.True(Assert.IsAssignableFrom<LayoutFloatingWindowControl>(incoming.DockWindow.Host).IsVisible);
        }
        finally { fail = false; view.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void A_Permitted_Parent_Callback_Can_Remove_Replace_Or_Move_The_Incoming_Window(int operation)
    {
        Fixture fixture = new(3);
        LayoutElement trigger = fixture.Create();
        LayoutElement replacement = fixture.Create();
        bool observed = false;
        trigger.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != "Parent" || observed) return;
            observed = true;
            if (operation == 0) fixture.Children.RemoveAt(2);
            else if (operation == 1) fixture.Children[2] = replacement;
            else fixture.Move(2, 0);
        };

        fixture.Children.Add(trigger);

        Assert.True(observed);
        Assert.Equal(operation == 0 ? 2 : 3, fixture.Children.Count);
        object[] native = Snapshot(fixture.Native());
        Assert.Equal(fixture.Children.Count, native.Length);
        for (int index = 0; index < native.Length; index++)
            Assert.Same(((LayoutFloatingWindow)fixture.Children[index]!).DockWindow, native[index]);
        if (operation == 0) Assert.Null(trigger.Parent);
        else if (operation == 1) { Assert.Same(replacement, fixture.Children[2]); Assert.Null(trigger.Parent); }
        else Assert.Same(trigger, fixture.Children[0]);
        if (operation != 2)
        {
            Assert.Null(((LayoutFloatingWindow)trigger).DockWindow.Owner);
            Assert.Null(((LayoutFloatingWindow)trigger).DockWindow.Factory);
            Assert.Null(((LayoutFloatingWindow)trigger).DockWindow.Host);
        }
    }

    [AvaloniaTheory]
    [InlineData(0, false)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(2, true)]
    [InlineData(3, false)]
    [InlineData(3, true)]
    public void A_Permitted_Nested_Add_Is_Visible_Before_The_Callback_Returns(int kind, bool parentCallback)
    {
        Fixture fixture = new(kind);
        LayoutElement trigger = fixture.Create();
        LayoutElement nested = fixture.Create();
        bool observed = false;
        void Add()
        {
            if (observed) return;
            observed = true;
            fixture.Children.Add(nested);
            Assert.Same(nested, fixture.Children[3]);
            Assert.NotNull(nested.Parent);
            Assert.Equal(4, Snapshot(fixture.Native()).Length);
        }
        if (parentCallback) trigger.PropertyChanged += (_, args) => { if (args.PropertyName == "Parent") Add(); };
        else ((System.ComponentModel.INotifyPropertyChanged)fixture.Children).PropertyChanged += (_, args) => { if (args.PropertyName == "Count") Add(); };

        fixture.Children.Add(trigger);

        Assert.True(observed);
        Assert.Equal(4, fixture.Children.Count);
        Assert.Same(trigger, fixture.Children[2]);
        Assert.Same(nested, fixture.Children[3]);
    }

    public static IEnumerable<object[]> RejectedMutations()
    {
        for (int kind = 0; kind < 4; kind++)
            for (int operation = 0; operation < 5; operation++)
                foreach (bool parentCallback in new[] { false, true })
                    yield return new object[] { kind, operation, parentCallback };
    }

    [AvaloniaTheory]
    [MemberData(nameof(RejectedMutations))]
    public void Reentrant_Public_Edits_Are_Rejected_Before_Changing_Public_Or_Native_Membership(int kind, int operation, bool parentCallback)
    {
        Fixture fixture = new(kind);
        LayoutElement trigger = fixture.Create();
        LayoutElement attempted = fixture.Create();
        bool observed = false;
        void Attempt()
        {
            if (observed) return;
            observed = true;
            object[] before = Snapshot(fixture.Children);
            object[] nativeBefore = Snapshot(fixture.Native());
            Assert.IsType<InvalidOperationException>(Record.Exception(() =>
            {
                if (operation == 0) fixture.Children.Add(attempted);
                else if (operation == 1) fixture.Children.RemoveAt(0);
                else if (operation == 2) fixture.Children[0] = attempted;
                else if (operation == 3) fixture.Move(0, 1);
                else fixture.Children.Clear();
            }));
            Assert.Equal(before, Snapshot(fixture.Children));
            Assert.Equal(nativeBefore, Snapshot(fixture.Native()));
            Assert.Null(attempted.Parent);
            Assert.Null(attempted.DockModel.Owner);
        }
        if (parentCallback) trigger.PropertyChanged += (_, args) => { if (args.PropertyName == "Parent") Attempt(); };
        ((INotifyCollectionChanged)fixture.Children).CollectionChanged += (_, _) => { if (!parentCallback) Attempt(); };

        fixture.Children.Add(trigger);

        Assert.True(observed);
        Assert.Equal(3, fixture.Children.Count);
        Assert.Same(trigger, fixture.Children[2]);
        Assert.Equal(3, Snapshot(fixture.Native()).Length);
    }

    private static object[] Snapshot(IEnumerable values)
    {
        List<object> result = new();
        foreach (object value in values) result.Add(value);
        return result.ToArray();
    }

    private sealed class Fixture
    {
        private readonly int _kind;
        internal LayoutRoot Root { get; } = new();
        internal IList Children { get; }
        internal Action<int, int> Move { get; }
        internal Func<IEnumerable> Native { get; }

        internal Fixture(int kind)
        {
            _kind = kind;
            if (kind == 0)
            {
                LayoutDocumentPane pane = new();
                Root.RootPanel = new(pane);
                Children = pane.Children;
                Move = pane.Children.Move;
                Native = () => (IEnumerable)((IDock)pane.DockModel).VisibleDockables!;
            }
            else if (kind == 1)
            {
                LayoutAnchorablePane pane = new();
                Root.RootPanel = new(pane);
                Children = pane.Children;
                Move = pane.Children.Move;
                Native = () => (IEnumerable)((IDock)pane.DockModel).VisibleDockables!;
            }
            else if (kind == 2)
            {
                Children = Root.Hidden;
                Move = Root.Hidden.Move;
                Native = () => (IEnumerable)((RootDock)Root.DockModel).HiddenDockables!;
            }
            else
            {
                Children = Root.FloatingWindows;
                Move = Root.FloatingWindows.Move;
                Native = () => (IEnumerable)((RootDock)Root.DockModel).Windows!;
            }
            Children.Add(Create());
            Children.Add(Create());
        }

        internal LayoutElement Create() => _kind switch
        {
            0 => new LayoutDocument { Content = new object() },
            3 => new LayoutDocumentFloatingWindow { RootDocument = new() { Content = new object() } },
            _ => new LayoutAnchorable { Content = new object() }
        };
    }
}
