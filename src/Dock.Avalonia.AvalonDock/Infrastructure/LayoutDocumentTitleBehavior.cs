using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using ReactiveUI.Reactive;

namespace AvalonDock.Infrastructure;

// Compose native window dragging with caption commands; the view remains passive.
internal sealed class LayoutDocumentTitleBehavior : AvaloniaObject
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<LayoutDocumentTitleBehavior, Control, bool>("IsEnabled");
    public static readonly AttachedProperty<ICommand?> MaximizeCommandProperty =
        AvaloniaProperty.RegisterAttached<LayoutDocumentTitleBehavior, Control, ICommand?>("MaximizeCommand");
    private static readonly AttachedProperty<Caption?> CaptionProperty =
        AvaloniaProperty.RegisterAttached<LayoutDocumentTitleBehavior, Control, Caption?>("Caption");

    static LayoutDocumentTitleBehavior()
    {
        IsEnabledProperty.Changed.AddClassHandler<Control>((control, change) =>
        {
            if (change.GetNewValue<bool>())
            {
                control.AttachedToVisualTree += Attached;
                control.DetachedFromVisualTree += Detached;
                Attach(control);
            }
            else
            {
                control.AttachedToVisualTree -= Attached;
                control.DetachedFromVisualTree -= Detached;
                Detach(control);
            }
        });
    }

    public static bool GetIsEnabled(Control control) => control.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(Control control, bool value) => control.SetValue(IsEnabledProperty, value);
    public static ICommand? GetMaximizeCommand(Control control) => control.GetValue(MaximizeCommandProperty);

    private static void Attached(object? sender, VisualTreeAttachmentEventArgs e) => Attach((Control)sender!);
    private static void Detached(object? sender, VisualTreeAttachmentEventArgs e) => Detach((Control)sender!);
    private static void Attach(Control control)
    {
        if (control.GetValue(CaptionProperty) is not null || TopLevel.GetTopLevel(control) is not HostWindow host) return;
        Caption caption = new(control, host);
        control.SetValue(CaptionProperty, caption);
        control.SetCurrentValue(MaximizeCommandProperty, caption.MaximizeCommand);
    }
    private static void Detach(Control control)
    {
        control.GetValue(CaptionProperty)?.Dispose();
        control.ClearValue(CaptionProperty);
        control.ClearValue(MaximizeCommandProperty);
    }

    private sealed class Caption : IDisposable
    {
        private readonly Control _control;
        private readonly WindowDragHelper _drag;
        internal ReactiveCommand<Unit, Unit> MaximizeCommand { get; }

        internal Caption(Control control, HostWindow host)
        {
            _control = control;
            WeakReference<HostWindow> window = new(host);
            MaximizeCommand = ReactiveCommand.Create(() =>
            {
                if (window.TryGetTarget(out HostWindow? current) && current.CanResize)
                    current.SetCurrentValue(Window.WindowStateProperty,
                        current.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized);
            }, host.GetObservable(Window.CanResizeProperty), ImmediateScheduler.Instance);
            _drag = new WindowDragHelper(control,
                () => window.TryGetTarget(out HostWindow? current) && current.Window?.Layout?.CanDrag != false,
                CanStartDrag);
            _drag.Attach();
            control.ContextRequested += ContextRequested;
            control.DoubleTapped += DoubleTapped;
        }

        private bool CanStartDrag(Control? source)
        {
            for (Visual? current = source; current is not null && !ReferenceEquals(current, _control); current = current.GetVisualParent())
                if (current is Control { Focusable: true } or Button) return false;
            return source is not null;
        }

        private void DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (!CanStartDrag(e.Source as Control) || !((ICommand)MaximizeCommand).CanExecute(null)) return;
            e.Handled = true;
            ((ICommand)MaximizeCommand).Execute(null);
        }

        private void ContextRequested(object? sender, ContextRequestedEventArgs e)
        {
            if (e is LayoutContextMenuRequest) return;
            for (Visual? current = e.Source as Visual; current is not null && !ReferenceEquals(current, _control); current = current.GetVisualParent())
                if (current is Control child && (child.ContextMenu is not null || child.ContextFlyout is not null)) return;
            if (_control.DataContext is LayoutDocumentFloatingWindow { RootDocument: { } document } &&
                document.Root?.Manager is { } manager && manager.GetLayoutItemFromModel(document) is { } item)
            {
                manager.ShowContextMenu(_control, item, LayoutHeaderKind.Document, e);
                e.Handled = true;
            }
        }

        public void Dispose()
        {
            _control.ContextRequested -= ContextRequested;
            _control.DoubleTapped -= DoubleTapped;
            _drag.Detach();
            MaximizeCommand.Dispose();
        }
    }
}
