using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Primitives;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Hosts multiple <see cref="DockControl"/> instances side by side or stacked.
/// </summary>
public class DockSiteControl : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<DockSiteControl, Orientation>(nameof(Orientation), Orientation.Horizontal);

    /// <summary>
    /// Defines the <see cref="DockControls"/> property.
    /// </summary>
    public static readonly StyledProperty<IList<IDockControl>> DockControlsProperty =
        AvaloniaProperty.Register<DockSiteControl, IList<IDockControl>>(nameof(DockControls), new List<IDockControl>());

    /// <summary>
    /// Gets or sets layout orientation.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets the hosted dock controls.
    /// </summary>
    public IList<IDockControl> DockControls
    {
        get => GetValue(DockControlsProperty);
        set => SetValue(DockControlsProperty, value);
    }
}
