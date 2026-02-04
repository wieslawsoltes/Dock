using System;
using Avalonia.Controls;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.LeakTests;

internal static partial class LeakTestCases
{
    internal static readonly WindowLeakCase[] WindowCases =
    [
        new WindowLeakCase("DockAdornerWindow", _ =>
            new WindowSetup(new DockAdornerWindow { Content = new Border() }, Array.Empty<object?>())),
        new WindowLeakCase("DragPreviewWindow", _ =>
            new WindowSetup(new DragPreviewWindow { Content = new Border() }, Array.Empty<object?>())),
        new WindowLeakCase("PinnedDockWindow", _ =>
            new WindowSetup(new PinnedDockWindow { Content = new Border() }, Array.Empty<object?>())),
        new WindowLeakCase("HostWindow", _ =>
            new WindowSetup(new HostWindow { Content = new Border() }, Array.Empty<object?>()))
    ];
}
