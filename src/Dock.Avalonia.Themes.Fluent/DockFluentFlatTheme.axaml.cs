// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Dock.Avalonia.Themes.Fluent;

/// <summary>
/// Fluent Dock theme variant that enables the flattened proportional dock presentation by default.
/// </summary>
public class DockFluentFlatTheme : Styles
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DockFluentFlatTheme"/> class.
    /// </summary>
    /// <param name="serviceProvider">The optional service provider.</param>
    public DockFluentFlatTheme(IServiceProvider? serviceProvider = null)
    {
        AvaloniaXamlLoader.Load(serviceProvider, this);
    }
}
