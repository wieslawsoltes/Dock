using System;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Manager assignments can identify content using a payload or a detached model.
internal static class LayoutActiveContentResolver
{
    internal static LayoutContent? Find(LayoutRoot root, object? value)
    {
        LayoutContent? supplied = value as LayoutContent;
        object? payload = supplied?.Content;
        string? identifier = supplied?.ContentId;
        foreach (ILayoutElement element in root.Descendents())
        {
            if (element is not LayoutContent candidate) continue;
            if (supplied is null)
            {
                if (ReferenceEquals(candidate, value) || ReferenceEquals(candidate.Content, value)) return candidate;
            }
            else if (payload is not null)
            {
                if (ReferenceEquals(candidate, payload) || ReferenceEquals(candidate.Content, payload)) return candidate;
            }
            else if (identifier is not null)
            {
                if (string.Equals(candidate.ContentId, identifier, StringComparison.Ordinal)) return candidate;
            }
            else if (candidate.Content is null) return candidate;
        }
        return null;
    }
}
