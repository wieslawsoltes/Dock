using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="ToolPinItemControl"/> xaml.
/// </summary>
public class ToolPinItemControl : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<ToolPinItemControl, Orientation>(nameof(Orientation), Orientation.Vertical);

    /// <summary>
    /// Gets or sets the orientation in which control will be layed out.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
}
