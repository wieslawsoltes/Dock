﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Runtime.Serialization;
using Avalonia;

namespace Dock.Model.Controls
{
    /// <summary>
    /// Proportional dock.
    /// </summary>
    [DataContract(IsReference = true)]
    public class ProportionalDock : DockBase, IProportionalDock
    {
        /// <summary>
        /// Defines the <see cref="Orientation"/> property.
        /// </summary>
        public static readonly DirectProperty<ProportionalDock, Orientation> OrientationProperty =
            AvaloniaProperty.RegisterDirect<ProportionalDock, Orientation>(nameof(Orientation), o => o.Orientation, (o, v) => o.Orientation = v);

        private Orientation _orientation;

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Orientation Orientation
        {
            get => _orientation;
            set => SetAndRaise(OrientationProperty, ref _orientation, value);
        }

        /// <inheritdoc/>
        public override IDockable Clone()
        {
            return CloneHelper.CloneProportionalDock(this);
        }
    }
}
