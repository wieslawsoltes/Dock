using System;
using System.Collections.Generic;

namespace Xceed.Wpf.AvalonDock.Layout.Serialization;

/// <summary>Resolves content identities while restoring an AvalonDock layout.</summary>
public abstract class LayoutSerializer
{
    private readonly Dictionary<string, LayoutContent> _previousContent = new(StringComparer.Ordinal);

    /// <summary>Creates a serializer and captures the existing content identities.</summary>
    public LayoutSerializer(DockingManager manager)
    {
        Manager = manager ?? throw new ArgumentNullException(nameof(manager));
        foreach (ILayoutElement element in manager.Layout.Descendents())
            if (element is LayoutContent content && !string.IsNullOrEmpty(content.ContentId))
                _previousContent.TryAdd(content.ContentId, content);
    }
    /// <summary>Gets the manager whose layout is serialized.</summary>
    public DockingManager Manager { get; }
    /// <summary>Occurs for each restored content item.</summary>
    public event EventHandler<LayoutSerializationCallbackEventArgs>? LayoutSerializationCallback;

    /// <summary>Resolves application content and removes canceled documents.</summary>
    protected virtual void FixupLayout(LayoutRoot layout)
    {
        List<LayoutContent> contents = new();
        Dictionary<string, LayoutElement> containers = new(StringComparer.Ordinal);
        foreach (ILayoutElement element in layout.Descendents())
        {
            if (element is LayoutContent content) contents.Add(content);
            else if (element is LayoutElement node && !string.IsNullOrEmpty(node.DockModel.Id)) containers.TryAdd(node.DockModel.Id, node);
        }
        foreach (LayoutContent content in contents)
        {
            if (content.PreviousContainerId is { Length: > 0 } id)
            {
                if (!containers.TryGetValue(id, out LayoutElement? previous) || previous is not ILayoutContainer parent)
                    throw new ArgumentException($"Unable to find a pane with id '{id}'.");
                content.PreviousContainer = parent;
                content.DockModel.OriginalOwner = previous.DockModel;
            }
            _previousContent.TryGetValue(content.ContentId, out LayoutContent? old);
            LayoutSerializationCallbackEventArgs args = new(content, old?.Content);
            LayoutSerializationCallback?.Invoke(this, args);
            if (args.Cancel)
            {
                content.Parent?.RemoveChild(content);
            }
            else if (args.Content is not null)
            {
                content.Content = args.Content;
                content.IconSource = old?.IconSource;
            }
            else if (content.Content is null)
            {
                if (content is LayoutAnchorable anchorable) anchorable.Hide(false);
                else content.Parent?.RemoveChild(content);
            }
        }
    }
}
