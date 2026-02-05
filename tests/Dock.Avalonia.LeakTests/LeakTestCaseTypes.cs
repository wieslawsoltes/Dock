using System;
using Avalonia.Controls;

namespace Dock.Avalonia.LeakTests;

internal sealed record ControlSetup(
    Control Control,
    object?[] KeepAlive,
    Action<Control>? BeforeCleanup = null,
    Action<Control>? AfterShow = null,
    LeakTestHelpers.InputInteractionMask? InteractionMask = null);

internal sealed record WindowSetup(Window Window, object?[] KeepAlive);

internal sealed record ControlLeakCase(string Name, Func<LeakContext, ControlSetup> Create);

internal sealed record WindowLeakCase(string Name, Func<LeakContext, WindowSetup> Create);

internal sealed record LeakResult(string Name, WeakReference[] References, object?[] KeepAlive, WeakReference? WindowReference = null);
