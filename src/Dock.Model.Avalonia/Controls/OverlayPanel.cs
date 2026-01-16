// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Overlay panel.
/// </summary>
[DataContract(IsReference = true)]
public class OverlayPanel : DockBase, IOverlayPanel
{
    /// <summary>
    /// Defines the <see cref="X"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, double> XProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, double>(nameof(X), o => o.X, (o, v) => o.X = v);

    /// <summary>
    /// Defines the <see cref="Y"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, double> YProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, double>(nameof(Y), o => o.Y, (o, v) => o.Y = v);

    /// <summary>
    /// Defines the <see cref="Width"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, double> WidthProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, double>(nameof(Width), o => o.Width, (o, v) => o.Width = v);

    /// <summary>
    /// Defines the <see cref="Height"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, double> HeightProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, double>(nameof(Height), o => o.Height, (o, v) => o.Height = v);

    /// <summary>
    /// Defines the <see cref="ZIndex"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, int> ZIndexProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, int>(nameof(ZIndex), o => o.ZIndex, (o, v) => o.ZIndex = v);

    /// <summary>
    /// Defines the <see cref="Anchor"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, OverlayAnchor> AnchorProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, OverlayAnchor>(nameof(Anchor), o => o.Anchor, (o, v) => o.Anchor = v);

    /// <summary>
    /// Defines the <see cref="AnchorOffsetX"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, double> AnchorOffsetXProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, double>(nameof(AnchorOffsetX), o => o.AnchorOffsetX, (o, v) => o.AnchorOffsetX = v);

    /// <summary>
    /// Defines the <see cref="AnchorOffsetY"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, double> AnchorOffsetYProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, double>(nameof(AnchorOffsetY), o => o.AnchorOffsetY, (o, v) => o.AnchorOffsetY = v);

    /// <summary>
    /// Defines the <see cref="Opacity"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, double> OpacityProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, double>(nameof(Opacity), o => o.Opacity, (o, v) => o.Opacity = v);

    /// <summary>
    /// Defines the <see cref="UseBackdropBlur"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, bool> UseBackdropBlurProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, bool>(nameof(UseBackdropBlur), o => o.UseBackdropBlur, (o, v) => o.UseBackdropBlur = v);

    /// <summary>
    /// Defines the <see cref="BlurRadius"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, double> BlurRadiusProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, double>(nameof(BlurRadius), o => o.BlurRadius, (o, v) => o.BlurRadius = v);

    /// <summary>
    /// Defines the <see cref="State"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, OverlayPanelState> StateProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, OverlayPanelState>(nameof(State), o => o.State, (o, v) => o.State = v);

    /// <summary>
    /// Defines the <see cref="IsPositionLocked"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, bool> IsPositionLockedProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, bool>(nameof(IsPositionLocked), o => o.IsPositionLocked, (o, v) => o.IsPositionLocked = v);

    /// <summary>
    /// Defines the <see cref="IsSizeLocked"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, bool> IsSizeLockedProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, bool>(nameof(IsSizeLocked), o => o.IsSizeLocked, (o, v) => o.IsSizeLocked = v);

    /// <summary>
    /// Defines the <see cref="IsDragging"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, bool> IsDraggingProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, bool>(nameof(IsDragging), o => o.IsDragging, (o, v) => o.IsDragging = v);

    /// <summary>
    /// Defines the <see cref="IsFocused"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, bool> IsFocusedProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, bool>(nameof(IsFocused), o => o.IsFocused, (o, v) => o.IsFocused = v);

    /// <summary>
    /// Defines the <see cref="ShowHeader"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, bool> ShowHeaderProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, bool>(nameof(ShowHeader), o => o.ShowHeader, (o, v) => o.ShowHeader = v, true);

    /// <summary>
    /// Defines the <see cref="ShowShadow"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, bool> ShowShadowProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, bool>(nameof(ShowShadow), o => o.ShowShadow, (o, v) => o.ShowShadow = v, true);

    /// <summary>
    /// Defines the <see cref="CornerRadius"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, double> CornerRadiusProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, double>(nameof(CornerRadius), o => o.CornerRadius, (o, v) => o.CornerRadius = v);

    /// <summary>
    /// Defines the <see cref="HorizontalContentAlignment"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, OverlayHorizontalAlignment> HorizontalContentAlignmentProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, OverlayHorizontalAlignment>(nameof(HorizontalContentAlignment), o => o.HorizontalContentAlignment, (o, v) => o.HorizontalContentAlignment = v);

    /// <summary>
    /// Defines the <see cref="VerticalContentAlignment"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, OverlayVerticalAlignment> VerticalContentAlignmentProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, OverlayVerticalAlignment>(nameof(VerticalContentAlignment), o => o.VerticalContentAlignment, (o, v) => o.VerticalContentAlignment = v);

    /// <summary>
    /// Defines the <see cref="Visibility"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, OverlayVisibility> VisibilityProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, OverlayVisibility>(nameof(Visibility), o => o.Visibility, (o, v) => o.Visibility = v);

    /// <summary>
    /// Defines the <see cref="AnimateVisibility"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, bool> AnimateVisibilityProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, bool>(nameof(AnimateVisibility), o => o.AnimateVisibility, (o, v) => o.AnimateVisibility = v, true);

    /// <summary>
    /// Defines the <see cref="VisibilityAnimation"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, OverlayVisibilityAnimation> VisibilityAnimationProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, OverlayVisibilityAnimation>(nameof(VisibilityAnimation), o => o.VisibilityAnimation, (o, v) => o.VisibilityAnimation = v);

    /// <summary>
    /// Defines the <see cref="VisibilityAnimationDuration"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, double> VisibilityAnimationDurationProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, double>(nameof(VisibilityAnimationDuration), o => o.VisibilityAnimationDuration, (o, v) => o.VisibilityAnimationDuration = v);

    /// <summary>
    /// Defines the <see cref="FloatOnDoubleClick"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, bool> FloatOnDoubleClickProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, bool>(nameof(FloatOnDoubleClick), o => o.FloatOnDoubleClick, (o, v) => o.FloatOnDoubleClick = v, true);

    /// <summary>
    /// Defines the <see cref="AllowDockInto"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, bool> AllowDockIntoProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, bool>(nameof(AllowDockInto), o => o.AllowDockInto, (o, v) => o.AllowDockInto = v, true);

    /// <summary>
    /// Defines the <see cref="ShowDockTargets"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlayPanel, bool> ShowDockTargetsProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, bool>(nameof(ShowDockTargets), o => o.ShowDockTargets, (o, v) => o.ShowDockTargets = v, true);

    private double _x;
    private double _y;
    private double _width = 320;
    private double _height = 240;
    private int _zIndex;
    private OverlayAnchor _anchor;
    private double _anchorOffsetX;
    private double _anchorOffsetY;
    private double _opacity = 1.0;
    private bool _useBackdropBlur;
    private double _blurRadius = 12.0;
    private OverlayPanelState _state;
    private bool _isPositionLocked;
    private bool _isSizeLocked;
    private bool _isDragging;
    private bool _isFocused;
    private bool _showHeader = true;
    private bool _showShadow = true;
    private double _cornerRadius = 4.0;
    private OverlayHorizontalAlignment _horizontalContentAlignment = OverlayHorizontalAlignment.Stretch;
    private OverlayVerticalAlignment _verticalContentAlignment = OverlayVerticalAlignment.Stretch;
    private OverlayVisibility _visibility = OverlayVisibility.Visible;
    private bool _animateVisibility = true;
    private OverlayVisibilityAnimation _visibilityAnimation = OverlayVisibilityAnimation.Fade;
    private double _visibilityAnimationDuration = 150.0;
    private bool _floatOnDoubleClick = true;
    private bool _allowDockInto = true;
    private bool _showDockTargets = true;

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public IDockable? Content
    {
        get => VisibleDockables is { Count: > 0 } list ? list[0] : null;
        set
        {
            if (Factory is { } factory)
            {
                factory.SetOverlayPanelContent(this, value);
            }
        }
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("X")]
    public double X
    {
        get => _x;
        set => SetAndRaise(XProperty, ref _x, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Y")]
    public double Y
    {
        get => _y;
        set => SetAndRaise(YProperty, ref _y, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Width")]
    public double Width
    {
        get => _width;
        set => SetAndRaise(WidthProperty, ref _width, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Height")]
    public double Height
    {
        get => _height;
        set => SetAndRaise(HeightProperty, ref _height, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("ZIndex")]
    public int ZIndex
    {
        get => _zIndex;
        set => SetAndRaise(ZIndexProperty, ref _zIndex, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Anchor")]
    public OverlayAnchor Anchor
    {
        get => _anchor;
        set => SetAndRaise(AnchorProperty, ref _anchor, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("AnchorOffsetX")]
    public double AnchorOffsetX
    {
        get => _anchorOffsetX;
        set => SetAndRaise(AnchorOffsetXProperty, ref _anchorOffsetX, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("AnchorOffsetY")]
    public double AnchorOffsetY
    {
        get => _anchorOffsetY;
        set => SetAndRaise(AnchorOffsetYProperty, ref _anchorOffsetY, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Opacity")]
    public double Opacity
    {
        get => _opacity;
        set => SetAndRaise(OpacityProperty, ref _opacity, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("UseBackdropBlur")]
    public bool UseBackdropBlur
    {
        get => _useBackdropBlur;
        set => SetAndRaise(UseBackdropBlurProperty, ref _useBackdropBlur, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("BlurRadius")]
    public double BlurRadius
    {
        get => _blurRadius;
        set => SetAndRaise(BlurRadiusProperty, ref _blurRadius, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("State")]
    public OverlayPanelState State
    {
        get => _state;
        set => SetAndRaise(StateProperty, ref _state, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("IsPositionLocked")]
    public bool IsPositionLocked
    {
        get => _isPositionLocked;
        set => SetAndRaise(IsPositionLockedProperty, ref _isPositionLocked, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("IsSizeLocked")]
    public bool IsSizeLocked
    {
        get => _isSizeLocked;
        set => SetAndRaise(IsSizeLockedProperty, ref _isSizeLocked, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public bool IsDragging
    {
        get => _isDragging;
        set => SetAndRaise(IsDraggingProperty, ref _isDragging, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public bool IsFocused
    {
        get => _isFocused;
        set => SetAndRaise(IsFocusedProperty, ref _isFocused, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("ShowHeader")]
    public bool ShowHeader
    {
        get => _showHeader;
        set => SetAndRaise(ShowHeaderProperty, ref _showHeader, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("ShowShadow")]
    public bool ShowShadow
    {
        get => _showShadow;
        set => SetAndRaise(ShowShadowProperty, ref _showShadow, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("CornerRadius")]
    public double CornerRadius
    {
        get => _cornerRadius;
        set => SetAndRaise(CornerRadiusProperty, ref _cornerRadius, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public IOverlaySplitterGroup? SplitterGroup { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("HorizontalContentAlignment")]
    public OverlayHorizontalAlignment HorizontalContentAlignment
    {
        get => _horizontalContentAlignment;
        set => SetAndRaise(HorizontalContentAlignmentProperty, ref _horizontalContentAlignment, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("VerticalContentAlignment")]
    public OverlayVerticalAlignment VerticalContentAlignment
    {
        get => _verticalContentAlignment;
        set => SetAndRaise(VerticalContentAlignmentProperty, ref _verticalContentAlignment, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Visibility")]
    public OverlayVisibility Visibility
    {
        get => _visibility;
        set => SetAndRaise(VisibilityProperty, ref _visibility, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("AnimateVisibility")]
    public bool AnimateVisibility
    {
        get => _animateVisibility;
        set => SetAndRaise(AnimateVisibilityProperty, ref _animateVisibility, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("VisibilityAnimation")]
    public OverlayVisibilityAnimation VisibilityAnimation
    {
        get => _visibilityAnimation;
        set => SetAndRaise(VisibilityAnimationProperty, ref _visibilityAnimation, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("VisibilityAnimationDuration")]
    public double VisibilityAnimationDuration
    {
        get => _visibilityAnimationDuration;
        set => SetAndRaise(VisibilityAnimationDurationProperty, ref _visibilityAnimationDuration, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("FloatOnDoubleClick")]
    public bool FloatOnDoubleClick
    {
        get => _floatOnDoubleClick;
        set => SetAndRaise(FloatOnDoubleClickProperty, ref _floatOnDoubleClick, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("AllowDockInto")]
    public bool AllowDockInto
    {
        get => _allowDockInto;
        set => SetAndRaise(AllowDockIntoProperty, ref _allowDockInto, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public IDock? NestedDock
    {
        get => Content as IDock;
        set => Content = value;
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("ShowDockTargets")]
    public bool ShowDockTargets
    {
        get => _showDockTargets;
        set => SetAndRaise(ShowDockTargetsProperty, ref _showDockTargets, value);
    }
}
