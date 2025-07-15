using Avalonia.Controls;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Canvas that hosts <see cref="OverlayHostWindow"/> instances.
/// </summary>
public class OverlayHostCanvas : Canvas
{
    /// <summary>
    /// Gets the global overlay host canvas instance.
    /// </summary>
    public static OverlayHostCanvas? Instance { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OverlayHostCanvas"/> class.
    /// </summary>
    public OverlayHostCanvas()
    {
        Instance = this;
    }
}
