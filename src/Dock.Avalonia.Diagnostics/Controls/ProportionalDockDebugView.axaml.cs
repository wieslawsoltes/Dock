// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Diagnostics.Controls;

/// <summary>
/// Debug view for a <see cref="ProportionalDockControl"/>.
/// </summary>
public partial class ProportionalDockDebugView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProportionalDockDebugView"/> class.
    /// </summary>
    public ProportionalDockDebugView()
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
