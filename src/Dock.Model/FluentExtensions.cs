// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Windows.Input;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Provides fluent creation and configuration helpers for <see cref="IFactory"/> and model contracts.
/// </summary>
public static class FluentExtensions
{
    // Creation helpers wrapping IFactory.Create* methods

    /// <summary>
    /// Creates a new <see cref="IRootDock"/> and optionally configures it.
    /// </summary>
    /// <param name="factory">The factory used to create the dock.</param>
    /// <param name="configure">Optional configuration action applied to the created dock.</param>
    /// <returns>The created <see cref="IRootDock"/>.</returns>
    public static IRootDock RootDock(this IFactory factory, Action<IRootDock>? configure = null)
    {
        var dock = factory.CreateRootDock();
        configure?.Invoke(dock);
        return dock;
    }

    /// <summary>
    /// Creates a new <see cref="IRootDock"/>, assigns it to <paramref name="dock"/>, and returns the same factory for chaining.
    /// </summary>
    /// <param name="factory">The factory used to create the dock.</param>
    /// <param name="dock">Outputs the created <see cref="IRootDock"/>.</param>
    /// <param name="configure">Optional configuration action applied to the created dock.</param>
    /// <returns>The same <see cref="IFactory"/> instance for fluent chaining.</returns>
    public static IFactory RootDock(this IFactory factory, out IRootDock dock, Action<IRootDock>? configure = null)
    {
        dock = factory.CreateRootDock();
        configure?.Invoke(dock);
        return factory;
    }

    /// <summary>
    /// Creates a new <see cref="IProportionalDock"/> with the specified orientation and optionally configures it.
    /// </summary>
    /// <param name="factory">The factory used to create the dock.</param>
    /// <param name="orientation">Initial orientation.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The created <see cref="IProportionalDock"/>.</returns>
    public static IProportionalDock ProportionalDock(this IFactory factory, Orientation orientation = Orientation.Horizontal, Action<IProportionalDock>? configure = null)
    {
        var dock = factory.CreateProportionalDock();
        dock.Orientation = orientation;
        configure?.Invoke(dock);
        return dock;
    }

    /// <summary>
    /// Creates a new <see cref="IProportionalDock"/>, assigns it to <paramref name="dock"/>, and returns the same factory for chaining.
    /// </summary>
    /// <param name="factory">The factory used to create the dock.</param>
    /// <param name="dock">Outputs the created <see cref="IProportionalDock"/>.</param>
    /// <param name="orientation">Initial orientation.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The same <see cref="IFactory"/> instance.</returns>
    public static IFactory ProportionalDock(this IFactory factory, out IProportionalDock dock, Orientation orientation = Orientation.Horizontal, Action<IProportionalDock>? configure = null)
    {
        dock = factory.CreateProportionalDock();
        dock.Orientation = orientation;
        configure?.Invoke(dock);
        return factory;
    }

    /// <summary>
    /// Creates a new <see cref="IDockDock"/> and optionally configures it.
    /// </summary>
    /// <param name="factory">The factory used to create the dock.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The created <see cref="IDockDock"/>.</returns>
    public static IDockDock DockDock(this IFactory factory, Action<IDockDock>? configure = null)
    {
        var dock = factory.CreateDockDock();
        configure?.Invoke(dock);
        return dock;
    }

    /// <summary>
    /// Creates a new <see cref="IDockDock"/>, assigns it to <paramref name="dock"/>, and returns the same factory for chaining.
    /// </summary>
    /// <param name="factory">The factory used to create the dock.</param>
    /// <param name="dock">Outputs the created <see cref="IDockDock"/>.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The same <see cref="IFactory"/> instance.</returns>
    public static IFactory DockDock(this IFactory factory, out IDockDock dock, Action<IDockDock>? configure = null)
    {
        dock = factory.CreateDockDock();
        configure?.Invoke(dock);
        return factory;
    }

    /// <summary>
    /// Creates a new <see cref="IStackDock"/> and optionally configures it.
    /// </summary>
    /// <param name="factory">The factory used to create the dock.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The created <see cref="IStackDock"/>.</returns>
    public static IStackDock StackDock(this IFactory factory, Action<IStackDock>? configure = null)
    {
        var dock = factory.CreateStackDock();
        configure?.Invoke(dock);
        return dock;
    }

    /// <summary>
    /// Creates a new <see cref="IStackDock"/>, assigns it to <paramref name="dock"/>, and returns the same factory for chaining.
    /// </summary>
    /// <param name="factory">The factory used to create the dock.</param>
    /// <param name="dock">Outputs the created <see cref="IStackDock"/>.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The same <see cref="IFactory"/> instance.</returns>
    public static IFactory StackDock(this IFactory factory, out IStackDock dock, Action<IStackDock>? configure = null)
    {
        dock = factory.CreateStackDock();
        configure?.Invoke(dock);
        return factory;
    }

    /// <summary>
    /// Creates a new <see cref="IGridDock"/> and optionally configures it.
    /// </summary>
    /// <param name="factory">The factory used to create the dock.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The created <see cref="IGridDock"/>.</returns>
    public static IGridDock GridDock(this IFactory factory, Action<IGridDock>? configure = null)
    {
        var dock = factory.CreateGridDock();
        configure?.Invoke(dock);
        return dock;
    }

    /// <summary>
    /// Creates a new <see cref="IGridDock"/>, assigns it to <paramref name="dock"/>, and returns the same factory for chaining.
    /// </summary>
    /// <param name="factory">The factory used to create the dock.</param>
    /// <param name="dock">Outputs the created <see cref="IGridDock"/>.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The same <see cref="IFactory"/> instance.</returns>
    public static IFactory GridDock(this IFactory factory, out IGridDock dock, Action<IGridDock>? configure = null)
    {
        dock = factory.CreateGridDock();
        configure?.Invoke(dock);
        return factory;
    }

    /// <summary>
    /// Creates a new <see cref="IWrapDock"/> and optionally configures it.
    /// </summary>
    /// <param name="factory">The factory used to create the dock.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The created <see cref="IWrapDock"/>.</returns>
    public static IWrapDock WrapDock(this IFactory factory, Action<IWrapDock>? configure = null)
    {
        var dock = factory.CreateWrapDock();
        configure?.Invoke(dock);
        return dock;
    }

    /// <summary>
    /// Creates a new <see cref="IWrapDock"/>, assigns it to <paramref name="dock"/>, and returns the same factory for chaining.
    /// </summary>
    /// <param name="factory">The factory used to create the dock.</param>
    /// <param name="dock">Outputs the created <see cref="IWrapDock"/>.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The same <see cref="IFactory"/> instance.</returns>
    public static IFactory WrapDock(this IFactory factory, out IWrapDock dock, Action<IWrapDock>? configure = null)
    {
        dock = factory.CreateWrapDock();
        configure?.Invoke(dock);
        return factory;
    }

    /// <summary>
    /// Creates a new <see cref="IUniformGridDock"/> and optionally configures it.
    /// </summary>
    /// <param name="factory">The factory used to create the dock.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The created <see cref="IUniformGridDock"/>.</returns>
    public static IUniformGridDock UniformGridDock(this IFactory factory, Action<IUniformGridDock>? configure = null)
    {
        var dock = factory.CreateUniformGridDock();
        configure?.Invoke(dock);
        return dock;
    }

    /// <summary>
    /// Creates a new <see cref="IUniformGridDock"/>, assigns it to <paramref name="dock"/>, and returns the same factory for chaining.
    /// </summary>
    /// <param name="factory">The factory used to create the dock.</param>
    /// <param name="dock">Outputs the created <see cref="IUniformGridDock"/>.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The same <see cref="IFactory"/> instance.</returns>
    public static IFactory UniformGridDock(this IFactory factory, out IUniformGridDock dock, Action<IUniformGridDock>? configure = null)
    {
        dock = factory.CreateUniformGridDock();
        configure?.Invoke(dock);
        return factory;
    }

    /// <summary>
    /// Creates a new <see cref="IProportionalDockSplitter"/> and optionally configures it.
    /// </summary>
    /// <param name="factory">The factory used to create the splitter.</param>
    /// <param name="canResize">Initial resize capability.</param>
    /// <param name="resizePreview">Initial resize preview setting.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The created <see cref="IProportionalDockSplitter"/>.</returns>
    public static IProportionalDockSplitter ProportionalDockSplitter(this IFactory factory, bool canResize = true, bool resizePreview = false, Action<IProportionalDockSplitter>? configure = null)
    {
        var splitter = factory.CreateProportionalDockSplitter();
        splitter.CanResize = canResize;
        splitter.ResizePreview = resizePreview;
        configure?.Invoke(splitter);
        return splitter;
    }

    /// <summary>
    /// Creates a new <see cref="IProportionalDockSplitter"/>, assigns it to <paramref name="splitter"/>, and returns the same factory for chaining.
    /// </summary>
    /// <param name="factory">The factory used to create the splitter.</param>
    /// <param name="splitter">Outputs the created <see cref="IProportionalDockSplitter"/>.</param>
    /// <param name="canResize">Initial resize capability.</param>
    /// <param name="resizePreview">Initial resize preview setting.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The same <see cref="IFactory"/> instance.</returns>
    public static IFactory ProportionalDockSplitter(this IFactory factory, out IProportionalDockSplitter splitter, bool canResize = true, bool resizePreview = false, Action<IProportionalDockSplitter>? configure = null)
    {
        splitter = factory.CreateProportionalDockSplitter();
        splitter.CanResize = canResize;
        splitter.ResizePreview = resizePreview;
        configure?.Invoke(splitter);
        return factory;
    }

    /// <summary>
    /// Creates a new <see cref="IGridDockSplitter"/> and optionally configures it.
    /// </summary>
    /// <param name="factory">The factory used to create the splitter.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The created <see cref="IGridDockSplitter"/>.</returns>
    public static IGridDockSplitter GridDockSplitter(this IFactory factory, Action<IGridDockSplitter>? configure = null)
    {
        var splitter = factory.CreateGridDockSplitter();
        configure?.Invoke(splitter);
        return splitter;
    }

    /// <summary>
    /// Creates a new <see cref="IGridDockSplitter"/>, assigns it to <paramref name="splitter"/>, and returns the same factory for chaining.
    /// </summary>
    /// <param name="factory">The factory used to create the splitter.</param>
    /// <param name="splitter">Outputs the created <see cref="IGridDockSplitter"/>.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The same <see cref="IFactory"/> instance.</returns>
    public static IFactory GridDockSplitter(this IFactory factory, out IGridDockSplitter splitter, Action<IGridDockSplitter>? configure = null)
    {
        splitter = factory.CreateGridDockSplitter();
        configure?.Invoke(splitter);
        return factory;
    }

    /// <summary>
    /// Creates a new <see cref="IToolDock"/> with the specified alignment and optionally configures it.
    /// </summary>
    /// <param name="factory">The factory used to create the dock.</param>
    /// <param name="alignment">Initial alignment.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The created <see cref="IToolDock"/>.</returns>
    public static IToolDock ToolDock(this IFactory factory, Alignment alignment = Alignment.Left, Action<IToolDock>? configure = null)
    {
        var dock = factory.CreateToolDock();
        dock.Alignment = alignment;
        configure?.Invoke(dock);
        return dock;
    }

    /// <summary>
    /// Creates a new <see cref="IToolDock"/>, assigns it to <paramref name="dock"/>, and returns the same factory for chaining.
    /// </summary>
    /// <param name="factory">The factory used to create the dock.</param>
    /// <param name="dock">Outputs the created <see cref="IToolDock"/>.</param>
    /// <param name="alignment">Initial alignment.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The same <see cref="IFactory"/> instance.</returns>
    public static IFactory ToolDock(this IFactory factory, out IToolDock dock, Alignment alignment = Alignment.Left, Action<IToolDock>? configure = null)
    {
        dock = factory.CreateToolDock();
        dock.Alignment = alignment;
        configure?.Invoke(dock);
        return factory;
    }

    /// <summary>
    /// Creates a new <see cref="IDocumentDock"/> and optionally configures it.
    /// </summary>
    /// <param name="factory">The factory used to create the dock.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The created <see cref="IDocumentDock"/>.</returns>
    public static IDocumentDock DocumentDock(this IFactory factory, Action<IDocumentDock>? configure = null)
    {
        var dock = factory.CreateDocumentDock();
        configure?.Invoke(dock);
        return dock;
    }

    /// <summary>
    /// Creates a new <see cref="IDocumentDock"/>, assigns it to <paramref name="dock"/>, and returns the same factory for chaining.
    /// </summary>
    /// <param name="factory">The factory used to create the dock.</param>
    /// <param name="dock">Outputs the created <see cref="IDocumentDock"/>.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The same <see cref="IFactory"/> instance.</returns>
    public static IFactory DocumentDock(this IFactory factory, out IDocumentDock dock, Action<IDocumentDock>? configure = null)
    {
        dock = factory.CreateDocumentDock();
        configure?.Invoke(dock);
        return factory;
    }

    /// <summary>
    /// Creates a new <see cref="IDockWindow"/> and optionally configures it.
    /// </summary>
    /// <param name="factory">The factory used to create the window.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The created <see cref="IDockWindow"/>.</returns>
    public static IDockWindow DockWindow(this IFactory factory, Action<IDockWindow>? configure = null)
    {
        var window = factory.CreateDockWindow();
        configure?.Invoke(window);
        return window;
    }

    /// <summary>
    /// Creates a new <see cref="IDockWindow"/>, assigns it to <paramref name="window"/>, and returns the same factory for chaining.
    /// </summary>
    /// <param name="factory">The factory used to create the window.</param>
    /// <param name="window">Outputs the created <see cref="IDockWindow"/>.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The same <see cref="IFactory"/> instance.</returns>
    public static IFactory DockWindow(this IFactory factory, out IDockWindow window, Action<IDockWindow>? configure = null)
    {
        window = factory.CreateDockWindow();
        configure?.Invoke(window);
        return factory;
    }

    // Generic list helper
    private static IList<T> EnsureList<T>(IList<T>? list)
    {
        return list ?? new List<T>();
    }

    // IDockable fluent setters
    /// <summary>
    /// Sets the <see cref="IDockable.Id"/>.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="id">The identifier.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithId<T>(this T dockable, string id) where T : IDockable { dockable.Id = id; return dockable; }
    /// <summary>
    /// Sets the <see cref="IDockable.Title"/>.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="title">The title.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithTitle<T>(this T dockable, string title) where T : IDockable { dockable.Title = title; return dockable; }
    /// <summary>
    /// Sets the <see cref="IDockable.Context"/>.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="context">The context.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithContext<T>(this T dockable, object? context) where T : IDockable { dockable.Context = context; return dockable; }
    /// <summary>
    /// Sets the <see cref="IDockable.Owner"/>.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="owner">The owner dockable.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithOwner<T>(this T dockable, IDockable? owner) where T : IDockable { dockable.Owner = owner; return dockable; }
    /// <summary>
    /// Sets the <see cref="IDockable.OriginalOwner"/>.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="owner">The original owner dockable.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithOriginalOwner<T>(this T dockable, IDockable? owner) where T : IDockable { dockable.OriginalOwner = owner; return dockable; }
    /// <summary>
    /// Sets the <see cref="IDockable.Factory"/>.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="factory">The factory instance.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithFactory<T>(this T dockable, IFactory? factory) where T : IDockable { dockable.Factory = factory; return dockable; }
    /// <summary>
    /// Sets the <see cref="IDockable.IsEmpty"/> flag.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="isEmpty">Whether the dockable is empty.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithIsEmpty<T>(this T dockable, bool isEmpty) where T : IDockable { dockable.IsEmpty = isEmpty; return dockable; }
    /// <summary>
    /// Sets the <see cref="IDockable.IsCollapsable"/> flag.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="isCollapsable">Whether the dock can be collapsed.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithIsCollapsable<T>(this T dockable, bool isCollapsable) where T : IDockable { dockable.IsCollapsable = isCollapsable; return dockable; }
    /// <summary>
    /// Sets the <see cref="IDockable.Proportion"/>.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="proportion">Proportion value.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithProportion<T>(this T dockable, double proportion) where T : IDockable { dockable.Proportion = proportion; return dockable; }
    /// <summary>
    /// Sets the <see cref="IDockable.Dock"/> mode.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="mode">Dock mode.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithDockMode<T>(this T dockable, DockMode mode) where T : IDockable { dockable.Dock = mode; return dockable; }
    /// <summary>
    /// Sets grid placement properties.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="column">Grid column.</param>
    /// <param name="row">Grid row.</param>
    /// <param name="columnSpan">Column span.</param>
    /// <param name="rowSpan">Row span.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithGrid<T>(this T dockable, int column = 0, int row = 0, int columnSpan = 1, int rowSpan = 1) where T : IDockable { dockable.Column = column; dockable.Row = row; dockable.ColumnSpan = columnSpan; dockable.RowSpan = rowSpan; return dockable; }
    /// <summary>
    /// Sets the <see cref="IDockable.IsSharedSizeScope"/> flag.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="isShared">Whether size is shared.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithSharedSizeScope<T>(this T dockable, bool isShared) where T : IDockable { dockable.IsSharedSizeScope = isShared; return dockable; }
    /// <summary>
    /// Sets the <see cref="IDockable.CollapsedProportion"/>.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="value">Collapsed proportion value.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithCollapsedProportion<T>(this T dockable, double value) where T : IDockable { dockable.CollapsedProportion = value; return dockable; }
    /// <summary>
    /// Sets the <see cref="IDockable.MinWidth"/>.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="value">Minimum width.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithMinWidth<T>(this T dockable, double value) where T : IDockable { dockable.MinWidth = value; return dockable; }
    /// <summary>
    /// Sets the <see cref="IDockable.MaxWidth"/>.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="value">Maximum width.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithMaxWidth<T>(this T dockable, double value) where T : IDockable { dockable.MaxWidth = value; return dockable; }
    /// <summary>
    /// Sets the <see cref="IDockable.MinHeight"/>.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="value">Minimum height.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithMinHeight<T>(this T dockable, double value) where T : IDockable { dockable.MinHeight = value; return dockable; }
    /// <summary>
    /// Sets the <see cref="IDockable.MaxHeight"/>.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="value">Maximum height.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithMaxHeight<T>(this T dockable, double value) where T : IDockable { dockable.MaxHeight = value; return dockable; }
    /// <summary>
    /// Sets the <see cref="IDockable.CanClose"/> flag.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="value">Whether it can be closed.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithCanClose<T>(this T dockable, bool value) where T : IDockable { dockable.CanClose = value; return dockable; }
    /// <summary>
    /// Sets the <see cref="IDockable.CanPin"/> flag.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="value">Whether it can be pinned.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithCanPin<T>(this T dockable, bool value) where T : IDockable { dockable.CanPin = value; return dockable; }
    /// <summary>
    /// Sets the <see cref="IDockable.CanFloat"/> flag.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="value">Whether it can float.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithCanFloat<T>(this T dockable, bool value) where T : IDockable { dockable.CanFloat = value; return dockable; }
    /// <summary>
    /// Sets the <see cref="IDockable.CanDrag"/> flag.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="value">Whether it can be dragged.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithCanDrag<T>(this T dockable, bool value) where T : IDockable { dockable.CanDrag = value; return dockable; }
    /// <summary>
    /// Sets the <see cref="IDockable.CanDrop"/> flag.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="value">Whether others can be dropped onto it.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithCanDrop<T>(this T dockable, bool value) where T : IDockable { dockable.CanDrop = value; return dockable; }
    /// <summary>
    /// Sets the <see cref="IDockable.IsModified"/> flag.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="value">Whether the dockable is modified.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithIsModified<T>(this T dockable, bool value) where T : IDockable { dockable.IsModified = value; return dockable; }
    /// <summary>
    /// Sets the docking group key.
    /// </summary>
    /// <typeparam name="T">Dockable type.</typeparam>
    /// <param name="dockable">The instance to configure.</param>
    /// <param name="group">Dock group identifier.</param>
    /// <returns>The same instance for chaining.</returns>
    public static T WithDockGroup<T>(this T dockable, string? group) where T : IDockable { dockable.DockGroup = group; return dockable; }

    // IDock fluent setters and helpers
    /// <summary>
    /// Adds one or more dockables to <see cref="IDock.VisibleDockables"/>.
    /// </summary>
    /// <typeparam name="T">Dock type.</typeparam>
    /// <param name="dock">The dock to configure.</param>
    /// <param name="items">Dockables to add.</param>
    /// <returns>The same dock for chaining.</returns>
    public static T WithVisibleDockables<T>(this T dock, params IDockable[] items) where T : IDock
    {
        dock.VisibleDockables = EnsureList(dock.VisibleDockables);
        foreach (var item in items) dock.VisibleDockables!.Add(item);
        return dock;
    }

    /// <summary>
    /// Adds a collection of dockables to <see cref="IDock.VisibleDockables"/>.
    /// </summary>
    /// <typeparam name="T">Dock type.</typeparam>
    /// <param name="dock">The dock to configure.</param>
    /// <param name="items">Dockables to add.</param>
    /// <returns>The same dock for chaining.</returns>
    public static T WithVisibleDockables<T>(this T dock, IEnumerable<IDockable> items) where T : IDock
    {
        dock.VisibleDockables = EnsureList(dock.VisibleDockables);
        foreach (var item in items) dock.VisibleDockables!.Add(item);
        return dock;
    }

    /// <summary>
    /// Shortcut for <see cref="WithVisibleDockables{T}(T, IDockable[])"/>.
    /// </summary>
    /// <typeparam name="T">Dock type.</typeparam>
    /// <param name="dock">The dock to configure.</param>
    /// <param name="items">Dockables to add.</param>
    /// <returns>The same dock for chaining.</returns>
    public static T Add<T>(this T dock, params IDockable[] items) where T : IDock
    {
        return dock.WithVisibleDockables(items);
    }

    /// <summary>
    /// Sets the <see cref="IDock.ActiveDockable"/>.
    /// </summary>
    /// <typeparam name="T">Dock type.</typeparam>
    /// <param name="dock">The dock to configure.</param>
    /// <param name="item">Dockable to mark active.</param>
    /// <returns>The same dock for chaining.</returns>
    public static T WithActiveDockable<T>(this T dock, IDockable? item) where T : IDock { dock.ActiveDockable = item; return dock; }
    /// <summary>
    /// Sets the <see cref="IDock.DefaultDockable"/>.
    /// </summary>
    /// <typeparam name="T">Dock type.</typeparam>
    /// <param name="dock">The dock to configure.</param>
    /// <param name="item">Default dockable.</param>
    /// <returns>The same dock for chaining.</returns>
    public static T WithDefaultDockable<T>(this T dock, IDockable? item) where T : IDock { dock.DefaultDockable = item; return dock; }
    /// <summary>
    /// Sets the <see cref="IDock.FocusedDockable"/>.
    /// </summary>
    /// <typeparam name="T">Dock type.</typeparam>
    /// <param name="dock">The dock to configure.</param>
    /// <param name="item">Focused dockable.</param>
    /// <returns>The same dock for chaining.</returns>
    public static T WithFocusedDockable<T>(this T dock, IDockable? item) where T : IDock { dock.FocusedDockable = item; return dock; }
    /// <summary>
    /// Sets the <see cref="IDock.IsActive"/> flag.
    /// </summary>
    /// <typeparam name="T">Dock type.</typeparam>
    /// <param name="dock">The dock to configure.</param>
    /// <param name="value">Whether active.</param>
    /// <returns>The same dock for chaining.</returns>
    public static T WithIsActive<T>(this T dock, bool value) where T : IDock { dock.IsActive = value; return dock; }
    /// <summary>
    /// Sets whether the last dockable can be closed.
    /// </summary>
    /// <typeparam name="T">Dock type.</typeparam>
    /// <param name="dock">The dock to configure.</param>
    /// <param name="value">Flag value.</param>
    /// <returns>The same dock for chaining.</returns>
    public static T WithCanCloseLastDockable<T>(this T dock, bool value) where T : IDock { dock.CanCloseLastDockable = value; return dock; }
    /// <summary>
    /// Sets whether global docking is enabled.
    /// </summary>
    /// <typeparam name="T">Dock type.</typeparam>
    /// <param name="dock">The dock to configure.</param>
    /// <param name="value">Flag value.</param>
    /// <returns>The same dock for chaining.</returns>
    public static T WithEnableGlobalDocking<T>(this T dock, bool value) where T : IDock { dock.EnableGlobalDocking = value; return dock; }

    // IRootDock fluent setters
    /// <summary>
    /// Sets whether the root is focusable.
    /// </summary>
    /// <param name="dock">The root dock.</param>
    /// <param name="value">Flag value.</param>
    /// <returns>The same root dock.</returns>
    public static IRootDock WithIsFocusableRoot(this IRootDock dock, bool value) { dock.IsFocusableRoot = value; return dock; }
    /// <summary>
    /// Adds hidden dockables to the root.
    /// </summary>
    /// <param name="dock">The root dock.</param>
    /// <param name="items">Dockables to add.</param>
    /// <returns>The same root dock.</returns>
    public static IRootDock WithHiddenDockables(this IRootDock dock, params IDockable[] items)
    {
        dock.HiddenDockables = EnsureList(dock.HiddenDockables);
        foreach (var item in items) dock.HiddenDockables!.Add(item);
        return dock;
    }
    /// <summary>
    /// Adds left pinned dockables to the root.
    /// </summary>
    /// <param name="dock">The root dock.</param>
    /// <param name="items">Dockables to add.</param>
    /// <returns>The same root dock.</returns>
    public static IRootDock WithLeftPinned(this IRootDock dock, params IDockable[] items)
    {
        dock.LeftPinnedDockables = EnsureList(dock.LeftPinnedDockables);
        foreach (var item in items) dock.LeftPinnedDockables!.Add(item);
        return dock;
    }
    /// <summary>
    /// Adds right pinned dockables to the root.
    /// </summary>
    /// <param name="dock">The root dock.</param>
    /// <param name="items">Dockables to add.</param>
    /// <returns>The same root dock.</returns>
    public static IRootDock WithRightPinned(this IRootDock dock, params IDockable[] items)
    {
        dock.RightPinnedDockables = EnsureList(dock.RightPinnedDockables);
        foreach (var item in items) dock.RightPinnedDockables!.Add(item);
        return dock;
    }
    /// <summary>
    /// Adds top pinned dockables to the root.
    /// </summary>
    /// <param name="dock">The root dock.</param>
    /// <param name="items">Dockables to add.</param>
    /// <returns>The same root dock.</returns>
    public static IRootDock WithTopPinned(this IRootDock dock, params IDockable[] items)
    {
        dock.TopPinnedDockables = EnsureList(dock.TopPinnedDockables);
        foreach (var item in items) dock.TopPinnedDockables!.Add(item);
        return dock;
    }
    /// <summary>
    /// Adds bottom pinned dockables to the root.
    /// </summary>
    /// <param name="dock">The root dock.</param>
    /// <param name="items">Dockables to add.</param>
    /// <returns>The same root dock.</returns>
    public static IRootDock WithBottomPinned(this IRootDock dock, params IDockable[] items)
    {
        dock.BottomPinnedDockables = EnsureList(dock.BottomPinnedDockables);
        foreach (var item in items) dock.BottomPinnedDockables!.Add(item);
        return dock;
    }
    /// <summary>
    /// Sets the pinned tool dock of the root.
    /// </summary>
    /// <param name="dock">The root dock.</param>
    /// <param name="pinned">Pinned tool dock.</param>
    /// <returns>The same root dock.</returns>
    public static IRootDock WithPinnedDock(this IRootDock dock, IToolDock? pinned) { dock.PinnedDock = pinned; return dock; }
    /// <summary>
    /// Sets the main window of the root.
    /// </summary>
    /// <param name="dock">The root dock.</param>
    /// <param name="window">Window instance.</param>
    /// <returns>The same root dock.</returns>
    public static IRootDock WithWindow(this IRootDock dock, IDockWindow? window) { dock.Window = window; return dock; }
    /// <summary>
    /// Adds owned windows to the root.
    /// </summary>
    /// <param name="dock">The root dock.</param>
    /// <param name="windows">Windows to add.</param>
    /// <returns>The same root dock.</returns>
    public static IRootDock WithWindows(this IRootDock dock, params IDockWindow[] windows)
    {
        dock.Windows = EnsureList(dock.Windows);
        foreach (var w in windows) dock.Windows!.Add(w);
        return dock;
    }

    // IProportionalDock fluent setter
    /// <summary>
    /// Sets the orientation of a proportional dock.
    /// </summary>
    /// <param name="dock">The proportional dock.</param>
    /// <param name="orientation">Orientation value.</param>
    /// <returns>The same dock.</returns>
    public static IProportionalDock WithOrientation(this IProportionalDock dock, Orientation orientation) { dock.Orientation = orientation; return dock; }

    // IWrapDock fluent setter
    /// <summary>
    /// Sets the orientation of a wrap dock.
    /// </summary>
    /// <param name="dock">The wrap dock.</param>
    /// <param name="orientation">Orientation value.</param>
    /// <returns>The same dock.</returns>
    public static IWrapDock WithOrientation(this IWrapDock dock, Orientation orientation) { dock.Orientation = orientation; return dock; }

    // IStackDock fluent setters
    /// <summary>
    /// Sets the orientation of a stack dock.
    /// </summary>
    /// <param name="dock">The stack dock.</param>
    /// <param name="orientation">Orientation value.</param>
    /// <returns>The same dock.</returns>
    public static IStackDock WithOrientation(this IStackDock dock, Orientation orientation) { dock.Orientation = orientation; return dock; }
    /// <summary>
    /// Sets spacing between items in a stack dock.
    /// </summary>
    /// <param name="dock">The stack dock.</param>
    /// <param name="spacing">Spacing value.</param>
    /// <returns>The same dock.</returns>
    public static IStackDock WithSpacing(this IStackDock dock, double spacing) { dock.Spacing = spacing; return dock; }

    // IGridDock fluent setters
    /// <summary>
    /// Sets grid column definitions string.
    /// </summary>
    /// <param name="dock">The grid dock.</param>
    /// <param name="definitions">Grid column definitions.</param>
    /// <returns>The same dock.</returns>
    public static IGridDock WithColumnDefinitions(this IGridDock dock, string? definitions) { dock.ColumnDefinitions = definitions; return dock; }
    /// <summary>
    /// Sets grid row definitions string.
    /// </summary>
    /// <param name="dock">The grid dock.</param>
    /// <param name="definitions">Grid row definitions.</param>
    /// <returns>The same dock.</returns>
    public static IGridDock WithRowDefinitions(this IGridDock dock, string? definitions) { dock.RowDefinitions = definitions; return dock; }

    // IUniformGridDock fluent setters
    /// <summary>
    /// Sets number of rows for uniform grid dock.
    /// </summary>
    /// <param name="dock">The uniform grid dock.</param>
    /// <param name="rows">Rows count.</param>
    /// <returns>The same dock.</returns>
    public static IUniformGridDock WithRows(this IUniformGridDock dock, int rows) { dock.Rows = rows; return dock; }
    /// <summary>
    /// Sets number of columns for uniform grid dock.
    /// </summary>
    /// <param name="dock">The uniform grid dock.</param>
    /// <param name="columns">Columns count.</param>
    /// <returns>The same dock.</returns>
    public static IUniformGridDock WithColumns(this IUniformGridDock dock, int columns) { dock.Columns = columns; return dock; }

    // IDockDock fluent setter
    /// <summary>
    /// Sets whether the last child fills remaining space.
    /// </summary>
    /// <param name="dock">The dock dock.</param>
    /// <param name="value">Flag value.</param>
    /// <returns>The same dock.</returns>
    public static IDockDock WithLastChildFill(this IDockDock dock, bool value) { dock.LastChildFill = value; return dock; }

    // IDocumentDock fluent setters and helpers
    /// <summary>
    /// Sets whether the document dock can create documents.
    /// </summary>
    /// <param name="dock">The document dock.</param>
    /// <param name="value">Flag value.</param>
    /// <returns>The same document dock.</returns>
    public static IDocumentDock WithCanCreateDocument(this IDocumentDock dock, bool value) { dock.CanCreateDocument = value; return dock; }
    /// <summary>
    /// Sets the command used to create documents.
    /// </summary>
    /// <param name="dock">The document dock.</param>
    /// <param name="command">Create document command.</param>
    /// <returns>The same document dock.</returns>
    public static IDocumentDock WithCreateDocument(this IDocumentDock dock, ICommand? command) { dock.CreateDocument = command; return dock; }
    /// <summary>
    /// Sets whether window drag is enabled for documents.
    /// </summary>
    /// <param name="dock">The document dock.</param>
    /// <param name="value">Flag value.</param>
    /// <returns>The same document dock.</returns>
    public static IDocumentDock WithEnableWindowDrag(this IDocumentDock dock, bool value) { dock.EnableWindowDrag = value; return dock; }
    /// <summary>
    /// Sets the tabs layout for the document dock.
    /// </summary>
    /// <param name="dock">The document dock.</param>
    /// <param name="layout">Tabs layout.</param>
    /// <returns>The same document dock.</returns>
    public static IDocumentDock WithTabsLayout(this IDocumentDock dock, DocumentTabLayout layout) { dock.TabsLayout = layout; return dock; }
    // Avoid shadowing instance methods; provide chainable variants with distinct names
    /// <summary>
    /// Appends a document to the document dock in a chainable way.
    /// </summary>
    /// <param name="dock">The document dock.</param>
    /// <param name="document">Document to add.</param>
    /// <returns>The same document dock.</returns>
    public static IDocumentDock AppendDocument(this IDocumentDock dock, IDockable document) { dock.AddDocument(document); return dock; }
    /// <summary>
    /// Appends a tool to the document dock in a chainable way.
    /// </summary>
    /// <param name="dock">The document dock.</param>
    /// <param name="tool">Tool to add.</param>
    /// <returns>The same document dock.</returns>
    public static IDocumentDock AppendTool(this IDocumentDock dock, IDockable tool) { dock.AddTool(tool); return dock; }

    // IDocumentDockContent fluent setter
    /// <summary>
    /// Sets the document template.
    /// </summary>
    /// <param name="dock">The document dock content.</param>
    /// <param name="template">Template instance.</param>
    /// <returns>The same instance.</returns>
    public static IDocumentDockContent WithDocumentTemplate(this IDocumentDockContent dock, IDocumentTemplate? template) { dock.DocumentTemplate = template; return dock; }

    // IToolDock fluent setters and helpers
    /// <summary>
    /// Sets tool dock alignment.
    /// </summary>
    /// <param name="dock">The tool dock.</param>
    /// <param name="alignment">Alignment value.</param>
    /// <returns>The same tool dock.</returns>
    public static IToolDock WithAlignment(this IToolDock dock, Alignment alignment) { dock.Alignment = alignment; return dock; }
    /// <summary>
    /// Sets whether the tool dock is expanded.
    /// </summary>
    /// <param name="dock">The tool dock.</param>
    /// <param name="value">Flag value.</param>
    /// <returns>The same tool dock.</returns>
    public static IToolDock WithIsExpanded(this IToolDock dock, bool value) { dock.IsExpanded = value; return dock; }
    /// <summary>
    /// Sets whether the tool dock auto hides.
    /// </summary>
    /// <param name="dock">The tool dock.</param>
    /// <param name="value">Flag value.</param>
    /// <returns>The same tool dock.</returns>
    public static IToolDock WithAutoHide(this IToolDock dock, bool value) { dock.AutoHide = value; return dock; }
    /// <summary>
    /// Sets the grip mode of the tool dock.
    /// </summary>
    /// <param name="dock">The tool dock.</param>
    /// <param name="mode">Grip mode.</param>
    /// <returns>The same tool dock.</returns>
    public static IToolDock WithGripMode(this IToolDock dock, GripMode mode) { dock.GripMode = mode; return dock; }
    /// <summary>
    /// Appends a tool to the tool dock in a chainable way.
    /// </summary>
    /// <param name="dock">The tool dock.</param>
    /// <param name="tool">Tool to add.</param>
    /// <returns>The same tool dock.</returns>
    public static IToolDock AppendTool(this IToolDock dock, IDockable tool) { dock.AddTool(tool); return dock; }

    // IProportionalDockSplitter fluent setters
    /// <summary>
    /// Sets whether the splitter can resize.
    /// </summary>
    /// <param name="splitter">The proportional dock splitter.</param>
    /// <param name="value">Flag value.</param>
    /// <returns>The same splitter.</returns>
    public static IProportionalDockSplitter WithCanResize(this IProportionalDockSplitter splitter, bool value) { splitter.CanResize = value; return splitter; }
    /// <summary>
    /// Sets whether resize preview is enabled.
    /// </summary>
    /// <param name="splitter">The proportional dock splitter.</param>
    /// <param name="value">Flag value.</param>
    /// <returns>The same splitter.</returns>
    public static IProportionalDockSplitter WithResizePreview(this IProportionalDockSplitter splitter, bool value) { splitter.ResizePreview = value; return splitter; }

    // IGridDockSplitter fluent setter
    /// <summary>
    /// Sets the resize direction of a grid splitter.
    /// </summary>
    /// <param name="splitter">The grid dock splitter.</param>
    /// <param name="direction">Resize direction.</param>
    /// <returns>The same splitter.</returns>
    public static IGridDockSplitter WithResizeDirection(this IGridDockSplitter splitter, GridResizeDirection direction) { splitter.ResizeDirection = direction; return splitter; }

    // IDockWindow fluent setters
    /// <summary>
    /// Sets the window identifier.
    /// </summary>
    /// <param name="window">The dock window.</param>
    /// <param name="id">Identifier value.</param>
    /// <returns>The same window.</returns>
    public static IDockWindow WithId(this IDockWindow window, string id) { window.Id = id; return window; }
    /// <summary>
    /// Sets the window position.
    /// </summary>
    /// <param name="window">The dock window.</param>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <returns>The same window.</returns>
    public static IDockWindow WithPosition(this IDockWindow window, double x, double y) { window.X = x; window.Y = y; return window; }
    /// <summary>
    /// Sets the window size.
    /// </summary>
    /// <param name="window">The dock window.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    /// <returns>The same window.</returns>
    public static IDockWindow WithSize(this IDockWindow window, double width, double height) { window.Width = width; window.Height = height; return window; }
    /// <summary>
    /// Sets whether the window is topmost.
    /// </summary>
    /// <param name="window">The dock window.</param>
    /// <param name="topmost">Flag value.</param>
    /// <returns>The same window.</returns>
    public static IDockWindow WithTopmost(this IDockWindow window, bool topmost) { window.Topmost = topmost; return window; }
    /// <summary>
    /// Sets the window title.
    /// </summary>
    /// <param name="window">The dock window.</param>
    /// <param name="title">Title text.</param>
    /// <returns>The same window.</returns>
    public static IDockWindow WithTitle(this IDockWindow window, string title) { window.Title = title; return window; }
    /// <summary>
    /// Sets the owner dockable for the window.
    /// </summary>
    /// <param name="window">The dock window.</param>
    /// <param name="owner">Owner dockable.</param>
    /// <returns>The same window.</returns>
    public static IDockWindow WithOwner(this IDockWindow window, IDockable? owner) { window.Owner = owner; return window; }
    /// <summary>
    /// Sets the factory associated with the window.
    /// </summary>
    /// <param name="window">The dock window.</param>
    /// <param name="factory">Factory instance.</param>
    /// <returns>The same window.</returns>
    public static IDockWindow WithFactory(this IDockWindow window, IFactory? factory) { window.Factory = factory; return window; }
    /// <summary>
    /// Sets the root layout hosted by the window.
    /// </summary>
    /// <param name="window">The dock window.</param>
    /// <param name="layout">Root layout.</param>
    /// <returns>The same window.</returns>
    public static IDockWindow WithLayout(this IDockWindow window, IRootDock? layout) { window.Layout = layout; return window; }
    /// <summary>
    /// Sets the host window adapter.
    /// </summary>
    /// <param name="window">The dock window.</param>
    /// <param name="host">Host adapter.</param>
    /// <returns>The same window.</returns>
    public static IDockWindow WithHost(this IDockWindow window, IHostWindow? host) { window.Host = host; return window; }
}
