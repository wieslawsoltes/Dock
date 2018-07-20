// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Runtime.Serialization;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Tool tab.
    /// </summary>
    [DataContract(IsReference = true)]
    public class ToolTab : ViewBase, IToolTab, IDocumentTab
    {
        public virtual void OnClose()
        {
        }
    }
}
