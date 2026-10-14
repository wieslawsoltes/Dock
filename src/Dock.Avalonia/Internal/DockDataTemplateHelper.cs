// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
#if AVALONIA_11
using CompiledBinding = Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindingExtension;
#endif
using Dock.Avalonia.Controls;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model.Controls;

namespace Dock.Avalonia.Internal;

/// <summary>
/// Helper class for creating default DataTemplates for dock controls.
/// </summary>
internal static class DockDataTemplateHelper
{
    /// <summary>
    /// Creates a collection of default DataTemplates for all dock control types.
    /// </summary>
    /// <returns>A collection of DataTemplates for dock controls.</returns>
    public static IEnumerable<IDataTemplate> CreateDefaultDataTemplates()
    {
        yield return CreateDataTemplate<IDocumentContent>(() => new DocumentContentControl());
        yield return CreateDataTemplate<IToolContent>(() => new ToolContentControl());
        yield return CreateDataTemplate<IProportionalDockSplitter>(() => new ProportionalStackPanelSplitter
        {
            [!ProportionalStackPanelSplitter.IsResizingEnabledProperty] = CreateSplitterBinding<IProportionalDockSplitter, bool>(
                nameof(IProportionalDockSplitter.CanResize), static splitter => splitter.CanResize),
            [!ProportionalStackPanelSplitter.PreviewResizeProperty] = CreateSplitterBinding<IProportionalDockSplitter, bool>(
                nameof(IProportionalDockSplitter.ResizePreview), static splitter => splitter.ResizePreview)
        });
        yield return CreateDataTemplate<IGridDockSplitter>(() => new GridSplitter
        {
            [!GridSplitter.ResizeDirectionProperty] = CreateSplitterBinding<IGridDockSplitter, global::Avalonia.Controls.GridResizeDirection>(
                nameof(IGridDockSplitter.ResizeDirection), static splitter => splitter.ResizeDirection switch
                {
                    Model.Core.GridResizeDirection.Rows => global::Avalonia.Controls.GridResizeDirection.Rows,
                    Model.Core.GridResizeDirection.Columns => global::Avalonia.Controls.GridResizeDirection.Columns,
                    _ => global::Avalonia.Controls.GridResizeDirection.Auto
                })
        });
        yield return CreateDataTemplate<IDocumentDock>(() => new DocumentDockControl());
        yield return CreateDataTemplate<IToolDock>(() => new ToolDockControl());
        yield return CreateDataTemplate<ISplitViewDock>(() => new SplitViewDockControl());
        yield return CreateDataTemplate<IProportionalDock>(() => new ProportionalDockControl());
        yield return CreateDataTemplate<IStackDock>(() => new StackDockControl());
        yield return CreateDataTemplate<IGridDock>(() => new GridDockControl());
        yield return CreateDataTemplate<IWrapDock>(() => new WrapDockControl());
        yield return CreateDataTemplate<IUniformGridDock>(() => new UniformGridDockControl());
        yield return CreateDataTemplate<IDockDock>(() => new DockDockControl());
        yield return CreateDataTemplate<IRootDock>(() => new RootDockControl());
    }

    private static CompiledBinding CreateSplitterBinding<T, TValue>(string propertyName, Func<T, TValue> getter)
        where T : class
    {
        // Keep the model property name for change notifications, with a typed getter instead of reflection.
        var property = new ClrPropertyInfo(propertyName, source => getter((T)source), null, typeof(TValue));
        return new CompiledBinding(new CompiledBindingPathBuilder()
            .TypeCast<T>()
            .Property(property, PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
            .Build())
        {
            Mode = BindingMode.OneWay
        };
    }

    /// <summary>
    /// Creates a generic DataTemplate for the specified type.
    /// </summary>
    /// <typeparam name="T">The data type for the template.</typeparam>
    /// <param name="factory">Factory function to create the control.</param>
    /// <returns>A DataTemplate for the specified type.</returns>
    private static FuncDataTemplate<T> CreateDataTemplate<T>(Func<Control> factory) where T : class
    {
        return new FuncDataTemplate<T>((_, _) => factory());
    }
}
