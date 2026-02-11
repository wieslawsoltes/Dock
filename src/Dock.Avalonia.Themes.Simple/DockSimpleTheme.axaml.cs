// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Dock.Avalonia.Themes;

namespace Dock.Avalonia.Themes.Simple;

/// <summary>
/// The Dock simple theme.
/// </summary>
public class DockSimpleTheme : Styles, IResourceNode
{
    private readonly ResourceDictionary _compactStyles;
    private DockDensityStyle _densityStyle;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockSimpleTheme"/> class.
    /// </summary>
    /// <param name="serviceProvider">The optional service provider.</param>
    public DockSimpleTheme(IServiceProvider? serviceProvider = null)
    {
        AvaloniaXamlLoader.Load(serviceProvider, this);
        _compactStyles = (ResourceDictionary)GetAndRemove("CompactStyles");

        object GetAndRemove(string key)
        {
            var value = Resources[key] ?? throw new KeyNotFoundException($"Key '{key}' was not found in resources.");
            Resources.Remove(key);
            return value;
        }
    }

    /// <summary>
    /// Identifies the <see cref="DensityStyle"/> direct property.
    /// </summary>
    public static readonly DirectProperty<DockSimpleTheme, DockDensityStyle> DensityStyleProperty =
        AvaloniaProperty.RegisterDirect<DockSimpleTheme, DockDensityStyle>(
            nameof(DensityStyle),
            o => o.DensityStyle,
            (o, v) => o.DensityStyle = v);

    /// <summary>
    /// Gets or sets the density style used by this theme.
    /// </summary>
    public DockDensityStyle DensityStyle
    {
        get => _densityStyle;
        set => SetAndRaise(DensityStyleProperty, ref _densityStyle, value);
    }

    /// <summary>
    /// Handles resource invalidation when theme properties change.
    /// </summary>
    /// <param name="change">The property change payload.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DensityStyleProperty)
        {
            Owner?.NotifyHostedResourcesChanged(new ResourcesChangedEventArgs());
        }
    }

    bool IResourceNode.TryGetResource(object key, ThemeVariant? theme, out object? value)
    {
        if (_densityStyle == DockDensityStyle.Compact && _compactStyles.TryGetResource(key, theme, out value))
        {
            return true;
        }

        return base.TryGetResource(key, theme, out value);
    }
}
