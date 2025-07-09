// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Core;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// Tool.
/// </summary>
[DataContract(IsReference = true)]
public partial class Tool : DockableBase, ITool, IDocument
{

    /// <summary>
    /// Initializes new instance of the <see cref="Tool"/> class.
    /// </summary>
    public Tool()
    {
        PinnedAlignment = Alignment.Top;
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial Alignment PinnedAlignment { get; set; }
}
