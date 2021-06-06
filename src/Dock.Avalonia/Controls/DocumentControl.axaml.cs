using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Interaction logic for <see cref="DocumentControl"/> xaml.
    /// </summary>
    [PseudoClasses(":active")]
    public class DocumentControl : TemplatedControl
    {
        /// <summary>
        /// Define the HeaderTemplate property.
        /// </summary>
        public static readonly AttachedProperty<IDataTemplate> HeaderTemplateProperty =
            AvaloniaProperty.RegisterAttached<DocumentControl, DocumentControl, IDataTemplate>("HeaderTemplate");

        /// <summary>
        /// Define the <see cref="IsActive"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsActiveProperty =
            AvaloniaProperty.Register<DocumentControl, bool>(nameof(IsActive));

        /// <summary>
        /// Gets the value of the HeaderTemplate attached property on the specified document control.
        /// </summary>
        /// <param name="documentControl">The document control.</param>
        /// <returns>The HeaderTemplate attached property.</returns>
        public static IDataTemplate GetHeaderTemplate(DocumentControl documentControl)
        {
            return documentControl.GetValue(HeaderTemplateProperty);
        }

        /// <summary>
        /// Sets the value of the HeaderTemplate attached property on the specified document control.
        /// </summary>
        /// <param name="documentControl">The control.</param>
        /// <param name="value">The value of the HeaderTemplate property.</param>
        public static void SetHeaderTemplate(DocumentControl documentControl, IDataTemplate value)
        {
            documentControl.SetValue(HeaderTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets if this is the currently active dockable.
        /// </summary>
        public bool IsActive
        {
            get => GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        /// <inheritdoc/>
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            AddHandler(PointerPressedEvent, Pressed, RoutingStrategies.Tunnel);
        }

        private void Pressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is IDock {Factory: { } factory} dock && dock.ActiveDockable is { })
            {
                if (factory.FindRoot(dock.ActiveDockable, _ => true) is { } root)
                {
                    factory.SetFocusedDockable(root, dock.ActiveDockable);
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == IsActiveProperty)
            {
                UpdatePseudoClasses(change.NewValue.GetValueOrDefault<bool>());
            }
        }

        private void UpdatePseudoClasses(bool isActive)
        {
            PseudoClasses.Set(":active", isActive);
        }
    }
}
