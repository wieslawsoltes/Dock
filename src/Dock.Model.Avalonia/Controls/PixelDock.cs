using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Dock that arranges children using pixel size.
/// </summary>
[DataContract(IsReference = true)]
public class PixelDock : DockBase, IPixelDock
{
    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly DirectProperty<PixelDock, Orientation> OrientationProperty =
        AvaloniaProperty.RegisterDirect<PixelDock, Orientation>(nameof(Orientation), o => o.Orientation, (o, v) => o.Orientation = v);

    private Orientation _orientation;

    /// <summary>
    /// Initializes new instance of the <see cref="PixelDock"/> class.
    /// </summary>
    public PixelDock()
    {
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Orientation")]
    public Orientation Orientation
    {
        get => _orientation;
        set => SetAndRaise(OrientationProperty, ref _orientation, value);
    }
}
