using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace AvalonDock.Controls;

/// <summary>A context menu that generates templated icon items and refreshes its bound source when opened.</summary>
public class ContextMenuEx : ContextMenu
{
    /// <summary>Creates a menu using Avalonia's context-menu theme and opening route.</summary>
    public ContextMenuEx() => AddHandler(OpenedEvent, OpenedCore);

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(ContextMenu);

    /// <summary>Creates the container for a data item. Explicit menu items and separators retain their identity.</summary>
    protected virtual AvaloniaObject GetContainerForItemOverride() => new MenuItemEx();

    /// <summary>Refreshes a bound item source, including computed snapshots that changed while the menu was closed.</summary>
    protected virtual void OnOpened(RoutedEventArgs e) => BindingOperations.GetBindingExpressionBase(this, ItemsSourceProperty)?.UpdateTarget();

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) =>
        GetContainerForItemOverride() as Control ?? throw new InvalidOperationException("A context-menu item container must be an Avalonia control.");

    private void OpenedCore(object? sender, RoutedEventArgs e)
    {
        if (ReferenceEquals(e.Source, this)) OnOpened(e);
    }
}
