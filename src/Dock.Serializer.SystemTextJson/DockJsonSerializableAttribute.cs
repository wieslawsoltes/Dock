// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Serializer.SystemTextJson;

/// <summary>
/// Registers an additional type for Dock source-generated <see cref="System.Text.Json"/> serialization.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class DockJsonSerializableAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DockJsonSerializableAttribute"/> class.
    /// </summary>
    /// <param name="type">The type to include in generated metadata.</param>
    public DockJsonSerializableAttribute(Type type)
    {
        Type = type;
    }

    /// <summary>
    /// Gets the type to include in generated metadata.
    /// </summary>
    public Type Type { get; }
}
