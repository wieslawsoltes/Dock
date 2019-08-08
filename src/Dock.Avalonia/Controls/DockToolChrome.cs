// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Dock tool chrome content control.
    /// </summary>
    public class DockToolChrome : ContentControl
    {
        static DockToolChrome()
        {
            PseudoClass<DockToolChrome>(IsActiveProperty, ":active");
        }

        /// <summary>
        /// Define <see cref="Title"/> property.
        /// </summary>
        public static readonly AvaloniaProperty<string> TitleProprty =
            AvaloniaProperty.Register<DockToolChrome, string>(nameof(Title));

        /// <summary>
        /// Gets or sets chrome tool title.
        /// </summary>
        public string Title
        {
            get { return GetValue(TitleProprty); }
            set { SetValue(TitleProprty, value); }
        }

        /// <summary>
        /// Define the <see cref="IsActive"/> property.
        /// </summary>
        public static readonly AvaloniaProperty<bool> IsActiveProperty =
            AvaloniaProperty.Register<DockToolChrome, bool>(nameof(IsActive));

        /// <summary>
        /// Gets or sets if this is the currently active Tool.
        /// </summary>
        public bool IsActive
        {
            get => GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        internal Control Grip { get; private set; }

        internal Button CloseButton { get; private set; }

        /// <inheritdoc/>
        protected override void OnTemplateApplied(TemplateAppliedEventArgs e)
        {
            base.OnTemplateApplied(e);

            if (VisualRoot is HostWindow window)
            {
                Grip = e.NameScope.Find<Control>("PART_Grip");

                CloseButton = e.NameScope.Find<Button>("PART_CloseButton");

                window.AttachGrip(this);

                this.PseudoClasses.Set(":floating", true);
            }
        }
    }
}
