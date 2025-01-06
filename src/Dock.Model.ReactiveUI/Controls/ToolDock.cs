using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Core;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// Tool dock.
/// </summary>
[DataContract(IsReference = true)]
public partial class ToolDock : DockBase, IToolDock
{
    /// <summary>
    /// Initializes new instance of the <see cref="ToolDock"/> class.
    /// </summary>
    public ToolDock()
    {
        _alignment = Alignment.Unset;
        _autoHide = true;
        _gripMode = GripMode.Visible;
    }
    
    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial Alignment Alignment { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool IsExpanded { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool AutoHide { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial GripMode GripMode { get; set; }
}
