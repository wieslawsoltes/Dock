using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Imaging;

namespace Dock.Avalonia.Controls;

/// <summary>
/// A control used as drag preview showing dock status.
/// </summary>
public class DragPreviewControl : TemplatedControl
{
    /// <summary>
    /// Defines <see cref="Title"/> property.
    /// </summary>
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<DragPreviewControl, string>(nameof(Title));

    /// <summary>
    /// Defines <see cref="Status"/> property.
    /// </summary>
    public static readonly StyledProperty<string> StatusProperty =
        AvaloniaProperty.Register<DragPreviewControl, string>(nameof(Status));

    /// <summary>
    /// Defines <see cref="Preview"/> property.
    /// </summary>
    public static readonly StyledProperty<IBitmap?> PreviewProperty =
        AvaloniaProperty.Register<DragPreviewControl, IBitmap?>(nameof(Preview));

    /// <summary>
    /// Gets or sets the preview title.
    /// </summary>
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the preview status.
    /// </summary>
    public string Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// Gets or sets the visual preview image.
    /// </summary>
    public IBitmap? Preview
    {
        get => GetValue(PreviewProperty);
        set => SetValue(PreviewProperty, value);
    }
}

