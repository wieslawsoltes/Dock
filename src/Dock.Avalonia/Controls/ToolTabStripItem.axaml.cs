using System;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Dock.Avalonia.Internal;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Tool TabStripItem custom control.
/// </summary>
public class ToolTabStripItem : TabStripItem, IStyleable
{
    Type IStyleable.StyleKey => typeof(ToolTabStripItem);

    private readonly ItemDragBehavior _itemDragBehavior;

    /// <summary>
    /// Initializes new instance of the <see cref="ToolTabStripItem"/> class.
    /// </summary>
    public ToolTabStripItem()
    {
        _itemDragBehavior = new ItemDragBehavior(this, Orientation.Horizontal);
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _itemDragBehavior.Attach();

        AddHandler(PointerPressedEvent, PressedHandler, RoutingStrategies.Tunnel);
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _itemDragBehavior.Detach();

        RemoveHandler(PointerPressedEvent, PressedHandler);
    }

    private void PressedHandler(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed)
        {
            if (DataContext is IDockable { Owner: IDock { Factory: { } factory } } dockable)
            {
                factory.CloseDockable(dockable);
            }
        }
    }
}
