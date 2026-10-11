using System;
using System.Collections.Generic;

namespace AvalonDock.Layout.Serialization;

/// <summary>Resolves content identities while restoring an AvalonDock layout.</summary>
public abstract class LayoutSerializer
{
    private readonly List<LayoutAnchorable> _previousAnchorables = new();
    private readonly List<LayoutDocument> _previousDocuments = new();
    private int _deserializationDepth;

    /// <summary>Creates a serializer and captures the existing content identities.</summary>
    public LayoutSerializer(DockingManager manager)
    {
        Manager = manager ?? throw new ArgumentNullException(nameof(manager));
        foreach (ILayoutElement element in manager.Layout.Descendents())
        {
            if (element is LayoutAnchorable anchorable) _previousAnchorables.Add(anchorable);
            else if (element is LayoutDocument document) _previousDocuments.Add(document);
        }
    }
    /// <summary>Gets the manager whose layout is serialized.</summary>
    public DockingManager Manager { get; }
    /// <summary>Occurs for each restored content item.</summary>
    public event EventHandler<LayoutSerializationCallbackEventArgs>? LayoutSerializationCallback;

    /// <summary>Suspends source collection notifications while layout contents are restored.</summary>
    protected void StartDeserialization()
    {
        _deserializationDepth++;
        Manager.SuspendSourceBindings();
    }

    /// <summary>Resumes source collection notifications after restoration, including failed attempts.</summary>
    protected void EndDeserialization()
    {
        if (_deserializationDepth == 0) return;
        _deserializationDepth--;
        Manager.ResumeSourceBindings();
    }

    /// <summary>Resolves return containers and missing application content through callbacks or captured models.</summary>
    protected virtual void FixupLayout(LayoutRoot layout)
    {
        List<LayoutContent> contents = new();
        List<LayoutAnchorGroup> anchorGroups = new();
        Dictionary<string, LayoutElement> containers = new(StringComparer.Ordinal);
        foreach (ILayoutElement element in layout.Descendents())
        {
            if (element is LayoutAnchorGroup anchorGroup) anchorGroups.Add(anchorGroup);
            if (element is LayoutContent content) contents.Add(content);
            else if (element is LayoutElement node && !string.IsNullOrEmpty(node.DockModel.Id)) containers.TryAdd(node.DockModel.Id, node);
        }
        foreach (LayoutAnchorGroup group in anchorGroups)
        {
            if (group.PreviousContainerId is not { Length: > 0 } id) continue;
            if (!containers.TryGetValue(id, out LayoutElement? previous) || previous is not ILayoutContainer parent)
                throw new ArgumentException($"Unable to find a pane with id '{id}'.");
            group.PreviousContainer = parent;
            foreach (LayoutAnchorable tool in group.Children)
            {
                tool.PreviousContainer = parent;
                tool.DockModel.OriginalOwner = previous.DockModel;
            }
            if (previous.DockModel is Dock.Model.Avalonia.Controls.ToolDock pane && group.Parent is LayoutAnchorSide side)
                pane.Alignment = side.DockAlignment;
        }
        foreach (LayoutContent content in contents)
        {
            if (content.InitialContainerId is { Length: > 0 } initialId)
            {
                if (!containers.TryGetValue(initialId, out LayoutElement? initial) || initial is not ILayoutContainer initialParent)
                    throw new ArgumentException($"Unable to find a pane with id '{initialId}'.");
                content.InitialContainer = initialParent;
            }
            if (content.PreviousContainerId is { Length: > 0 } id)
            {
                if (!containers.TryGetValue(id, out LayoutElement? previous) || previous is not ILayoutContainer parent)
                    throw new ArgumentException($"Unable to find a pane with id '{id}'.");
                content.PreviousContainer = parent;
                // Dock uses OriginalOwner to determine the location of hidden
                // and pinned items. Visible contents may remember a floating
                // return pane without currently belonging to that window.
                if (content.Parent is LayoutRoot or LayoutAnchorGroup)
                    content.DockModel.OriginalOwner = previous.DockModel;
            }
        }

        // Each phase snapshots unresolved content before invoking application code.
        // A tool callback can populate a document before its later phase begins.
        List<LayoutAnchorable> tools = new();
        foreach (ILayoutElement element in layout.Descendents())
            if (element is LayoutAnchorable { Content: null } tool) tools.Add(tool);
        foreach (LayoutAnchorable tool in tools) RestoreContent(tool, FindPrevious(_previousAnchorables, tool.ContentId));
        List<LayoutDocument> documents = new();
        foreach (ILayoutElement element in layout.Descendents())
            if (element is LayoutDocument { Content: null } document) documents.Add(document);
        foreach (LayoutDocument document in documents) RestoreContent(document, FindPrevious(_previousDocuments, document.ContentId));
        layout.CollectGarbage();
    }

    private static T? FindPrevious<T>(List<T> previous, string? contentId) where T : LayoutContent
    {
        if (contentId is null) return null;
        // The snapshot retains models, not their keys: applications may change
        // an identifier after constructing the serializer and before restoring.
        foreach (T content in previous)
            if (string.Equals(content.ContentId, contentId, StringComparison.Ordinal)) return content;
        return null;
    }

    private void RestoreContent(LayoutContent content, LayoutContent? previous)
    {
        if (LayoutSerializationCallback is { } callback)
        {
            LayoutSerializationCallbackEventArgs args = new(content, previous?.Content);
            callback(this, args);
            if (args.Cancel) content.Close();
            else if (args.Content is not null) content.Content = args.Content;
            else if (content.Content is not null) RemoveUnresolved(content);
        }
        else if (previous is null) RemoveUnresolved(content);
        else
        {
            content.Content = previous.Content;
            content.IconSource = previous.IconSource;
        }
    }

    private static void RemoveUnresolved(LayoutContent content)
    {
        if (content is LayoutAnchorable tool) tool.Hide(false);
        else content.Close();
    }
}
