using System.Runtime.Serialization;
using System.Windows.Input;
using Dock.Model.Controls;
using Dock.Model.ReactiveUI.Core;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// Document dock.
/// </summary>
[DataContract(IsReference = true)]
public partial class DocumentDock : DockBase, IDocumentDock
{
    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool CanCreateDocument { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand? CreateDocument { get; set; }
}
