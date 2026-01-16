// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using Dock.Model.Adapters;
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveProperty.Core;

namespace Dock.Model.ReactiveProperty.Controls;

/// <summary>
/// Overlay panel.
/// </summary>
[DataContract(IsReference = true)]
public class OverlayPanel : DockBase, IOverlayPanel
{
    private static readonly IOverlayAdapter s_overlayAdapter = new OverlayAdapter();

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
    public IDockable? Content
    {
        get => VisibleDockables is { Count: > 0 } list ? list[0] : null;
        set
        {
            if (Factory is { } factory)
            {
                factory.SetOverlayPanelContent(this, value);
            }
            else
            {
                s_overlayAdapter.SetOverlayPanelContent(this, value, () => new List<IDockable>());
            }
        }
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double X
    {
        get => _x;
        set => SetProperty(ref _x, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double Y
    {
        get => _y;
        set => SetProperty(ref _y, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double Width
    {
        get => _width;
        set => SetProperty(ref _width, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double Height
    {
        get => _height;
        set => SetProperty(ref _height, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int ZIndex
    {
        get => _zIndex;
        set => SetProperty(ref _zIndex, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public OverlayAnchor Anchor
    {
        get => _anchor;
        set => SetProperty(ref _anchor, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double AnchorOffsetX
    {
        get => _anchorOffsetX;
        set => SetProperty(ref _anchorOffsetX, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double AnchorOffsetY
    {
        get => _anchorOffsetY;
        set => SetProperty(ref _anchorOffsetY, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double Opacity
    {
        get => _opacity;
        set => SetProperty(ref _opacity, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool UseBackdropBlur
    {
        get => _useBackdropBlur;
        set => SetProperty(ref _useBackdropBlur, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double BlurRadius
    {
        get => _blurRadius;
        set => SetProperty(ref _blurRadius, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public OverlayPanelState State
    {
        get => _state;
        set => SetProperty(ref _state, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsPositionLocked
    {
        get => _isPositionLocked;
        set => SetProperty(ref _isPositionLocked, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsSizeLocked
    {
        get => _isSizeLocked;
        set => SetProperty(ref _isSizeLocked, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public bool IsDragging
    {
        get => _isDragging;
        set => SetProperty(ref _isDragging, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public bool IsFocused
    {
        get => _isFocused;
        set => SetProperty(ref _isFocused, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool ShowHeader
    {
        get => _showHeader;
        set => SetProperty(ref _showHeader, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool ShowShadow
    {
        get => _showShadow;
        set => SetProperty(ref _showShadow, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double CornerRadius
    {
        get => _cornerRadius;
        set => SetProperty(ref _cornerRadius, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public IOverlaySplitterGroup? SplitterGroup { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public OverlayHorizontalAlignment HorizontalContentAlignment
    {
        get => _horizontalContentAlignment;
        set => SetProperty(ref _horizontalContentAlignment, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public OverlayVerticalAlignment VerticalContentAlignment
    {
        get => _verticalContentAlignment;
        set => SetProperty(ref _verticalContentAlignment, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public OverlayVisibility Visibility
    {
        get => _visibility;
        set => SetProperty(ref _visibility, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool AnimateVisibility
    {
        get => _animateVisibility;
        set => SetProperty(ref _animateVisibility, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public OverlayVisibilityAnimation VisibilityAnimation
    {
        get => _visibilityAnimation;
        set => SetProperty(ref _visibilityAnimation, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double VisibilityAnimationDuration
    {
        get => _visibilityAnimationDuration;
        set => SetProperty(ref _visibilityAnimationDuration, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool FloatOnDoubleClick
    {
        get => _floatOnDoubleClick;
        set => SetProperty(ref _floatOnDoubleClick, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool AllowDockInto
    {
        get => _allowDockInto;
        set => SetProperty(ref _allowDockInto, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public IDock? NestedDock
    {
        get => Content as IDock;
        set => Content = value;
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool ShowDockTargets
    {
        get => _showDockTargets;
        set => SetProperty(ref _showDockTargets, value);
    }
}
