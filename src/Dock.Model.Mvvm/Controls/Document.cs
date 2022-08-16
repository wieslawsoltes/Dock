using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Mvvm.Core;

namespace Dock.Model.Mvvm.Controls;

/// <summary>
/// Document.
/// </summary>
[DataContract(IsReference = true)]
public class Document : DockableBase, IDocument
{
}
