// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Runtime.Serialization;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Tool dock.
    /// </summary>
    [DataContract(IsReference = true)]
    public class ToolDock : DockBase, IToolDock
    {
        /// <summary>
        /// Initializes new instance of the <see cref="ToolDock"/> class.
        /// </summary>
        public ToolDock()
        {
            Id = nameof(IToolDock);
            Title = nameof(IToolDock);
        }

        /// <inheritdoc/>
        public override IDockable Clone()
        {
            return CloneHelper.CloneToolDock(this);
        }
    }
}
