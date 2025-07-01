using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

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
    /// Defines <see cref="PreviewVisual"/> property.
    /// </summary>
    public static readonly StyledProperty<Visual?> PreviewVisualProperty =
        AvaloniaProperty.Register<DragPreviewControl, Visual?>(nameof(PreviewVisual));

    /// <summary>
    /// Defines <see cref="PreviewHeight"/> property.
    /// </summary>
    public static readonly StyledProperty<double> PreviewHeightProperty =
        AvaloniaProperty.Register<DragPreviewControl, double>(nameof(PreviewHeight), 32d);

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
    /// Gets or sets the visual used for live preview.
    /// </summary>
    public Visual? PreviewVisual
    {
        get => GetValue(PreviewVisualProperty);
        set => SetValue(PreviewVisualProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of the preview.
    /// </summary>
    public double PreviewHeight
    {
        get => GetValue(PreviewHeightProperty);
        set => SetValue(PreviewHeightProperty, value);
    }
}

