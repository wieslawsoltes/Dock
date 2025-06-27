using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Core;

namespace Dock.Model.Mvvm.Controls;

/// <summary>
/// Pixel dock.
/// </summary>
[DataContract(IsReference = true)]
public class PixelDock : DockBase, IPixelDock
{
    private Orientation _orientation;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public Orientation Orientation
    {
        get => _orientation;
        set => SetProperty(ref _orientation, value);
    }
}
