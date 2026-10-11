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
        return QualifiedBclType().Replace(key, static match => match.Groups["type"].Value);
    }

    // Canonicalize only these known BCL types' imported and fully qualified spellings.
    // Do not erase arbitrary namespaces or map WPF framework types to Avalonia.
    [GeneratedRegex(@"(?<![\w.:])(?:global::)?System\.(?:Globalization\.(?<type>CultureInfo)|ComponentModel\.(?<type>CancelEventArgs)|Xml\.(?<type>XmlReader|XmlWriter)|IO\.(?<type>Stream|TextReader|TextWriter))(?![\w.:])", RegexOptions.CultureInvariant)]
    private static partial Regex QualifiedBclType();
}
