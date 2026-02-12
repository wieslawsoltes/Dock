// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

namespace Dock.Avalonia.Themes;

/// <summary>
/// Provides shared logic for switching Dock theme presets stored as merged resource includes.
/// </summary>
public abstract class DockPresetThemeManagerBase : IDockThemeManager
{
    private readonly IReadOnlyList<string> _presetNames;
    private readonly IReadOnlyList<Uri> _presetUris;
    private readonly string _presetUriPrefix;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockPresetThemeManagerBase"/> class.
    /// </summary>
    /// <param name="presetNames">The display names for presets.</param>
    /// <param name="presetUris">The preset resource URIs.</param>
    /// <param name="presetUriPrefix">The URI prefix used to identify preset includes.</param>
    protected DockPresetThemeManagerBase(
        IReadOnlyList<string> presetNames,
        IReadOnlyList<Uri> presetUris,
        string presetUriPrefix)
    {
        _presetNames = presetNames ?? throw new ArgumentNullException(nameof(presetNames));
        _presetUris = presetUris ?? throw new ArgumentNullException(nameof(presetUris));
        _presetUriPrefix = !string.IsNullOrWhiteSpace(presetUriPrefix)
            ? presetUriPrefix
            : throw new ArgumentException("Preset URI prefix cannot be empty.", nameof(presetUriPrefix));

        if (_presetNames.Count == 0)
        {
            throw new ArgumentException("At least one preset name is required.", nameof(presetNames));
        }

        if (_presetUris.Count == 0)
        {
            throw new ArgumentException("At least one preset URI is required.", nameof(presetUris));
        }

        if (_presetNames.Count != _presetUris.Count)
        {
            throw new ArgumentException("Preset names and preset URIs must have the same count.");
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<string> PresetNames => _presetNames;

    /// <inheritdoc />
    public int CurrentPresetIndex => GetCurrentPresetIndex();

    /// <inheritdoc />
    public void Switch(int index)
    {
        if (Application.Current is null)
        {
            return;
        }

        Application.Current.RequestedThemeVariant = index switch
        {
            0 => ThemeVariant.Light,
            1 => ThemeVariant.Dark,
            _ => Application.Current.RequestedThemeVariant
        };
    }

    /// <inheritdoc />
    public void SwitchPreset(int index)
    {
        if (index < 0 || index >= _presetUris.Count)
        {
            return;
        }

        if (Application.Current is not { } application)
        {
            return;
        }

        var selectedPresetUri = _presetUris[index];

        if (TryFindPresetUri(application, out var currentPresetUri) &&
            currentPresetUri is not null &&
            AreEqualUris(currentPresetUri, selectedPresetUri))
        {
            return;
        }

        foreach (var rootDictionary in EnumerateRootDictionaries(application))
        {
            RemovePresetIncludes(rootDictionary);
        }

        application.Resources.MergedDictionaries.Add(CreatePresetInclude(selectedPresetUri));
        if (TryGetDefaultPresetOwner(application, out var defaultOwner) &&
            defaultOwner is not null &&
            !ReferenceEquals(defaultOwner, application.Resources))
        {
            defaultOwner.MergedDictionaries.Add(CreatePresetInclude(selectedPresetUri));
        }
    }

    private int GetCurrentPresetIndex()
    {
        if (Application.Current is not { } application)
        {
            return 0;
        }

        if (!TryFindPresetUri(application, out var currentSource) || currentSource is null)
        {
            return 0;
        }

        for (var i = 0; i < _presetUris.Count; i++)
        {
            if (AreEqualUris(currentSource, _presetUris[i]))
            {
                return i;
            }
        }

        return 0;
    }

    /// <summary>
    /// Attempts to resolve the default resource dictionary that should host preset includes.
    /// </summary>
    /// <param name="application">The current Avalonia application.</param>
    /// <param name="owner">When this method returns, contains the default owner dictionary if found.</param>
    /// <returns><c>true</c> if a default owner dictionary was found; otherwise, <c>false</c>.</returns>
    protected abstract bool TryGetDefaultPresetOwner(Application application, out IResourceDictionary? owner);

    private bool TryFindPresetUri(Application application, out Uri? presetUri)
    {
        foreach (var rootDictionary in EnumerateRootDictionaries(application))
        {
            if (TryFindPresetUri(rootDictionary, out presetUri))
            {
                return true;
            }
        }

        presetUri = null;
        return false;
    }

    private static IEnumerable<IResourceDictionary> EnumerateRootDictionaries(Application application)
    {
        yield return application.Resources;

        foreach (var style in application.Styles)
        {
            foreach (var styleResources in EnumerateStyleResourceDictionaries(style))
            {
                yield return styleResources;
            }
        }
    }

    private static IEnumerable<IResourceDictionary> EnumerateStyleResourceDictionaries(IStyle style)
    {
        if (style is Styles styles)
        {
            if (styles.Resources is { } resources)
            {
                yield return resources;
            }

            foreach (var childStyle in styles)
            {
                foreach (var nestedResources in EnumerateStyleResourceDictionaries(childStyle))
                {
                    yield return nestedResources;
                }
            }
        }
    }

    private bool TryFindPresetUri(IResourceDictionary dictionary, out Uri? presetUri)
    {
        foreach (var mergedDictionary in dictionary.MergedDictionaries)
        {
            if (mergedDictionary is ResourceInclude resourceInclude &&
                resourceInclude.Source is { } source &&
                IsPresetUri(source))
            {
                presetUri = source;
                return true;
            }

            if (mergedDictionary is IResourceDictionary mergedResourceDictionary &&
                TryFindPresetUri(mergedResourceDictionary, out presetUri))
            {
                return true;
            }
        }

        presetUri = null;
        return false;
    }

    private void RemovePresetIncludes(IResourceDictionary dictionary)
    {
        for (var i = dictionary.MergedDictionaries.Count - 1; i >= 0; i--)
        {
            var mergedDictionary = dictionary.MergedDictionaries[i];

            if (mergedDictionary is ResourceInclude include &&
                include.Source is { } source &&
                IsPresetUri(source))
            {
                dictionary.MergedDictionaries.RemoveAt(i);
                continue;
            }

            if (mergedDictionary is IResourceDictionary mergedResourceDictionary)
            {
                RemovePresetIncludes(mergedResourceDictionary);
            }
        }
    }

    private bool IsPresetUri(Uri source)
    {
        return source.ToString().StartsWith(_presetUriPrefix, StringComparison.OrdinalIgnoreCase);
    }

    private static bool AreEqualUris(Uri left, Uri right)
    {
        return string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static ResourceInclude CreatePresetInclude(Uri presetUri)
    {
#pragma warning disable IL2026
        return new ResourceInclude(presetUri)
        {
            Source = presetUri
        };
#pragma warning restore IL2026
    }
}
