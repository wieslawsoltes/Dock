// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Window used to display drag preview content.
/// </summary>
public class DragPreviewWindow : Window
{
    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(DragPreviewWindow);
}
