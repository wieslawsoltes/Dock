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

    static OverlayHost()
    {
        ContentProperty.Changed.AddClassHandler<OverlayHost>((host, _) => host.RebuildPipeline());
        ContentTemplateProperty.Changed.AddClassHandler<OverlayHost>((host, _) => host.RebuildPipeline());
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
        if (_overlayLayers is INotifyCollectionChanged list)
        {
            list.CollectionChanged += OnOverlayLayersCollectionChanged;
        }

        SubscribeLayerChanges(_overlayLayers);
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
        OverlayLayerRegistry.ProviderChanged += OnOverlayLayerProviderChanged;
        ResetServiceLayers();
        RebuildPipeline();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        OverlayLayerRegistry.ProviderChanged -= OnOverlayLayerProviderChanged;
        ResetServiceLayers();
    }

    private void OnOverlaysChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (_overlays is INotifyCollectionChanged oldList)
        {
            oldList.CollectionChanged -= OnOverlaysCollectionChanged;
        }

        _overlays = args.NewValue as IList<Control> ?? new List<Control>();

        if (_overlays is INotifyCollectionChanged newList)
        {
            newList.CollectionChanged += OnOverlaysCollectionChanged;
        }

        RebuildPipeline();
    }

    private void OnOverlaysCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildPipeline();
    }

    private void OnOverlayLayersChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (_overlayLayers is INotifyCollectionChanged oldList)
        {
            oldList.CollectionChanged -= OnOverlayLayersCollectionChanged;
        }

        UnsubscribeLayerChanges(_overlayLayers);
        _overlayLayers = args.NewValue as OverlayLayerCollection ?? new OverlayLayerCollection();
        SubscribeLayerChanges(_overlayLayers);

        if (_overlayLayers is INotifyCollectionChanged newList)
        {
            newList.CollectionChanged += OnOverlayLayersCollectionChanged;
        }

        RebuildPipeline();
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

        RebuildPipeline();
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

        RebuildPipeline();
    }

    private void OnUseServiceLayersChanged()
    {
        ResetServiceLayers();
        RebuildPipeline();
    }

    private void OnOverlayLayerProviderChanged(object? sender, EventArgs e)
    {
        ResetServiceLayers();
        RebuildPipeline();
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
        RebuildPipeline();
    }

    private void RebuildPipeline()
    {
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

            if (!TryDetachOverlay(overlay))
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

    private static bool TryDetachOverlay(Control overlay)
    {
        if (overlay.Parent is null)
        {
            return true;
        }

        switch (overlay.Parent)
        {
            case Panel panel:
                panel.Children.Remove(overlay);
                return true;
            case Decorator decorator when ReferenceEquals(decorator.Child, overlay):
                decorator.Child = null;
                return true;
            case ContentPresenter presenter when ReferenceEquals(presenter.Content, overlay):
                presenter.Content = null;
                return true;
            case ContentControl contentControl when ReferenceEquals(contentControl.Content, overlay):
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
