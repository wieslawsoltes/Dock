// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

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
    /// Defines <see cref="ShowContent"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowContentProperty =
        AvaloniaProperty.Register<DragPreviewControl, bool>(nameof(ShowContent));

    /// <summary>
    /// Defines <see cref="PreviewContentWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> PreviewContentWidthProperty =
        AvaloniaProperty.Register<DragPreviewControl, double>(nameof(PreviewContentWidth), double.NaN);

    /// <summary>
    /// Defines <see cref="PreviewContentHeight"/> property.
    /// </summary>
    public static readonly StyledProperty<double> PreviewContentHeightProperty =
        AvaloniaProperty.Register<DragPreviewControl, double>(nameof(PreviewContentHeight), double.NaN);
    
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
    /// Gets or sets whether the dockable content is shown.
    /// </summary>
    public bool ShowContent
    {
        get => GetValue(ShowContentProperty);
        set => SetValue(ShowContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the preview content width.
    /// </summary>
    public double PreviewContentWidth
    {
        get => GetValue(PreviewContentWidthProperty);
        set => SetValue(PreviewContentWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the preview content height.
    /// </summary>
    public double PreviewContentHeight
    {
        get => GetValue(PreviewContentHeightProperty);
        set => SetValue(PreviewContentHeightProperty, value);
    }
}
