using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

// Avalonia has no Hidden visibility state. Suppress rendering and input with
// disposable value layers, preserving measurement and all application bindings.
internal sealed class LayoutTabOverflow(DocumentPaneTabPanel panel)
{
    private readonly Dictionary<Control, Entry> _entries = new();
    private readonly List<Control> _removed = new();
    private bool _childrenChanged = true;
    private bool _refreshing;
    private bool _clearing;
    private int _generation;

    internal void ChildrenChanged()
    {
        _childrenChanged = true;
        Refresh();
    }

    internal void Refresh()
    {
        if (_refreshing || _clearing) return;
        _refreshing = true;
        int generation = _generation;
        try
        {
            do
            {
                if (_childrenChanged)
                {
                    _childrenChanged = false;
                    foreach (Control child in _entries.Keys)
                        if (!panel.Children.Contains(child)) _removed.Add(child);
                    for (int i = 0; i < _removed.Count; i++)
                    {
                        if (_entries.Remove(_removed[i], out Entry? removed)) removed.Dispose();
                        if (generation != _generation) return;
                    }
                    _removed.Clear();
                }
                foreach (Control child in panel.Children)
                {
                    if (!_entries.TryGetValue(child, out Entry? entry))
                    {
                        entry = new Entry(child, panel);
                        _entries.Add(child, entry);
                    }
                    entry.RefreshModel();
                }
            }
            while (_childrenChanged);
        }
        finally { _refreshing = false; }
    }

    internal bool BringSelectedTabForward()
    {
        foreach (Control child in panel.Children)
        {
            Entry entry = _entries[child];
            if (!child.IsVisible || !entry.Hidden || entry.Model is not { IsSelected: true, Parent: ILayoutPane pane } model ||
                pane is not ILayoutContentSelector selector || pane.ChildrenCount < 2) continue;
            int index = selector.IndexOf(model);
            if (index <= 0) continue;
            pane.MoveChild(index, 0);
            selector.SelectedContentIndex = 0;
            // Native and application ItemsControls update their child order
            // through the same observable pane collection. Re-enter after layout.
            panel.InvalidateMeasure();
            return true;
        }
        return false;
    }

    internal void SetHidden(Control child, bool hidden) => _entries[child].SetHidden(hidden);

    internal void Clear()
    {
        if (_clearing) return;
        _clearing = true;
        _generation++;
        try
        {
            Entry[] entries = new Entry[_entries.Count];
            _entries.Values.CopyTo(entries, 0);
            _entries.Clear();
            _removed.Clear();
            _childrenChanged = true;
            foreach (Entry entry in entries) entry.Dispose();
        }
        finally { _clearing = false; }
    }

    private sealed class Entry : IDisposable
    {
        private readonly Control _child;
        private readonly DocumentPaneTabPanel _panel;
        private ModelSubscription? _subscription;
        private IDisposable? _opacity;
        private IDisposable? _input;
        private IDisposable? _enabled;
        private int _generation;
        private bool _disposed;
        internal LayoutContent? Model { get; private set; }
        internal bool Hidden { get; private set; }

        internal Entry(Control child, DocumentPaneTabPanel panel)
        {
            _child = child;
            _panel = panel;
            child.PropertyChanged += ChildChanged;
        }

        internal void RefreshModel()
        {
            LayoutContent? model = _child switch
            {
                LayoutDocumentTabItem tab => tab.Model,
                LayoutAnchorableTabItem tab => tab.Model,
                TabItem { Content: LayoutContent content } => content,
                { DataContext: LayoutContent content } => content,
                { DataContext: IDockable { Factory: LayoutFactory factory } native } => factory.GetLayoutContent(native),
                _ => null
            };
            if (ReferenceEquals(model, Model)) return;
            _subscription?.Dispose();
            Model = model;
            _subscription = model is null ? null : new ModelSubscription(model, _panel);
            _panel.InvalidateArrange();
        }

        private void ChildChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
        {
            if (args.Property == StyledElement.DataContextProperty || args.Property == ContentControl.ContentProperty ||
                args.Property == LayoutDocumentTabItem.ModelProperty || args.Property == LayoutAnchorableTabItem.ModelProperty) RefreshModel();
        }

        internal void SetHidden(bool hidden)
        {
            if (_disposed || Hidden == hidden) return;
            Hidden = hidden;
            int generation = ++_generation;
            if (hidden)
            {
                IDisposable? opacity = _child.SetValue(Visual.OpacityProperty, 0d, BindingPriority.Animation);
                if (generation != _generation) { opacity?.Dispose(); return; }
                _opacity = opacity;
                IDisposable? input = _child.SetValue(InputElement.IsHitTestVisibleProperty, false, BindingPriority.Animation);
                if (generation != _generation) { input?.Dispose(); return; }
                _input = input;
                IDisposable? enabled = _child.SetValue(InputElement.IsEnabledProperty, false, BindingPriority.Animation);
                if (generation != _generation) { enabled?.Dispose(); return; }
                _enabled = enabled;
                if (Model is { IsSelected: true, Parent: ILayoutContentSelector selector } && selector.IndexOf(Model) > 0)
                    _panel.InvalidateArrange();
            }
            else
            {
                ReleaseValues();
            }
        }

        private void ReleaseValues()
        {
            IDisposable? opacity = _opacity, input = _input, enabled = _enabled;
            _opacity = _input = _enabled = null;
            opacity?.Dispose(); input?.Dispose(); enabled?.Dispose();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _generation++;
            Hidden = false;
            _child.PropertyChanged -= ChildChanged;
            _subscription?.Dispose();
            _subscription = null;
            Model = null;
            ReleaseValues();
        }
    }

    private sealed class ModelSubscription : IDisposable
    {
        private readonly LayoutContent _model;
        private readonly WeakReference<DocumentPaneTabPanel> _panel;

        internal ModelSubscription(LayoutContent model, DocumentPaneTabPanel panel)
        {
            _model = model;
            _panel = new(panel);
            model.PropertyChanged += Changed;
        }

        private void Changed(object? sender, PropertyChangedEventArgs args)
        {
            if (!_panel.TryGetTarget(out DocumentPaneTabPanel? panel)) { Dispose(); return; }
            if (args.PropertyName is nameof(LayoutContent.IsSelected) or nameof(LayoutContent.Parent)) panel.InvalidateArrange();
        }

        public void Dispose() => _model.PropertyChanged -= Changed;
    }
}
