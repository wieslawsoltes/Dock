using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace DockReactiveUICanonicalSample.Controls;

public sealed class ConfirmationDialogControl : TemplatedControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<ConfirmationDialogControl, string?>(nameof(Title));

    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<ConfirmationDialogControl, string?>(nameof(Message));

    public static readonly StyledProperty<string?> ConfirmTextProperty =
        AvaloniaProperty.Register<ConfirmationDialogControl, string?>(nameof(ConfirmText));

    public static readonly StyledProperty<string?> CancelTextProperty =
        AvaloniaProperty.Register<ConfirmationDialogControl, string?>(nameof(CancelText));

    public static readonly StyledProperty<ICommand?> ConfirmCommandProperty =
        AvaloniaProperty.Register<ConfirmationDialogControl, ICommand?>(nameof(ConfirmCommand));

    public static readonly StyledProperty<ICommand?> CancelCommandProperty =
        AvaloniaProperty.Register<ConfirmationDialogControl, ICommand?>(nameof(CancelCommand));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string? ConfirmText
    {
        get => GetValue(ConfirmTextProperty);
        set => SetValue(ConfirmTextProperty, value);
    }

    public string? CancelText
    {
        get => GetValue(CancelTextProperty);
        set => SetValue(CancelTextProperty, value);
    }

    public ICommand? ConfirmCommand
    {
        get => GetValue(ConfirmCommandProperty);
        set => SetValue(ConfirmCommandProperty, value);
    }

    public ICommand? CancelCommand
    {
        get => GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }
}
