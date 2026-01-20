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
/// Displays dialog overlays for hosted content.
/// </summary>
public sealed class DialogOverlayControl : TemplatedControl, IOverlayContentHost, IVisualTreeLifecycle
{
    /// <summary>
    /// Defines the <see cref="DialogService"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<IDockDialogService?> DialogServiceProperty =
        AvaloniaProperty.Register<DialogOverlayControl, IDockDialogService?>(nameof(DialogService));

    /// <summary>
    /// Defines the <see cref="GlobalDialogService"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<IDockGlobalDialogService?> GlobalDialogServiceProperty =
        AvaloniaProperty.Register<DialogOverlayControl, IDockGlobalDialogService?>(nameof(GlobalDialogService));

    /// <summary>
    /// Defines the <see cref="Content"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<DialogOverlayControl, object?>(nameof(Content));

    /// <summary>
    /// Defines the <see cref="ContentTemplate"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        AvaloniaProperty.Register<DialogOverlayControl, IDataTemplate?>(nameof(ContentTemplate));

    /// <summary>
    /// Defines the <see cref="BlocksInput"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<bool> BlocksInputProperty =
        AvaloniaProperty.Register<DialogOverlayControl, bool>(nameof(BlocksInput), true);

    /// <summary>
    /// Defines the <see cref="IsOverlayVisible"/> direct property.
    /// </summary>
    public static readonly DirectProperty<DialogOverlayControl, bool> IsOverlayVisibleProperty =
        AvaloniaProperty.RegisterDirect<DialogOverlayControl, bool>(
            nameof(IsOverlayVisible),
            control => control.IsOverlayVisible);

    /// <summary>
    /// Defines the <see cref="ShowDialogs"/> direct property.
    /// </summary>
    public static readonly DirectProperty<DialogOverlayControl, bool> ShowDialogsProperty =
        AvaloniaProperty.RegisterDirect<DialogOverlayControl, bool>(
            nameof(ShowDialogs),
            control => control.ShowDialogs);

    /// <summary>
    /// Defines the <see cref="ShowGlobalMessage"/> direct property.
    /// </summary>
    public static readonly DirectProperty<DialogOverlayControl, bool> ShowGlobalMessageProperty =
        AvaloniaProperty.RegisterDirect<DialogOverlayControl, bool>(
            nameof(ShowGlobalMessage),
            control => control.ShowGlobalMessage);

    /// <summary>
    /// Defines the <see cref="OverlayMessage"/> direct property.
    /// </summary>
    public static readonly DirectProperty<DialogOverlayControl, string?> OverlayMessageProperty =
        AvaloniaProperty.RegisterDirect<DialogOverlayControl, string?>(
            nameof(OverlayMessage),
            control => control.OverlayMessage);

    /// <summary>
    /// Defines the <see cref="IsContentEnabled"/> direct property.
    /// </summary>
    public static readonly DirectProperty<DialogOverlayControl, bool> IsContentEnabledProperty =
        AvaloniaProperty.RegisterDirect<DialogOverlayControl, bool>(
            nameof(IsContentEnabled),
            control => control.IsContentEnabled);

    /// <summary>
    /// Defines the <see cref="Dialogs"/> direct property.
    /// </summary>
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
    private IDockDialogService? _attachedDialogService;
    private IDockGlobalDialogService? _attachedGlobalDialogService;

    static DialogOverlayControl()
    {
        DialogServiceProperty.Changed.AddClassHandler<DialogOverlayControl>((control, args) =>
            control.OnDialogServiceChanged(args));
        GlobalDialogServiceProperty.Changed.AddClassHandler<DialogOverlayControl>((control, args) =>
            control.OnGlobalDialogServiceChanged(args));
        BlocksInputProperty.Changed.AddClassHandler<DialogOverlayControl>((control, _) =>
            control.SyncFromServices());
    }

    /// <summary>
    /// Gets or sets the per-host dialog service.
    /// </summary>
    public IDockDialogService? DialogService
    {
        get => GetValue(DialogServiceProperty);
        set => SetValue(DialogServiceProperty, value);
    }

    /// <summary>
    /// Gets or sets the global dialog service.
    /// </summary>
    public IDockGlobalDialogService? GlobalDialogService
    {
        get => GetValue(GlobalDialogServiceProperty);
        set => SetValue(GlobalDialogServiceProperty, value);
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
    /// Gets a value indicating whether local dialogs are shown.
    /// </summary>
    public bool ShowDialogs
    {
        get => _showDialogs;
        private set => SetAndRaise(ShowDialogsProperty, ref _showDialogs, value);
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
    /// Gets the dialog requests bound to the overlay.
    /// </summary>
    public ReadOnlyObservableCollection<DialogRequest>? Dialogs
    {
        get => _dialogs;
        private set => SetAndRaise(DialogsProperty, ref _dialogs, value);
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

    private void OnDialogServiceChanged(AvaloniaPropertyChangedEventArgs args)
    {
        AttachService(args.NewValue as IDockDialogService, ref _attachedDialogService, OnDialogServicePropertyChanged);
        Dialogs = _attachedDialogService?.Dialogs;
        SyncFromServices();
    }

    private void OnGlobalDialogServiceChanged(AvaloniaPropertyChangedEventArgs args)
    {
        AttachService(args.NewValue as IDockGlobalDialogService, ref _attachedGlobalDialogService, OnGlobalDialogServicePropertyChanged);

        SyncFromServices();
    }

    private void OnDialogServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IDockDialogService.HasDialogs)
            || e.PropertyName == nameof(IDockDialogService.ActiveDialog))
        {
            SyncFromServices();
        }
    }

    private void OnGlobalDialogServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IDockGlobalDialogService.IsDialogOpen)
            || e.PropertyName == nameof(IDockGlobalDialogService.Message))
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
        IsContentEnabled = !(overlayVisible && BlocksInput);
    }

    private void RebindServices()
    {
        AttachService(DialogService, ref _attachedDialogService, OnDialogServicePropertyChanged);
        AttachService(GlobalDialogService, ref _attachedGlobalDialogService, OnGlobalDialogServicePropertyChanged);
        Dialogs = _attachedDialogService?.Dialogs;
        SyncFromServices();
    }

    private void DetachServices()
    {
        DetachService(ref _attachedDialogService, OnDialogServicePropertyChanged);
        DetachService(ref _attachedGlobalDialogService, OnGlobalDialogServicePropertyChanged);
        Dialogs = null;
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
