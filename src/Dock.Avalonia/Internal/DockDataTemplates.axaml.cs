// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.Internal;

internal sealed partial class DockDataTemplates : DataTemplates
{
    public DockDataTemplates()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
