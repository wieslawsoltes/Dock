// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="ProportionalDockControl"/> xaml.
/// </summary>
[TemplatePart("PART_ItemsControl", typeof(ItemsControl), IsRequired = true)]
public class ProportionalDockControl : TemplatedControl
{
}
