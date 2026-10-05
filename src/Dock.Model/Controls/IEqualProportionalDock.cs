namespace Dock.Model.Controls;

/// <summary>Optional sizing policy for a proportional dock that rebalances after content changes.</summary>
public interface IEqualProportionalDock : IProportionalDock
{
    /// <summary>
    /// Gets or sets whether panes are balanced after insertion or removal.
    /// Nested groups along the same orientation receive a share weighted by their pane count.
    /// </summary>
    bool KeepProportionsEqual { get; set; }
}
