using System;
using Avalonia.Controls;
using AvalonDock.Themes;

namespace AvalonDock.Infrastructure;

internal static class LayoutThemeResources
{
    internal static IResourceProvider? Create(Theme? theme)
    {
        if (theme is null) return null;
        if (theme is DictionaryTheme dictionary)
            return dictionary.ThemeResourceDictionary is { } resources
                ? LayoutThemeDictionary.CreateProvider(resources) : throw new ArgumentNullException("item");
        Uri uri = theme.GetResourceUri() ?? throw new ArgumentException("A URI theme must supply a resource URI.");
        if (uri.Equals(new Uri(GenericTheme.ResourcePath))) return new GenericThemeResources();
        throw new NotSupportedException("Loading custom theme resource URIs is not implemented.");
    }
}
