using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Diagnostics;
using AvalonDock.Infrastructure;
using AvalonDock.Layout;

namespace AvalonDock.Controls;

/// <summary>A native Dock host presenting an AvalonDock floating tool group.</summary>
public class LayoutAnchorableFloatingWindowControl : LayoutFloatingWindowControl
{
    /// <summary>Defines the item container selected in the only visible tool pane.</summary>
    public static readonly StyledProperty<LayoutItem?> SingleContentLayoutItemProperty =
        AvaloniaProperty.Register<LayoutAnchorableFloatingWindowControl, LayoutItem?>(name: nameof(SingleContentLayoutItem));

    private readonly LayoutAnchorableFloatingWindow _model;
    private readonly LayoutFloatingWindowCommands _commands;

    /// <summary>Creates a tool host, including a host subclass supplied through the native window locator.</summary>
    /// <param name="model">The floating tool model.</param>
    /// <param name="isContentImmutable">Whether loaded content is protected from replacement.</param>
    protected internal LayoutAnchorableFloatingWindowControl(LayoutAnchorableFloatingWindow model, bool isContentImmutable = false)
        : base(model, isContentImmutable)
    {
        _model = model;
        _commands = new(model, () => Model, CanClose, CanHide,
            parameter => LayoutFloatingWindowOperations.Execute<LayoutAnchorable>(Model, hide: false, parameter), HideContents);
    }

    /// <inheritdoc/>
    public override ILayoutElement Model => _model;
    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(LayoutAnchorableFloatingWindowControl);
    /// <summary>Gets or sets the container selected in a single visible tool pane.</summary>
    public LayoutItem? SingleContentLayoutItem
    {
        get => GetValue(SingleContentLayoutItemProperty);
        set => SetValue(SingleContentLayoutItemProperty, value);
    }
    /// <summary>Gets the command that closes all tools through their current item commands.</summary>
    public ICommand CloseWindowCommand => _commands.Close;
    /// <summary>Gets the command that hides all tools through their current item commands.</summary>
    public ICommand HideWindowCommand => _commands.Hide;
    /// <summary>Called when the selected single-pane container changes.</summary>
    protected virtual void OnSingleContentLayoutItemChanged(AvaloniaPropertyChangedEventArgs e) { }

    /// <summary>Starts observing tool-panel visibility and removal after native initialization.</summary>
    /// <remarks>Call the base hook to retain model-driven hiding, restoration, and closure.</remarks>
    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        ObserveLayoutModel(cleanupInDerivedHook: true);
    }

    /// <summary>Checks tool descendants of the current Model, including a model supplied by an override.</summary>
    protected override bool CanClose(object? parameter = null) =>
        LayoutFloatingWindowOperations.CanExecute<LayoutAnchorable>(Model, hide: false, parameter);
    /// <summary>Checks tool hide commands under the current Model, including a model supplied by an override.</summary>
    protected override bool CanHide(object? parameter = null) =>
        LayoutFloatingWindowOperations.CanExecute<LayoutAnchorable>(Model, hide: true, parameter);
    /// <inheritdoc/>
    protected override void DoHide() => HideContents(null);

    /// <summary>Activates a clicked native caption and routes a single-pane menu before native fallback.</summary>
    protected override IntPtr FilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        LayoutFloatingCaptionMessages.Process(this, _model, msg, wParam, lParam, ref handled);
        return base.FilterMessage(hwnd, msg, wParam, lParam, ref handled);
    }

    private void HideContents(object? parameter)
    {
        LayoutFloatingWindowOperations.Execute<LayoutAnchorable>(Model, hide: true, parameter);
        RestoreOwnerOrManagerFocus();
    }

    /// <inheritdoc/>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        StopObservingLayoutModel();
        ClearLocalBinding(IsVisibleProperty);
        ClearLocalBinding(SingleContentLayoutItemProperty);
        _commands?.Dispose();
    }

    private void ClearLocalBinding<T>(StyledProperty<T> property)
    {
        if (BindingOperations.GetBindingExpressionBase(this, property) is { } binding &&
            this.GetDiagnostic(property).Priority == BindingPriority.LocalValue)
            binding.Dispose();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SingleContentLayoutItemProperty) OnSingleContentLayoutItemChanged(change);
    }
}
