using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Dock.Avalonia.Themes;

namespace Dock.Avalonia.Themes.Browser;

/// <summary>
/// Browser-style Dock theme overlay that can be applied on top of <c>DockFluentTheme</c>.
/// </summary>
public class BrowserTabTheme : Styles, IResourceNode
{
    private readonly ResourceDictionary _compactStyles;
    private DockDensityStyle _densityStyle;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserTabTheme"/> class.
    /// </summary>
    /// <param name="serviceProvider">Optional XAML service provider used during loading.</param>
    public BrowserTabTheme(IServiceProvider? serviceProvider = null)
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
    /// Backing direct property for <see cref="DensityStyle"/>.
    /// </summary>
    public static readonly DirectProperty<BrowserTabTheme, DockDensityStyle> DensityStyleProperty =
        AvaloniaProperty.RegisterDirect<BrowserTabTheme, DockDensityStyle>(
            nameof(DensityStyle),
            o => o.DensityStyle,
            (o, v) => o.DensityStyle = v);

    /// <summary>
    /// Gets or sets the density mode used by the browser theme resources.
    /// </summary>
    public DockDensityStyle DensityStyle
    {
        get => _densityStyle;
        set => SetAndRaise(DensityStyleProperty, ref _densityStyle, value);
    }

    /// <summary>
    /// Notifies hosted resources when density changes so consumers can refresh theme resources.
    /// </summary>
    /// <param name="change">Property change information.</param>
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
