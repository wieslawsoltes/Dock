using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Model;
using Dock.Model.Controls;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Represents a pin attached to side of dock, which can expand and collapse
    /// </summary>
    public class PinDockControl : ContentControl
    {
        private IDisposable? _disposable;

        /// <inheritdoc/>
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);        
            _disposable = this.AddDisposableHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Tunnel);
        }

        /// <inheritdoc/>
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            _disposable?.Dispose();
        }

        private void Pressed(object sender, PointerPressedEventArgs e)
        {
            if (this.DataContext is IDock dock && dock.Factory is IFactory factory)
            {
                if (dock.ActiveDockable != null)
                {
                    if (factory.FindRoot(dock.ActiveDockable) is IRootDock root)
                    {
                        Debug.WriteLine($"{nameof(PinDockControl)} SetFocusedDockable {dock.ActiveDockable.GetType().Name}, owner: {dock.Title}");
                        factory.SetFocusedDockable(root, dock.ActiveDockable);
                    }
                }
            }
        }
    }
}
