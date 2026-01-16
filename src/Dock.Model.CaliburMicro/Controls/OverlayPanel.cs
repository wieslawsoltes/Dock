// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.CaliburMicro.Core;

namespace Dock.Model.CaliburMicro.Controls;

/// <summary>
/// Overlay panel.
/// </summary>
[DataContract(IsReference = true)]
public class OverlayPanel : DockBase, IOverlayPanel
{
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
                return;
            }

            var list = VisibleDockables;
            if (list is null)
            {
                if (value is null)
                {
                    return;
                }

                value.Owner = this;
                list = new List<IDockable> { value };
                VisibleDockables = list;
                return;
            }

            if (list.Count == 0)
            {
                if (value is null)
                {
                    return;
                }

                value.Owner = this;
                list.Add(value);
                return;
            }

            if (value is null)
            {
                list.RemoveAt(0);
                return;
            }

            value.Owner = this;
            list[0] = value;
        }
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double X
    {
        get => _x;
        set => Set(ref _x, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double Y
    {
        get => _y;
        set => Set(ref _y, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double Width
    {
        get => _width;
        set => Set(ref _width, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double Height
    {
        get => _height;
        set => Set(ref _height, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int ZIndex
    {
        get => _zIndex;
        set => Set(ref _zIndex, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public OverlayAnchor Anchor
    {
        get => _anchor;
        set => Set(ref _anchor, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double AnchorOffsetX
    {
        get => _anchorOffsetX;
        set => Set(ref _anchorOffsetX, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double AnchorOffsetY
    {
        get => _anchorOffsetY;
        set => Set(ref _anchorOffsetY, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double Opacity
    {
        get => _opacity;
        set => Set(ref _opacity, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool UseBackdropBlur
    {
        get => _useBackdropBlur;
        set => Set(ref _useBackdropBlur, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double BlurRadius
    {
        get => _blurRadius;
        set => Set(ref _blurRadius, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public OverlayPanelState State
    {
        get => _state;
        set => Set(ref _state, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsPositionLocked
    {
        get => _isPositionLocked;
        set => Set(ref _isPositionLocked, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsSizeLocked
    {
        get => _isSizeLocked;
        set => Set(ref _isSizeLocked, value);
    }

    [IgnoreDataMember]
    public bool IsDragging
    {
        get => _isDragging;
        set => Set(ref _isDragging, value);
    }

    [IgnoreDataMember]
    public bool IsFocused
    {
        get => _isFocused;
        set => Set(ref _isFocused, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool ShowHeader
    {
        get => _showHeader;
        set => Set(ref _showHeader, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool ShowShadow
    {
        get => _showShadow;
        set => Set(ref _showShadow, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double CornerRadius
    {
        get => _cornerRadius;
        set => Set(ref _cornerRadius, value);
    }

    [IgnoreDataMember]
    public IOverlaySplitterGroup? SplitterGroup { get; set; }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public OverlayHorizontalAlignment HorizontalContentAlignment
    {
        get => _horizontalContentAlignment;
        set => Set(ref _horizontalContentAlignment, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public OverlayVerticalAlignment VerticalContentAlignment
    {
        get => _verticalContentAlignment;
        set => Set(ref _verticalContentAlignment, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public OverlayVisibility Visibility
    {
        get => _visibility;
        set => Set(ref _visibility, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool AnimateVisibility
    {
        get => _animateVisibility;
        set => Set(ref _animateVisibility, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public OverlayVisibilityAnimation VisibilityAnimation
    {
        get => _visibilityAnimation;
        set => Set(ref _visibilityAnimation, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double VisibilityAnimationDuration
    {
        get => _visibilityAnimationDuration;
        set => Set(ref _visibilityAnimationDuration, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool FloatOnDoubleClick
    {
        get => _floatOnDoubleClick;
        set => Set(ref _floatOnDoubleClick, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool AllowDockInto
    {
        get => _allowDockInto;
        set => Set(ref _allowDockInto, value);
    }

    [IgnoreDataMember]
    public IDock? NestedDock
    {
        get => Content as IDock;
        set => Content = value;
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool ShowDockTargets
    {
        get => _showDockTargets;
        set => Set(ref _showDockTargets, value);
    }
}
