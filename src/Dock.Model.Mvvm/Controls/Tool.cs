using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Mvvm.Core;

namespace Dock.Model.Mvvm.Controls;

/// <summary>
/// Tool.
/// </summary>
[DataContract(IsReference = true)]
public class Tool : DockableBase, ITool, IDocument
{
}
