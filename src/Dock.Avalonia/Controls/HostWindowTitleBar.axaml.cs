using System;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Interaction logic for <see cref="HostWindowTitleBar"/> xaml.
    /// </summary>
    public class HostWindowTitleBar : TitleBar, IStyleable
    {
        internal Control? BackgroundControl { get; private set; }
        
        Type IStyleable.StyleKey => typeof(HostWindowTitleBar);

        /// <inheritdoc/>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            BackgroundControl = e.NameScope.Find<Control>("PART_Background");
        }
    }
}
