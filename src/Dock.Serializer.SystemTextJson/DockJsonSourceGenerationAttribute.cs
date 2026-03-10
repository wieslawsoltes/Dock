// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Serializer.SystemTextJson;

/// <summary>
/// Activates Dock source-generated <see cref="System.Text.Json"/> serialization support for the current assembly.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class DockJsonSourceGenerationAttribute : Attribute
{
}
