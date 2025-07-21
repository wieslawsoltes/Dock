// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.Controls.Diagnostics;

/// <summary>
/// Displays a document template for debugging purposes.
/// </summary>
public partial class DocumentTemplateDebugView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentTemplateDebugView"/> class.
    /// </summary>
    public DocumentTemplateDebugView()
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
