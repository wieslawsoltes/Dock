// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Diagnostics.Helpers;
using Dock.Model.Core;
using Dock.Model.Core.Events;
using ModelWindowClosingEventArgs = Dock.Model.Core.Events.WindowClosingEventArgs;

namespace Dock.Avalonia.Diagnostics.Controls;

/// <summary>
/// Window that logs Dock events and displays layout information.
/// </summary>
public partial class RootDockDebug : UserControl, INotifyPropertyChanged
{
    /// <summary>
    /// Event raised when a dockable should be selected from hit testing.
    /// </summary>
    public static event Action<IDockable?>? SelectDockableRequested;

    private List<IDisposable>? _subscriptions;
    private string? _filter;
    private SelectionOverlayHelper? _overlayHelper;
    private TreeView? _visibleTree;

    /// <summary>
    /// Gets collection with logged events.
    /// </summary>
    public ObservableCollection<string> Events { get; } = new();

    /// <summary>
    /// Gets or sets filter text.
    /// </summary>
    public string? Filter
    {
        get => _filter;
        set
        {
            if (_filter != value)
            {
                _filter = value;
                OnPropertyChanged(nameof(Filter));
                OnPropertyChanged(nameof(FilteredEvents));
            }
        }
    }

    /// <summary>
    /// Gets filtered events collection.
    /// </summary>
    public IEnumerable<string> FilteredEvents =>
        string.IsNullOrEmpty(Filter)
            ? Events
            : Events.Where(x => x?.IndexOf(Filter!, StringComparison.OrdinalIgnoreCase) >= 0);

    /// <summary>
    /// Initializes new instance of the <see cref="RootDockDebug"/> class.
    /// </summary>
    public RootDockDebug()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _visibleTree = this.FindControl<TreeView>("Visible");
        if (_visibleTree is not null)
        {
            _visibleTree.SelectionChanged += OnVisibleSelectionChanged;
        }

        SelectDockableRequested += OnSelectDockableRequested;
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (_visibleTree is not null)
        {
            _visibleTree.SelectionChanged -= OnVisibleSelectionChanged;
            _visibleTree = null;
        }

        _overlayHelper?.RemoveOverlay();
        _overlayHelper = null;

        SelectDockableRequested -= OnSelectDockableRequested;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_subscriptions is not null)
        {
            foreach (var d in _subscriptions)
            {
                d.Dispose();
            }
            _subscriptions = null;
        }

        if (DataContext is IDockable { Factory: { } factory })
        {
            var disposables = new List<IDisposable>();

            factory.ActiveDockableChanged += OnActiveDockableChanged;
            disposables.Add(new ActionDisposable(() => factory.ActiveDockableChanged -= OnActiveDockableChanged));
            factory.FocusedDockableChanged += OnFocusedDockableChanged;
            disposables.Add(new ActionDisposable(() => factory.FocusedDockableChanged -= OnFocusedDockableChanged));
            factory.DockableInit += OnDockableInit;
            disposables.Add(new ActionDisposable(() => factory.DockableInit -= OnDockableInit));
            factory.DockableAdded += OnDockableAdded;
            disposables.Add(new ActionDisposable(() => factory.DockableAdded -= OnDockableAdded));
            factory.DockableRemoved += OnDockableRemoved;
            disposables.Add(new ActionDisposable(() => factory.DockableRemoved -= OnDockableRemoved));
            factory.DockableClosed += OnDockableClosed;
            disposables.Add(new ActionDisposable(() => factory.DockableClosed -= OnDockableClosed));
            factory.DockableMoved += OnDockableMoved;
            disposables.Add(new ActionDisposable(() => factory.DockableMoved -= OnDockableMoved));
            factory.DockableDocked += OnDockableDocked;
            disposables.Add(new ActionDisposable(() => factory.DockableDocked -= OnDockableDocked));
            factory.DockableUndocked += OnDockableUndocked;
            disposables.Add(new ActionDisposable(() => factory.DockableUndocked -= OnDockableUndocked));
            factory.DockableSwapped += OnDockableSwapped;
            disposables.Add(new ActionDisposable(() => factory.DockableSwapped -= OnDockableSwapped));
            factory.DockablePinned += OnDockablePinned;
            disposables.Add(new ActionDisposable(() => factory.DockablePinned -= OnDockablePinned));
            factory.DockableUnpinned += OnDockableUnpinned;
            disposables.Add(new ActionDisposable(() => factory.DockableUnpinned -= OnDockableUnpinned));
            factory.DockableHidden += OnDockableHidden;
            disposables.Add(new ActionDisposable(() => factory.DockableHidden -= OnDockableHidden));
            factory.DockableRestored += OnDockableRestored;
            disposables.Add(new ActionDisposable(() => factory.DockableRestored -= OnDockableRestored));
            factory.WindowOpened += OnWindowOpened;
            disposables.Add(new ActionDisposable(() => factory.WindowOpened -= OnWindowOpened));
            factory.WindowClosing += OnWindowClosing;
            disposables.Add(new ActionDisposable(() => factory.WindowClosing -= OnWindowClosing));
            factory.WindowClosed += OnWindowClosed;
            disposables.Add(new ActionDisposable(() => factory.WindowClosed -= OnWindowClosed));
            factory.WindowAdded += OnWindowAdded;
            disposables.Add(new ActionDisposable(() => factory.WindowAdded -= OnWindowAdded));
            factory.WindowRemoved += OnWindowRemoved;
            disposables.Add(new ActionDisposable(() => factory.WindowRemoved -= OnWindowRemoved));
            factory.WindowMoveDragBegin += OnWindowMoveDragBegin;
            disposables.Add(new ActionDisposable(() => factory.WindowMoveDragBegin -= OnWindowMoveDragBegin));
            factory.WindowMoveDrag += OnWindowMoveDrag;
            disposables.Add(new ActionDisposable(() => factory.WindowMoveDrag -= OnWindowMoveDrag));
            factory.WindowMoveDragEnd += OnWindowMoveDragEnd;
            disposables.Add(new ActionDisposable(() => factory.WindowMoveDragEnd -= OnWindowMoveDragEnd));

            _subscriptions = disposables;
        }
    }

    private void AddEvent(string message)
    {
        Events.Add($"{DateTime.Now:HH:mm:ss.fff} {message}");
        OnPropertyChanged(nameof(FilteredEvents));
    }

    private void OnActiveDockableChanged(object? sender, ActiveDockableChangedEventArgs e) =>
        AddEvent($"ActiveDockableChanged {e.Dockable?.Title}");
    private void OnFocusedDockableChanged(object? sender, FocusedDockableChangedEventArgs e) =>
        AddEvent($"FocusedDockableChanged {e.Dockable?.Title}");
    private void OnDockableInit(object? sender, DockableInitEventArgs e) =>
        AddEvent($"DockableInit {e.Dockable?.Title}");
    private void OnDockableAdded(object? sender, DockableAddedEventArgs e) =>
        AddEvent($"DockableAdded {e.Dockable?.Title}");
    private void OnDockableRemoved(object? sender, DockableRemovedEventArgs e) =>
        AddEvent($"DockableRemoved {e.Dockable?.Title}");
    private void OnDockableClosed(object? sender, DockableClosedEventArgs e) =>
        AddEvent($"DockableClosed {e.Dockable?.Title}");
    private void OnDockableMoved(object? sender, DockableMovedEventArgs e) =>
        AddEvent($"DockableMoved {e.Dockable?.Title}");
    private void OnDockableDocked(object? sender, DockableDockedEventArgs e) =>
        AddEvent($"DockableDocked {e.Dockable?.Title} {e.Operation}");
    private void OnDockableUndocked(object? sender, DockableUndockedEventArgs e) =>
        AddEvent($"DockableUndocked {e.Dockable?.Title} {e.Operation}");
    private void OnDockableSwapped(object? sender, DockableSwappedEventArgs e) =>
        AddEvent($"DockableSwapped {e.Dockable?.Title}");
    private void OnDockablePinned(object? sender, DockablePinnedEventArgs e) =>
        AddEvent($"DockablePinned {e.Dockable?.Title}");
    private void OnDockableUnpinned(object? sender, DockableUnpinnedEventArgs e) =>
        AddEvent($"DockableUnpinned {e.Dockable?.Title}");
    private void OnDockableHidden(object? sender, DockableHiddenEventArgs e) =>
        AddEvent($"DockableHidden {e.Dockable?.Title}");
    private void OnDockableRestored(object? sender, DockableRestoredEventArgs e) =>
        AddEvent($"DockableRestored {e.Dockable?.Title}");
    private void OnWindowOpened(object? sender, WindowOpenedEventArgs e) =>
        AddEvent($"WindowOpened {e.Window?.Title}");
    private void OnWindowClosing(object? sender, ModelWindowClosingEventArgs e) =>
        AddEvent($"WindowClosing {e.Window?.Title}");
    private void OnWindowClosed(object? sender, WindowClosedEventArgs e) =>
        AddEvent($"WindowClosed {e.Window?.Title}");
    private void OnWindowAdded(object? sender, WindowAddedEventArgs e) =>
        AddEvent($"WindowAdded {e.Window?.Title}");
    private void OnWindowRemoved(object? sender, WindowRemovedEventArgs e) =>
        AddEvent($"WindowRemoved {e.Window?.Title}");
    private void OnWindowMoveDragBegin(object? sender, WindowMoveDragBeginEventArgs e) =>
        AddEvent($"WindowMoveDragBegin {e.Window?.Title}");
    private void OnWindowMoveDrag(object? sender, WindowMoveDragEventArgs e) =>
        AddEvent($"WindowMoveDrag {e.Window?.Title}");
    private void OnWindowMoveDragEnd(object? sender, WindowMoveDragEndEventArgs e) =>
        AddEvent($"WindowMoveDragEnd {e.Window?.Title}");

    private void OnVisibleSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_visibleTree?.SelectedItem is not IDockable dockable || dockable.Factory is not { } factory)
        {
            _overlayHelper?.Highlight(null);
            return;
        }

        if (SelectionOverlayHelper.TryGetControl(factory, dockable, out var root, out var control) &&
            root is not null && control is not null)
        {
            _overlayHelper ??= new SelectionOverlayHelper();
            _overlayHelper.AttachOverlay(root);
            _overlayHelper.Highlight(control);
        }
        else
        {
            _overlayHelper?.Highlight(null);
        }
    }

    private void OnSelectDockableRequested(IDockable? dockable)
    {
        if (dockable is null || _visibleTree is null)
        {
            return;
        }

        _visibleTree.SelectedItem = dockable;
    }

    /// <summary>
    /// Clears the event log.
    /// </summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">The routed event args.</param>
    private void OnClearEvents(object? sender, RoutedEventArgs e)
    {
        Events.Clear();
        OnPropertyChanged(nameof(FilteredEvents));
    }

    /// <inheritdoc />
    public new event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private sealed class ActionDisposable : IDisposable
    {
        private readonly Action _dispose;

        public ActionDisposable(Action dispose) => _dispose = dispose;

        /// <summary>
        /// Executes the stored dispose action.
        /// </summary>
        public void Dispose() => _dispose();
    }
}
