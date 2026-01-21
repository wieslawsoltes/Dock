using System;

namespace Dock.Avalonia.Controls.Overlays;

/// <summary>
/// Wraps a delegate to create overlay layer instances.
/// </summary>
public sealed class OverlayLayerFactory : IOverlayLayerFactory
{
    private readonly Func<IOverlayLayer> _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="OverlayLayerFactory"/> class.
    /// </summary>
    /// <param name="factory">The factory used to create overlay layers.</param>
    public OverlayLayerFactory(Func<IOverlayLayer> factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <inheritdoc />
    public IOverlayLayer Create() => _factory();
}
