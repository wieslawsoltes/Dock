using System;

namespace AvalonDock.Themes;

/// <summary>Selects the adapter's standard Dock and AvalonDock control resources.</summary>
public class GenericTheme : Theme
{
    internal const string ResourcePath = "avares://Dock.Avalonia.AvalonDock/Themes/Generic.axaml";

    /// <summary>Gets the URI of the compiled standard resources shipped in the adapter assembly.</summary>
    public override Uri GetResourceUri() => new(ResourcePath);
}
