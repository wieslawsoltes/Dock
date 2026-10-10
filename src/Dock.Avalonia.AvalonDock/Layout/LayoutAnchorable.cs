using System;
using System.ComponentModel;
using Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Xceed.Wpf.AvalonDock.Infrastructure;

namespace Xceed.Wpf.AvalonDock.Layout;

/// <summary>An AvalonDock tool backed by a Dock Avalonia tool.</summary>
public class LayoutAnchorable : LayoutContent
{
    private bool _canHide = true;
    private double _autoHideMinWidth = 100;
    private double _autoHideMinHeight = 100;
    /// <summary>Creates an anchorable tool.</summary>
    public LayoutAnchorable() : this(new Tool { CanClose = false }) { }
    internal LayoutAnchorable(Tool model, LayoutModelContext? context = null) : base(model, context) { }
    /// <summary>Gets or sets the auto-hide popup width.</summary>
    public double AutoHideWidth
    {
        get => DockModel.PinnedBounds?.Width ?? 0;
        set => DockModel.PinnedBounds = new DockRect(0, 0, value, AutoHideHeight);
    }
    /// <summary>Gets or sets the auto-hide popup height.</summary>
    public double AutoHideHeight
    {
        get => DockModel.PinnedBounds?.Height ?? 0;
        set => DockModel.PinnedBounds = new DockRect(0, 0, AutoHideWidth, value);
    }
    /// <summary>Gets or sets the minimum auto-hide width.</summary>
    public double AutoHideMinWidth { get => _autoHideMinWidth; set => SetField(ref _autoHideMinWidth, value); }
    /// <summary>Gets or sets the minimum auto-hide height.</summary>
    public double AutoHideMinHeight { get => _autoHideMinHeight; set => SetField(ref _autoHideMinHeight, value); }
    /// <summary>Gets or sets whether the tool can be hidden.</summary>
    public bool CanHide { get => _canHide; set => SetField(ref _canHide, value); }
    /// <summary>Gets or sets whether the tool can be auto-hidden.</summary>
    public bool CanAutoHide { get => DockModel.CanPin; set => DockModel.CanPin = value; }
    /// <summary>Gets or sets whether the tool can become a document tab.</summary>
    public bool CanDockAsTabbedDocument { get => DockModel.CanDockAsDocument; set => DockModel.CanDockAsDocument = value; }
    /// <summary>Gets whether this tool is in an auto-hide strip.</summary>
    public bool IsAutoHidden => (DockModel.DockingState & DockingWindowState.Pinned) != 0;
    /// <summary>Gets whether this tool is hidden.</summary>
    public bool IsHidden => (DockModel.DockingState & DockingWindowState.Hidden) != 0;
    /// <summary>Gets whether this tool is visible in a layout.</summary>
    public bool IsVisible => Parent is not null && !IsHidden;
    /// <summary>Occurs when visibility changes.</summary>
    public event EventHandler? IsVisibleChanged;
    /// <summary>Occurs before hiding and permits cancellation.</summary>
    public event EventHandler<CancelEventArgs>? Hiding;
    /// <summary>Occurs after hiding.</summary>
    public event EventHandler? Hidden;
    /// <inheritdoc/>
    public override void Close() => CloseCore();
    /// <summary>Hides this tool without destroying its content.</summary>
    public void Hide(bool cancelable = true)
    {
        if (IsHidden || !CanHide) return;
        CancelEventArgs args = new();
        if (cancelable) Hiding?.Invoke(this, args);
        if (args.Cancel) return;
        DockModel.Factory?.HideDockable(DockModel);
        if (IsHidden) Hidden?.Invoke(this, EventArgs.Empty);
    }
    /// <summary>Restores this tool to its previous pane.</summary>
    public void Show() => DockModel.Factory?.RestoreDockable(DockModel);
    /// <summary>Toggles Dock's auto-hide state.</summary>
    public void ToggleAutoHide()
    {
        if (!CanAutoHide) return;
        if (IsAutoHidden) DockModel.Factory?.UnpinDockable(DockModel);
        else DockModel.Factory?.PinDockable(DockModel);
    }
    internal override void OnModelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        base.OnModelPropertyChanged(sender, e);
        if (e.Property == DockableBase.DockingStateProperty)
        {
            RaisePropertyChanged(nameof(IsHidden));
            RaisePropertyChanged(nameof(IsAutoHidden));
            RaisePropertyChanged(nameof(IsVisible));
            IsVisibleChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
