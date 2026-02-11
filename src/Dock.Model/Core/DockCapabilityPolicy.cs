// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;

namespace Dock.Model.Core;

/// <summary>
/// Default capability policy implementation used by root and dock policies.
/// Null values inherit lower-precedence values.
/// </summary>
[DataContract]
public class DockCapabilityPolicy : IDockCapabilityPolicy
{
    /// <inheritdoc />
    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public bool? CanClose { get; set; }

    /// <inheritdoc />
    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public bool? CanPin { get; set; }

    /// <inheritdoc />
    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public bool? CanFloat { get; set; }

    /// <inheritdoc />
    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public bool? CanDrag { get; set; }

    /// <inheritdoc />
    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public bool? CanDrop { get; set; }

    /// <inheritdoc />
    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public bool? CanDockAsDocument { get; set; }

    /// <summary>
    /// Gets the configured value for the specified capability.
    /// </summary>
    /// <param name="capability">The capability to query.</param>
    /// <returns>The configured value, or null when unset.</returns>
    public virtual bool? Get(DockCapability capability)
    {
        return capability switch
        {
            DockCapability.Close => CanClose,
            DockCapability.Pin => CanPin,
            DockCapability.Float => CanFloat,
            DockCapability.Drag => CanDrag,
            DockCapability.Drop => CanDrop,
            DockCapability.DockAsDocument => CanDockAsDocument,
            _ => null
        };
    }
}
