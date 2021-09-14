using System;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Dock.Avalonia.Internal;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Dock TabStripItem custom control.
    /// </summary>
    public class DockTabStripItem : TabStripItem, IStyleable
    {
        Type IStyleable.StyleKey => typeof(TabStripItem);

        /// <summary>
        /// Initializes new instance of the <see cref="DockTabStripItem"/> class.
        /// </summary>
        public DockTabStripItem()
        {
        }
        
        /// <inheritdoc/>
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            AddHandler(PointerPressedEvent, PressedHandler, RoutingStrategies.Tunnel);
        }

        /// <inheritdoc/>
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

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
}
