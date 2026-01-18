using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace Dock.Avalonia.Controls.Overlays;

/// <summary>
/// Provides a dialog container shell with title and close affordance.
/// </summary>
public sealed class DialogShellControl : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="Title"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<DialogShellControl, string?>(nameof(Title));

    /// <summary>
    /// Defines the <see cref="Content"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<DialogShellControl, object?>(nameof(Content));

    /// <summary>
    /// Defines the <see cref="ContentTemplate"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        AvaloniaProperty.Register<DialogShellControl, IDataTemplate?>(nameof(ContentTemplate));

    /// <summary>
    /// Defines the <see cref="CloseCommand"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> CloseCommandProperty =
        AvaloniaProperty.Register<DialogShellControl, ICommand?>(nameof(CloseCommand));

    /// <summary>
    /// Defines the <see cref="IsCloseVisible"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<bool> IsCloseVisibleProperty =
        AvaloniaProperty.Register<DialogShellControl, bool>(nameof(IsCloseVisible), true);

    /// <summary>
    /// Gets or sets the dialog title.
    /// </summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the dialog content.
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
    /// Gets or sets the close command.
    /// </summary>
    public ICommand? CloseCommand
    {
        get => GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the close button is visible.
    /// </summary>
    public bool IsCloseVisible
    {
        get => GetValue(IsCloseVisibleProperty);
        set => SetValue(IsCloseVisibleProperty, value);
    }
}
