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
/// Tool.
/// </summary>
[DataContract(IsReference = true)]
public class Tool : DockableBase, ITool, IDocument, IMdiDocument, IToolContent, ITemplate<Control?>, IRecyclingDataTemplate
{
    /// <summary>
    /// Defines the <see cref="Content"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<Tool, object?>(nameof(Content));

    /// <summary>
    /// Defines the <see cref="MdiBounds"/> property.
    /// </summary>
    public static readonly DirectProperty<Tool, DockRect> MdiBoundsProperty =
        AvaloniaProperty.RegisterDirect<Tool, DockRect>(nameof(MdiBounds), o => o.MdiBounds, (o, v) => o.MdiBounds = v);

    /// <summary>
    /// Defines the <see cref="MdiState"/> property.
    /// </summary>
    public static readonly DirectProperty<Tool, MdiWindowState> MdiStateProperty =
        AvaloniaProperty.RegisterDirect<Tool, MdiWindowState>(nameof(MdiState), o => o.MdiState, (o, v) => o.MdiState = v, MdiWindowState.Normal);

    /// <summary>
    /// Defines the <see cref="MdiZIndex"/> property.
    /// </summary>
    public static readonly DirectProperty<Tool, int> MdiZIndexProperty =
        AvaloniaProperty.RegisterDirect<Tool, int>(nameof(MdiZIndex), o => o.MdiZIndex, (o, v) => o.MdiZIndex = v);

    private DockRect _mdiBounds;
    private MdiWindowState _mdiState = MdiWindowState.Normal;
    private int _mdiZIndex;



    /// <summary>
    /// Initializes new instance of the <see cref="Tool"/> class.
    /// </summary>
    public Tool()
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

    /// <summary>
    /// 
    /// </summary>
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
        return TemplateHelper.Build(Content, this);
    }

}
