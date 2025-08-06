// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace Dock.Model.Core;

/// <summary>
/// Indicates that an interface requires a DataTemplate in DockControl.
/// This attribute is used to mark dock-related interfaces that need
/// corresponding DataTemplates for proper rendering in the dock system.
/// </summary>
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class RequiresDataTemplateAttribute : Attribute
{
}