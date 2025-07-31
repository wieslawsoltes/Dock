// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections;
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
/// Document.
/// </summary>
[DataContract(IsReference = true)]
public class Document : DockableBase, IDocument, IDocumentContent, ITemplate<Control?>, IRecyclingDataTemplate
{
    /// <summary>
    /// Defines the <see cref="Content"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<Document, object?>(nameof(Content));

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

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool OnClose()
    {
        var canClose = base.OnClose();

        if (!canClose)
        {
            return false;
        }

        if (Context is { } context && Owner is DocumentDock docDock && docDock.ItemsSource is IList items)
        {
            items.Remove(context);
        }

        return true;
    }
}
