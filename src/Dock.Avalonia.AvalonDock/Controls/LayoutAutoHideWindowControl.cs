using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Styling;
using AvalonDock.Layout;
using AvalonDock.Infrastructure;

namespace AvalonDock.Controls;

/// <summary>Presents Dock's existing auto-hide preview and its current public tool model.</summary>
/// <remarks>Supported desktop backends present the same Dock visuals in a native embedded child.
/// Backends without embedding retain the in-tree presenter. The control is focusable and hidden by default.</remarks>
public class LayoutAutoHideWindowControl : ContentControl, ILayoutControl
{
    /// <summary>Identifies the theme applied to the preview's anchorable content control.</summary>
    public static readonly StyledProperty<ControlTheme?> AnchorableStyleProperty =
        AvaloniaProperty.Register<LayoutAutoHideWindowControl, ControlTheme?>(name: nameof(AnchorableStyle), metadata: AvalonDockProperty.StyledMetadata<ControlTheme?>());
    /// <summary>Identifies the preview background.</summary>
    public new static readonly StyledProperty<IBrush?> BackgroundProperty =
        TemplatedControl.BackgroundProperty.AddOwner<LayoutAutoHideWindowControl>(AvalonDockProperty.StyledMetadata<IBrush?>());
    /// <summary>Identifies the current public tool, or null while the preview is closed.</summary>
    public static readonly DirectProperty<LayoutAutoHideWindowControl, ILayoutElement?> ModelProperty =
        AvaloniaProperty.RegisterDirect<LayoutAutoHideWindowControl, ILayoutElement?>(nameof(Model), control => control.Model);

    private ILayoutElement? _model;
    private ContentPresenter? _logicalPresenter;
    private readonly LogicalChildrenView _logicalChildrenView;
    private readonly LayoutAutoHideNativeRoot _nativeRoot;

    static LayoutAutoHideWindowControl()
    {
        FocusableProperty.OverrideDefaultValue<LayoutAutoHideWindowControl>(true);
        IsVisibleProperty.OverrideDefaultValue<LayoutAutoHideWindowControl>(false);
    }

    internal LayoutAutoHideWindowControl()
    {
        _nativeRoot = new(this);
        LogicalChildrenSource = _logicalChildrenView = new(() => LogicalChildren);
        HorizontalContentAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch;
        VerticalContentAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch;
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
    protected override Size MeasureOverride(Size constraint)
    {
        Size measured = base.MeasureOverride(constraint);
        if (!_nativeRoot.IsHosted) return measured;
        Size native = _nativeRoot.Measure(constraint);
        return new Size(Math.Max(measured.Width, native.Width), Math.Max(measured.Height, native.Height));
    }
    /// <summary>Arranges the existing native preview content.</summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        _nativeRoot.Arrange(finalSize);
        return base.ArrangeOverride(finalSize);
    }
    /// <summary>Returns the upstream native-host focus hook's false result.</summary>
    /// <remarks>Actual Avalonia focus remains available through <see cref="global::Avalonia.Input.InputElement.IsKeyboardFocusWithin"/>.</remarks>
    protected virtual bool HasFocusWithinCore() => false;

    /// <summary>Enumerates the preview's existing content presenter.</summary>
    protected new virtual IEnumerator LogicalChildren => EnumerateLogicalPresenter().GetEnumerator();

    /// <summary>Creates the real embedded child used by the native host lifecycle.</summary>
    /// <param name="hwndParent">The platform parent supplied by Avalonia's native control host.</param>
    /// <returns>The owned HWND on Windows, or the equivalent NSView handle on macOS.</returns>
    /// <remarks>Avalonia completes native parenting and owns clipping and positioning after this callback.</remarks>
    protected virtual System.Runtime.InteropServices.HandleRef BuildWindowCore(System.Runtime.InteropServices.HandleRef hwndParent) => _nativeRoot.Build();

    /// <summary>Disposes the embedded root associated with the supplied native child handle.</summary>
    protected virtual void DestroyWindowCore(System.Runtime.InteropServices.HandleRef hwnd) => _nativeRoot.Destroy(hwnd);

    /// <inheritdoc/>
    protected override bool RegisterContentPresenter(ContentPresenter presenter)
    {
        if (!base.RegisterContentPresenter(presenter)) return false;
        if (!ReferenceEquals(_logicalPresenter, presenter))
        {
            if (_logicalPresenter is not null) base.LogicalChildren.Remove(_logicalPresenter);
            _logicalPresenter = presenter;
            if (presenter.Parent is not null && !ReferenceEquals(presenter.Parent, this))
                ((ISetLogicalParent)presenter).SetParent(null);
            if (!base.LogicalChildren.Contains(presenter)) base.LogicalChildren.Add(presenter);
            _logicalChildrenView.NotifyChanged();
        }
        // Retain ContentControl.Presenter registration, but let this presenter own its
        // content logically, matching the child presentation root of the native host.
        if (Content is ILogical content) base.LogicalChildren.Remove(content);
        return false;
    }

    /// <inheritdoc/>
    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        // The native presenter is outside the old template's visual tree, so the
        // framework cannot release its editor while dismantling that template.
        // Retire that template binding before a replacement presenter takes ownership.
        if (_logicalPresenter is { } presenter)
        {
            presenter.Content = null;
            presenter.UpdateChild();
        }
        base.OnTemplateChanged(e);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property != ContentProperty) return;
        if (change.NewValue is ILogical content) base.LogicalChildren.Remove(content);
        if (_logicalPresenter?.Child is { Parent: null } child)
            ((ISetLogicalParent)child).SetParent(_logicalPresenter);
    }

    internal bool IsNativeHosted => _nativeRoot.IsHosted;

    internal bool IsPreviewPointerOver => IsPointerOver || _logicalPresenter?.IsPointerOver == true;

    internal HandleRef CreateNativeHost(HandleRef parent, Decorator? inline)
    {
        _nativeRoot.InlinePresenter = inline;
        return BuildWindowCore(parent);
    }

    internal void DestroyNativeHost(HandleRef handle) => DestroyWindowCore(handle);

    private IEnumerable<ContentPresenter> EnumerateLogicalPresenter()
    {
        if (_logicalPresenter is not null) yield return _logicalPresenter;
    }

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
