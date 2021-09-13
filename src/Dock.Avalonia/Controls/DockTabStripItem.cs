using System;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Dock TabStripItem custom control.
    /// </summary>
    public class DockTabStripItem : TabStripItem, IStyleable
    {
        Type IStyleable.StyleKey => typeof(TabStripItem);
        
        /// <inheritdoc/>
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            AddHandler(PointerPressedEvent, PressedHandler, RoutingStrategies.Tunnel);
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
}
