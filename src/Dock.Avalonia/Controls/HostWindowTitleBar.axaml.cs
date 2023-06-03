using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="HostWindowTitleBar"/> xaml.
/// </summary>
public class HostWindowTitleBar : TitleBar
{
    internal Control? BackgroundControl { get; private set; }
        
    protected override Type StyleKeyOverride => typeof(HostWindowTitleBar);

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        BackgroundControl = e.NameScope.Find<Control>("PART_Background");
    }
}
