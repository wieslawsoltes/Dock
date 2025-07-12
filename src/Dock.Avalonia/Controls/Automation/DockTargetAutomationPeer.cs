using Avalonia.Automation.Peers;

namespace Dock.Avalonia.Controls.Automation;

/// <summary>
/// Automation peer for dock targets.
/// </summary>
public class DockTargetAutomationPeer : ControlAutomationPeer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DockTargetAutomationPeer"/> class.
    /// </summary>
    /// <param name="owner">The owning control.</param>
    public DockTargetAutomationPeer(DockTargetBase owner) : base(owner)
    {
    }

    /// <inheritdoc/>
    protected override string GetClassNameCore() => nameof(DockTarget);
}
