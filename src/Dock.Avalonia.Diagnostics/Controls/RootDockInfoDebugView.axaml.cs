// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.Diagnostics.Controls;

/// <summary>
/// Displays information about the root dock.
/// </summary>
public partial class RootDockInfoDebugView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RootDockInfoDebugView"/> class.
    /// </summary>
    public RootDockInfoDebugView()
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
