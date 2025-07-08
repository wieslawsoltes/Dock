using System;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Window used to display pinned dock content when using floating overlay.
/// </summary>
public class PinnedDockWindow : Window
{
    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(PinnedDockWindow);
}
