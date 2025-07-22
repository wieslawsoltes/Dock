// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.Controls.Diagnostics;

/// <summary>
/// Shows the contents of a document dock for debugging.
/// </summary>
public partial class DocumentDockContentDebugView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentDockContentDebugView"/> class.
    /// </summary>
    public DocumentDockContentDebugView()
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
