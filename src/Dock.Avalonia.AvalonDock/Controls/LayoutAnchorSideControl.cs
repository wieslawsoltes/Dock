using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.Primitives;
using AvalonDock.Infrastructure;
using AvalonDock.Layout;

namespace AvalonDock.Controls;

/// <summary>Presents one auto-hide side and its public groups using Dock's pinned controls.</summary>
public class LayoutAnchorSideControl : TemplatedControl, ILayoutControl
{
    /// <summary>Identifies whether the side is on the left edge.</summary>
    public static readonly DirectProperty<LayoutAnchorSideControl, bool> IsLeftSideProperty =
        AvaloniaProperty.RegisterDirect<LayoutAnchorSideControl, bool>(nameof(IsLeftSide), control => control.IsLeftSide);
    /// <summary>Identifies whether the side is on the top edge.</summary>
    public static readonly DirectProperty<LayoutAnchorSideControl, bool> IsTopSideProperty =
        AvaloniaProperty.RegisterDirect<LayoutAnchorSideControl, bool>(nameof(IsTopSide), control => control.IsTopSide);
    /// <summary>Identifies whether the side is on the right edge.</summary>
    public static readonly DirectProperty<LayoutAnchorSideControl, bool> IsRightSideProperty =
        AvaloniaProperty.RegisterDirect<LayoutAnchorSideControl, bool>(nameof(IsRightSide), control => control.IsRightSide);
    /// <summary>Identifies whether the side is on the bottom edge.</summary>
    public static readonly DirectProperty<LayoutAnchorSideControl, bool> IsBottomSideProperty =
        AvaloniaProperty.RegisterDirect<LayoutAnchorSideControl, bool>(nameof(IsBottomSide), control => control.IsBottomSide);

    private readonly LayoutAnchorSide _model;
    private readonly LayoutAnchorChildren<LayoutAnchorGroup, LayoutAnchorGroupControl> _children;
    private readonly LayoutAnchorViewBinding _binding;
    private bool _left, _top, _right, _bottom;

    internal LayoutAnchorSideControl(LayoutAnchorSide model)
    {
        ArgumentNullException.ThrowIfNull(model);
        _model = model;
        _children = new(model.Children, static child => new LayoutAnchorGroupControl(child));
        _binding = new(this, model, DockingManager.AnchorSideTemplateProperty, _ => UpdateSide());
        _binding.Initialize();
    }

    /// <summary>Gets the existing public auto-hide side.</summary>
    public ILayoutElement Model => _model;
    /// <summary>Gets the group controls in model order, preserving controls during reordering.</summary>
    public ObservableCollection<LayoutAnchorGroupControl> Children => _children.Views;
    /// <summary>Gets whether the side is on the left edge.</summary>
    public bool IsLeftSide => _left;
    /// <summary>Gets whether the side is on the top edge.</summary>
    public bool IsTopSide => _top;
    /// <summary>Gets whether the side is on the right edge.</summary>
    public bool IsRightSide => _right;
    /// <summary>Gets whether the side is on the bottom edge.</summary>
    public bool IsBottomSide => _bottom;
    /// <summary>Updates the left-edge flag exposed to templates.</summary>
    protected void SetIsLeftSide(bool value) => SetAndRaise(IsLeftSideProperty, ref _left, value);
    /// <summary>Updates the top-edge flag exposed to templates.</summary>
    protected void SetIsTopSide(bool value) => SetAndRaise(IsTopSideProperty, ref _top, value);
    /// <summary>Updates the right-edge flag exposed to templates.</summary>
    protected void SetIsRightSide(bool value) => SetAndRaise(IsRightSideProperty, ref _right, value);
    /// <summary>Updates the bottom-edge flag exposed to templates.</summary>
    protected void SetIsBottomSide(bool value) => SetAndRaise(IsBottomSideProperty, ref _bottom, value);

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(LayoutAnchorSideControl);
    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _binding.Initialize();
    }

    private void UpdateSide()
    {
        SetIsLeftSide(_model.Side == AnchorSide.Left);
        SetIsTopSide(_model.Side == AnchorSide.Top);
        SetIsRightSide(_model.Side == AnchorSide.Right);
        SetIsBottomSide(_model.Side == AnchorSide.Bottom);
    }
}
