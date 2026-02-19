// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes;

namespace Dock.Avalonia.Themes.Simple;

/// <summary>
/// The Dock simple theme.
/// </summary>
public class DockSimpleTheme : Styles, IResourceNode
{
    private readonly ResourceDictionary _compactStyles;
    private DockDensityStyle _densityStyle;
    private bool _cacheDocumentTabContent;
    private Style? _cachedDocumentTemplateOverrideStyle;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockSimpleTheme"/> class.
    /// </summary>
    /// <param name="serviceProvider">The optional service provider.</param>
    public DockSimpleTheme(IServiceProvider? serviceProvider = null)
    {
        AvaloniaXamlLoader.Load(serviceProvider, this);
        _compactStyles = (ResourceDictionary)GetAndRemove("CompactStyles");
        UpdateDocumentTemplateOverride();

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
    /// Identifies the <see cref="CacheDocumentTabContent"/> direct property.
    /// </summary>
    public static readonly DirectProperty<DockSimpleTheme, bool> CacheDocumentTabContentProperty =
        AvaloniaProperty.RegisterDirect<DockSimpleTheme, bool>(
            nameof(CacheDocumentTabContent),
            o => o.CacheDocumentTabContent,
            (o, v) => o.CacheDocumentTabContent = v);

    /// <summary>
    /// Gets or sets the density style used by this theme.
    /// </summary>
    public DockDensityStyle DensityStyle
    {
        get => _densityStyle;
        set => SetAndRaise(DensityStyleProperty, ref _densityStyle, value);
    }

    /// <summary>
    /// Gets or sets whether document tabs should keep content views alive between tab switches.
    /// </summary>
    public bool CacheDocumentTabContent
    {
        get => _cacheDocumentTabContent;
        set => SetAndRaise(CacheDocumentTabContentProperty, ref _cacheDocumentTabContent, value);
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

        if (change.Property == CacheDocumentTabContentProperty)
        {
            UpdateDocumentTemplateOverride();
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

    private void UpdateDocumentTemplateOverride()
    {
        if (!_cacheDocumentTabContent)
        {
            if (_cachedDocumentTemplateOverrideStyle is not null)
            {
                Remove(_cachedDocumentTemplateOverrideStyle);
                _cachedDocumentTemplateOverrideStyle = null;
            }

            return;
        }

        if (!base.TryGetResource("DockDocumentControlCachedContentTemplate", null, out var templateObject)
            || templateObject is not IControlTemplate template)
        {
            return;
        }

        if (_cachedDocumentTemplateOverrideStyle is null)
        {
            _cachedDocumentTemplateOverrideStyle = new Style(x => x.OfType<DocumentControl>());
            _cachedDocumentTemplateOverrideStyle.Setters.Add(new Setter(TemplatedControl.TemplateProperty, template));
            Add(_cachedDocumentTemplateOverrideStyle);
            return;
        }

        _cachedDocumentTemplateOverrideStyle.Setters.Clear();
        _cachedDocumentTemplateOverrideStyle.Setters.Add(new Setter(TemplatedControl.TemplateProperty, template));
    }
}
