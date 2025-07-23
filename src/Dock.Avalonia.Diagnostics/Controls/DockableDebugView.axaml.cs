// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.Diagnostics.Controls;

/// <summary>
/// Shows debug information common to all dockables.
/// </summary>
public partial class DockableDebugView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DockableDebugView"/> class.
    /// </summary>
    public DockableDebugView()
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
