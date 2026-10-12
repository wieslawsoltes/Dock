// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia;
using Avalonia.Controls;

namespace Dock.Avalonia.Controls;

/// <summary>Creates floating hosts for Dock's existing drop selectors and indicators.</summary>
public interface IDockAdornerWindowFactory
{
    /// <summary>Creates an unshown window, or returns null to use the standard host.</summary>
    /// <param name="adornedElement">The visual whose docking area is being displayed.</param>
    /// <param name="adorner">The native target control; Dock assigns its content, size, position, and owner.</param>
    DockAdornerWindow? CreateWindow(Visual adornedElement, Control adorner);
}
