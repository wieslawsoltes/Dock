using System;
using System.IO;
using System.Text.Json;
using Dock.Model.Core;
using Dock.Serializer.Yaml;

namespace Dock.Model;

/// <summary>
/// Helper class that loads a layout configuration and builds a Dock model.
/// </summary>
public static class LayoutGenerator
{
    /// <summary>
    /// Loads a layout from the specified file.
    /// </summary>
    /// <param name="path">Path to a JSON or YAML configuration file.</param>
    /// <returns>The generated <see cref="IRootDock"/> or <c>null</c> if parsing fails.</returns>
    public static IRootDock? Load(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var text = File.ReadAllText(path);
        LayoutConfig? config = null;
        if (path.EndsWith(".yml", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase))
        {
            var serializer = new DockYamlSerializer();
            config = serializer.Deserialize<LayoutConfig>(text);
        }
        else
        {
            config = JsonSerializer.Deserialize<LayoutConfig>(text);
        }

        if (config is null)
        {
            return null;
        }

        var builder = new DockLayoutBuilder();
        return builder.Build(config);
    }
}

internal class LayoutConfig
{
    public string Orientation { get; set; } = "Horizontal";
    public DocumentConfig[] Documents { get; set; } = Array.Empty<DocumentConfig>();
    public ToolConfig[] Tools { get; set; } = Array.Empty<ToolConfig>();
}

internal class DocumentConfig
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

internal class ToolConfig
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Alignment { get; set; } = "Left";
    public double Proportion { get; set; } = 0.25;
}
