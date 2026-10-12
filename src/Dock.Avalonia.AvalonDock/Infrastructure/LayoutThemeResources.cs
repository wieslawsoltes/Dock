using System;
using Avalonia.Controls;
using AvalonDock.Themes;

namespace AvalonDock.Infrastructure;

internal static class LayoutThemeResources
{
    internal static IResourceProvider? Create(Theme? theme, ThemeResourceRegistry registry, IResourceDictionary ownerResources)
    {
        if (theme is null) return null;
        ResourceDictionary resources;
        if (theme is DictionaryTheme dictionary)
            resources = dictionary.ThemeResourceDictionary ?? throw new ArgumentNullException("item");
        else
        {
            Uri uri = theme.GetResourceUri() ?? throw new ArgumentException("A URI theme must supply a resource URI.");
            if (uri.Equals(new Uri(GenericTheme.ResourcePath))) return new GenericThemeResources();
            resources = registry.Create(uri);
        }
        if (ReferenceEquals(resources, ownerResources))
            throw new InvalidOperationException("A theme dictionary cannot contain the resource scope that consumes it.");
        return LayoutThemeDictionary.CreateProvider(resources);
    }
}
