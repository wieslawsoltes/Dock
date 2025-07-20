// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;

namespace Dock.Avalonia.Controls;

/// <summary>
/// A control used as drag preview showing dock status.
/// </summary>
public class DragPreviewControl : TemplatedControl
{
    /// <summary>
    /// Define the <see cref="ContentTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> ContentTemplateProperty = 
        AvaloniaProperty.Register<DocumentControl, IDataTemplate>(nameof(ContentTemplate));

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
    /// Defines <see cref="PreviewWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> PreviewWidthProperty =
        AvaloniaProperty.Register<DragPreviewControl, double>(nameof(PreviewWidth));

    /// <summary>
    /// Defines <see cref="PreviewHeight"/> property.
    /// </summary>
    public static readonly StyledProperty<double> PreviewHeightProperty =
        AvaloniaProperty.Register<DragPreviewControl, double>(nameof(PreviewHeight));
    
    /// <summary>
    /// Gets or sets tab header template.
    /// </summary>
    public IDataTemplate ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
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
    /// Gets or sets drag preview visual.
    /// </summary>
    public Visual? PreviewVisual
    {
        get => GetValue(PreviewVisualProperty);
        set => SetValue(PreviewVisualProperty, value);
    }

    /// <summary>
    /// Gets or sets preview width.
    /// </summary>
    public double PreviewWidth
    {
        get => GetValue(PreviewWidthProperty);
        set => SetValue(PreviewWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets preview height.
    /// </summary>
    public double PreviewHeight
    {
        get => GetValue(PreviewHeightProperty);
        set => SetValue(PreviewHeightProperty, value);
    }
}

