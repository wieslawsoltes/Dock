using System;
using System.Collections.Generic;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class PaneSelectionProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        int checks = 0;
        foreach (bool tools in new[] { false, true })
        foreach (bool rooted in new[] { false, true })
        {
            Check("content flag before parent", () =>
            {
                Fixture fixture = new(tools, rooted);
                bool? observedFlag = null;
                fixture.Pane.PropertyChanging += (_, args) =>
                {
                    if (args.PropertyName == "SelectedContentIndex") observedFlag ??= fixture.Second.IsSelected;
                };
                fixture.Second.IsSelected = true;
                Require(observedFlag == true && ReferenceEquals(fixture.Selector.SelectedContent, fixture.Second));
                GC.KeepAlive(fixture.Root);
            });
            Check("nested clear", () =>
            {
                Fixture fixture = new(tools, rooted);
                List<int> indices = new();
                fixture.Pane.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName == "SelectedContentIndex") indices.Add(fixture.Selector.SelectedContentIndex);
                };
                fixture.Selector.SelectedContentIndex = 1;
                Require(indices.Count == 2 && indices[0] == -1 && indices[1] == 1);
                GC.KeepAlive(fixture.Root);
            });
            Check("callback failure", () =>
            {
                Fixture fixture = new(tools, rooted);
                bool fail = true;
                int events = 0;
                fixture.Second.IsSelectedChanged += (_, _) => events++;
                fixture.Pane.PropertyChanging += (_, args) =>
                {
                    if (fail && args.PropertyName == "SelectedContentIndex") throw new ProbeException();
                };
                bool threw = false;
                try { fixture.Second.IsSelected = true; }
                catch (ProbeException) { threw = true; }
                Require(threw && fixture.First.IsSelected && fixture.Second.IsSelected && events == 0 && fixture.Selector.SelectedContentIndex == 0);
                // Equality is checked against the flag, even when the parent
                // was left at its earlier index by an application exception.
                fixture.Second.IsSelected = true;
                fail = false;
                fixture.Second.IsSelected = false;
                Require(fixture.Selector.SelectedContentIndex == -1 && !fixture.First.IsSelected && !fixture.Second.IsSelected);
                GC.KeepAlive(fixture.Root);
            });
            Check("reordering", () =>
            {
                Fixture fixture = new(tools, rooted);
                LayoutContent third = tools ? new LayoutAnchorable() : new LayoutDocument();
                fixture.Pane.InsertChildAt(2, third);
                fixture.Selector.SelectedContentIndex = 1;
                ((ILayoutPane)fixture.Pane).MoveChild(2, 0);
                Require(fixture.Selector.SelectedContentIndex == (tools ? 0 : 1) &&
                    ReferenceEquals(fixture.Selector.SelectedContent, tools ? third : fixture.First) && fixture.Second.IsSelected);
                GC.KeepAlive(fixture.Root);
            });

            void Check(string name, Action action)
            {
                checks++;
                try { action(); }
                catch (Exception exception) { failures.Add($"{name}, tools={tools}, rooted={rooted}: {exception.Message}"); }
            }
        }
        if (failures.Count != 0) throw new InvalidOperationException("Packaged pane selection failures: " + string.Join("; ", failures));
        Console.WriteLine($"Pane selection: {checks} packaged checks passed.");
    }

    private static void Require(bool condition)
    {
        if (!condition) throw new InvalidOperationException("Public selection callback/state contract changed.");
    }

    private sealed class Fixture
    {
        internal Fixture(bool tools, bool rooted)
        {
            First = tools ? new LayoutAnchorable() : new LayoutDocument();
            Second = tools ? new LayoutAnchorable() : new LayoutDocument();
            Pane = tools ? new LayoutAnchorablePane() : new LayoutDocumentPane();
            Pane.InsertChildAt(0, First);
            Pane.InsertChildAt(1, Second);
            if (rooted) Root = new LayoutRoot { RootPanel = new LayoutPanel((ILayoutPanelElement)Pane) };
            Selector = (ILayoutContentSelector)Pane;
        }

        internal ILayoutGroup Pane { get; }
        internal ILayoutContentSelector Selector { get; }
        internal LayoutContent First { get; }
        internal LayoutContent Second { get; }
        internal LayoutRoot? Root { get; }
    }

    private sealed class ProbeException : Exception;
}
