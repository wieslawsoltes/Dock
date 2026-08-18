// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Dock.Model.Controls;
using Dock.Model.Core;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace BrowserTabTheme;

/// <summary>
/// Coordinates the browser sample layout and its main-window chrome state.
/// </summary>
public sealed partial class MainWindowViewModel : ReactiveObject, IDisposable
{
    private readonly HashSet<IDocumentDock> _documentDocks = new();
    private readonly HashSet<INotifyCollectionChanged> _dockCollections = new();
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    /// <param name="factory">The layout factory.</param>
    /// <param name="layout">The main root layout.</param>
    public MainWindowViewModel(IFactory factory, IRootDock layout)
    {
        Factory = factory ?? throw new ArgumentNullException(nameof(factory));
        Layout = layout ?? throw new ArgumentNullException(nameof(layout));
        ExtendClientAreaToDecorationsHint = true;
        RefreshSubscriptions();
    }

    /// <summary>
    /// Gets the layout factory used by the dock control.
    /// </summary>
    public IFactory Factory { get; }

    /// <summary>
    /// Gets the main root layout.
    /// </summary>
    public IRootDock Layout { get; }

    /// <summary>
    /// Gets whether content should extend into the main window decorations.
    /// </summary>
    [Reactive]
    public partial bool ExtendClientAreaToDecorationsHint { get; private set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        ClearSubscriptions();
    }

    private void RefreshSubscriptions()
    {
        ClearSubscriptions();

        var pending = new Stack<IDockable>();
        var visited = new HashSet<IDockable>();
        pending.Push(Layout);

        while (pending.Count > 0)
        {
            var dockable = pending.Pop();
            if (!visited.Add(dockable))
            {
                continue;
            }

            if (dockable is IDocumentDock documentDock)
            {
                _documentDocks.Add(documentDock);
                if (documentDock is INotifyPropertyChanged notifyingDocumentDock)
                {
                    notifyingDocumentDock.PropertyChanged += OnDocumentDockPropertyChanged;
                }
            }

            if (dockable is not IDock { VisibleDockables: { } visibleDockables })
            {
                continue;
            }

            if (visibleDockables is INotifyCollectionChanged notifyingCollection
                && _dockCollections.Add(notifyingCollection))
            {
                notifyingCollection.CollectionChanged += OnDockCollectionChanged;
            }

            for (var index = visibleDockables.Count - 1; index >= 0; index--)
            {
                pending.Push(visibleDockables[index]);
            }
        }

        UpdateChromeState();
    }

    private void ClearSubscriptions()
    {
        foreach (var documentDock in _documentDocks)
        {
            if (documentDock is INotifyPropertyChanged notifyingDocumentDock)
            {
                notifyingDocumentDock.PropertyChanged -= OnDocumentDockPropertyChanged;
            }
        }

        foreach (var dockCollection in _dockCollections)
        {
            dockCollection.CollectionChanged -= OnDockCollectionChanged;
        }

        _documentDocks.Clear();
        _dockCollections.Clear();
    }

    private void OnDockCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshSubscriptions();
    }

    private void OnDocumentDockPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(IDocumentDock.LayoutMode))
        {
            UpdateChromeState();
        }
    }

    private void UpdateChromeState()
    {
        foreach (var documentDock in _documentDocks)
        {
            if (documentDock.LayoutMode == DocumentLayoutMode.Mdi)
            {
                ExtendClientAreaToDecorationsHint = false;
                return;
            }
        }

        ExtendClientAreaToDecorationsHint = true;
    }
}
