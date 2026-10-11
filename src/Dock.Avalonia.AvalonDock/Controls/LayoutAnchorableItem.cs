using System.Windows.Input;
using Avalonia;
using AvalonDock.Infrastructure;
using AvalonDock.Layout;
using Dock.Model.Core;

namespace AvalonDock.Controls;

/// <summary>A bindable container for an anchorable tool.</summary>
public class LayoutAnchorableItem : LayoutItem
{
    /// <summary>Identifies the tool hide command.</summary>
    public static readonly StyledProperty<ICommand?> HideCommandProperty =
        AvaloniaProperty.Register<LayoutAnchorableItem, ICommand?>(nameof(HideCommand));
    /// <summary>Identifies the tool auto-hide command.</summary>
    public static readonly StyledProperty<ICommand?> AutoHideCommandProperty =
        AvaloniaProperty.Register<LayoutAnchorableItem, ICommand?>(nameof(AutoHideCommand));
    /// <summary>Identifies the command returning a floating tool to its pane.</summary>
    public static readonly StyledProperty<ICommand?> DockCommandProperty =
        AvaloniaProperty.Register<LayoutAnchorableItem, ICommand?>(nameof(DockCommand));
    /// <summary>Identifies the tool hide capability.</summary>
    public static readonly StyledProperty<bool> CanHideProperty =
        AvaloniaProperty.Register<LayoutAnchorableItem, bool>(nameof(CanHide), true);

    /// <summary>Gets or sets the tool hide command.</summary>
    public ICommand? HideCommand { get => GetValue(HideCommandProperty); set => SetValue(HideCommandProperty, value); }
    /// <summary>Gets or sets the tool auto-hide command.</summary>
    public ICommand? AutoHideCommand { get => GetValue(AutoHideCommandProperty); set => SetValue(AutoHideCommandProperty, value); }
    /// <summary>Gets or sets the command returning a floating tool to its pane.</summary>
    public ICommand? DockCommand { get => GetValue(DockCommandProperty); set => SetValue(DockCommandProperty, value); }
    /// <summary>Gets or sets whether the tool can be hidden.</summary>
    public bool CanHide { get => GetValue(CanHideProperty); set => SetValue(CanHideProperty, value); }

    /// <inheritdoc/>
    protected override void Close() => LayoutElement?.Close();

    /// <inheritdoc/>
    protected override bool CanExecuteDockAsDocumentCommand() =>
        base.CanExecuteDockAsDocumentCommand() && LayoutElement is LayoutAnchorable { CanDockAsTabbedDocument: true };

    /// <inheritdoc/>
    protected override void InitDefaultCommands()
    {
        base.InitDefaultCommands();
        DefaultCommands.Add(HideCommandProperty, () => ((LayoutAnchorable?)LayoutElement)?.Hide(),
            () => IsAttached && LayoutElement is LayoutAnchorable { CanHide: true });
        DefaultCommands.Add(AutoHideCommandProperty, () => ((LayoutAnchorable?)LayoutElement)?.ToggleAutoHide(),
            () => IsAttached && LayoutElement is LayoutAnchorable { CanAutoHide: true, IsHidden: false } tool &&
                tool.FindParent<LayoutAnchorableFloatingWindow>() is null &&
                LayoutCommandCapabilities.IsEnabled(tool, DockCapability.Pin));
        DefaultCommands.Add(DockCommandProperty, () => LayoutElement?.Dock(),
            () => IsAttached && LayoutElement?.FindParent<LayoutAnchorableFloatingWindow>() is not null);
    }

    /// <summary>Called when the tool hide command changes.</summary>
    protected virtual void OnHideCommandChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Called when the tool auto-hide command changes.</summary>
    protected virtual void OnAutoHideCommandChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Called when the tool docking command changes.</summary>
    protected virtual void OnDockCommandChanged(AvaloniaPropertyChangedEventArgs e) { }
    /// <summary>Forwards a change to the tool hide capability.</summary>
    protected virtual void OnCanHideChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (!IsSynchronizing && LayoutElement is LayoutAnchorable tool) tool.CanHide = CanHide;
    }

    /// <inheritdoc/>
    protected override void OnVisibilityChanged()
    {
        if (IsSynchronizing || !IsAttached || LayoutElement is not LayoutAnchorable tool) return;
        if (IsVisible && tool.IsHidden) tool.Show();
        else if (!IsVisible && tool.CanHide) tool.Hide();
        SynchronizeModel();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == HideCommandProperty) OnHideCommandChanged(change);
        else if (change.Property == AutoHideCommandProperty) OnAutoHideCommandChanged(change);
        else if (change.Property == DockCommandProperty) OnDockCommandChanged(change);
        else if (change.Property == CanHideProperty) OnCanHideChanged(change);
    }

    internal override void UpdateModelProperties(string? propertyName)
    {
        base.UpdateModelProperties(propertyName);
        if (LayoutElement is not LayoutAnchorable tool) return;
        if (propertyName is null or nameof(CanHide)) SetCurrentValue(CanHideProperty, tool.CanHide);
        if (propertyName is null or nameof(IsVisible) or "Parent" or "Root") SetCurrentValue(IsVisibleProperty, tool.IsVisible);
    }
}
