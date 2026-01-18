using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using Dock.Model.Services;

namespace Dock.Avalonia.Controls.Overlays;

/// <summary>
/// Displays busy and reload overlays for hosted content.
/// </summary>
public sealed class BusyOverlayControl : TemplatedControl, IOverlayContentHost
{
    /// <summary>
    /// Defines the <see cref="BusyService"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<IDockBusyService?> BusyServiceProperty =
        AvaloniaProperty.Register<BusyOverlayControl, IDockBusyService?>(nameof(BusyService));

    /// <summary>
    /// Defines the <see cref="GlobalBusyService"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<IDockGlobalBusyService?> GlobalBusyServiceProperty =
        AvaloniaProperty.Register<BusyOverlayControl, IDockGlobalBusyService?>(nameof(GlobalBusyService));

    /// <summary>
    /// Defines the <see cref="Content"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<BusyOverlayControl, object?>(nameof(Content));

    /// <summary>
    /// Defines the <see cref="ContentTemplate"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        AvaloniaProperty.Register<BusyOverlayControl, IDataTemplate?>(nameof(ContentTemplate));

    /// <summary>
    /// Defines the <see cref="IsBusy"/> direct property.
    /// </summary>
    public static readonly DirectProperty<BusyOverlayControl, bool> IsBusyProperty =
        AvaloniaProperty.RegisterDirect<BusyOverlayControl, bool>(
            nameof(IsBusy),
            control => control.IsBusy);

    /// <summary>
    /// Defines the <see cref="BusyMessage"/> direct property.
    /// </summary>
    public static readonly DirectProperty<BusyOverlayControl, string?> BusyMessageProperty =
        AvaloniaProperty.RegisterDirect<BusyOverlayControl, string?>(
            nameof(BusyMessage),
            control => control.BusyMessage);

    /// <summary>
    /// Defines the <see cref="IsContentEnabled"/> direct property.
    /// </summary>
    public static readonly DirectProperty<BusyOverlayControl, bool> IsContentEnabledProperty =
        AvaloniaProperty.RegisterDirect<BusyOverlayControl, bool>(
            nameof(IsContentEnabled),
            control => control.IsContentEnabled);

    /// <summary>
    /// Defines the <see cref="ShowProgress"/> direct property.
    /// </summary>
    public static readonly DirectProperty<BusyOverlayControl, bool> ShowProgressProperty =
        AvaloniaProperty.RegisterDirect<BusyOverlayControl, bool>(
            nameof(ShowProgress),
            control => control.ShowProgress);

    /// <summary>
    /// Defines the <see cref="ShowReloadButton"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<bool?> ShowReloadButtonProperty =
        AvaloniaProperty.Register<BusyOverlayControl, bool?>(nameof(ShowReloadButton));

    /// <summary>
    /// Defines the <see cref="IsReloadVisible"/> direct property.
    /// </summary>
    public static readonly DirectProperty<BusyOverlayControl, bool> IsReloadVisibleProperty =
        AvaloniaProperty.RegisterDirect<BusyOverlayControl, bool>(
            nameof(IsReloadVisible),
            control => control.IsReloadVisible);

    /// <summary>
    /// Defines the <see cref="IsReloadEnabled"/> direct property.
    /// </summary>
    public static readonly DirectProperty<BusyOverlayControl, bool> IsReloadEnabledProperty =
        AvaloniaProperty.RegisterDirect<BusyOverlayControl, bool>(
            nameof(IsReloadEnabled),
            control => control.IsReloadEnabled);

    /// <summary>
    /// Defines the <see cref="ReloadCommand"/> direct property.
    /// </summary>
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
    private IDockBusyService? _attachedService;
    private IDockGlobalBusyService? _attachedGlobalService;

    static BusyOverlayControl()
    {
        BusyServiceProperty.Changed.AddClassHandler<BusyOverlayControl>((control, args) =>
            control.OnBusyServiceChanged(args));
        GlobalBusyServiceProperty.Changed.AddClassHandler<BusyOverlayControl>((control, args) =>
            control.OnGlobalBusyServiceChanged(args));
        ShowReloadButtonProperty.Changed.AddClassHandler<BusyOverlayControl>((control, _) =>
            control.SyncFromServices());
    }

    /// <summary>
    /// Gets or sets the per-host busy service.
    /// </summary>
    public IDockBusyService? BusyService
    {
        get => GetValue(BusyServiceProperty);
        set => SetValue(BusyServiceProperty, value);
    }

    /// <summary>
    /// Gets or sets the global busy service.
    /// </summary>
    public IDockGlobalBusyService? GlobalBusyService
    {
        get => GetValue(GlobalBusyServiceProperty);
        set => SetValue(GlobalBusyServiceProperty, value);
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
    /// Gets a value indicating whether the busy overlay is visible.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        private set => SetAndRaise(IsBusyProperty, ref _isBusy, value);
    }

    /// <summary>
    /// Gets the busy message to display.
    /// </summary>
    public string? BusyMessage
    {
        get => _busyMessage;
        private set => SetAndRaise(BusyMessageProperty, ref _busyMessage, value);
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
    /// Gets a value indicating whether the progress indicator is visible.
    /// </summary>
    public bool ShowProgress
    {
        get => _showProgress;
        private set => SetAndRaise(ShowProgressProperty, ref _showProgress, value);
    }

    /// <summary>
    /// Gets or sets an explicit reload button visibility override.
    /// </summary>
    public bool? ShowReloadButton
    {
        get => GetValue(ShowReloadButtonProperty);
        set => SetValue(ShowReloadButtonProperty, value);
    }

    /// <summary>
    /// Gets a value indicating whether the reload button is visible.
    /// </summary>
    public bool IsReloadVisible
    {
        get => _isReloadVisible;
        private set => SetAndRaise(IsReloadVisibleProperty, ref _isReloadVisible, value);
    }

    /// <summary>
    /// Gets a value indicating whether the reload button is enabled.
    /// </summary>
    public bool IsReloadEnabled
    {
        get => _isReloadEnabled;
        private set => SetAndRaise(IsReloadEnabledProperty, ref _isReloadEnabled, value);
    }

    /// <summary>
    /// Gets the reload command.
    /// </summary>
    public ICommand? ReloadCommand
    {
        get => _reloadCommand;
        private set => SetAndRaise(ReloadCommandProperty, ref _reloadCommand, value);
    }

    /// <summary>
    /// Detaches service subscriptions when the control leaves the visual tree.
    /// </summary>
    /// <param name="e">The visual tree attachment event arguments.</param>
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

        _attachedService = args.NewValue as IDockBusyService;

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

        _attachedGlobalService = args.NewValue as IDockGlobalBusyService;

        if (_attachedGlobalService is not null)
        {
            _attachedGlobalService.PropertyChanged += OnGlobalBusyServicePropertyChanged;
        }

        SyncFromServices();
    }

    private void OnBusyServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IDockBusyService.IsBusy)
            || e.PropertyName == nameof(IDockBusyService.Message)
            || e.PropertyName == nameof(IDockBusyService.IsReloadVisible)
            || e.PropertyName == nameof(IDockBusyService.CanReload))
        {
            SyncFromServices();
        }
    }

    private void OnGlobalBusyServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IDockGlobalBusyService.IsBusy)
            || e.PropertyName == nameof(IDockGlobalBusyService.Message))
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
