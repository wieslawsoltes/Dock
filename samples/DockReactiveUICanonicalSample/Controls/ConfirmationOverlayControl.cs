using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using DockReactiveUICanonicalSample.Services;

namespace DockReactiveUICanonicalSample.Controls;

public sealed class ConfirmationOverlayControl : TemplatedControl
{
    public static readonly StyledProperty<IConfirmationService?> ConfirmationServiceProperty =
        AvaloniaProperty.Register<ConfirmationOverlayControl, IConfirmationService?>(nameof(ConfirmationService));

    public static readonly StyledProperty<IGlobalConfirmationService?> GlobalConfirmationServiceProperty =
        AvaloniaProperty.Register<ConfirmationOverlayControl, IGlobalConfirmationService?>(nameof(GlobalConfirmationService));

    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<ConfirmationOverlayControl, object?>(nameof(Content));

    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        AvaloniaProperty.Register<ConfirmationOverlayControl, IDataTemplate?>(nameof(ContentTemplate));

    public static readonly DirectProperty<ConfirmationOverlayControl, bool> IsOverlayVisibleProperty =
        AvaloniaProperty.RegisterDirect<ConfirmationOverlayControl, bool>(
            nameof(IsOverlayVisible),
            control => control.IsOverlayVisible);

    public static readonly DirectProperty<ConfirmationOverlayControl, bool> ShowConfirmationsProperty =
        AvaloniaProperty.RegisterDirect<ConfirmationOverlayControl, bool>(
            nameof(ShowConfirmations),
            control => control.ShowConfirmations);

    public static readonly DirectProperty<ConfirmationOverlayControl, bool> ShowGlobalMessageProperty =
        AvaloniaProperty.RegisterDirect<ConfirmationOverlayControl, bool>(
            nameof(ShowGlobalMessage),
            control => control.ShowGlobalMessage);

    public static readonly DirectProperty<ConfirmationOverlayControl, string?> OverlayMessageProperty =
        AvaloniaProperty.RegisterDirect<ConfirmationOverlayControl, string?>(
            nameof(OverlayMessage),
            control => control.OverlayMessage);

    public static readonly DirectProperty<ConfirmationOverlayControl, bool> IsContentEnabledProperty =
        AvaloniaProperty.RegisterDirect<ConfirmationOverlayControl, bool>(
            nameof(IsContentEnabled),
            control => control.IsContentEnabled);

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
    private IConfirmationService? _attachedConfirmationService;
    private IGlobalConfirmationService? _attachedGlobalConfirmationService;

    static ConfirmationOverlayControl()
    {
        ConfirmationServiceProperty.Changed.AddClassHandler<ConfirmationOverlayControl>((control, args) =>
            control.OnConfirmationServiceChanged(args));
        GlobalConfirmationServiceProperty.Changed.AddClassHandler<ConfirmationOverlayControl>((control, args) =>
            control.OnGlobalConfirmationServiceChanged(args));
    }

    public IConfirmationService? ConfirmationService
    {
        get => GetValue(ConfirmationServiceProperty);
        set => SetValue(ConfirmationServiceProperty, value);
    }

    public IGlobalConfirmationService? GlobalConfirmationService
    {
        get => GetValue(GlobalConfirmationServiceProperty);
        set => SetValue(GlobalConfirmationServiceProperty, value);
    }

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    public bool IsOverlayVisible
    {
        get => _isOverlayVisible;
        private set => SetAndRaise(IsOverlayVisibleProperty, ref _isOverlayVisible, value);
    }

    public bool ShowConfirmations
    {
        get => _showConfirmations;
        private set => SetAndRaise(ShowConfirmationsProperty, ref _showConfirmations, value);
    }

    public bool ShowGlobalMessage
    {
        get => _showGlobalMessage;
        private set => SetAndRaise(ShowGlobalMessageProperty, ref _showGlobalMessage, value);
    }

    public string? OverlayMessage
    {
        get => _overlayMessage;
        private set => SetAndRaise(OverlayMessageProperty, ref _overlayMessage, value);
    }

    public bool IsContentEnabled
    {
        get => _isContentEnabled;
        private set => SetAndRaise(IsContentEnabledProperty, ref _isContentEnabled, value);
    }

    public ReadOnlyObservableCollection<ConfirmationRequest>? Confirmations
    {
        get => _confirmations;
        private set => SetAndRaise(ConfirmationsProperty, ref _confirmations, value);
    }

    private void OnConfirmationServiceChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (_attachedConfirmationService is not null)
        {
            _attachedConfirmationService.PropertyChanged -= OnConfirmationServicePropertyChanged;
        }

        _attachedConfirmationService = args.NewValue as IConfirmationService;

        if (_attachedConfirmationService is not null)
        {
            _attachedConfirmationService.PropertyChanged += OnConfirmationServicePropertyChanged;
        }

        Confirmations = _attachedConfirmationService?.Confirmations;
        SyncFromServices();
    }

    private void OnGlobalConfirmationServiceChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (_attachedGlobalConfirmationService is not null)
        {
            _attachedGlobalConfirmationService.PropertyChanged -= OnGlobalConfirmationServicePropertyChanged;
        }

        _attachedGlobalConfirmationService = args.NewValue as IGlobalConfirmationService;

        if (_attachedGlobalConfirmationService is not null)
        {
            _attachedGlobalConfirmationService.PropertyChanged += OnGlobalConfirmationServicePropertyChanged;
        }

        SyncFromServices();
    }

    private void OnConfirmationServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IConfirmationService.HasConfirmations)
            || e.PropertyName == nameof(IConfirmationService.ActiveConfirmation))
        {
            SyncFromServices();
        }
    }

    private void OnGlobalConfirmationServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IGlobalConfirmationService.IsConfirmationOpen)
            || e.PropertyName == nameof(IGlobalConfirmationService.Message))
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
        IsContentEnabled = !overlayVisible;
    }
}
