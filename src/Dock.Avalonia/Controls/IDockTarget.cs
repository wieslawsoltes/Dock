namespace Dock.Avalonia.Controls;

/// <summary>
/// Interface all DockTargets should implement.
/// </summary>
public interface IDockTarget
{
    /// <summary>
    /// Resets the state of the DockTarget to be reused.
    /// </summary>
    void Reset();
}
    
