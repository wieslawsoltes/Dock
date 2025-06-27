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

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Tool.
/// </summary>
[DataContract(IsReference = true)]
public class Tool : DockableBase, ITool, IDocument, IToolContent, ITemplate<Control?>, IRecyclingDataTemplate
{
    private double _minWidth = double.NaN;
    private double _maxWidth = double.NaN;
    private double _minHeight = double.NaN;
    private double _maxHeight = double.NaN;
    /// <summary>
    /// Defines the <see cref="Content"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<Tool, object?>(nameof(Content));

    /// <summary>
    /// Defines the <see cref="MinWidth"/> property.
    /// </summary>
    public static readonly DirectProperty<Tool, double> MinWidthProperty =
        AvaloniaProperty.RegisterDirect<Tool, double>(nameof(MinWidth), o => o.MinWidth, (o, v) => o.MinWidth = v, double.NaN);

    /// <summary>
    /// Defines the <see cref="MaxWidth"/> property.
    /// </summary>
    public static readonly DirectProperty<Tool, double> MaxWidthProperty =
        AvaloniaProperty.RegisterDirect<Tool, double>(nameof(MaxWidth), o => o.MaxWidth, (o, v) => o.MaxWidth = v, double.NaN);

    /// <summary>
    /// Defines the <see cref="MinHeight"/> property.
    /// </summary>
    public static readonly DirectProperty<Tool, double> MinHeightProperty =
        AvaloniaProperty.RegisterDirect<Tool, double>(nameof(MinHeight), o => o.MinHeight, (o, v) => o.MinHeight = v, double.NaN);

    /// <summary>
    /// Defines the <see cref="MaxHeight"/> property.
    /// </summary>
    public static readonly DirectProperty<Tool, double> MaxHeightProperty =
        AvaloniaProperty.RegisterDirect<Tool, double>(nameof(MaxHeight), o => o.MaxHeight, (o, v) => o.MaxHeight = v, double.NaN);

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

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double MinWidth
    {
        get => _minWidth;
        set => SetAndRaise(MinWidthProperty, ref _minWidth, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double MaxWidth
    {
        get => _maxWidth;
        set => SetAndRaise(MaxWidthProperty, ref _maxWidth, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double MinHeight
    {
        get => _minHeight;
        set => SetAndRaise(MinHeightProperty, ref _minHeight, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double MaxHeight
    {
        get => _maxHeight;
        set => SetAndRaise(MaxHeightProperty, ref _maxHeight, value);
    }
}
