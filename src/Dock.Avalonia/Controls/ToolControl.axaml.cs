using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Interaction logic for <see cref="ToolControl"/> xaml.
    /// </summary>
    public class ToolControl : TemplatedControl
    {
        /// <summary>
        /// Define the <see cref="HeaderTemplate"/> property.
        /// </summary>
        public static readonly StyledProperty<IDataTemplate> HeaderTemplateProperty =
            AvaloniaProperty.Register<ToolControl, IDataTemplate>(nameof(HeaderTemplate));

        /// <summary>
        /// Gets or sets tab header template.
        /// </summary>
        public IDataTemplate HeaderTemplate
        {
            get => GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        /// <inheritdoc/>
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            AddHandler(PointerPressedEvent, PressedHandler, RoutingStrategies.Tunnel);
        }

        private void PressedHandler(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is IDock {Factory: { } factory} dock && dock.ActiveDockable is { })
            {
                if (factory.FindRoot(dock.ActiveDockable, _ => true) is { } root)
                {
                    factory.SetFocusedDockable(root, dock.ActiveDockable);
                }
            }
        }
    }
}
