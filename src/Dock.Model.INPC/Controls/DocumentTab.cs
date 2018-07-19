// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Runtime.Serialization;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Document tab.
    /// </summary>
    [DataContract(IsReference = true)]
    public class DocumentTab : ViewBase, IDocumentTab
    {
    }
}
