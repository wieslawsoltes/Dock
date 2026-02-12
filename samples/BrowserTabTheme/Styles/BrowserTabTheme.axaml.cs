using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Dock.Avalonia.Themes;

namespace BrowserTabTheme.Themes;

public class BrowserTabTheme : Styles, IResourceNode
{
    private readonly ResourceDictionary _compactStyles;
    private DockDensityStyle _densityStyle;

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

    public static readonly DirectProperty<BrowserTabTheme, DockDensityStyle> DensityStyleProperty =
        AvaloniaProperty.RegisterDirect<BrowserTabTheme, DockDensityStyle>(
            nameof(DensityStyle),
            o => o.DensityStyle,
            (o, v) => o.DensityStyle = v);

    public DockDensityStyle DensityStyle
    {
        get => _densityStyle;
        set => SetAndRaise(DensityStyleProperty, ref _densityStyle, value);
    }

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
