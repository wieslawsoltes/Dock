using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace DockReactiveUICanonicalSample.Controls;

public sealed class DialogShellControl : TemplatedControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<DialogShellControl, string?>(nameof(Title));

    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<DialogShellControl, object?>(nameof(Content));

    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        AvaloniaProperty.Register<DialogShellControl, IDataTemplate?>(nameof(ContentTemplate));

    public static readonly StyledProperty<ICommand?> CloseCommandProperty =
        AvaloniaProperty.Register<DialogShellControl, ICommand?>(nameof(CloseCommand));

    public static readonly StyledProperty<bool> IsCloseVisibleProperty =
        AvaloniaProperty.Register<DialogShellControl, bool>(nameof(IsCloseVisible), true);

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
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

    public ICommand? CloseCommand
    {
        get => GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    public bool IsCloseVisible
    {
        get => GetValue(IsCloseVisibleProperty);
        set => SetValue(IsCloseVisibleProperty, value);
    }
}
