// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using Dock.Avalonia.Internal;
using Dock.Avalonia.Mdi;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Managed window layer hosting floating windows inside the main visual tree.
/// </summary>
[TemplatePart("PART_OverlayCanvas", typeof(Canvas))]
public sealed class ManagedWindowLayer : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="Dock"/> property.
    /// </summary>
    public static readonly StyledProperty<IDock?> DockProperty =
        AvaloniaProperty.Register<ManagedWindowLayer, IDock?>(nameof(Dock));

    /// <summary>
    /// Defines the <see cref="LayoutManager"/> property.
    /// </summary>
    public static readonly StyledProperty<IMdiLayoutManager?> LayoutManagerProperty =
        AvaloniaProperty.Register<ManagedWindowLayer, IMdiLayoutManager?>(nameof(LayoutManager), ClassicMdiLayoutManager.Instance);

    private readonly Dictionary<string, Control> _overlays = new();
    private readonly Dictionary<ManagedDockWindowDocument, PropertyChangedEventHandler> _windowSubscriptions = new();
    private INotifyPropertyChanged? _dockSubscription;
    private INotifyCollectionChanged? _dockablesSubscription;
    private Canvas? _overlayCanvas;
    private Point? _cachedWindowContentOffset;

    /// <summary>
    /// Gets or sets the dock that owns managed windows.
    /// </summary>
    public IDock? Dock
    {
        get => GetValue(DockProperty);
        set => SetValue(DockProperty, value);
    }

    /// <summary>
    /// Gets or sets the layout manager used for MDI layout.
    /// </summary>
    public IMdiLayoutManager? LayoutManager
    {
        get => GetValue(LayoutManagerProperty);
        set => SetValue(LayoutManagerProperty, value);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _overlayCanvas = e.NameScope.Find<Canvas>("PART_OverlayCanvas");
        _cachedWindowContentOffset = null;
        AttachDock(Dock);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DockProperty)
        {
            AttachDock(change.GetNewValue<IDock>());
        }
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (Dock?.Factory is { } factory)
        {
            ManagedWindowRegistry.UnregisterLayer(factory, this);
        }
    }

    /// <summary>
    /// Attempts to locate the managed window layer for a visual.
    /// </summary>
    public static ManagedWindowLayer? TryGetLayer(Visual visual)
    {
        if (visual is null)
        {
            return null;
        }

        var dockControl = visual as DockControl ?? visual.FindAncestorOfType<DockControl>();
        if (dockControl is { })
        {
            var layer = dockControl.GetVisualDescendants().OfType<ManagedWindowLayer>().FirstOrDefault();
            if (layer is not null)
            {
                return layer;
            }
        }

        if (visual.GetVisualRoot() is not Visual root)
        {
            return null;
        }

        return root.GetVisualDescendants().OfType<ManagedWindowLayer>().FirstOrDefault();
    }

    /// <summary>
    /// Shows or updates an overlay element.
    /// </summary>
    public void ShowOverlay(string key, Control control, Rect bounds, bool hitTestVisible)
    {
        ShowOverlay(key, control, bounds.Position, bounds.Size, hitTestVisible);
    }

    /// <summary>
    /// Shows or updates an overlay element at the given position.
    /// </summary>
    public void ShowOverlay(string key, Control control, Point position, Size? size, bool hitTestVisible)
    {
        if (_overlayCanvas is null)
        {
            ApplyTemplate();
        }

        if (_overlayCanvas is null)
        {
            return;
        }

        if (!_overlays.TryGetValue(key, out var existing))
        {
            existing = control;
            _overlays[key] = existing;
            _overlayCanvas.Children.Add(existing);
        }

        existing.IsHitTestVisible = hitTestVisible;
        if (size.HasValue)
        {
            existing.Width = size.Value.Width;
            existing.Height = size.Value.Height;
        }
        else
        {
            existing.Width = double.NaN;
            existing.Height = double.NaN;
        }
        Canvas.SetLeft(existing, position.X);
        Canvas.SetTop(existing, position.Y);
    }

    /// <summary>
    /// Removes an overlay element.
    /// </summary>
    public void HideOverlay(string key)
    {
        if (_overlayCanvas is null)
        {
            return;
        }

        if (_overlays.TryGetValue(key, out var control))
        {
            _overlayCanvas.Children.Remove(control);
            _overlays.Remove(key);
        }
    }

    internal bool TryGetWindowContentOffset(out Point offset)
    {
        offset = default;

        if (_cachedWindowContentOffset.HasValue)
        {
            offset = _cachedWindowContentOffset.Value;
            return true;
        }

        var window = this.GetVisualDescendants()
            .OfType<MdiDocumentWindow>()
            .FirstOrDefault(candidate => candidate.IsVisible);

        if (window is { } && window.TryGetContentOffset(out offset))
        {
            _cachedWindowContentOffset = offset;
            return true;
        }

        if (TryMeasureWindowContentOffset(out offset))
        {
            _cachedWindowContentOffset = offset;
            return true;
        }

        return false;
    }

    private bool TryMeasureWindowContentOffset(out Point offset)
    {
        offset = default;

        if (_overlayCanvas is null)
        {
            ApplyTemplate();
        }

        if (_overlayCanvas is null)
        {
            return false;
        }

        var probe = new MdiDocumentWindow
        {
            Width = 300,
            Height = 200,
            Opacity = 0,
            IsHitTestVisible = false,
            DataContext = new OffsetProbeDockable { Title = string.Empty }
        };

        ShowOverlay("ManagedWindowOffsetProbe", probe, new Point(0, 0), new Size(300, 200), false);
        probe.ApplyTemplate();
        probe.Measure(new Size(300, 200));
        probe.Arrange(new Rect(new Point(0, 0), new Size(300, 200)));

        var measured = probe.TryGetContentOffset(out offset);
        HideOverlay("ManagedWindowOffsetProbe");
        return measured;
    }

    private sealed class OffsetProbeDockable : ManagedDockableBase
    {
    }

    private void AttachDock(IDock? dock)
    {
        DetachDock();

        if (dock is null)
        {
            return;
        }

        DataContext = dock;

        if (dock is INotifyPropertyChanged propertyChanged)
        {
            _dockSubscription = propertyChanged;
            _dockSubscription.PropertyChanged += DockPropertyChanged;
        }

        AttachDockablesCollection(dock.VisibleDockables as INotifyCollectionChanged);
        TrackDockables(dock.VisibleDockables);
        UpdateZOrder();
    }

    private void DetachDock()
    {
        ClearDockableSubscriptions();

        if (_dockSubscription is not null)
        {
            _dockSubscription.PropertyChanged -= DockPropertyChanged;
            _dockSubscription = null;
        }

        if (_dockablesSubscription is not null)
        {
            _dockablesSubscription.CollectionChanged -= DockablesCollectionChanged;
            _dockablesSubscription = null;
        }

        DataContext = null;
    }

    private void DockPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (Dock is null)
        {
            return;
        }

        if (e.PropertyName == nameof(IDock.VisibleDockables))
        {
            AttachDockablesCollection(Dock.VisibleDockables as INotifyCollectionChanged);
            TrackDockables(Dock.VisibleDockables);
            UpdateZOrder();
            return;
        }

        if (e.PropertyName == nameof(IDock.ActiveDockable))
        {
            UpdateZOrder();
        }
    }

    private void AttachDockablesCollection(INotifyCollectionChanged? collection)
    {
        if (_dockablesSubscription is not null)
        {
            _dockablesSubscription.CollectionChanged -= DockablesCollectionChanged;
            _dockablesSubscription = null;
        }

        if (collection is null)
        {
            return;
        }

        _dockablesSubscription = collection;
        _dockablesSubscription.CollectionChanged += DockablesCollectionChanged;
    }

    private void DockablesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems is { } added)
                {
                    foreach (var item in added.OfType<ManagedDockWindowDocument>())
                    {
                        AttachDockableSubscription(item);
                    }
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems is { } removed)
                {
                    foreach (var item in removed.OfType<ManagedDockWindowDocument>())
                    {
                        DetachDockableSubscription(item);
                    }
                }
                break;
            case NotifyCollectionChangedAction.Reset:
                TrackDockables(Dock?.VisibleDockables);
                break;
            case NotifyCollectionChangedAction.Replace:
                if (e.OldItems is { } replacedOld)
                {
                    foreach (var item in replacedOld.OfType<ManagedDockWindowDocument>())
                    {
                        DetachDockableSubscription(item);
                    }
                }
                if (e.NewItems is { } replacedNew)
                {
                    foreach (var item in replacedNew.OfType<ManagedDockWindowDocument>())
                    {
                        AttachDockableSubscription(item);
                    }
                }
                break;
        }

        UpdateZOrder();
    }

    private void UpdateZOrder()
    {
        if (Dock?.VisibleDockables is null)
        {
            return;
        }

        var documents = Dock.VisibleDockables.OfType<IMdiDocument>().ToList();
        var manager = LayoutManager ?? ClassicMdiLayoutManager.Instance;
        var active = Dock.ActiveDockable as IMdiDocument;

        var topmostDocuments = documents.Where(IsTopmost).ToList();
        if (topmostDocuments.Count == 0)
        {
            manager.UpdateZOrder(documents, active);
            return;
        }

        var normalDocuments = documents.Where(document => !IsTopmost(document)).ToList();
        var activeNormal = active is not null && normalDocuments.Contains(active) ? active : null;
        var activeTopmost = active is not null && topmostDocuments.Contains(active) ? active : null;

        manager.UpdateZOrder(normalDocuments, activeNormal);
        manager.UpdateZOrder(topmostDocuments, activeTopmost);

        var offset = normalDocuments.Count;
        foreach (var document in topmostDocuments)
        {
            document.MdiZIndex += offset;
        }
    }

    private static bool IsTopmost(IMdiDocument document)
    {
        return document is ManagedDockWindowDocument { Window: { Topmost: true } };
    }

    private void TrackDockables(IList<IDockable>? dockables)
    {
        ClearDockableSubscriptions();

        if (dockables is null)
        {
            return;
        }

        foreach (var document in dockables.OfType<ManagedDockWindowDocument>())
        {
            AttachDockableSubscription(document);
        }
    }

    private void ClearDockableSubscriptions()
    {
        if (_windowSubscriptions.Count == 0)
        {
            return;
        }

        foreach (var document in _windowSubscriptions.Keys.ToList())
        {
            DetachDockableSubscription(document);
        }

        _windowSubscriptions.Clear();
    }

    private void AttachDockableSubscription(ManagedDockWindowDocument document)
    {
        if (_windowSubscriptions.ContainsKey(document))
        {
            return;
        }

        if (document.Window is not INotifyPropertyChanged window)
        {
            return;
        }

        PropertyChangedEventHandler handler = (_, e) =>
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(IDockWindow.Topmost))
            {
                UpdateZOrder();
            }
        };

        window.PropertyChanged += handler;
        _windowSubscriptions[document] = handler;
    }

    private void DetachDockableSubscription(ManagedDockWindowDocument document)
    {
        if (!_windowSubscriptions.TryGetValue(document, out var handler))
        {
            return;
        }

        if (document.Window is INotifyPropertyChanged window)
        {
            window.PropertyChanged -= handler;
        }

        _windowSubscriptions.Remove(document);
    }
}
