using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Dock TabStrip custom control.
    /// </summary>
    [PseudoClasses(":create")]
    public class DockTabStrip : TabStrip, IStyleable
    {
        /// <summary>
        /// Defines the <see cref="CanCreateItem"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> CanCreateItemProperty =
            AvaloniaProperty.Register<DockPanel, bool>(nameof(CanCreateItem));

        /// <summary>
        /// Gets or sets if tab strop dock can create new items.
        /// </summary>
        public bool CanCreateItem
        {
            get => GetValue(CanCreateItemProperty);
            set => SetValue(CanCreateItemProperty, value);
        }

        Type IStyleable.StyleKey => typeof(TabStrip);

        /// <summary>
        /// Initializes new instance of the <see cref="DockTabStrip"/> class.
        /// </summary>
        public DockTabStrip()
        {
            UpdatePseudoClasses(CanCreateItem);
        }

        /// <inheritdoc/>
        protected override IItemContainerGenerator CreateItemContainerGenerator()
        {
            return new ItemContainerGenerator<DockTabStripItem>(
                this,
                ContentControl.ContentProperty,
                ContentControl.ContentTemplateProperty);
        }

        /// <inheritdoc/>
        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == CanCreateItemProperty)
            {
                UpdatePseudoClasses(change.NewValue.GetValueOrDefault<bool>());
            }
        }

        private void UpdatePseudoClasses(bool canCreate)
        {
            PseudoClasses.Set(":create", canCreate);
        }
    }
}
