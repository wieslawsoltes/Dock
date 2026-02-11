// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using Avalonia.Styling;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Document.
/// </summary>
public class Document : DockableBase, IMdiDocument, IDocumentContent, ITemplate<Control?>, IRecyclingDataTemplate, IDockingWindowState
{
    /// <summary>
    /// Defines the <see cref="Content"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<Document, object?>(nameof(Content));

    /// <summary>
    /// Defines the <see cref="MdiBounds"/> property.
    /// </summary>
    public static readonly DirectProperty<Document, DockRect> MdiBoundsProperty =
        AvaloniaProperty.RegisterDirect<Document, DockRect>(nameof(MdiBounds), o => o.MdiBounds, (o, v) => o.MdiBounds = v);

    /// <summary>
    /// Defines the <see cref="MdiState"/> property.
    /// </summary>
    public static readonly DirectProperty<Document, MdiWindowState> MdiStateProperty =
        AvaloniaProperty.RegisterDirect<Document, MdiWindowState>(nameof(MdiState), o => o.MdiState, (o, v) => o.MdiState = v, MdiWindowState.Normal);

    /// <summary>
    /// Defines the <see cref="MdiZIndex"/> property.
    /// </summary>
    public static readonly DirectProperty<Document, int> MdiZIndexProperty =
        AvaloniaProperty.RegisterDirect<Document, int>(nameof(MdiZIndex), o => o.MdiZIndex, (o, v) => o.MdiZIndex = v);

    /// <summary>
    /// Defines the <see cref="IsOpen"/> property.
    /// </summary>
    public static readonly DirectProperty<Document, bool> IsOpenProperty =
        AvaloniaProperty.RegisterDirect<Document, bool>(nameof(IsOpen), o => o.IsOpen, (o, v) => o.IsOpen = v);

    /// <summary>
    /// Defines the <see cref="IsActive"/> property.
    /// </summary>
    public static readonly DirectProperty<Document, bool> IsActiveProperty =
        AvaloniaProperty.RegisterDirect<Document, bool>(nameof(IsActive), o => o.IsActive, (o, v) => o.IsActive = v);

    /// <summary>
    /// Defines the <see cref="IsSelected"/> property.
    /// </summary>
    public static readonly DirectProperty<Document, bool> IsSelectedProperty =
        AvaloniaProperty.RegisterDirect<Document, bool>(nameof(IsSelected), o => o.IsSelected, (o, v) => o.IsSelected = v);

    private DockRect _mdiBounds;
    private MdiWindowState _mdiState = MdiWindowState.Normal;
    private int _mdiZIndex;
    private bool _isOpen;
    private bool _isActive;
    private bool _isSelected;

    /// <summary>
    /// Initializes new instance of the <see cref="Document"/> class.
    /// </summary>
    public Document()
    {
    }

    /// <summary>
    /// Gets or sets the content to display.
    /// </summary>
    [Content]
    [TemplateContent]
    [ResolveByName]
    [IgnoreDataMember]
    [JsonIgnore]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("MdiBounds")]
    public DockRect MdiBounds
    {
        get => _mdiBounds;
        set => SetAndRaise(MdiBoundsProperty, ref _mdiBounds, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("MdiState")]
    public MdiWindowState MdiState
    {
        get => _mdiState;
        set => SetAndRaise(MdiStateProperty, ref _mdiState, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("MdiZIndex")]
    public int MdiZIndex
    {
        get => _mdiZIndex;
        set => SetAndRaise(MdiZIndexProperty, ref _mdiZIndex, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (_isOpen == value)
            {
                return;
            }

            SetAndRaise(IsOpenProperty, ref _isOpen, value);
            NotifyDockingWindowStateChanged(DockingWindowStateProperty.IsOpen);
        }
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive == value)
            {
                return;
            }

            SetAndRaise(IsActiveProperty, ref _isActive, value);
            NotifyDockingWindowStateChanged(DockingWindowStateProperty.IsActive);
        }
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
            {
                return;
            }

            SetAndRaise(IsSelectedProperty, ref _isSelected, value);
            NotifyDockingWindowStateChanged(DockingWindowStateProperty.IsSelected);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [DataType]
    [IgnoreDataMember]
    [JsonIgnore]
    public Type? DataType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Control? Build()
    {
        return TemplateHelper.Load(Content)?.Result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    object? ITemplate.Build() => Build();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool Match(object? data)
    {
        if (DataType == null)
        {
            return true;
        }

        return DataType.IsInstanceOfType(data);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public Control? Build(object? data) => Build(data, null);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="existing"></param>
    /// <returns></returns>
    public Control? Build(object? data, Control? existing)
    {
        return TemplateHelper.Build(Content, this, existing);
    }
}
