using System;
using System.Collections.Generic;
using System.Linq;
using AvalonDock.Layout;
using Dock.Model.Avalonia.Controls;

namespace PackageConsumer;

internal static class RootSideLifetimeProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        foreach (AnchorSide edge in new[] { AnchorSide.Left, AnchorSide.Top, AnchorSide.Right, AnchorSide.Bottom })
        {
            Check(edge + " retained parent", () =>
            {
                LayoutRoot root = new();
                LayoutAnchorSide side = Read(root, edge)!;
                LayoutAnchorable tool = AddTool(side);
                int parents = 0, removed = 0, updates = 0;
                side.PropertyChanged += (_, e) => { if (e.PropertyName == "Parent") parents++; };
                tool.PropertyChanged += (_, e) => { if (e.PropertyName == "Parent") parents++; };
                root.ElementRemoved += (_, _) => removed++;
                root.Updated += (_, _) => updates++;
                Write(root, edge, null);
                if (!ReferenceEquals(side.Parent, root) || !ReferenceEquals(tool.Root, root) ||
                    parents != 0 || removed != 0 || updates != 0 || side.DockModel.Owner is not null || Pinned(root, edge) != 0)
                    throw new Exception("Clearing a slot changed the public tree or retained the native projection.");
            });
            Check(edge + " cross-root assignment", () =>
            {
                LayoutRoot previous = new();
                LayoutRoot current = new();
                LayoutAnchorSide side = Read(previous, edge)!;
                LayoutAnchorable tool = AddTool(side);
                Write(current, edge, side);
                if (!ReferenceEquals(Read(previous, edge), side) || !ReferenceEquals(side.Parent, current) ||
                    !ReferenceEquals(tool.DockModel.Owner, current.DockModel) || Pinned(previous, edge) != 0 || Pinned(current, edge) != 1)
                    throw new Exception("Assignment removed the former slot or failed to transfer native ownership.");
            });
            Check(edge + " shared slots", () =>
            {
                LayoutRoot root = new();
                LayoutAnchorSide side = Read(root, edge)!;
                AddTool(side);
                AnchorSide other = edge == AnchorSide.Left ? AnchorSide.Top : AnchorSide.Left;
                Write(root, other, side);
                if (!ReferenceEquals(Read(root, edge), side) || side.Side != edge ||
                    root.Children.Count(child => ReferenceEquals(child, side)) != 2 || Pinned(root, edge) != 1 || Pinned(root, other) != 0)
                    throw new Exception("The second slot changed the side's existing parent or edge.");
                Write(root, edge, null);
                if (side.Side != edge || Pinned(root, edge) != 0 || Pinned(root, other) != 1)
                    throw new Exception("The surviving slot lost its native projection.");
            });
        }
        foreach (bool before in new[] { true, false })
            Check("incoming parent failure " + before, () =>
            {
                LayoutRoot previous = new();
                LayoutRoot current = new();
                LayoutAnchorSide side = previous.TopSide!;
                AddTool(side);
                InvalidOperationException expected = new("parent callback");
                side.PropertyChanging += (_, e) => { if (before && e.PropertyName == "Parent") throw expected; };
                side.PropertyChanged += (_, e) => { if (!before && e.PropertyName == "Parent") throw expected; };
                Exception? actual = null;
                try { current.LeftSide = side; }
                catch (Exception error) { actual = error; }
                if (!ReferenceEquals(actual, expected) || !ReferenceEquals(current.LeftSide, side) ||
                    !ReferenceEquals(previous.TopSide, side) || !ReferenceEquals(side.Parent, before ? previous : current) ||
                    Pinned(previous, AnchorSide.Top) != (before ? 1 : 0) || Pinned(current, AnchorSide.Left) != (before ? 0 : 1))
                    throw new Exception("The failed parent callback did not retain the assigned slot and completed phase.");
            });
        if (failures.Count != 0) throw new Exception("Packaged side-lifetime failures: " + string.Join("; ", failures));

        void Check(string name, Action action)
        {
            try { action(); }
            catch (Exception error) { failures.Add(name + ": " + error.Message); }
        }
    }

    private static LayoutAnchorable AddTool(LayoutAnchorSide side)
    {
        LayoutAnchorable tool = new();
        LayoutAnchorGroup group = new();
        group.Children.Add(tool);
        side.Children.Add(group);
        return tool;
    }

    private static LayoutAnchorSide? Read(LayoutRoot root, AnchorSide edge) => edge switch
    {
        AnchorSide.Left => root.LeftSide,
        AnchorSide.Top => root.TopSide,
        AnchorSide.Right => root.RightSide,
        _ => root.BottomSide
    };

    private static void Write(LayoutRoot root, AnchorSide edge, LayoutAnchorSide? value)
    {
        switch (edge)
        {
            case AnchorSide.Left: root.LeftSide = value; break;
            case AnchorSide.Top: root.TopSide = value; break;
            case AnchorSide.Right: root.RightSide = value; break;
            default: root.BottomSide = value; break;
        }
    }

    private static int Pinned(LayoutRoot layout, AnchorSide edge)
    {
        RootDock root = (RootDock)layout.DockModel;
        return (edge switch
        {
            AnchorSide.Left => root.LeftPinnedDockables,
            AnchorSide.Top => root.TopPinnedDockables,
            AnchorSide.Right => root.RightPinnedDockables,
            _ => root.BottomPinnedDockables
        })?.Count ?? 0;
    }
}
