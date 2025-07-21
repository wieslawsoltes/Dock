// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.Controls.Diagnostics;

/// <summary>
/// Debug view that displays information about a document's content.
/// </summary>
public partial class DocumentContentDebugView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentContentDebugView"/> class.
    /// </summary>
    public DocumentContentDebugView()
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
