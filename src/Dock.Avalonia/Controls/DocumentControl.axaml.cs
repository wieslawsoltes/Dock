using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Interaction logic for <see cref="DocumentControl"/> xaml.
    /// </summary>
    public class DocumentControl : TemplatedControl
    {
        /// <inheritdoc/>
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            AddHandler(PointerPressedEvent, Pressed, RoutingStrategies.Tunnel);
        }

        private void Pressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is IDock {Factory: { } factory} dock && dock.ActiveDockable != null)
            {
                if (factory.FindRoot(dock.ActiveDockable, _ => true) is { } root)
                {
                    factory.SetFocusedDockable(root, dock.ActiveDockable);
                }
            }
        }
    }
}
