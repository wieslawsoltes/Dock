using Avalonia.Controls;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Xceed.Wpf.AvalonDock.Infrastructure;

namespace Xceed.Wpf.AvalonDock.Layout;

/// <summary>A layout group with docked and floating dimensions.</summary>
/// <typeparam name="T">The accepted child type.</typeparam>
public abstract class LayoutPositionableGroup<T> : LayoutGroup<T> where T : class, ILayoutElement
{
    private GridLength _dockWidth = new(1, GridUnitType.Star);
    private GridLength _dockHeight = new(1, GridUnitType.Star);
    private bool _allowDuplicateContent = true;
    private bool _canRepositionItems = true;
    private double _floatingWidth;
    private double _floatingHeight;
    private double _floatingLeft;
    private double _floatingTop;
    private bool _isMaximized;

    /// <summary>Creates a positionable split group.</summary>
    public LayoutPositionableGroup() : this(new ProportionalDock()) { }
    internal LayoutPositionableGroup(DockBase model, LayoutModelContext? context = null) : base(model, context) { }

    /// <summary>Gets or sets the preferred docked width.</summary>
    public GridLength DockWidth { get => _dockWidth; set => SetField(ref _dockWidth, value); }
    /// <summary>Gets or sets the preferred docked height.</summary>
    public GridLength DockHeight { get => _dockHeight; set => SetField(ref _dockHeight, value); }
    /// <summary>Gets or sets whether duplicate content is allowed.</summary>
    public bool AllowDuplicateContent { get => _allowDuplicateContent; set => SetField(ref _allowDuplicateContent, value); }
    /// <summary>Gets or sets whether items can be repositioned.</summary>
    public bool CanRepositionItems { get => _canRepositionItems; set => SetField(ref _canRepositionItems, value); }
    /// <summary>Gets or sets the minimum docked width.</summary>
    public double DockMinWidth { get => DockModel.MinWidth; set => DockModel.MinWidth = value; }
    /// <summary>Gets or sets the minimum docked height.</summary>
    public double DockMinHeight { get => DockModel.MinHeight; set => DockModel.MinHeight = value; }
    /// <summary>Gets or sets the floating width.</summary>
    public double FloatingWidth { get => _floatingWidth; set => SetField(ref _floatingWidth, value); }
    /// <summary>Gets or sets the floating height.</summary>
    public double FloatingHeight { get => _floatingHeight; set => SetField(ref _floatingHeight, value); }
    /// <summary>Gets or sets the floating horizontal position.</summary>
    public double FloatingLeft { get => _floatingLeft; set => SetField(ref _floatingLeft, value); }
    /// <summary>Gets or sets the floating vertical position.</summary>
    public double FloatingTop { get => _floatingTop; set => SetField(ref _floatingTop, value); }
    /// <summary>Gets or sets whether the floating window is maximized.</summary>
    public bool IsMaximized { get => _isMaximized; set => SetField(ref _isMaximized, value); }
}
