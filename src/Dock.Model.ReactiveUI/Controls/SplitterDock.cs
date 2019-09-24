// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Runtime.Serialization;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Splitter dock.
    /// </summary>
    [DataContract(IsReference = true)]
    public class SplitterDock : DockBase, ISplitterDock
    {
        /// <summary>
        /// Initializes new instance of the <see cref="SplitterDock"/> class.
        /// </summary>
        public SplitterDock()
        {
            Id = nameof(ISplitterDock);
            Title = nameof(ISplitterDock);
        }

        /// <inheritdoc/>
        public override IDockable? Clone()
        {
            return CloneHelper.CloneSplitterDock(this);
        }
    }
}
