using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using AvalonDock.Layout;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;

namespace PackageConsumer;

internal static class CollectionReentrancyProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        int checks = 0;
        for (int kind = 0; kind < 4; kind++)
        {
            for (int operation = 0; operation < 5; operation++)
            {
                Check($"rejected parent edit {kind}/{operation}", () =>
                {
                    Fixture fixture = new(kind);
                    LayoutElement trigger = fixture.Create();
                    LayoutElement attempted = fixture.Create();
                    bool observed = false;
                    trigger.PropertyChanged += (_, args) =>
                    {
                        if (args.PropertyName != "Parent" || observed) return;
                        observed = true;
                        object[] before = Snapshot(fixture.Children);
                        object[] nativeBefore = Snapshot(fixture.Native());
                        Exception? caught = null;
                        try
                        {
                            if (operation == 0) fixture.Children.Add(attempted);
                            else if (operation == 1) fixture.Children.RemoveAt(0);
                            else if (operation == 2) fixture.Children[0] = attempted;
                            else if (operation == 3) fixture.Move(0, 1);
                            else fixture.Children.Clear();
                        }
                        catch (Exception error) { caught = error; }
                        Require(caught is InvalidOperationException);
                        Equal(before, Snapshot(fixture.Children));
                        Equal(nativeBefore, Snapshot(fixture.Native()));
                        Require(attempted.Parent is null && attempted.DockModel.Owner is null);
                    };
                    ((INotifyCollectionChanged)fixture.Children).CollectionChanged += (_, _) => { };
                    fixture.Children.Add(trigger);
                    Require(observed && fixture.Children.Count == 3);
                });
            }
            Check($"permitted nested addition {kind}", () =>
            {
                Fixture fixture = new(kind);
                LayoutElement trigger = fixture.Create();
                LayoutElement nested = fixture.Create();
                bool observed = false;
                trigger.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName != "Parent" || observed) return;
                    observed = true;
                    fixture.Children.Add(nested);
                    Require(ReferenceEquals(nested, fixture.Children[3]) && nested.Parent is not null && Snapshot(fixture.Native()).Length == 4);
                };
                fixture.Children.Add(trigger);
                Require(observed && fixture.Children.Count == 4);
            });
        }
        for (int operation = 0; operation < 3; operation++)
        {
            Check($"permitted incoming window edit {operation}", () =>
            {
                Fixture fixture = new(3);
                LayoutFloatingWindow trigger = (LayoutFloatingWindow)fixture.Create();
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
                Require(observed && fixture.Children.Count == (operation == 0 ? 2 : 3));
                object[] native = Snapshot(fixture.Native());
                Require(native.Length == fixture.Children.Count);
                for (int index = 0; index < native.Length; index++)
                    Require(ReferenceEquals(native[index], ((LayoutFloatingWindow)fixture.Children[index]!).DockWindow));
                if (operation != 2) Require(trigger.Parent is null && trigger.DockWindow.Owner is null && trigger.DockWindow.Factory is null && trigger.DockWindow.Host is null);
                if (operation == 1) Require(ReferenceEquals(replacement, fixture.Children[2]));
                if (operation == 2) Require(ReferenceEquals(trigger, fixture.Children[0]));
            });
        }
        if (failures.Count != 0) throw new InvalidOperationException($"Packaged collection reentrancy: {failures.Count}/{checks} failed: " + string.Join("; ", failures));
        Console.WriteLine($"Collection reentrancy: {checks} packaged checks passed.");

        void Check(string name, Action action)
        {
            checks++;
            try { action(); }
            catch (Exception error) { failures.Add(name + ": " + error.Message); }
        }
    }

    private static object[] Snapshot(IEnumerable values)
    {
        List<object> result = new();
        foreach (object value in values) result.Add(value);
        return result.ToArray();
    }
    private static void Equal(object[] expected, object[] actual)
    {
        Require(expected.Length == actual.Length);
        for (int index = 0; index < expected.Length; index++) Require(ReferenceEquals(expected[index], actual[index]));
    }
    private static void Require(bool condition)
    {
        if (!condition) throw new InvalidOperationException("Collection reentrancy contract changed.");
    }
    private sealed class Fixture
    {
        private readonly int _kind;
        internal IList Children { get; }
        internal Action<int, int> Move { get; }
        internal Func<IEnumerable> Native { get; }

        internal Fixture(int kind)
        {
            _kind = kind;
            LayoutRoot root = new();
            if (kind == 0)
            {
                LayoutDocumentPane pane = new();
                root.RootPanel = new(pane);
                Children = pane.Children;
                Move = pane.Children.Move;
                Native = () => (IEnumerable)((IDock)pane.DockModel).VisibleDockables!;
            }
            else if (kind == 1)
            {
                LayoutAnchorablePane pane = new();
                root.RootPanel = new(pane);
                Children = pane.Children;
                Move = pane.Children.Move;
                Native = () => (IEnumerable)((IDock)pane.DockModel).VisibleDockables!;
            }
            else if (kind == 2)
            {
                Children = root.Hidden;
                Move = root.Hidden.Move;
                Native = () => (IEnumerable)((RootDock)root.DockModel).HiddenDockables!;
            }
            else
            {
                Children = root.FloatingWindows;
                Move = root.FloatingWindows.Move;
                Native = () => (IEnumerable)((RootDock)root.DockModel).Windows!;
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
