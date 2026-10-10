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
    protected override bool GetVisibility()
    {
        foreach (LayoutAnchorGroup group in Children) if (group.IsVisible) return true;
        return false;
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
