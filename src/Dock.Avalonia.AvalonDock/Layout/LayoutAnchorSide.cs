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
    public AnchorSide Side => _side;
    internal Alignment DockAlignment => Side switch
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

    internal void Attach(LayoutRoot root, AnchorSide side)
    {
        root.Context.Adopt(Context);
        DockModel.Owner = root.DockModel;
        SetField(ref _side, side, nameof(Side));
        _binding = new LayoutAnchorSideBinding(root, this);
    }

    internal void Detach()
    {
        _binding?.Dispose();
        _binding = null;
        DockModel.Owner = null;
    }

    internal void Reconnect() => _binding?.Reconnect();
}
