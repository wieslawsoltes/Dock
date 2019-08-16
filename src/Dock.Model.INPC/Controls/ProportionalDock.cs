// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Runtime.Serialization;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Proportional dock.
    /// </summary>
    [DataContract(IsReference = true)]
    public class ProportionalDock : DockBase, IProportionalDock
    {
        private Orientation _orientation;

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Orientation Orientation
        {
            get => _orientation;
            set => this.RaiseAndSetIfChanged(ref _orientation, value);
        }

        /// <inheritdoc/>
        public override IDockable Clone()
        {
            return CloneHelper.CloneProportionalDock(this);
        }
    }
}
