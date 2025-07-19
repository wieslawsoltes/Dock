namespace Dock.Model;

/// <summary>
/// Options used by <see cref="DockManager"/> to control docking behaviour.
/// </summary>
public class DockManagerOptions
{
    /// <summary>
    /// Gets or sets a value that prevents docking tools with conflicting fixed sizes.
    /// </summary>
    public bool PreventSizeConflicts { get; set; } = true;
}

