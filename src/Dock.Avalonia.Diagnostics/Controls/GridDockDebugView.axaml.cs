// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Diagnostics.Controls;

/// <summary>
/// Debug view for <see cref="GridDockControl"/> instances.
/// </summary>
public partial class GridDockDebugView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GridDockDebugView"/> class.
    /// </summary>
    public GridDockDebugView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Loads the control's XAML.
    /// </summary>
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
