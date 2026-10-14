using System;
using Avalonia;
using Avalonia.Data;
using AvalonDock.Infrastructure;

namespace AvalonDock;

public partial class DockingManager
{
    /// <summary>Defines the active content property, which supports styles and defaults to one-way binding.</summary>
    public static readonly StyledProperty<object?> ActiveContentProperty =
        AvaloniaProperty.Register<DockingManager, object?>(name: nameof(ActiveContent), defaultBindingMode: BindingMode.OneWay);

    private bool _settingActiveContent;
    private EventHandler<AvaloniaPropertyChangedEventArgs>? _activeContentNotification;

    /// <summary>Gets or sets the manager's active content value.</summary>
    /// <remarks>
    /// Explicit assignments retain the supplied object and activate its matching layout content.
    /// Layout-driven activation publishes the public layout model while preserving an existing binding.
    /// Explicit assignment replaces a one-way binding and preserves a writable binding.
    /// Clearing removes the binding; layout-driven current values retain it.
    /// </remarks>
    public object? ActiveContent
    {
        get => GetValue(ActiveContentProperty);
        set => SetValue(ActiveContentProperty, value);
    }

    /// <summary>Called after an active content value change has been applied to the layout.</summary>
    /// <param name="e">The previous and new manager values.</param>
    /// <remarks>The base implementation raises <see cref="ActiveContentChanged"/>.</remarks>
    protected virtual void OnActiveContentChanged(AvaloniaPropertyChangedEventArgs e) =>
        ActiveContentChanged?.Invoke(this, EventArgs.Empty);

    private void ApplyActiveContentChange(AvaloniaPropertyChangedEventArgs change)
    {
        bool previous = _settingActiveContent;
        _settingActiveContent = true;
        try { Layout.ActiveContent = LayoutActiveContentResolver.Find(Layout, change.NewValue); }
        finally { _settingActiveContent = previous; }
        // Binding source updates are dispatched after OnPropertyChanged. Keep
        // the public compatibility callback after those existing subscribers.
        _activeContentNotification ??= ActiveContentPropertyChanged;
        PropertyChanged -= _activeContentNotification;
        PropertyChanged += _activeContentNotification;
    }

    private void ActiveContentPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == ActiveContentProperty) OnActiveContentChanged(change);
    }

    private void SynchronizeActiveContent()
    {
        if (!_settingActiveContent) SetCurrentValue(ActiveContentProperty, Layout.ActiveContent);
    }
}
