using System;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Host window used for MDI style child windows.
/// </summary>
public class MdiHostWindow : HostWindow
{
    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(MdiHostWindow);

    /// <summary>
    /// Initializes new instance of the <see cref="MdiHostWindow"/> class.
    /// </summary>
    public MdiHostWindow()
    {
        SystemDecorations = SystemDecorations.None;
        ShowInTaskbar = false;
    }
}
