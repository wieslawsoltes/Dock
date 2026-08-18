using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Dock.Avalonia.Controls.Overlays;

/// <summary>
/// Composes overlay controls around hosted content in order.
/// </summary>
public sealed class OverlayHost : Decorator
{
    /// <summary>
    /// Defines the <see cref="Content"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<OverlayHost, object?>(nameof(Content));

    /// <summary>
    /// Defines the <see cref="ContentTemplate"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        AvaloniaProperty.Register<OverlayHost, IDataTemplate?>(nameof(ContentTemplate));

    /// <summary>
    /// Defines the <see cref="Overlays"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<IList<Control>> OverlaysProperty =
        AvaloniaProperty.Register<OverlayHost, IList<Control>>(nameof(Overlays));

    /// <summary>
    /// Defines the <see cref="OverlayLayers"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<OverlayLayerCollection> OverlayLayersProperty =
        AvaloniaProperty.Register<OverlayHost, OverlayLayerCollection>(nameof(OverlayLayers));

    /// <summary>
    /// Defines the <see cref="UseServiceLayers"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<bool> UseServiceLayersProperty =
        AvaloniaProperty.Register<OverlayHost, bool>(nameof(UseServiceLayers), true);

    private IList<Control> _overlays = new List<Control>();
    private OverlayLayerCollection _overlayLayers = new();
    private OverlayLayerCollection? _serviceLayers;
    private bool _isAttached;
    private bool _isRebuilding;
    private bool _rebuildRequested;
    private bool _rebuildScheduled;

    static OverlayHost()
    {
        ContentProperty.Changed.AddClassHandler<OverlayHost>((host, _) => host.RequestRebuild());
        ContentTemplateProperty.Changed.AddClassHandler<OverlayHost>((host, _) => host.RequestRebuild());
        OverlaysProperty.Changed.AddClassHandler<OverlayHost>((host, args) => host.OnOverlaysChanged(args));
        OverlayLayersProperty.Changed.AddClassHandler<OverlayHost>((host, args) => host.OnOverlayLayersChanged(args));
        UseServiceLayersProperty.Changed.AddClassHandler<OverlayHost>((host, _) => host.OnUseServiceLayersChanged());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OverlayHost"/> class.
    /// </summary>
    public OverlayHost()
    {
        Overlays = new AvaloniaList<Control>();
    }

    /// <summary>
    /// Gets or sets the hosted content.
    /// </summary>
    [Content]
    [DependsOn(nameof(ContentTemplate))]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the data template for the content.
    /// </summary>
    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the overlay controls applied in order.
    /// </summary>
    public IList<Control> Overlays
    {
        get => GetValue(OverlaysProperty);
        set => SetValue(OverlaysProperty, value);
    }

    /// <summary>
    /// Gets or sets the overlay layers applied in order.
    /// </summary>
    public OverlayLayerCollection OverlayLayers
    {
        get => GetValue(OverlayLayersProperty) ?? _overlayLayers;
        set => SetValue(OverlayLayersProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether service-registered overlay layers should be used.
    /// </summary>
    public bool UseServiceLayers
    {
        get => GetValue(UseServiceLayersProperty);
        set => SetValue(UseServiceLayersProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        AttachOverlayCollections();
        OverlayLayerRegistry.ProviderChanged += OnOverlayLayerProviderChanged;
        ResetServiceLayers();
        RebuildPipelineCore();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _isAttached = false;
        OverlayLayerRegistry.ProviderChanged -= OnOverlayLayerProviderChanged;
        DetachOverlayCollections();
        ResetServiceLayers();
    }

    private void OnOverlaysChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (_overlays is INotifyCollectionChanged oldList)
        {
            oldList.CollectionChanged -= OnOverlaysCollectionChanged;
        }

        _overlays = args.NewValue as IList<Control> ?? new List<Control>();

        if (_isAttached && _overlays is INotifyCollectionChanged newList)
        {
            newList.CollectionChanged += OnOverlaysCollectionChanged;
        }

        RequestRebuild();
    }

    private void OnOverlaysCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RequestRebuild();
    }

    private void OnOverlayLayersChanged(AvaloniaPropertyChangedEventArgs args)
    {
        var newLayers = args.NewValue as OverlayLayerCollection ?? new OverlayLayerCollection();

        if (IsStructurallyEquivalent(_overlayLayers, newLayers))
        {
            // Treat a structurally-equivalent replacement as a no-op and keep the current,
            // already-attached layers/content untouched.
            return;
        }

        if (_overlayLayers is INotifyCollectionChanged oldList)
        {
            oldList.CollectionChanged -= OnOverlayLayersCollectionChanged;
        }

        UnsubscribeLayerChanges(_overlayLayers);
        _overlayLayers = newLayers;
        if (_isAttached)
        {
            SubscribeLayerChanges(_overlayLayers);
        }

        if (_isAttached && _overlayLayers is INotifyCollectionChanged newList)
        {
            newList.CollectionChanged += OnOverlayLayersCollectionChanged;
        }

        RequestRebuild();
    }

    /// <summary>
    /// Compares two overlay layer collections for structural equivalence: same ordered
    /// sequence of layer kinds/visibility/z-order/style-key, regardless of whether the
    /// individual layer/control instances are the same objects.
    /// </summary>
    private static bool IsStructurallyEquivalent(OverlayLayerCollection oldLayers, OverlayLayerCollection newLayers)
    {
        if (ReferenceEquals(oldLayers, newLayers))
        {
            return true;
        }

        if (oldLayers.Count != newLayers.Count)
        {
            return false;
        }

        for (var i = 0; i < oldLayers.Count; i++)
        {
            var a = oldLayers[i];
            var b = newLayers[i];

            if (a is null || b is null)
            {
                if (!ReferenceEquals(a, b))
                {
                    return false;
                }

                continue;
            }

            if (a.ZIndex != b.ZIndex
                || a.IsVisible != b.IsVisible
                || a.BlocksInput != b.BlocksInput
                || !Equals(a.StyleKey, b.StyleKey))
            {
                return false;
            }

            var overlayA = a.Overlay;
            var overlayB = b.Overlay;

            if (ReferenceEquals(overlayA, overlayB))
            {
                continue;
            }

            if (overlayA is null || overlayB is null || overlayA.GetType() != overlayB.GetType())
            {
                return false;
            }
        }

        return true;
    }

    private void OnOverlayLayersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            UnsubscribeLayerChanges(e.OldItems.OfType<IOverlayLayer>());
        }

        if (e.NewItems is not null)
        {
            SubscribeLayerChanges(e.NewItems.OfType<IOverlayLayer>());
        }

        RequestRebuild();
    }

    private void OnServiceLayersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            UnsubscribeLayerChanges(e.OldItems.OfType<IOverlayLayer>());
        }

        if (e.NewItems is not null)
        {
            SubscribeLayerChanges(e.NewItems.OfType<IOverlayLayer>());
        }

        RequestRebuild();
    }

    private void OnUseServiceLayersChanged()
    {
        ResetServiceLayers();
        RequestRebuild();
    }

    private void OnOverlayLayerProviderChanged(object? sender, EventArgs e)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnOverlayLayerProviderChanged(sender, e));
            return;
        }

        if (!_isAttached)
        {
            return;
        }

        ResetServiceLayers();
        RequestRebuild();
    }

    private void AttachOverlayCollections()
    {
        if (_overlays is INotifyCollectionChanged overlaysList)
        {
            overlaysList.CollectionChanged += OnOverlaysCollectionChanged;
        }

        if (_overlayLayers is INotifyCollectionChanged layersList)
        {
            layersList.CollectionChanged += OnOverlayLayersCollectionChanged;
        }

        SubscribeLayerChanges(_overlayLayers);
    }

    private void DetachOverlayCollections()
    {
        if (_overlays is INotifyCollectionChanged overlaysList)
        {
            overlaysList.CollectionChanged -= OnOverlaysCollectionChanged;
        }

        if (_overlayLayers is INotifyCollectionChanged layersList)
        {
            layersList.CollectionChanged -= OnOverlayLayersCollectionChanged;
        }

        UnsubscribeLayerChanges(_overlayLayers);
    }

    private void SubscribeLayerChanges(IEnumerable<IOverlayLayer> layers)
    {
        foreach (var layer in layers)
        {
            if (layer is AvaloniaObject avaloniaObject)
            {
                avaloniaObject.PropertyChanged += OnOverlayLayerPropertyChanged;
            }
        }
    }

    private void UnsubscribeLayerChanges(IEnumerable<IOverlayLayer> layers)
    {
        foreach (var layer in layers)
        {
            if (layer is AvaloniaObject avaloniaObject)
            {
                avaloniaObject.PropertyChanged -= OnOverlayLayerPropertyChanged;
            }
        }
    }

    private void OnOverlayLayerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        RequestRebuild();
    }

    /// <summary>
    /// Requests that the overlay pipeline be rebuilt.
    /// </summary>
    /// <remarks>
    /// Multiple requests arriving before the pipeline is rebuilt are coalesced onto a
    /// single dispatcher callback.
    /// </remarks>
    private void RequestRebuild()
    {
        if (!_isAttached)
        {
            // Nothing is observing the pipeline yet; OnAttachedToVisualTree will
            // build it synchronously with the latest values once attached.
            return;
        }

        if (_isRebuilding)
        {
            _rebuildRequested = true;
            return;
        }

        if (_rebuildScheduled)
        {
            return;
        }

        _rebuildScheduled = true;
        Dispatcher.UIThread.Post(() =>
        {
            _rebuildScheduled = false;

            if (!_isAttached)
            {
                return;
            }

            RebuildPipelineCore();
        });
    }

    /// <summary>
    /// Rebuilds the overlay pipeline, guarding against reentrancy.
    /// </summary>
    /// <remarks>
    /// If <see cref="RebuildPipeline"/> triggers another rebuild request while it is
    /// executing (directly or indirectly, e.g. via a property changed on an overlay it
    /// just attached), that request is not run recursively.
    /// </remarks>s
    private void RebuildPipelineCore()
    {
        if (_isRebuilding)
        {
            _rebuildRequested = true;
            return;
        }

        _isRebuilding = true;
        try
        {
            do
            {
                _rebuildRequested = false;
                RebuildPipeline();
            }
            while (_rebuildRequested);
        }
        finally
        {
            _isRebuilding = false;
        }
    }

    private void RebuildPipeline()
    {
        if (Content is Control hostedContent)
        {
            var parent = hostedContent.Parent ?? hostedContent.GetVisualParent();
            if (parent is not null && !TryDetachControl(hostedContent))
            {
                return;
            }
        }

        Child = null;

        var contentPresenter = new ContentPresenter
        {
            Content = Content,
            ContentTemplate = ContentTemplate
        };

        Control current = contentPresenter;

        foreach (var layer in GetLayerEntries())
        {
            var overlay = layer.Overlay;
            if (overlay is null)
            {
                continue;
            }

            if (!TryDetachControl(overlay))
            {
                continue;
            }

            ApplyStyleKey(layer.StyleKey, overlay);

            if (overlay is IOverlayContentHost contentHost)
            {
                contentHost.Content = current;
                contentHost.ContentTemplate = null;
                contentHost.BlocksInput = layer.BlocksInput;
                current = overlay;
                continue;
            }

            if (overlay is ContentControl contentControl)
            {
                contentControl.Content = current;
                contentControl.ContentTemplate = null;
                current = overlay;
                continue;
            }

            if (overlay is Decorator decorator)
            {
                decorator.Child = current;
                current = decorator;
                continue;
            }

            overlay.IsHitTestVisible = layer.BlocksInput;

            var grid = new Grid();
            grid.Children.Add(current);
            if (layer.ZIndex != 0)
            {
                overlay.SetValue(Panel.ZIndexProperty, layer.ZIndex);
            }

            grid.Children.Add(overlay);
            current = grid;
        }

        Child = current;
    }

    private static bool TryDetachControl(Control control)
    {
        var parent = control.Parent ?? control.GetVisualParent();
        if (parent is null)
        {
            return true;
        }

        switch (parent)
        {
            case Panel panel:
                panel.Children.Remove(control);
                return true;
            case Decorator decorator when ReferenceEquals(decorator.Child, control):
                decorator.Child = null;
                return true;
            case ContentPresenter presenter:
                if (ReferenceEquals(presenter.Child, control))
                {
                    presenter.SetCurrentValue(ContentPresenter.ContentProperty, null);
                    presenter.UpdateChild();
                    return control.GetVisualParent() is null;
                }

                if (ReferenceEquals(presenter.Content, control))
                {
                    presenter.SetCurrentValue(ContentPresenter.ContentProperty, null);
                    presenter.UpdateChild();
                    return true;
                }

                return false;
            case ContentControl contentControl when ReferenceEquals(contentControl.Content, control):
                contentControl.Content = null;
                return true;
            default:
                return false;
        }
    }

    private IEnumerable<OverlayLayerEntry> GetLayerEntries()
    {
        var serviceLayers = EnsureServiceLayers();
        if (serviceLayers is not null && serviceLayers.Count > 0)
        {
            foreach (var layer in serviceLayers.GetOrderedLayers())
            {
                if (!layer.IsVisible || layer.Overlay is null)
                {
                    continue;
                }

                yield return new OverlayLayerEntry(
                    layer.Overlay,
                    layer.ZIndex,
                    layer.IsVisible,
                    layer.BlocksInput,
                    layer.StyleKey);
            }

            yield break;
        }

        if (_overlayLayers.Count > 0)
        {
            foreach (var layer in _overlayLayers.GetOrderedLayers())
            {
                if (!layer.IsVisible || layer.Overlay is null)
                {
                    continue;
                }

                yield return new OverlayLayerEntry(
                    layer.Overlay,
                    layer.ZIndex,
                    layer.IsVisible,
                    layer.BlocksInput,
                    layer.StyleKey);
            }

            yield break;
        }

        foreach (var overlay in _overlays)
        {
            if (overlay is null)
            {
                continue;
            }

            yield return new OverlayLayerEntry(overlay, 0, true, true, null);
        }
    }

    private OverlayLayerCollection? EnsureServiceLayers()
    {
        if (!UseServiceLayers)
        {
            return null;
        }

        if (_serviceLayers is not null)
        {
            return _serviceLayers;
        }

        var factoryProvider = OverlayLayerRegistry.FactoryProvider;
        if (factoryProvider is not null)
        {
            var factories = factoryProvider.Invoke();
            var factoryList = factories?.Where(factory => factory is not null).ToList()
                ?? new List<IOverlayLayerFactory>();

            if (factoryList.Count > 0)
            {
                var created = new OverlayLayerCollection();
                foreach (var factory in factoryList)
                {
                    var layer = factory.Create();
                    if (layer is not null)
                    {
                        created.Add(layer);
                    }
                }

                if (created.Count > 0)
                {
                    AttachServiceLayers(created);
                    _serviceLayers = created;
                    return _serviceLayers;
                }
            }
        }

        var provider = OverlayLayerRegistry.Provider;
        if (provider is null)
        {
            return null;
        }

        var layers = provider.Invoke();
        if (layers is OverlayLayerCollection collection && collection.Count > 0)
        {
            AttachServiceLayers(collection);
            _serviceLayers = collection;
            return _serviceLayers;
        }

        var list = layers?.Where(layer => layer is not null).ToList() ?? new List<IOverlayLayer>();
        if (list.Count == 0)
        {
            return null;
        }

        var createdLayers = new OverlayLayerCollection();
        foreach (var layer in list)
        {
            createdLayers.Add(layer);
        }

        AttachServiceLayers(createdLayers);
        _serviceLayers = createdLayers;
        return _serviceLayers;
    }

    private void AttachServiceLayers(OverlayLayerCollection collection)
    {
        if (collection is INotifyCollectionChanged list)
        {
            list.CollectionChanged += OnServiceLayersCollectionChanged;
        }

        SubscribeLayerChanges(collection);
    }

    private void ResetServiceLayers()
    {
        if (_serviceLayers is null)
        {
            return;
        }

        if (_serviceLayers is INotifyCollectionChanged list)
        {
            list.CollectionChanged -= OnServiceLayersCollectionChanged;
        }

        UnsubscribeLayerChanges(_serviceLayers);
        _serviceLayers = null;
    }

    private void ApplyStyleKey(object? styleKey, Control overlay)
    {
        if (styleKey is null)
        {
            return;
        }

        if (this.TryFindResource(styleKey, out var value) && value is ControlTheme theme)
        {
            overlay.Theme = theme;
        }
    }

    private readonly struct OverlayLayerEntry
    {
        public OverlayLayerEntry(
            Control overlay,
            int zIndex,
            bool isVisible,
            bool blocksInput,
            object? styleKey)
        {
            Overlay = overlay;
            ZIndex = zIndex;
            IsVisible = isVisible;
            BlocksInput = blocksInput;
            StyleKey = styleKey;
        }

        public Control Overlay { get; }

        public int ZIndex { get; }

        public bool IsVisible { get; }

        public bool BlocksInput { get; }

        public object? StyleKey { get; }
    }
}
