using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Dock tool chrome content control.
    /// </summary>
    [PseudoClasses(":floating", ":active")]
    public class ToolChromeControl : ContentControl
    {
        /// <summary>
        /// Define <see cref="Title"/> property.
        /// </summary>
        public static readonly StyledProperty<string> TitleProprty =
            AvaloniaProperty.Register<ToolChromeControl, string>(nameof(Title));

        /// <summary>
        /// Define the <see cref="IsActive"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsActiveProperty =
            AvaloniaProperty.Register<ToolChromeControl, bool>(nameof(IsActive));

        /// <summary>
        /// Initialize the new instance of the <see cref="ToolChromeControl"/>.
        /// </summary>
        public ToolChromeControl()
        {
            UpdatePseudoClasses(IsActive);
        }

        /// <summary>
        /// Gets or sets chrome tool title.
        /// </summary>
        public string Title
        {
            get => GetValue(TitleProprty);
            set => SetValue(TitleProprty, value);
        }

        /// <summary>
        /// Gets or sets if this is the currently active Tool.
        /// </summary>
        public bool IsActive
        {
            get => GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        internal Control? Grip { get; private set; }

        internal Button? CloseButton { get; private set; }

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

        /// <inheritdoc/>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (VisualRoot is HostWindow window)
            {
                Grip = e.NameScope.Find<Control>("PART_Grip");
                CloseButton = e.NameScope.Find<Button>("PART_CloseButton");

                window.AttachGrip(this);

                PseudoClasses.Set(":floating", true);
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
