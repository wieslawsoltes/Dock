using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;

namespace DockAvalonDockSample.Services;

internal sealed class AvalonDockWorkspace : ILayoutWorkspace
{
    private readonly DockingManager _manager;
    private readonly string _path;
    private readonly Dictionary<string, object?> _content = new(StringComparer.Ordinal);

    public AvalonDockWorkspace(DockingManager manager, string path)
    {
        _manager = manager;
        _path = path;
        foreach (ILayoutElement element in manager.Layout.Descendents())
            if (element is LayoutContent content) _content[content.ContentId] = content.Content;
    }

    public string Save() => Run(() =>
    {
        string? directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        // Replace the previous file only after the complete UTF-8 XML has been written.
        string temporary = _path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            new XmlLayoutSerializer(_manager).Serialize(temporary);
            File.Move(temporary, _path, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
        return $"Saved layout to {_path}";
    });

    public string Load() => Run(() =>
    {
        if (!File.Exists(_path)) return "Save a layout before loading it.";
        XmlLayoutSerializer serializer = new(_manager);
        serializer.LayoutSerializationCallback += (_, args) =>
        {
            if (_content.TryGetValue(args.Model.ContentId, out object? content)) args.Content = content;
        };
        serializer.Deserialize(_path);
        return "Layout restored. Document content and editor text were preserved.";
    });

    public string HideJournal()
    {
        FindJournal()?.Hide();
        return "Journal hidden. Use Show Journal to restore it.";
    }

    public string ShowJournal()
    {
        LayoutAnchorable? journal = FindJournal();
        journal?.Show();
        if (journal is not null) journal.IsActive = true;
        return "Journal restored.";
    }

    private LayoutAnchorable? FindJournal()
    {
        foreach (ILayoutElement element in _manager.Layout.Descendents())
            if (element is LayoutAnchorable { ContentId: "journal" } journal) return journal;
        return null;
    }

    private static string Run(Func<string> action)
    {
        try { return action(); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or XmlException or NotSupportedException)
        {
            return error.Message;
        }
    }
}
