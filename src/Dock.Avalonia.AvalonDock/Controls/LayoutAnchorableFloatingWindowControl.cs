using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using AvalonDock.Infrastructure;
using AvalonDock.Layout;

namespace AvalonDock.Controls;

/// <summary>A native Dock host presenting an AvalonDock floating tool group.</summary>
public class LayoutAnchorableFloatingWindowControl : LayoutFloatingWindowControl
{
    /// <summary>Defines the item container selected in the only visible tool pane.</summary>
    public static readonly StyledProperty<LayoutItem?> SingleContentLayoutItemProperty =
        AvaloniaProperty.Register<LayoutAnchorableFloatingWindowControl, LayoutItem?>(nameof(SingleContentLayoutItem));

    private readonly LayoutAnchorableFloatingWindow _model;
    private readonly LayoutFloatingWindowCommands _commands;

    /// <summary>Creates a tool host, including a host subclass supplied through the native window locator.</summary>
    /// <param name="model">The floating tool model.</param>
    /// <param name="isContentImmutable">Whether loaded content is protected from replacement.</param>
    protected internal LayoutAnchorableFloatingWindowControl(LayoutAnchorableFloatingWindow model, bool isContentImmutable = false)
        : base(model, isContentImmutable)
    {
        _model = model;
        _commands = new(model, CanClose, CanHide,
            parameter => LayoutFloatingWindowOperations.Execute(model, hide: false, parameter), HideContents);
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
        ObserveLayoutModel();
    }

    /// <inheritdoc/>
    protected override bool CanClose(object? parameter = null) =>
        LayoutFloatingWindowOperations.CanExecute(_model, hide: false, parameter);
    /// <inheritdoc/>
    protected override bool CanHide(object? parameter = null) =>
        LayoutFloatingWindowOperations.CanExecute(_model, hide: true, parameter);
    /// <inheritdoc/>
    protected override void DoHide() => HideContents(null);

    /// <summary>Activates a clicked native caption and routes a single-pane menu before native fallback.</summary>
    protected override IntPtr FilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        LayoutFloatingCaptionMessages.Process(this, msg, wParam, lParam, ref handled);
        return base.FilterMessage(hwnd, msg, wParam, lParam, ref handled);
    }

    private void HideContents(object? parameter)
    {
        DockingManager? manager = _model.Root?.Manager;
        LayoutFloatingWindowOperations.Execute(_model, hide: true, parameter);
        if (Owner is { } owner) owner.Focus();
        else if (manager is not null)
            foreach (Visual visual in manager.GetVisualDescendants())
                if (visual is Control { Focusable: true, IsEffectivelyVisible: true, IsEffectivelyEnabled: true } control && control.Focus()) break;
    }

    /// <inheritdoc/>
    protected override void OnClosed(EventArgs e)
    {
        try { _commands?.Dispose(); }
        finally { base.OnClosed(e); }
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SingleContentLayoutItemProperty) OnSingleContentLayoutItemChanged(change);
    }
}
