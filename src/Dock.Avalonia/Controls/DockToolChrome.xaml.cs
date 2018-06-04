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