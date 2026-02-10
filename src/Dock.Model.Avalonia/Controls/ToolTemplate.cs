// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Metadata;
using Avalonia.Styling;
using Dock.Model.Controls;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Tool template.
/// </summary>
public sealed class ToolTemplate : IToolTemplate, ITemplate<Control?>, IRecyclingDataTemplate
{
    /// <summary>
    /// Initializes new instance of the <see cref="ToolTemplate"/> class.
    /// </summary>
    public ToolTemplate()
    {
    }

    /// <summary>
    /// Gets or sets tool content.
    /// </summary>
    [Content]
    [TemplateContent]
    [ResolveByName]
    [IgnoreDataMember]
    [JsonIgnore]
    public object? Content { get; set; }

    /// <summary>
    /// Gets or sets data type.
    /// </summary>
    [DataType]
    [IgnoreDataMember]
    [JsonIgnore]
    public Type? DataType { get; set; }

    /// <summary>
    /// Builds the tool content control.
    /// </summary>
    /// <returns>The built control.</returns>
    public Control? Build()
    {
        return Load(Content)?.Result;
    }

    /// <summary>
    /// Builds template content.
    /// </summary>
    /// <returns>The built control.</returns>
    object? ITemplate.Build() => Build();

    /// <summary>
    /// Checks whether template matches provided data.
    /// </summary>
    /// <param name="data">Data object.</param>
    /// <returns>True when matching.</returns>
    public bool Match(object? data)
    {
        if (DataType == null)
        {
            return true;
        }

        return DataType.IsInstanceOfType(data);
    }

    /// <summary>
    /// Builds template content.
    /// </summary>
    /// <param name="data">Data object.</param>
    /// <returns>The built control.</returns>
    public Control? Build(object? data) => Build(data, null);

    /// <summary>
    /// Builds template content.
    /// </summary>
    /// <param name="data">Data object.</param>
    /// <param name="existing">Existing control.</param>
    /// <returns>The built control.</returns>
    public Control? Build(object? data, Control? existing)
    {
        return existing ?? TemplateContent.Load(Content)?.Result;
    }

    private static TemplateResult<Control>? Load(object? templateContent)
    {
        if (templateContent is Func<IServiceProvider, object> direct)
        {
            return (TemplateResult<Control>)direct(null!);
        }

        throw new ArgumentException(nameof(templateContent));
    }
}
