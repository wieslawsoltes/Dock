// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Dock.Avalonia.Internal;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Managed in-app host window implementation.
/// </summary>
public sealed class ManagedHostWindow : IHostWindow
{
    private readonly ManagedHostWindowState _hostWindowState;
    private ManagedDockWindowDocument? _document;
    private ManagedWindowDock? _dock;
    private IDock? _layout;
    private string? _title;
    private double _x;
    private double _y;
    private double _width = 400;
    private double _height = 300;
    private bool _closed;
    private bool _lastCloseCanceled;

    /// <summary>
    /// Gets the current z-index for managed ordering.
    /// </summary>
    public int ManagedZIndex => _document?.MdiZIndex ?? 0;

    internal bool LastCloseCanceled => _lastCloseCanceled;

    /// <inheritdoc />
    public IHostWindowState HostWindowState => _hostWindowState;

    /// <inheritdoc />
    public bool IsTracked { get; set; }

    /// <inheritdoc />
    public IDockWindow? Window { get; set; }

    public ManagedHostWindow()
    {
        _hostWindowState = new ManagedHostWindowState(new DockManager(new DockService()), this);
    }

    /// <inheritdoc />
    public void Present(bool isDialog)
    {
        if (Window?.Factory is not { } factory)
        {
            return;
        }

        _dock = ManagedWindowRegistry.GetOrCreateDock(factory);
        _layout ??= Window.Layout;
        _title ??= Window.Title;

        if (_document is null)
        {
            _document = new ManagedDockWindowDocument(Window);
            _document.Title = _title ?? _document.Title;
            _document.MdiBounds = new DockRect(_x, _y, _width, _height);
            if (_layout is { } root)
            {
                _document.Content = ManagedDockWindowDocumentContent.Create(root);
            }
        }

        _dock.AddWindow(_document);
        Window.Host = this;
        IsTracked = true;
        _closed = false;

        if (!factory.HostWindows.Contains(this))
        {
            factory.HostWindows.Add(this);
        }

        factory.OnWindowOpened(Window);
    }

    /// <inheritdoc />
    public void Exit()
    {
        if (_closed)
        {
            return;
        }

        _lastCloseCanceled = false;

        if (Window is { } window)
        {
            if (window.Factory is { } factory)
            {
                if (factory.OnWindowClosing(window) == false)
                {
                    _lastCloseCanceled = true;
                    return;
                }
            }
            else if (!window.OnClose())
            {
                _lastCloseCanceled = true;
                return;
            }

            if (IsTracked)
            {
                window.Save();

                if (window.Layout is IDock root && root.Close.CanExecute(null))
                {
                    root.Close.Execute(null);
                }
            }
        }

        _closed = true;

        if (Window?.Factory is { } windowFactory)
        {
            windowFactory.HostWindows.Remove(this);

            if (Window is { } && Window.Layout is { })
            {
                windowFactory.CloseWindow(Window);
            }

            windowFactory.OnWindowClosed(Window);

            if (Window is { } && IsTracked)
            {
                windowFactory.RemoveWindow(Window);
            }
        }

        if (_dock is { } && _document is { })
        {
            _dock.RemoveWindow(_document);
            _document.Dispose();
            _document = null;
        }

        if (Window is { })
        {
            Window.Host = null;
        }

        IsTracked = false;
    }

    /// <inheritdoc />
    public void SetPosition(double x, double y)
    {
        if (double.IsNaN(x) || double.IsNaN(y))
        {
            return;
        }

        _x = x;
        _y = y;
        UpdateBounds();
    }

    /// <inheritdoc />
    public void GetPosition(out double x, out double y)
    {
        x = _x;
        y = _y;
    }

    /// <inheritdoc />
    public void SetSize(double width, double height)
    {
        if (double.IsNaN(width) || double.IsNaN(height))
        {
            return;
        }

        _width = width;
        _height = height;
        UpdateBounds();
    }

    /// <inheritdoc />
    public void GetSize(out double width, out double height)
    {
        width = _width;
        height = _height;
    }

    /// <inheritdoc />
    public void SetTitle(string? title)
    {
        _title = title;
        if (_document is { })
        {
            _document.Title = title ?? string.Empty;
        }
    }

    /// <inheritdoc />
    public void SetLayout(IDock layout)
    {
        _layout = layout;
        if (_document is { } document)
        {
            document.Content = ManagedDockWindowDocumentContent.Create(layout);
        }
    }

    /// <inheritdoc />
    public void SetActive()
    {
        if (_dock is { } && _document is { })
        {
            _dock.ActiveDockable = _document;
        }
    }

    private void UpdateBounds()
    {
        if (_document is not null)
        {
            _document.MdiBounds = new DockRect(_x, _y, _width, _height);
        }
    }

    internal void UpdateBoundsFromDocument(DockRect bounds)
    {
        _x = bounds.X;
        _y = bounds.Y;
        _width = bounds.Width;
        _height = bounds.Height;
    }

    internal void ProcessDrag(PixelPoint screenPoint, EventType eventType)
    {
        _hostWindowState.Process(screenPoint, eventType);
    }

    private static class ManagedDockWindowDocumentContent
    {
        public static object? Create(IDock? layout)
        {
            if (layout is null)
            {
                return null;
            }

            if (layout is not IRootDock rootDock)
            {
                return null;
            }

            return new DockControl
            {
                Layout = rootDock,
                InitializeFactory = false,
                InitializeLayout = false,
                EnableManagedWindowLayer = false
            };
        }
    }
}
