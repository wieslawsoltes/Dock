// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Tool TabStripItem custom control.
/// </summary>
public class ToolTabStripItem : DraggableTabStripItem<ToolTabStrip>
{
    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(ToolTabStripItem);

        
}
