using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace Dock.Avalonia.Controls.Overlays;

/// <summary>
/// Provides a confirmation dialog shell with title, message, and buttons.
/// </summary>
public sealed class ConfirmationDialogControl : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="Title"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<ConfirmationDialogControl, string?>(nameof(Title));

    /// <summary>
    /// Defines the <see cref="Message"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<ConfirmationDialogControl, string?>(nameof(Message));

    /// <summary>
    /// Defines the <see cref="ConfirmText"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<string?> ConfirmTextProperty =
        AvaloniaProperty.Register<ConfirmationDialogControl, string?>(nameof(ConfirmText));

    /// <summary>
    /// Defines the <see cref="CancelText"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<string?> CancelTextProperty =
        AvaloniaProperty.Register<ConfirmationDialogControl, string?>(nameof(CancelText));

    /// <summary>
    /// Defines the <see cref="ConfirmCommand"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> ConfirmCommandProperty =
        AvaloniaProperty.Register<ConfirmationDialogControl, ICommand?>(nameof(ConfirmCommand));

    /// <summary>
    /// Defines the <see cref="CancelCommand"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> CancelCommandProperty =
        AvaloniaProperty.Register<ConfirmationDialogControl, ICommand?>(nameof(CancelCommand));

    /// <summary>
    /// Gets or sets the dialog title.
    /// </summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the dialog message.
    /// </summary>
    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>
    /// Gets or sets the confirm button label.
    /// </summary>
    public string? ConfirmText
    {
        get => GetValue(ConfirmTextProperty);
        set => SetValue(ConfirmTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the cancel button label.
    /// </summary>
    public string? CancelText
    {
        get => GetValue(CancelTextProperty);
        set => SetValue(CancelTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the confirm command.
    /// </summary>
    public ICommand? ConfirmCommand
    {
        get => GetValue(ConfirmCommandProperty);
        set => SetValue(ConfirmCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the cancel command.
    /// </summary>
    public ICommand? CancelCommand
    {
        get => GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }
}
