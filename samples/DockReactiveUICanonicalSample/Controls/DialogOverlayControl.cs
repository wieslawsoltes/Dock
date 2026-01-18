using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using DockReactiveUICanonicalSample.Services;

namespace DockReactiveUICanonicalSample.Controls;

public sealed class DialogOverlayControl : TemplatedControl
{
    public static readonly StyledProperty<IDialogService?> DialogServiceProperty =
        AvaloniaProperty.Register<DialogOverlayControl, IDialogService?>(nameof(DialogService));

    public static readonly StyledProperty<IGlobalDialogService?> GlobalDialogServiceProperty =
        AvaloniaProperty.Register<DialogOverlayControl, IGlobalDialogService?>(nameof(GlobalDialogService));

    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<DialogOverlayControl, object?>(nameof(Content));

    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        AvaloniaProperty.Register<DialogOverlayControl, IDataTemplate?>(nameof(ContentTemplate));

    public static readonly DirectProperty<DialogOverlayControl, bool> IsOverlayVisibleProperty =
        AvaloniaProperty.RegisterDirect<DialogOverlayControl, bool>(
            nameof(IsOverlayVisible),
            control => control.IsOverlayVisible);

    public static readonly DirectProperty<DialogOverlayControl, bool> ShowDialogsProperty =
        AvaloniaProperty.RegisterDirect<DialogOverlayControl, bool>(
            nameof(ShowDialogs),
            control => control.ShowDialogs);

    public static readonly DirectProperty<DialogOverlayControl, bool> ShowGlobalMessageProperty =
        AvaloniaProperty.RegisterDirect<DialogOverlayControl, bool>(
            nameof(ShowGlobalMessage),
            control => control.ShowGlobalMessage);

    public static readonly DirectProperty<DialogOverlayControl, string?> OverlayMessageProperty =
        AvaloniaProperty.RegisterDirect<DialogOverlayControl, string?>(
            nameof(OverlayMessage),
            control => control.OverlayMessage);

    public static readonly DirectProperty<DialogOverlayControl, bool> IsContentEnabledProperty =
        AvaloniaProperty.RegisterDirect<DialogOverlayControl, bool>(
            nameof(IsContentEnabled),
            control => control.IsContentEnabled);

    public static readonly DirectProperty<DialogOverlayControl, ReadOnlyObservableCollection<DialogRequest>?> DialogsProperty =
        AvaloniaProperty.RegisterDirect<DialogOverlayControl, ReadOnlyObservableCollection<DialogRequest>?>(
            nameof(Dialogs),
            control => control.Dialogs);

    private bool _isOverlayVisible;
    private bool _showDialogs;
    private bool _showGlobalMessage;
    private string? _overlayMessage;
    private bool _isContentEnabled = true;
    private ReadOnlyObservableCollection<DialogRequest>? _dialogs;
    private IDialogService? _attachedDialogService;
    private IGlobalDialogService? _attachedGlobalDialogService;

    static DialogOverlayControl()
    {
        DialogServiceProperty.Changed.AddClassHandler<DialogOverlayControl>((control, args) =>
            control.OnDialogServiceChanged(args));
        GlobalDialogServiceProperty.Changed.AddClassHandler<DialogOverlayControl>((control, args) =>
            control.OnGlobalDialogServiceChanged(args));
    }

    public IDialogService? DialogService
    {
        get => GetValue(DialogServiceProperty);
        set => SetValue(DialogServiceProperty, value);
    }

    public IGlobalDialogService? GlobalDialogService
    {
        get => GetValue(GlobalDialogServiceProperty);
        set => SetValue(GlobalDialogServiceProperty, value);
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

    public bool ShowDialogs
    {
        get => _showDialogs;
        private set => SetAndRaise(ShowDialogsProperty, ref _showDialogs, value);
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

    public ReadOnlyObservableCollection<DialogRequest>? Dialogs
    {
        get => _dialogs;
        private set => SetAndRaise(DialogsProperty, ref _dialogs, value);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (_attachedDialogService is not null)
        {
            _attachedDialogService.PropertyChanged -= OnDialogServicePropertyChanged;
            _attachedDialogService = null;
        }

        if (_attachedGlobalDialogService is not null)
        {
            _attachedGlobalDialogService.PropertyChanged -= OnGlobalDialogServicePropertyChanged;
            _attachedGlobalDialogService = null;
        }

        Dialogs = null;
    }

    private void OnDialogServiceChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (_attachedDialogService is not null)
        {
            _attachedDialogService.PropertyChanged -= OnDialogServicePropertyChanged;
        }

        _attachedDialogService = args.NewValue as IDialogService;

        if (_attachedDialogService is not null)
        {
            _attachedDialogService.PropertyChanged += OnDialogServicePropertyChanged;
        }

        Dialogs = _attachedDialogService?.Dialogs;
        SyncFromServices();
    }

    private void OnGlobalDialogServiceChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (_attachedGlobalDialogService is not null)
        {
            _attachedGlobalDialogService.PropertyChanged -= OnGlobalDialogServicePropertyChanged;
        }

        _attachedGlobalDialogService = args.NewValue as IGlobalDialogService;

        if (_attachedGlobalDialogService is not null)
        {
            _attachedGlobalDialogService.PropertyChanged += OnGlobalDialogServicePropertyChanged;
        }

        SyncFromServices();
    }

    private void OnDialogServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IDialogService.HasDialogs)
            || e.PropertyName == nameof(IDialogService.ActiveDialog))
        {
            SyncFromServices();
        }
    }

    private void OnGlobalDialogServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IGlobalDialogService.IsDialogOpen)
            || e.PropertyName == nameof(IGlobalDialogService.Message))
        {
            SyncFromServices();
        }
    }

    private void SyncFromServices()
    {
        var hasLocalDialogs = _attachedDialogService?.HasDialogs ?? false;
        var hasGlobalDialog = _attachedGlobalDialogService?.IsDialogOpen ?? false;
        var overlayVisible = hasLocalDialogs || hasGlobalDialog;

        IsOverlayVisible = overlayVisible;
        ShowDialogs = hasLocalDialogs;
        ShowGlobalMessage = hasGlobalDialog && !hasLocalDialogs;
        OverlayMessage = hasLocalDialogs
            ? null
            : _attachedGlobalDialogService?.Message;
        IsContentEnabled = !overlayVisible;
    }
}
