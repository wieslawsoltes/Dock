using System;
using System.Collections.Generic;
using AvalonDock.Layout;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

internal enum LayoutOperation { Silent, Dock, Float }

internal interface ILayoutOperationEvents
{
    IDockOperationScope? Begin(IDockable source, LayoutOperation operation);
}

// One notification pair covers a native operation and all its internal moves.
// Track contents rather than a global depth so handlers can operate another item.
internal sealed class LayoutOperationEvents(LayoutRoot root) : ILayoutOperationEvents
{
    private readonly HashSet<LayoutContent> _active = new();

    public IDockOperationScope? Begin(IDockable source, LayoutOperation operation)
    {
        if (root.Manager is not { } manager) return null;
        List<LayoutContent> contents = new();
        LayoutElement element = root.Context.Get(source);
        if (element is LayoutContent content) contents.Add(content);
        else if (element is ILayoutContainer container)
            foreach (ILayoutElement child in container.Descendents())
                if (child is LayoutContent item) contents.Add(item);
        if (contents.Count == 0) return null;
        foreach (LayoutContent item in contents) if (_active.Contains(item)) return null;

        LayoutContent selected = contents[0];
        if (operation == LayoutOperation.Float)
        {
            foreach (LayoutContent item in contents)
                if (item.IsSelected) { selected = item; break; }
            if (root.ActiveContent is { } active && contents.Contains(active)) selected = active;
        }
        foreach (LayoutContent item in contents) _active.Add(item);
        Scope scope = new(this, manager, contents, selected, operation);
        try
        {
            if (operation != LayoutOperation.Silent)
                manager.NotifyLayoutOperation(selected, operation == LayoutOperation.Float, preview: true);
            return scope;
        }
        catch
        {
            scope.Dispose();
            throw;
        }
    }

    private sealed class Scope : IDockOperationScope
    {
        private LayoutOperationEvents? _owner;
        private DockingManager? _manager;
        private List<LayoutContent>? _contents;
        private LayoutContent? _content;
        private LayoutFloatingWindow? _previousWindow;
        private readonly LayoutOperation _operation;
        private bool _completed;

        internal Scope(LayoutOperationEvents owner, DockingManager manager, List<LayoutContent> contents,
            LayoutContent content, LayoutOperation operation)
        {
            _owner = owner;
            _manager = manager;
            _contents = contents;
            _content = content;
            _operation = operation;
            _previousWindow = content.FindParent<LayoutFloatingWindow>();
        }

        public void Complete()
        {
            if (_completed || _owner is null || _manager is null || _content is null ||
                _operation == LayoutOperation.Silent) return;
            _completed = true;
            if (!ReferenceEquals(_owner.Root.Manager, _manager)) return;
            if (_operation == LayoutOperation.Float &&
                (_content.FindParent<LayoutFloatingWindow>() is not { } window || ReferenceEquals(window, _previousWindow))) return;
            _manager.NotifyLayoutOperation(_content, _operation == LayoutOperation.Float, preview: false);
        }

        public void Dispose()
        {
            if (_owner is not null && _contents is not null)
                foreach (LayoutContent item in _contents) _owner._active.Remove(item);
            _owner = null;
            _manager = null;
            _contents = null;
            _content = null;
            _previousWindow = null;
        }
    }

    private LayoutRoot Root => root;
}
