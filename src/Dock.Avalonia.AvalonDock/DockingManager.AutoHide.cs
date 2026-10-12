using Avalonia;
using AvalonDock.Controls;
using AvalonDock.Infrastructure;

namespace AvalonDock;

public partial class DockingManager
{
    /// <summary>Identifies the manager's native auto-hide preview presentation.</summary>
    public static readonly DirectProperty<DockingManager, LayoutAutoHideWindowControl?> AutoHideWindowProperty =
        AvaloniaProperty.RegisterDirect<DockingManager, LayoutAutoHideWindowControl?>(nameof(AutoHideWindow), manager => manager.AutoHideWindow);
    /// <summary>Identifies the idle delay in milliseconds before an inactive auto-hide preview closes.</summary>
    public static readonly StyledProperty<int> AutoHideWindowClosingTimerProperty =
        AvaloniaProperty.Register<DockingManager, int>(nameof(AutoHideWindowClosingTimer), 1500,
            coerce: static (_, value) => value < 0 ? 1500 : value);

    private LayoutAutoHideWindowControl? _autoHideWindow;
    private readonly LayoutAutoHideController _autoHide;

    /// <summary>Gets the preview presentation created when the manager applies its template or loads.</summary>
    public LayoutAutoHideWindowControl? AutoHideWindow => _autoHideWindow;
    /// <summary>Gets or sets the idle closing delay in milliseconds; negative values coerce to the 1500-millisecond default.</summary>
    public int AutoHideWindowClosingTimer { get => GetValue(AutoHideWindowClosingTimerProperty); set => SetValue(AutoHideWindowClosingTimerProperty, value); }
    /// <summary>Sets the auto-hide preview presentation exposed to native templates.</summary>
    protected void SetAutoHideWindow(LayoutAutoHideWindowControl? value) => SetAndRaise(AutoHideWindowProperty, ref _autoHideWindow, value);
    /// <summary>Releases the previous presentation and applies the current preview model to its replacement.</summary>
    protected virtual void OnAutoHideWindowChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is LayoutAutoHideWindowControl previous)
        {
            previous.SetModel(null);
            previous.SetContent(null);
        }
        _autoHide.Refresh();
    }
    /// <summary>Applies the callback's new interval to the initialized close timer, including a pending deadline.</summary>
    /// <remarks>Suppressing this base call retains the applied interval. Before preview initialization,
    /// the interval is read from the current property when the timer is first initialized.</remarks>
    protected virtual void OnAutoHideWindowClosingTimerChanged(AvaloniaPropertyChangedEventArgs e) => _autoHide.ResetDelay((int)e.NewValue!);

    private void EnsureAutoHideWindow()
    {
        _autoHide.InitializeClosing();
        if (AutoHideWindow is null) SetAutoHideWindow(new LayoutAutoHideWindowControl());
    }
}
