// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Windows.Input;
using Avalonia.Controls;

namespace Dock.Avalonia.Commands;

/// <summary>
/// Defines supported line scroll directions for a scroll viewer.
/// </summary>
public enum ScrollViewerLineDirection
{
    /// <summary>
    /// Scroll left by a line.
    /// </summary>
    Left,
    /// <summary>
    /// Scroll right by a line.
    /// </summary>
    Right,
    /// <summary>
    /// Scroll up by a line.
    /// </summary>
    Up,
    /// <summary>
    /// Scroll down by a line.
    /// </summary>
    Down
}

/// <summary>
/// Command that scrolls a ScrollViewer by a line in the configured direction.
/// </summary>
public sealed class ScrollViewerLineCommand : ICommand
{
    /// <summary>
    /// Gets or sets the scroll direction.
    /// </summary>
    public ScrollViewerLineDirection Direction { get; set; }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
    {
        return parameter is ScrollViewer;
    }

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        if (parameter is not ScrollViewer scrollViewer)
        {
            return;
        }

        switch (Direction)
        {
            case ScrollViewerLineDirection.Left:
                scrollViewer.LineLeft();
                break;
            case ScrollViewerLineDirection.Right:
                scrollViewer.LineRight();
                break;
            case ScrollViewerLineDirection.Up:
                scrollViewer.LineUp();
                break;
            case ScrollViewerLineDirection.Down:
                scrollViewer.LineDown();
                break;
        }
    }
}
