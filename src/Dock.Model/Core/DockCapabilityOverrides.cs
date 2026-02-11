// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;

namespace Dock.Model.Core;

/// <summary>
/// Per-dockable capability overrides with highest precedence.
/// Null values inherit from dock/root policy resolution.
/// </summary>
[DataContract]
public class DockCapabilityOverrides : DockCapabilityPolicy
{
    /// <summary>
    /// Gets a value indicating whether at least one override value is configured.
    /// </summary>
    [IgnoreDataMember]
    public bool HasAnyOverride =>
        CanClose.HasValue
        || CanPin.HasValue
        || CanFloat.HasValue
        || CanDrag.HasValue
        || CanDrop.HasValue
        || CanDockAsDocument.HasValue;
}
