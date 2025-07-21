// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.Controls.Diagnostics;

/// <summary>
/// Debug view showing splitter positions in proportional docks.
/// </summary>
public partial class ProportionalDockSplitterDebugView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProportionalDockSplitterDebugView"/> class.
    /// </summary>
    public ProportionalDockSplitterDebugView()
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
