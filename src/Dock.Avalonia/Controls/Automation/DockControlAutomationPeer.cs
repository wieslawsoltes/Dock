using Avalonia.Automation.Peers;
using Avalonia.Automation;

namespace Dock.Avalonia.Controls.Automation;

/// <summary>
/// Automation peer for <see cref="DockControl"/>.
/// </summary>
public class DockControlAutomationPeer : ControlAutomationPeer
{
    private readonly DockControl _owner;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockControlAutomationPeer"/> class.
    /// </summary>
    /// <param name="owner">The owning control.</param>
    public DockControlAutomationPeer(DockControl owner) : base(owner)
    {
        _owner = owner;
    }

    /// <inheritdoc/>
    protected override string GetClassNameCore() => nameof(DockControl);

    /// <inheritdoc/>
    protected override string? GetNameCore()
    {
        var name = AutomationProperties.GetName(_owner);
        if (!string.IsNullOrEmpty(name))
            return name;

        if (_owner.Layout is { } dock && !string.IsNullOrEmpty(dock.Id))
            return dock.Id;

        return base.GetNameCore();
    }
}
