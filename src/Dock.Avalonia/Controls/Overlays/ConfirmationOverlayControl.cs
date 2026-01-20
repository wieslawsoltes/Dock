using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using Dock.Model.Services;

namespace Dock.Avalonia.Controls.Overlays;

/// <summary>
/// Displays confirmation overlays for hosted content.
/// </summary>
public sealed class ConfirmationOverlayControl : TemplatedControl, IOverlayContentHost, IVisualTreeLifecycle
{
    /// <summary>
    /// Defines the <see cref="ConfirmationService"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<IDockConfirmationService?> ConfirmationServiceProperty =
        AvaloniaProperty.Register<ConfirmationOverlayControl, IDockConfirmationService?>(nameof(ConfirmationService));

    /// <summary>
    /// Defines the <see cref="GlobalConfirmationService"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<IDockGlobalConfirmationService?> GlobalConfirmationServiceProperty =
        AvaloniaProperty.Register<ConfirmationOverlayControl, IDockGlobalConfirmationService?>(nameof(GlobalConfirmationService));

    /// <summary>
    /// Defines the <see cref="Content"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<ConfirmationOverlayControl, object?>(nameof(Content));

    /// <summary>
    /// Defines the <see cref="ContentTemplate"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        AvaloniaProperty.Register<ConfirmationOverlayControl, IDataTemplate?>(nameof(ContentTemplate));

    /// <summary>
    /// Defines the <see cref="BlocksInput"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<bool> BlocksInputProperty =
        AvaloniaProperty.Register<ConfirmationOverlayControl, bool>(nameof(BlocksInput), true);

    /// <summary>
    /// Defines the <see cref="IsOverlayVisible"/> direct property.
    /// </summary>
    public static readonly DirectProperty<ConfirmationOverlayControl, bool> IsOverlayVisibleProperty =
        AvaloniaProperty.RegisterDirect<ConfirmationOverlayControl, bool>(
            nameof(IsOverlayVisible),
            control => control.IsOverlayVisible);

    /// <summary>
    /// Defines the <see cref="ShowConfirmations"/> direct property.
    /// </summary>
    public static readonly DirectProperty<ConfirmationOverlayControl, bool> ShowConfirmationsProperty =
        AvaloniaProperty.RegisterDirect<ConfirmationOverlayControl, bool>(
            nameof(ShowConfirmations),
            control => control.ShowConfirmations);

    /// <summary>
    /// Defines the <see cref="ShowGlobalMessage"/> direct property.
    /// </summary>
    public static readonly DirectProperty<ConfirmationOverlayControl, bool> ShowGlobalMessageProperty =
        AvaloniaProperty.RegisterDirect<ConfirmationOverlayControl, bool>(
            nameof(ShowGlobalMessage),
            control => control.ShowGlobalMessage);

    /// <summary>
    /// Defines the <see cref="OverlayMessage"/> direct property.
    /// </summary>
    public static readonly DirectProperty<ConfirmationOverlayControl, string?> OverlayMessageProperty =
        AvaloniaProperty.RegisterDirect<ConfirmationOverlayControl, string?>(
            nameof(OverlayMessage),
            control => control.OverlayMessage);

    /// <summary>
    /// Defines the <see cref="IsContentEnabled"/> direct property.
    /// </summary>
    public static readonly DirectProperty<ConfirmationOverlayControl, bool> IsContentEnabledProperty =
        AvaloniaProperty.RegisterDirect<ConfirmationOverlayControl, bool>(
            nameof(IsContentEnabled),
            control => control.IsContentEnabled);

    /// <summary>
    /// Defines the <see cref="Confirmations"/> direct property.
    /// </summary>
    public static readonly DirectProperty<ConfirmationOverlayControl, ReadOnlyObservableCollection<ConfirmationRequest>?> ConfirmationsProperty =
        AvaloniaProperty.RegisterDirect<ConfirmationOverlayControl, ReadOnlyObservableCollection<ConfirmationRequest>?>(
            nameof(Confirmations),
            control => control.Confirmations);

    private bool _isOverlayVisible;
    private bool _showConfirmations;
    private bool _showGlobalMessage;
    private string? _overlayMessage;
    private bool _isContentEnabled = true;
    private ReadOnlyObservableCollection<ConfirmationRequest>? _confirmations;
    private IDockConfirmationService? _attachedConfirmationService;
    private IDockGlobalConfirmationService? _attachedGlobalConfirmationService;

    static ConfirmationOverlayControl()
    {
        ConfirmationServiceProperty.Changed.AddClassHandler<ConfirmationOverlayControl>((control, args) =>
            control.OnConfirmationServiceChanged(args));
        GlobalConfirmationServiceProperty.Changed.AddClassHandler<ConfirmationOverlayControl>((control, args) =>
            control.OnGlobalConfirmationServiceChanged(args));
        BlocksInputProperty.Changed.AddClassHandler<ConfirmationOverlayControl>((control, _) =>
            control.SyncFromServices());
    }

    /// <summary>
    /// Gets or sets the per-host confirmation service.
    /// </summary>
    public IDockConfirmationService? ConfirmationService
    {
        get => GetValue(ConfirmationServiceProperty);
        set => SetValue(ConfirmationServiceProperty, value);
    }

    /// <summary>
    /// Gets or sets the global confirmation service.
    /// </summary>
    public IDockGlobalConfirmationService? GlobalConfirmationService
    {
        get => GetValue(GlobalConfirmationServiceProperty);
        set => SetValue(GlobalConfirmationServiceProperty, value);
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
    /// Gets or sets a value indicating whether the overlay blocks input to the hosted content.
    /// </summary>
    public bool BlocksInput
    {
        get => GetValue(BlocksInputProperty);
        set => SetValue(BlocksInputProperty, value);
    }

    /// <summary>
    /// Gets a value indicating whether the overlay is visible.
    /// </summary>
    public bool IsOverlayVisible
    {
        get => _isOverlayVisible;
        private set => SetAndRaise(IsOverlayVisibleProperty, ref _isOverlayVisible, value);
    }

    /// <summary>
    /// Gets a value indicating whether local confirmations are shown.
    /// </summary>
    public bool ShowConfirmations
    {
        get => _showConfirmations;
        private set => SetAndRaise(ShowConfirmationsProperty, ref _showConfirmations, value);
    }

    /// <summary>
    /// Gets a value indicating whether the global message is shown.
    /// </summary>
    public bool ShowGlobalMessage
    {
        get => _showGlobalMessage;
        private set => SetAndRaise(ShowGlobalMessageProperty, ref _showGlobalMessage, value);
    }

    /// <summary>
    /// Gets the overlay message to display.
    /// </summary>
    public string? OverlayMessage
    {
        get => _overlayMessage;
        private set => SetAndRaise(OverlayMessageProperty, ref _overlayMessage, value);
    }

    /// <summary>
    /// Gets a value indicating whether the underlying content is enabled.
    /// </summary>
    public bool IsContentEnabled
    {
        get => _isContentEnabled;
        private set => SetAndRaise(IsContentEnabledProperty, ref _isContentEnabled, value);
    }

    /// <summary>
    /// Gets the confirmation requests bound to the overlay.
    /// </summary>
    public ReadOnlyObservableCollection<ConfirmationRequest>? Confirmations
    {
        get => _confirmations;
        private set => SetAndRaise(ConfirmationsProperty, ref _confirmations, value);
    }

    /// <summary>
    /// Detaches service subscriptions when the control leaves the visual tree.
    /// </summary>
    /// <param name="e">The visual tree attachment event arguments.</param>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        OnAttachedToVisualTree();
    }

    /// <summary>
    /// Detaches service subscriptions when the control leaves the visual tree.
    /// </summary>
    /// <param name="e">The visual tree attachment event arguments.</param>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        OnDetachedFromVisualTree();
    }

    /// <inheritdoc />
    public void OnAttachedToVisualTree()
    {
        RebindServices();
    }

    /// <inheritdoc />
    public void OnDetachedFromVisualTree()
    {
        DetachServices();
    }

    private void OnConfirmationServiceChanged(AvaloniaPropertyChangedEventArgs args)
    {
        AttachService(args.NewValue as IDockConfirmationService, ref _attachedConfirmationService, OnConfirmationServicePropertyChanged);
        Confirmations = _attachedConfirmationService?.Confirmations;
        SyncFromServices();
    }

    private void OnGlobalConfirmationServiceChanged(AvaloniaPropertyChangedEventArgs args)
    {
        AttachService(args.NewValue as IDockGlobalConfirmationService, ref _attachedGlobalConfirmationService, OnGlobalConfirmationServicePropertyChanged);

        SyncFromServices();
    }

    private void OnConfirmationServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IDockConfirmationService.HasConfirmations)
            || e.PropertyName == nameof(IDockConfirmationService.ActiveConfirmation))
        {
            SyncFromServices();
        }
    }

    private void OnGlobalConfirmationServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IDockGlobalConfirmationService.IsConfirmationOpen)
            || e.PropertyName == nameof(IDockGlobalConfirmationService.Message))
        {
            SyncFromServices();
        }
    }

    private void SyncFromServices()
    {
        var hasLocalConfirmations = _attachedConfirmationService?.HasConfirmations ?? false;
        var hasGlobalConfirmation = _attachedGlobalConfirmationService?.IsConfirmationOpen ?? false;
        var overlayVisible = hasLocalConfirmations || hasGlobalConfirmation;

        IsOverlayVisible = overlayVisible;
        ShowConfirmations = hasLocalConfirmations;
        ShowGlobalMessage = hasGlobalConfirmation && !hasLocalConfirmations;
        OverlayMessage = hasLocalConfirmations
            ? null
            : _attachedGlobalConfirmationService?.Message;
        IsContentEnabled = !(overlayVisible && BlocksInput);
    }

    private void RebindServices()
    {
        AttachService(ConfirmationService, ref _attachedConfirmationService, OnConfirmationServicePropertyChanged);
        AttachService(GlobalConfirmationService, ref _attachedGlobalConfirmationService, OnGlobalConfirmationServicePropertyChanged);
        Confirmations = _attachedConfirmationService?.Confirmations;
        SyncFromServices();
    }

    private void DetachServices()
    {
        DetachService(ref _attachedConfirmationService, OnConfirmationServicePropertyChanged);
        DetachService(ref _attachedGlobalConfirmationService, OnGlobalConfirmationServicePropertyChanged);
        Confirmations = null;
    }

    private static void AttachService<TService>(
        TService? service,
        ref TService? attached,
        PropertyChangedEventHandler handler)
        where TService : class, INotifyPropertyChanged
    {
        if (ReferenceEquals(attached, service))
        {
            return;
        }

        if (attached is not null)
        {
            attached.PropertyChanged -= handler;
        }

        attached = service;

        if (attached is not null)
        {
            attached.PropertyChanged += handler;
        }
    }

    private static void DetachService<TService>(ref TService? attached, PropertyChangedEventHandler handler)
        where TService : class, INotifyPropertyChanged
    {
        if (attached is null)
        {
            return;
        }

        attached.PropertyChanged -= handler;
        attached = null;
    }
}
