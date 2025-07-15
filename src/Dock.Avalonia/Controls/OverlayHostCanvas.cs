using Avalonia.Controls;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Canvas that hosts <see cref="OverlayHostWindow"/> instances.
/// </summary>
public class OverlayHostCanvas : Canvas, IOverlayHost
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OverlayHostCanvas"/> class.
    /// </summary>
    public OverlayHostCanvas()
    {
        OverlayHost.Current = this;
    }

    /// <inheritdoc/>
    public void Add(OverlayHostWindow window)
    {
        if (!Children.Contains(window))
        {
            Children.Add(window);
        }
    }

    /// <inheritdoc/>
    public void Remove(OverlayHostWindow window)
    {
        Children.Remove(window);
    }
}
