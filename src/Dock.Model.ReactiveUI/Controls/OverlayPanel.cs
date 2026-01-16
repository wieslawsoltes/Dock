// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Runtime.Serialization;
using Dock.Model.Adapters;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Core;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// Overlay panel.
/// </summary>
[DataContract(IsReference = true)]
public partial class OverlayPanel : DockBase, IOverlayPanel
{
    private static readonly IOverlayAdapter s_overlayAdapter = new OverlayAdapter();

    private bool _isDragging;
    private bool _isFocused;

    /// <summary>
    /// Initializes new instance of the <see cref="OverlayPanel"/> class.
    /// </summary>
    public OverlayPanel()
    {
        Width = 320;
        Height = 240;
        Opacity = 1.0;
        BlurRadius = 12.0;
        ShowHeader = true;
        ShowShadow = true;
        CornerRadius = 4.0;
        HorizontalContentAlignment = OverlayHorizontalAlignment.Stretch;
        VerticalContentAlignment = OverlayVerticalAlignment.Stretch;
        Visibility = OverlayVisibility.Visible;
        AnimateVisibility = true;
        VisibilityAnimation = OverlayVisibilityAnimation.Fade;
        VisibilityAnimationDuration = 150.0;
        FloatOnDoubleClick = true;
        AllowDockInto = true;
        ShowDockTargets = true;
    }

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
    [Reactive]
    public partial double X { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial double Y { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial double Width { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial double Height { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial int ZIndex { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial OverlayAnchor Anchor { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial double AnchorOffsetX { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial double AnchorOffsetY { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial double Opacity { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool UseBackdropBlur { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial double BlurRadius { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial OverlayPanelState State { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool IsPositionLocked { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool IsSizeLocked { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public bool IsDragging
    {
        get => _isDragging;
        set => this.RaiseAndSetIfChanged(ref _isDragging, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public bool IsFocused
    {
        get => _isFocused;
        set => this.RaiseAndSetIfChanged(ref _isFocused, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool ShowHeader { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool ShowShadow { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial double CornerRadius { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public IOverlaySplitterGroup? SplitterGroup { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial OverlayHorizontalAlignment HorizontalContentAlignment { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial OverlayVerticalAlignment VerticalContentAlignment { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial OverlayVisibility Visibility { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool AnimateVisibility { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial OverlayVisibilityAnimation VisibilityAnimation { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial double VisibilityAnimationDuration { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool FloatOnDoubleClick { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool AllowDockInto { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public IDock? NestedDock
    {
        get => Content as IDock;
        set => Content = value;
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool ShowDockTargets { get; set; }
}
