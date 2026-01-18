using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using DockReactiveUICanonicalSample.Services;

namespace DockReactiveUICanonicalSample.Controls;

public sealed class BusyOverlayControl : TemplatedControl
{
    public static readonly StyledProperty<IBusyService?> BusyServiceProperty =
        AvaloniaProperty.Register<BusyOverlayControl, IBusyService?>(nameof(BusyService));

    public static readonly StyledProperty<IGlobalBusyService?> GlobalBusyServiceProperty =
        AvaloniaProperty.Register<BusyOverlayControl, IGlobalBusyService?>(nameof(GlobalBusyService));

    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<BusyOverlayControl, object?>(nameof(Content));

    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        AvaloniaProperty.Register<BusyOverlayControl, IDataTemplate?>(nameof(ContentTemplate));

    public static readonly DirectProperty<BusyOverlayControl, bool> IsBusyProperty =
        AvaloniaProperty.RegisterDirect<BusyOverlayControl, bool>(
            nameof(IsBusy),
            control => control.IsBusy);

    public static readonly DirectProperty<BusyOverlayControl, string?> BusyMessageProperty =
        AvaloniaProperty.RegisterDirect<BusyOverlayControl, string?>(
            nameof(BusyMessage),
            control => control.BusyMessage);

    public static readonly DirectProperty<BusyOverlayControl, bool> IsContentEnabledProperty =
        AvaloniaProperty.RegisterDirect<BusyOverlayControl, bool>(
            nameof(IsContentEnabled),
            control => control.IsContentEnabled);

    public static readonly DirectProperty<BusyOverlayControl, bool> ShowProgressProperty =
        AvaloniaProperty.RegisterDirect<BusyOverlayControl, bool>(
            nameof(ShowProgress),
            control => control.ShowProgress);

    public static readonly StyledProperty<bool?> ShowReloadButtonProperty =
        AvaloniaProperty.Register<BusyOverlayControl, bool?>(nameof(ShowReloadButton));

    public static readonly DirectProperty<BusyOverlayControl, bool> IsReloadVisibleProperty =
        AvaloniaProperty.RegisterDirect<BusyOverlayControl, bool>(
            nameof(IsReloadVisible),
            control => control.IsReloadVisible);

    public static readonly DirectProperty<BusyOverlayControl, bool> IsReloadEnabledProperty =
        AvaloniaProperty.RegisterDirect<BusyOverlayControl, bool>(
            nameof(IsReloadEnabled),
            control => control.IsReloadEnabled);

    public static readonly DirectProperty<BusyOverlayControl, ICommand?> ReloadCommandProperty =
        AvaloniaProperty.RegisterDirect<BusyOverlayControl, ICommand?>(
            nameof(ReloadCommand),
            control => control.ReloadCommand);

    private bool _isBusy;
    private string? _busyMessage;
    private bool _isContentEnabled = true;
    private bool _showProgress;
    private bool _isReloadVisible = true;
    private bool _isReloadEnabled;
    private ICommand? _reloadCommand;
    private IBusyService? _attachedService;
    private IGlobalBusyService? _attachedGlobalService;

    static BusyOverlayControl()
    {
        BusyServiceProperty.Changed.AddClassHandler<BusyOverlayControl>((control, args) =>
            control.OnBusyServiceChanged(args));
        GlobalBusyServiceProperty.Changed.AddClassHandler<BusyOverlayControl>((control, args) =>
            control.OnGlobalBusyServiceChanged(args));
        ShowReloadButtonProperty.Changed.AddClassHandler<BusyOverlayControl>((control, _) =>
            control.SyncFromServices());
    }

    public IBusyService? BusyService
    {
        get => GetValue(BusyServiceProperty);
        set => SetValue(BusyServiceProperty, value);
    }

    public IGlobalBusyService? GlobalBusyService
    {
        get => GetValue(GlobalBusyServiceProperty);
        set => SetValue(GlobalBusyServiceProperty, value);
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

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetAndRaise(IsBusyProperty, ref _isBusy, value);
    }

    public string? BusyMessage
    {
        get => _busyMessage;
        private set => SetAndRaise(BusyMessageProperty, ref _busyMessage, value);
    }

    public bool IsContentEnabled
    {
        get => _isContentEnabled;
        private set => SetAndRaise(IsContentEnabledProperty, ref _isContentEnabled, value);
    }

    public bool ShowProgress
    {
        get => _showProgress;
        private set => SetAndRaise(ShowProgressProperty, ref _showProgress, value);
    }

    public bool? ShowReloadButton
    {
        get => GetValue(ShowReloadButtonProperty);
        set => SetValue(ShowReloadButtonProperty, value);
    }

    public bool IsReloadVisible
    {
        get => _isReloadVisible;
        private set => SetAndRaise(IsReloadVisibleProperty, ref _isReloadVisible, value);
    }

    public bool IsReloadEnabled
    {
        get => _isReloadEnabled;
        private set => SetAndRaise(IsReloadEnabledProperty, ref _isReloadEnabled, value);
    }

    public ICommand? ReloadCommand
    {
        get => _reloadCommand;
        private set => SetAndRaise(ReloadCommandProperty, ref _reloadCommand, value);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (_attachedService is not null)
        {
            _attachedService.PropertyChanged -= OnBusyServicePropertyChanged;
            _attachedService = null;
        }

        if (_attachedGlobalService is not null)
        {
            _attachedGlobalService.PropertyChanged -= OnGlobalBusyServicePropertyChanged;
            _attachedGlobalService = null;
        }
    }

    private void OnBusyServiceChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (_attachedService is not null)
        {
            _attachedService.PropertyChanged -= OnBusyServicePropertyChanged;
        }

        _attachedService = args.NewValue as IBusyService;

        if (_attachedService is not null)
        {
            _attachedService.PropertyChanged += OnBusyServicePropertyChanged;
        }

        SyncFromServices();
    }

    private void OnGlobalBusyServiceChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (_attachedGlobalService is not null)
        {
            _attachedGlobalService.PropertyChanged -= OnGlobalBusyServicePropertyChanged;
        }

        _attachedGlobalService = args.NewValue as IGlobalBusyService;

        if (_attachedGlobalService is not null)
        {
            _attachedGlobalService.PropertyChanged += OnGlobalBusyServicePropertyChanged;
        }

        SyncFromServices();
    }

    private void OnBusyServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IBusyService.IsBusy)
            || e.PropertyName == nameof(IBusyService.Message)
            || e.PropertyName == nameof(IBusyService.IsReloadVisible)
            || e.PropertyName == nameof(IBusyService.CanReload))
        {
            SyncFromServices();
        }
    }

    private void OnGlobalBusyServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IGlobalBusyService.IsBusy)
            || e.PropertyName == nameof(IGlobalBusyService.Message))
        {
            SyncFromServices();
        }
    }

    private void SyncFromServices()
    {
        var localBusy = _attachedService?.IsBusy ?? false;
        var globalBusy = _attachedGlobalService?.IsBusy ?? false;
        var isBusy = localBusy || globalBusy;
        var showReload = ShowReloadButton ?? _attachedService?.IsReloadVisible ?? true;

        IsBusy = isBusy;
        ShowProgress = localBusy;
        BusyMessage = localBusy
            ? _attachedService?.Message
            : _attachedGlobalService?.Message;
        IsContentEnabled = !isBusy;
        ReloadCommand = _attachedService?.ReloadCommand;
        IsReloadVisible = showReload;
        IsReloadEnabled = !isBusy && (_attachedService?.CanReload ?? false);
    }
}
