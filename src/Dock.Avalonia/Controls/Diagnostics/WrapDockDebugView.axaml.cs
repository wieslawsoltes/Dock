// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.Controls.Diagnostics;

/// <summary>
/// Debug view for wrap dock layouts.
/// </summary>
public partial class WrapDockDebugView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WrapDockDebugView"/> class.
    /// </summary>
    public WrapDockDebugView()
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
