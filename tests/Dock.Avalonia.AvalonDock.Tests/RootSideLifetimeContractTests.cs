using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Collections;
using Avalonia.Headless.XUnit;
using AvalonDock.Layout;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class RootSideLifetimeContractTests
{
    [AvaloniaTheory]
    [InlineData(AnchorSide.Left)]
    [InlineData(AnchorSide.Top)]
    [InlineData(AnchorSide.Right)]
    [InlineData(AnchorSide.Bottom)]
    public void Removing_A_Side_Retains_Public_Parents_Without_Membership_Or_Parent_Notifications(AnchorSide edge)
    {
        LayoutRoot root = new();
        LayoutAnchorSide side = Read(root, edge)!;
        LayoutAnchorGroup group = new();
        LayoutAnchorable tool = new();
        group.Children.Add(tool);
        side.Children.Add(group);
        List<string> events = ObserveParents(side, group, tool);
        root.ElementRemoved += (_, _) => events.Add("removed");
        root.ElementAdded += (_, _) => events.Add("added");
        root.Updated += (_, _) => events.Add("updated");

        Write(root, edge, null);

        Assert.Empty(events);
        Assert.Same(root, side.Parent);
        Assert.Same(root, tool.Root);
        Assert.Same(side, group.Parent);
        Assert.Same(group, tool.Parent);
        Assert.Null(side.DockModel.Owner);
        Assert.Same(group.DockModel, tool.DockModel.Owner);
        Assert.Empty(Pinned(root, edge));
        group.Children.Add(new LayoutAnchorable());
        Assert.Empty(Pinned(root, edge));
    }

    [AvaloniaTheory]
    [InlineData(AnchorSide.Left)]
    [InlineData(AnchorSide.Top)]
    [InlineData(AnchorSide.Right)]
    [InlineData(AnchorSide.Bottom)]
    public void Moving_A_Side_To_Another_Root_Preserves_The_Previous_Slot_And_Moves_Only_Native_Ownership(AnchorSide edge)
    {
        LayoutRoot previous = new();
        LayoutRoot current = new();
        LayoutAnchorSide side = Read(previous, edge)!;
        LayoutAnchorable tool = AddTool(side);
        AnchorSide destination = Next(edge);
        LayoutAnchorSide displaced = Read(current, destination)!;

        Write(current, destination, side);

        Assert.Same(side, Read(previous, edge));
        Assert.Same(side, Read(current, destination));
        Assert.Same(current, side.Parent);
        Assert.Same(current, displaced.Parent);
        Assert.Same(current, tool.Root);
        Assert.Equal(destination, side.Side);
        Assert.Empty(Pinned(previous, edge));
        Assert.Same(tool.DockModel, Assert.Single(Pinned(current, destination)));
        Assert.Same(current.DockModel, tool.DockModel.Owner);

        ReplacePinned(previous, edge);
        Write(previous, edge, null);
        AddTool(side);
        Assert.Empty(Pinned(previous, edge));
        Assert.Equal(2, Pinned(current, destination).Count);
    }

    [AvaloniaTheory]
    [InlineData(AnchorSide.Left)]
    [InlineData(AnchorSide.Top)]
    [InlineData(AnchorSide.Right)]
    [InlineData(AnchorSide.Bottom)]
    public void Shared_Slots_Retain_The_Side_Value_And_Use_One_Native_Projection(AnchorSide edge)
    {
        LayoutRoot root = new();
        LayoutAnchorSide side = Read(root, edge)!;
        LayoutAnchorable tool = AddTool(side);
        AnchorSide destination = Next(edge);
        List<string> events = ObserveParents(side, side.Children[0], tool);
        int updates = 0;
        root.Updated += (_, _) => updates++;

        Write(root, destination, side);

        Assert.Same(side, Read(root, edge));
        Assert.Same(side, Read(root, destination));
        Assert.Equal(2, root.Children.Count(child => ReferenceEquals(child, side)));
        Assert.Equal(edge, side.Side);
        Assert.Empty(events);
        Assert.Equal(0, updates);
        Assert.Single(Pinned(root, edge));
        Assert.Empty(Pinned(root, destination));

        Write(root, edge, null);

        Assert.Equal(edge, side.Side);
        Assert.Same(root, side.Parent);
        Assert.Empty(Pinned(root, edge));
        Assert.Same(tool.DockModel, Assert.Single(Pinned(root, destination)));
        Assert.Empty(events);
        Assert.Equal(0, updates);
        AddTool(side);
        Assert.Equal(2, Pinned(root, destination).Count);
    }

    [AvaloniaTheory]
    [InlineData(AnchorSide.Left)]
    [InlineData(AnchorSide.Top)]
    [InlineData(AnchorSide.Right)]
    [InlineData(AnchorSide.Bottom)]
    public void Reusing_A_Retained_Side_Does_Not_Recompute_Its_Side_Or_Steal_Another_Strips_Tools(AnchorSide edge)
    {
        LayoutRoot root = new();
        LayoutAnchorSide side = Read(root, edge)!;
        LayoutAnchorable tool = AddTool(side);
        LayoutAnchorSide replacement = new();
        LayoutAnchorable replacementTool = AddTool(replacement);
        Write(root, edge, replacement);
        List<string> events = ObserveParents(side, side.Children[0], tool);
        AnchorSide destination = Next(edge);

        Write(root, destination, side);

        Assert.Equal(edge, side.Side);
        Assert.Empty(events);
        Assert.Same(replacementTool.DockModel, Assert.Single(Pinned(root, edge)));
        Assert.Same(tool.DockModel, Assert.Single(Pinned(root, destination)));
        Assert.Same(side, tool.Parent!.Parent);
        Assert.Same(replacement, replacementTool.Parent!.Parent);
    }

    [AvaloniaTheory]
    [InlineData("Parent", true)]
    [InlineData("Parent", false)]
    [InlineData("Side", true)]
    [InlineData("Side", false)]
    public void Incoming_Observer_Errors_Retain_The_Assigned_Slot_And_Project_The_Actual_Parent(string property, bool before)
    {
        LayoutRoot previous = new();
        LayoutRoot current = new();
        LayoutAnchorSide side = previous.TopSide!;
        LayoutAnchorable tool = AddTool(side);
        InvalidOperationException error = new("side parent observer");
        bool fail = true;
        side.PropertyChanging += (_, e) => { if (fail && before && e.PropertyName == property) throw error; };
        side.PropertyChanged += (_, e) => { if (fail && !before && e.PropertyName == property) throw error; };

        Assert.Same(error, Assert.Throws<InvalidOperationException>(() => current.LeftSide = side));

        Assert.Same(side, previous.TopSide);
        Assert.Same(side, current.LeftSide);
        bool parentChanged = property != "Parent" || !before;
        Assert.Same(parentChanged ? current : previous, side.Parent);
        Assert.Equal(parentChanged ? 0 : 1, Pinned(previous, AnchorSide.Top).Count);
        Assert.Equal(parentChanged ? 1 : 0, Pinned(current, AnchorSide.Left).Count);
        Assert.Same((parentChanged ? current : previous).DockModel, tool.DockModel.Owner);
        fail = false;
        current.LeftSide = null;
        current.LeftSide = side;
        Assert.Same(current, side.Parent);
        Assert.Single(Pinned(current, AnchorSide.Left));
    }

    [AvaloniaFact]
    public void A_Reentrant_Changing_Observer_Retains_The_Interim_Sides_Public_Parent()
    {
        LayoutRoot root = new();
        LayoutAnchorSide original = root.TopSide!;
        LayoutAnchorSide interim = new();
        LayoutAnchorSide final = new();
        LayoutAnchorable originalTool = AddTool(original);
        LayoutAnchorable interimTool = AddTool(interim);
        LayoutAnchorable finalTool = AddTool(final);
        bool nested = false;
        root.PropertyChanging += (_, e) =>
        {
            if (e.PropertyName != nameof(root.TopSide) || nested) return;
            nested = true;
            root.TopSide = interim;
        };

        root.TopSide = final;

        Assert.All(new[] { original, interim, final }, side => Assert.Same(root, side.Parent));
        Assert.All(new[] { originalTool, interimTool, finalTool }, tool => Assert.Same(root, tool.Root));
        Assert.Null(original.DockModel.Owner);
        Assert.Null(interim.DockModel.Owner);
        Assert.Same(finalTool.DockModel, Assert.Single(Pinned(root, AnchorSide.Top)));
    }

    [AvaloniaFact]
    public void Replacing_An_Incoming_Side_During_Its_Parent_Callback_Does_Not_Resurrect_Its_Native_Projection()
    {
        LayoutRoot root = new();
        LayoutAnchorSide incoming = new();
        LayoutAnchorSide replacement = new();
        LayoutAnchorable incomingTool = AddTool(incoming);
        LayoutAnchorable replacementTool = AddTool(replacement);
        incoming.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(incoming.Parent)) root.TopSide = replacement;
        };

        root.TopSide = incoming;

        Assert.Same(replacement, root.TopSide);
        Assert.Same(root, incoming.Parent);
        Assert.Null(incoming.DockModel.Owner);
        Assert.Same(root, incomingTool.Root);
        Assert.Same(replacementTool.DockModel, Assert.Single(Pinned(root, AnchorSide.Top)));
    }

    [AvaloniaFact]
    public void A_Retained_Root_Does_Not_Keep_Replaced_Sides_Or_Their_Tools_Alive()
    {
        LayoutRoot root = new();
        (WeakReference Side, WeakReference Tool) references = ReplaceSide(root);
        for (int i = 0; i < 3; i++) { GC.Collect(); GC.WaitForPendingFinalizers(); }
        Assert.False(references.Side.IsAlive);
        Assert.False(references.Tool.IsAlive);
        GC.KeepAlive(root);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference, WeakReference) ReplaceSide(LayoutRoot root)
    {
        LayoutAnchorSide side = root.TopSide!;
        LayoutAnchorable tool = AddTool(side);
        root.TopSide = new LayoutAnchorSide();
        return (new WeakReference(side), new WeakReference(tool));
    }

    private static List<string> ObserveParents(params LayoutElement[] elements)
    {
        List<string> events = new();
        foreach (LayoutElement element in elements)
        {
            element.PropertyChanging += (_, e) => { if (e.PropertyName is "Parent" or "Root" or "Side") events.Add("before:" + e.PropertyName); };
            element.PropertyChanged += (_, e) => { if (e.PropertyName is "Parent" or "Root" or "Side") events.Add("after:" + e.PropertyName); };
        }
        return events;
    }

    private static LayoutAnchorable AddTool(LayoutAnchorSide side)
    {
        LayoutAnchorable tool = new();
        LayoutAnchorGroup group = new();
        group.Children.Add(tool);
        side.Children.Add(group);
        return tool;
    }

    private static AnchorSide Next(AnchorSide edge) => edge switch
    {
        AnchorSide.Left => AnchorSide.Top,
        AnchorSide.Top => AnchorSide.Right,
        AnchorSide.Right => AnchorSide.Bottom,
        _ => AnchorSide.Left
    };

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

    private static IList<IDockable> Pinned(LayoutRoot layout, AnchorSide edge)
    {
        RootDock root = (RootDock)layout.DockModel;
        return edge switch
        {
            AnchorSide.Left => root.LeftPinnedDockables!,
            AnchorSide.Top => root.TopPinnedDockables!,
            AnchorSide.Right => root.RightPinnedDockables!,
            _ => root.BottomPinnedDockables!
        };
    }

    private static void ReplacePinned(LayoutRoot layout, AnchorSide edge)
    {
        RootDock root = (RootDock)layout.DockModel;
        switch (edge)
        {
            case AnchorSide.Left: root.LeftPinnedDockables = new AvaloniaList<IDockable>(); break;
            case AnchorSide.Top: root.TopPinnedDockables = new AvaloniaList<IDockable>(); break;
            case AnchorSide.Right: root.RightPinnedDockables = new AvaloniaList<IDockable>(); break;
            default: root.BottomPinnedDockables = new AvaloniaList<IDockable>(); break;
        }
    }
}
