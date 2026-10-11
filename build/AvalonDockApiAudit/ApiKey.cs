using System;
using System.Text.RegularExpressions;

namespace AvalonDockApiAudit;

internal static partial class ApiKey
{
    internal static string Normalize(string declaration)
    {
        int separator = declaration.IndexOf('\t');
        string key = separator < 0 ? declaration : declaration[..separator];
        key = key.Replace("Xceed.Wpf.AvalonDock.", "AvalonDock.", StringComparison.Ordinal);
        // Removing a nullable element annotation from Roslyn's "object? []"
        // leaves a formatting-only space before the array rank specifier.
        key = key.Replace(" [", "[", StringComparison.Ordinal);
        return QualifiedCultureInfo().Replace(key, "CultureInfo");
    }

    // Canonicalize this known BCL type's imported and fully qualified spellings.
    // Do not erase arbitrary namespaces or map WPF framework types to Avalonia.
    [GeneratedRegex(@"(?<![\w.:])(?:global::)?System\.Globalization\.CultureInfo(?![\w.:])", RegexOptions.CultureInvariant)]
    private static partial Regex QualifiedCultureInfo();
}
