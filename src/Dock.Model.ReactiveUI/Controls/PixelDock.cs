using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Core;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// Pixel dock.
/// </summary>
[DataContract(IsReference = true)]
public partial class PixelDock : DockBase, IPixelDock
{
    private Orientation _orientation;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public Orientation Orientation
    {
        get => _orientation;
        set => this.RaiseAndSetIfChanged(ref _orientation, value);
    }
}
