using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Model;
using Dock.Model.Controls;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Interaction logic for <see cref="ToolControl"/> xaml.
    /// </summary>
    public class ToolControl : TemplatedControl
    {
        /// <inheritdoc/>
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Tunnel);
        }

        /// <inheritdoc/>
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
        }

        private void Pressed(object sender, PointerPressedEventArgs e)
        {
            if (this.DataContext is IDock dock && dock.Factory is IFactory factory)
            {
                if (dock.ActiveDockable != null)
                {
                    if (factory.FindRoot(dock.ActiveDockable) is IRootDock root)
                    {
                        Debug.WriteLine($"{nameof(ToolControl)} SetFocusedDockable {dock.ActiveDockable.GetType().Name}, owner: {dock.Title}");
                        factory.SetFocusedDockable(root, dock.ActiveDockable);
                    }
                }
            }
        }
    }
}
