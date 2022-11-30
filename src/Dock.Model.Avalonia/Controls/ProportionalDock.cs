using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Proportional dock.
/// </summary>
[DataContract(IsReference = true)]
[JsonSerializable(typeof(ProportionalDock))]
public class ProportionalDock : DockBase, IProportionalDock
{
    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly DirectProperty<ProportionalDock, Orientation> OrientationProperty =
        AvaloniaProperty.RegisterDirect<ProportionalDock, Orientation>(nameof(Orientation), o => o.Orientation, (o, v) => o.Orientation = v);

    private Orientation _orientation;

    /// <summary>
    /// Initializes new instance of the <see cref="ProportionalDock"/> class.
    /// </summary>
    [JsonConstructor]
    public ProportionalDock()
    {
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonInclude]
    public Orientation Orientation
    {
        get => _orientation;
        set => SetAndRaise(OrientationProperty, ref _orientation, value);
    }
}
