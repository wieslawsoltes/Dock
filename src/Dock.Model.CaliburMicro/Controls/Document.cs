// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.CaliburMicro.Core;

namespace Dock.Model.CaliburMicro.Controls;

/// <summary>
/// Document.
/// </summary>
[DataContract(IsReference = true)]
public class Document : DockableBase, IDocument;
