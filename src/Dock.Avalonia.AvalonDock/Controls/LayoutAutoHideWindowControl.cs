using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using AvalonDock.Layout;

namespace AvalonDock.Controls;

/// <summary>Presents Dock's existing auto-hide preview and its current public tool model.</summary>
/// <remarks>The Avalonia counterpart hosts native Dock visuals instead of a WPF child HWND.</remarks>
public class LayoutAutoHideWindowControl : ContentControl, ILayoutControl
{
    /// <summary>Identifies the theme applied to the preview's anchorable content control.</summary>
    public static readonly StyledProperty<ControlTheme?> AnchorableStyleProperty =
        AvaloniaProperty.Register<LayoutAutoHideWindowControl, ControlTheme?>(nameof(AnchorableStyle));
    /// <summary>Identifies the preview background.</summary>
    public new static readonly StyledProperty<IBrush?> BackgroundProperty =
        TemplatedControl.BackgroundProperty.AddOwner<LayoutAutoHideWindowControl>();
    /// <summary>Identifies the current public tool, or null while the preview is closed.</summary>
    public static readonly DirectProperty<LayoutAutoHideWindowControl, ILayoutElement?> ModelProperty =
        AvaloniaProperty.RegisterDirect<LayoutAutoHideWindowControl, ILayoutElement?>(nameof(Model), control => control.Model);

    private ILayoutElement? _model;

    internal LayoutAutoHideWindowControl()
    {
        HorizontalContentAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch;
        VerticalContentAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch;
        Focusable = true;
        IsVisible = false;
    }

    /// <summary>Gets or sets the theme of the existing preview's <see cref="LayoutAnchorableControl"/>.</summary>
    public ControlTheme? AnchorableStyle { get => GetValue(AnchorableStyleProperty); set => SetValue(AnchorableStyleProperty, value); }
    /// <summary>Gets or sets the brush behind the preview content.</summary>
    public new IBrush? Background { get => GetValue(BackgroundProperty); set => SetValue(BackgroundProperty, value); }
    /// <summary>Gets the public tool being previewed, or null while the preview is closed.</summary>
    public ILayoutElement? Model => _model;
    ILayoutElement ILayoutControl.Model => _model!;

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(LayoutAutoHideWindowControl);
    /// <summary>Measures the existing native preview content.</summary>
    protected override Size MeasureOverride(Size constraint) => base.MeasureOverride(constraint);
    /// <summary>Arranges the existing native preview content.</summary>
    protected override Size ArrangeOverride(Size finalSize) => base.ArrangeOverride(finalSize);
    /// <summary>Reports whether Avalonia keyboard focus is inside the preview.</summary>
    protected virtual bool HasFocusWithinCore() => IsKeyboardFocusWithin;

    internal void SetModel(LayoutAnchorable? model)
    {
        SetAndRaise(ModelProperty, ref _model, model);
        SetCurrentValue(IsVisibleProperty, model is not null);
    }

    internal void SetContent(Control? content)
    {
        SetCurrentValue(ContentProperty, content);
        if (content is null) Presenter?.UpdateChild();
    }
}
