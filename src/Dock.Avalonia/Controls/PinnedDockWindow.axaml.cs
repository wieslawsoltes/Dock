using System;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Dock.Avalonia.Automation.Peers;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Window used to display pinned dock content when using floating overlay.
/// </summary>
public class PinnedDockWindow : Window
{
    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new PinnedDockWindowAutomationPeer(this);
    }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(PinnedDockWindow);
}
