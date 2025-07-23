// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Diagnostics.Controls;

/// <summary>
/// Displays debug information for a <see cref="DockControl"/>.
/// </summary>
public partial class DockDebugView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DockDebugView"/> class.
    /// </summary>
    public DockDebugView()
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
