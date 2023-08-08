using System.Runtime.Serialization;
using System.Windows.Input;
using Dock.Model.Controls;
using Dock.Model.Mvvm.Core;

namespace Dock.Model.Mvvm.Controls;

/// <summary>
/// Document dock.
/// </summary>
[DataContract(IsReference = true)]
public class DocumentDock : DockBase, IDocumentDock
{
    private readonly bool _canCreateDocument;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanCreateDocument
    {
        get => _canCreateDocument;
        set => SetProperty(ref _canCreateDocument, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand? CreateDocument { get; set; }
}
