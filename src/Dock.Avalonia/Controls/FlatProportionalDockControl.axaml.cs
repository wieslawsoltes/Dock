// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Presents an <see cref="Dock.Model.Controls.IProportionalDock"/> through a flattened visual panel.
/// </summary>
[TemplatePart("PART_Panel", typeof(FlatProportionalDockPanel), IsRequired = true)]
public class FlatProportionalDockControl : TemplatedControl
{
}
