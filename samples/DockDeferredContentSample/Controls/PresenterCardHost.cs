using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace DockDeferredContentSample.Controls;

public class PresenterCardHost : TemplatedControl
{
    public static readonly StyledProperty<object?> CardProperty =
        AvaloniaProperty.Register<PresenterCardHost, object?>(nameof(Card));

    public static readonly StyledProperty<IDataTemplate?> CardTemplateProperty =
        AvaloniaProperty.Register<PresenterCardHost, IDataTemplate?>(nameof(CardTemplate));

    public object? Card
    {
        get => GetValue(CardProperty);
        set => SetValue(CardProperty, value);
    }

    public IDataTemplate? CardTemplate
    {
        get => GetValue(CardTemplateProperty);
        set => SetValue(CardTemplateProperty, value);
    }
}
