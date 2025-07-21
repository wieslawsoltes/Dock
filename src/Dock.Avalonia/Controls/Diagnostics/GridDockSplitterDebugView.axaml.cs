// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.Controls.Diagnostics;

/// <summary>
/// Debug view displaying splitter information for a grid dock.
/// </summary>
public partial class GridDockSplitterDebugView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GridDockSplitterDebugView"/> class.
    /// </summary>
    public GridDockSplitterDebugView()
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
