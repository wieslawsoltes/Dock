// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.Controls.Diagnostics;

/// <summary>
/// Displays debug information for an <see cref="IDockWindow"/>.
/// </summary>
public partial class DockWindowDebugView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DockWindowDebugView"/> class.
    /// </summary>
    public DockWindowDebugView()
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
