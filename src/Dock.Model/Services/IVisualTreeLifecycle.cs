namespace Dock.Model.Services;

/// <summary>
/// Provides hooks for visual tree attachment and detachment.
/// </summary>
public interface IVisualTreeLifecycle
{
    /// <summary>
    /// Called when the instance is attached to the visual tree.
    /// </summary>
    void OnAttachedToVisualTree();

    /// <summary>
    /// Called when the instance is detached from the visual tree.
    /// </summary>
    void OnDetachedFromVisualTree();
}
