// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Serializer.SystemTextJson;

/// <summary>
/// Optional marker for Dock source-generated <see cref="System.Text.Json"/> serialization support.
/// Metadata is generated automatically for serializer consumers; this marker remains supported for compatibility.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class DockJsonSourceGenerationAttribute : Attribute
{
}
