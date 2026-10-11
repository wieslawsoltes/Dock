namespace AvalonDock.Layout;

/// <summary>A control presenting an AvalonDock layout model.</summary>
public interface ILayoutControl
{
    /// <summary>Gets the public layout model displayed by this control.</summary>
    ILayoutElement Model { get; }
}
