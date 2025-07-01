using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Control used to preview tab insertion inside tabstrip.
/// </summary>
public class TabPreviewControl : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="Title"/> property.
    /// </summary>
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<TabPreviewControl, string>(nameof(Title));

    private Border? _border;

    /// <summary>
    /// Gets or sets preview title.
    /// </summary>
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _border = e.NameScope.Find<Border>("PART_Border");
    }

    /// <summary>
    /// Move preview to specified rectangle.
    /// </summary>
    /// <param name="rect">Preview rectangle.</param>
    public void Move(Rect rect)
    {
        if (_border is null)
            return;

        Canvas.SetLeft(_border, rect.X);
        Canvas.SetTop(_border, rect.Y);
        _border.Width = rect.Width;
        _border.Height = rect.Height;
    }
}
