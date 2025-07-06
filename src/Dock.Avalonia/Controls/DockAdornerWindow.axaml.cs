using System;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Window used to display dock adorners when using floating overlay.
/// </summary>
public class DockAdornerWindow : Window
{
    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(DockAdornerWindow);
}
