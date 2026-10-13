using System;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using AvalonDock.Infrastructure;

namespace AvalonDock.Layout;

/// <summary>Groups the tools in one of Dock's native auto-hide strips.</summary>
public class LayoutAnchorSide : LayoutGroup<LayoutAnchorGroup>
{
    private AnchorSide _side;
    private LayoutAnchorSideBinding? _binding;

    /// <summary>Creates an empty auto-hide side.</summary>
    public LayoutAnchorSide() : base(new ProportionalDock()) { }
    /// <summary>Gets the edge assigned by the containing layout root.</summary>
    /// <remarks>The edge is resolved when the parent changes. Reusing this side in another slot of the same root retains the existing value.</remarks>
    public AnchorSide Side => _side;
    internal Alignment DockAlignment => (_binding?.Edge ?? Side) switch
    {
        AnchorSide.Left => Alignment.Left,
        AnchorSide.Top => Alignment.Top,
        AnchorSide.Right => Alignment.Right,
        _ => Alignment.Bottom
    };
    /// <inheritdoc/>
    protected override bool GetVisibility() => Children.Count > 0;

    /// <summary>Resolves the edge from the new root before publishing the parent change.</summary>
    protected override void OnParentChanged(ILayoutContainer? oldValue, ILayoutContainer? newValue)
    {
        base.OnParentChanged(oldValue, newValue);
        if (Root is not { } root) return;
        if (ReferenceEquals(root.LeftSide, this)) SetField(ref _side, AnchorSide.Left, nameof(Side));
        else if (ReferenceEquals(root.TopSide, this)) SetField(ref _side, AnchorSide.Top, nameof(Side));
        else if (ReferenceEquals(root.RightSide, this)) SetField(ref _side, AnchorSide.Right, nameof(Side));
        else if (ReferenceEquals(root.BottomSide, this)) SetField(ref _side, AnchorSide.Bottom, nameof(Side));
    }

    internal void Attach(LayoutRoot root)
    {
        try { Parent = root; }
        finally { SynchronizeProjection(); }
    }

    internal void ReleaseProjection(LayoutRoot root)
    {
        if (ReferenceEquals(Parent, root)) SynchronizeProjection();
    }

    private void ReleaseProjection()
    {
        // Root side properties retain their former value's public parent. The
        // native pinned list and its subscriptions still belong only to live slots.
        SetProjectedParent(Parent);
        LayoutAnchorSideBinding? binding = _binding;
        _binding = null;
        binding?.Dispose();
        DockModel.Owner = null;
    }

    private void SynchronizeProjection()
    {
        if (Parent is not LayoutRoot root || root.GetSideSlot(this) is not { } edge)
        {
            ReleaseProjection();
            return;
        }
        if (_binding?.IsBoundTo(root, edge) == true) return;
        ReleaseProjection();
        root.Context.Adopt(Context);
        DockModel.Owner = root.DockModel;
        SetProjectedParent(null);
        _binding = new LayoutAnchorSideBinding(root, this, edge);
    }

    internal void Reconnect() => _binding?.Reconnect();

    internal void MovePinned(Action move)
    {
        if (_binding is { } binding) binding.MovePinned(move);
        else move();
    }

    internal void Pin(LayoutAnchorable tool, Action pin)
    {
        if (_binding is { } binding) binding.Pin(tool, pin);
        else pin();
    }
}
