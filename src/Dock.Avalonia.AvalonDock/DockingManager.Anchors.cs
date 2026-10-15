using AvalonDock.Infrastructure;
using Avalonia;
using Avalonia.Controls.Templates;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock;

public partial class DockingManager
{
    /// <summary>Identifies the control template for auto-hide side controls.</summary>
    public static readonly StyledProperty<IControlTemplate?> AnchorSideTemplateProperty =
        AvaloniaProperty.Register<DockingManager, IControlTemplate?>(name: nameof(AnchorSideTemplate));
    /// <summary>Identifies the control template for auto-hide group controls.</summary>
    public static readonly StyledProperty<IControlTemplate?> AnchorGroupTemplateProperty =
        AvaloniaProperty.Register<DockingManager, IControlTemplate?>(name: nameof(AnchorGroupTemplate));
    /// <summary>Identifies the control template for individual auto-hide anchors.</summary>
    public static readonly StyledProperty<IControlTemplate?> AnchorTemplateProperty =
        AvaloniaProperty.Register<DockingManager, IControlTemplate?>(name: nameof(AnchorTemplate));
    /// <summary>Identifies the displayed left auto-hide side control.</summary>
    public static readonly StyledProperty<LayoutAnchorSideControl?> LeftSidePanelProperty =
        AvaloniaProperty.Register<DockingManager, LayoutAnchorSideControl?>(name: nameof(LeftSidePanel));
    /// <summary>Identifies the displayed top auto-hide side control.</summary>
    public static readonly StyledProperty<LayoutAnchorSideControl?> TopSidePanelProperty =
        AvaloniaProperty.Register<DockingManager, LayoutAnchorSideControl?>(name: nameof(TopSidePanel));
    /// <summary>Identifies the displayed right auto-hide side control.</summary>
    public static readonly StyledProperty<LayoutAnchorSideControl?> RightSidePanelProperty =
        AvaloniaProperty.Register<DockingManager, LayoutAnchorSideControl?>(name: nameof(RightSidePanel));
    /// <summary>Identifies the displayed bottom auto-hide side control.</summary>
    public static readonly StyledProperty<LayoutAnchorSideControl?> BottomSidePanelProperty =
        AvaloniaProperty.Register<DockingManager, LayoutAnchorSideControl?>(name: nameof(BottomSidePanel));

    private LayoutAnchorSide? _presentedLeftSide, _presentedTopSide, _presentedRightSide, _presentedBottomSide;

    /// <summary>Gets or sets the template applied to all auto-hide side controls.</summary>
    public IControlTemplate? AnchorSideTemplate { get => GetValue(AnchorSideTemplateProperty); set => SetValue(AnchorSideTemplateProperty, value); }
    /// <summary>Gets or sets the template applied to all auto-hide group controls.</summary>
    public IControlTemplate? AnchorGroupTemplate { get => GetValue(AnchorGroupTemplateProperty); set => SetValue(AnchorGroupTemplateProperty, value); }
    /// <summary>Gets or sets the template applied to all individual auto-hide anchors.</summary>
    public IControlTemplate? AnchorTemplate { get => GetValue(AnchorTemplateProperty); set => SetValue(AnchorTemplateProperty, value); }
    /// <summary>Gets or sets the displayed left auto-hide side; null removes its presentation.</summary>
    public LayoutAnchorSideControl? LeftSidePanel { get => GetValue(LeftSidePanelProperty); set => SetValue(LeftSidePanelProperty, value); }
    /// <summary>Gets or sets the displayed top auto-hide side; null removes its presentation.</summary>
    public LayoutAnchorSideControl? TopSidePanel { get => GetValue(TopSidePanelProperty); set => SetValue(TopSidePanelProperty, value); }
    /// <summary>Gets or sets the displayed right auto-hide side; null removes its presentation.</summary>
    public LayoutAnchorSideControl? RightSidePanel { get => GetValue(RightSidePanelProperty); set => SetValue(RightSidePanelProperty, value); }
    /// <summary>Gets or sets the displayed bottom auto-hide side; null removes its presentation.</summary>
    public LayoutAnchorSideControl? BottomSidePanel { get => GetValue(BottomSidePanelProperty); set => SetValue(BottomSidePanelProperty, value); }

    /// <summary>Called after the displayed left auto-hide side changes.</summary>
    protected virtual void OnLeftSidePanelChanged(AvaloniaPropertyChangedEventArgs e) => ReplaceLogicalChild(e);
    /// <summary>Called after the displayed top auto-hide side changes.</summary>
    protected virtual void OnTopSidePanelChanged(AvaloniaPropertyChangedEventArgs e) => ReplaceLogicalChild(e);
    /// <summary>Called after the displayed right auto-hide side changes.</summary>
    protected virtual void OnRightSidePanelChanged(AvaloniaPropertyChangedEventArgs e) => ReplaceLogicalChild(e);
    /// <summary>Called after the displayed bottom auto-hide side changes.</summary>
    protected virtual void OnBottomSidePanelChanged(AvaloniaPropertyChangedEventArgs e) => ReplaceLogicalChild(e);

    private void OnAnchorControlPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == LeftSidePanelProperty) OnLeftSidePanelChanged(e);
        if (e.Property == TopSidePanelProperty) OnTopSidePanelChanged(e);
        if (e.Property == RightSidePanelProperty) OnRightSidePanelChanged(e);
        if (e.Property == BottomSidePanelProperty) OnBottomSidePanelChanged(e);
    }

    internal void EnsureAnchorSidePanels(bool force = false)
    {
        Ensure(LeftSidePanelProperty, Layout.LeftSide, ref _presentedLeftSide);
        Ensure(TopSidePanelProperty, Layout.TopSide, ref _presentedTopSide);
        Ensure(RightSidePanelProperty, Layout.RightSide, ref _presentedRightSide);
        Ensure(BottomSidePanelProperty, Layout.BottomSide, ref _presentedBottomSide);

        void Ensure(StyledProperty<LayoutAnchorSideControl?> property, LayoutAnchorSide? side, ref LayoutAnchorSide? previous)
        {
            if (!force && ReferenceEquals(previous, side)) return;
            previous = side;
            SetCurrentValue(property, side is null ? null : new LayoutAnchorSideControl(side));
        }
    }
}
