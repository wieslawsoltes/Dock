// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using System.Windows.Input;
using Dock.Model.Controls;
using Dock.Model.Mvvm.Core;
using Dock.Model.Core;

namespace Dock.Model.Mvvm.Controls;

/// <summary>
/// Document dock.
/// </summary>
[DataContract(IsReference = true)]
public class DocumentDock : DockBase, IDocumentDock
{
    private bool _canCreateDocument;

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

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DocumentTabLayout TabsLayout { get; set; } = DocumentTabLayout.Top;
}
