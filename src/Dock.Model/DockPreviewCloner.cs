using System.Collections.Generic;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

internal sealed class DockPreviewCloner
{
    private readonly Dictionary<IFactory, IFactory> _factories = new(ReferenceEqualityComparer.Instance);

    internal Dictionary<IDockable, IDockable> Copies { get; } = new();

    internal IDockable? Copy(IDockable original)
    {
        if (Copies.TryGetValue(original, out var existing)) return existing;
        if (original.Factory is not { } liveFactory || liveFactory is not IDockPreviewFactoryProvider provider)
            return null;
        if (!_factories.TryGetValue(liveFactory, out var factory))
        {
            factory = provider.CreatePreviewFactory();
            if (factory is null || ReferenceEquals(factory, liveFactory)) return null;
            _factories.Add(liveFactory, factory);
        }
        IDockable? copy = original switch
        {
            IRootDock => factory.CreateRootDock(),
            IProportionalDock => factory.CreateProportionalDock(),
            IToolDock => factory.CreateToolDock(),
            IDocumentDock => factory.CreateDocumentDock(),
            IWrapDock => factory.CreateWrapDock(),
            IProportionalDockSplitter => factory.CreateProportionalDockSplitter(),
            ITool => factory.CreateTool(),
            IDocument => factory.CreateDocument(),
            _ => null
        };
        if (copy is null) return null;
        Copies.Add(original, copy);
        copy.Id = original.Id;
        copy.Title = original.Title;
        copy.Factory = factory;
        copy.DockGroup = original.DockGroup;
        copy.IsCollapsable = original.IsCollapsable;
        copy.IsEmpty = original.IsEmpty;
        copy.Proportion = original.Proportion;
        copy.CollapsedProportion = original.CollapsedProportion;
        copy.MinWidth = original.MinWidth;
        copy.MaxWidth = original.MaxWidth;
        copy.MinHeight = original.MinHeight;
        copy.MaxHeight = original.MaxHeight;
        copy.CanDrag = original.CanDrag;
        copy.CanDrop = original.CanDrop;
        copy.CanDockAsDocument = original.CanDockAsDocument;
        if (original is IDockableDockingRestrictions restrictions && copy is IDockableDockingRestrictions copiedRestrictions)
        {
            copiedRestrictions.AllowedDockOperations = restrictions.AllowedDockOperations;
            copiedRestrictions.AllowedDropOperations = restrictions.AllowedDropOperations;
        }
        if (original is IProportionalDock row && copy is IProportionalDock copiedRow) copiedRow.Orientation = row.Orientation;
        if (original is IRootDock root && copy is IRootDock copiedRoot) copiedRoot.EnableGlobalDocking = root.EnableGlobalDocking;
        if (original is IToolDock toolDock && copy is IToolDock copiedToolDock)
        {
            copiedToolDock.Alignment = toolDock.Alignment;
            copiedToolDock.IsExpanded = toolDock.IsExpanded;
        }
        if (original is IDock dock && copy is IDock copiedDock)
        {
            copiedDock.VisibleDockables = factory.CreateList<IDockable>();
            if (dock.VisibleDockables is { } children)
            {
                foreach (var child in children)
                {
                    var childCopy = Copy(child);
                    if (childCopy is null) return null;
                    childCopy.Owner = copiedDock;
                    copiedDock.VisibleDockables.Add(childCopy);
                }
            }
            if (dock.ActiveDockable is { } active && Copies.TryGetValue(active, out var activeCopy)) copiedDock.ActiveDockable = activeCopy;
            if (dock.DefaultDockable is { } defaultDockable && Copies.TryGetValue(defaultDockable, out var defaultCopy)) copiedDock.DefaultDockable = defaultCopy;
        }
        return copy;
    }
}
